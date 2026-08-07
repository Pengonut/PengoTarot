// PengoTarot: TiltContainer (3D perspective SubViewport) lifecycle & child filtering.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace PengoTarot.BalatroEffect
{
    public static partial class TiltWrapper
    {
        // ── Shader parameter keys (shared with PartWrapper / FullCardWrapper) ──
        public static readonly StringName XRotKey = "x_rot";
        public static readonly StringName YRotKey = "y_rot";
        public static readonly StringName EffectModeKey = "effect_mode";
        public static readonly StringName IntensityKey = "intensity";
        public static readonly StringName SeedKey = "seed";
        public static readonly StringName UvOffsetKey = "uv_offset";
        public static readonly StringName UvScaleKey = "uv_scale";
        public static readonly StringName FoilAltUvOffsetKey = "foil_alt_uv_offset";

        // ── Node names ──
        public const string TcName = "BalatroTiltContainer";
        public const string TiltRootName = "BalatroTiltRoot";

        // ── UV reference frame (480×480) ──
        public static readonly Vector2 UvRefSize = new(480f, 480f);
        public static readonly Vector2 UvRefHalf = new(240f, 240f);

        // ── Child filtering ──
        /// <summary>Nodes that always stay on Body (never enter Tilt SubViewport).</summary>
        public static readonly HashSet<string> SkipNames = new() { "CardVfxContainer", "RareGlow", "UncommonGlow" };
        /// <summary>card.tscn direct children — only these enter Tilt. Dynamic VFX stay on Body.</summary>
        public static readonly HashSet<string> CardTemplateNames = new()
        {
            "Shadow", "Highlight", "PortraitCanvasGroup",
            "AncientBorderGlassOverlay", "AncientBorder", "AncientTextBg",
            "Lock", "Frame", "DescriptionLabel", "PortraitBorder",
            "OverlayContainer", "TitleBanner", "AncientBanner", "TitleLabel", "TypePlaque",
            "EnergyIcon", "StarIcon", "Enchantment", "EnchantmentVfxOverride"
        };

        // ── Tilt shader (lazy loaded) ──
        public static readonly Lazy<Shader?> TiltShader = new(() =>
        {
            try { return GD.Load<Shader>("res://balatroeffect/Shaders/balatro_effects.gdshader"); }
            catch (Exception ex) { GD.PrintErr($"[PengoTarot] Tilt shader: {ex.Message}"); return null; }
        });

        // ── External node inclusion filter ──
        private static readonly HashSet<string> VfxKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "vfx", "effect", "particle", "anim", "trail", "glow", "sparkle"
        };

        /// <summary>
        /// 决定一个非模板、非跳过列表的外部节点是否应该被纳入 Tilt 包裹。
        /// 默认排除名称中含有特效关键词的节点。
        /// </summary>
        private static bool ShouldIncludeExternalNode(Node node)
        {
            if (!(node is Control)) return false;
            string name = node.Name.ToString();
            foreach (var keyword in VfxKeywords)
                if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    return false;
            return true;
        }

        // ========================================================================
        // PUBLIC LIFECYCLE
        // ========================================================================
        public static TiltContainer? CreateTilt(NCard card, string cid, bool fullCard = false)
        {
            var m = new ShaderMaterial { Shader = TiltShader.Value };
            if (m.Shader == null) return null;
            m.SetShaderParameter(SeedKey, card.GetHashCode() % 10000 / 10.0f);
            m.SetShaderParameter(EffectModeKey, 0);

            var t = new TiltContainer
            {
                Name = TcName, Material = m, Size = new Vector2I(1200, 1200), Stretch = true,
                MouseFilter = Control.MouseFilterEnum.Ignore, Position = new Vector2(-600, -600),
                PivotOffset = new Vector2(600, 600), ProcessedCardId = cid, _nCard = card
            };
            var vp = new SubViewport { Name = "SubViewport", TransparentBg = true, Disable3D = true, Size = new Vector2I(1200, 1200) };
            var root = new Control { Name = TiltRootName, Position = new Vector2(600, 600) };
            vp.AddChild(root); t.AddChild(vp);

            // 保存 Body 当前**完整**子节点顺序的逻辑名称快照（包括 SkipNames 中的节点）
            var logicalNames = new List<string>();
            for (int i = 0; i < card.Body.GetChildCount(); i++)
            {
                var child = card.Body.GetChild(i);
                string name = child.Name;
                // 去除可能的 ShaderPart 前缀，统一为原始部件名称
                if (name.StartsWith(PartWrapper.SpPrefix))
                    name = name.Substring(PartWrapper.SpPrefix.Length);
                logicalNames.Add(name);
            }
            t.OriginalBodyNames = logicalNames.ToArray();

            // 移动节点到 tilt（仍保持原有过滤规则）
            var kids = new Node[card.Body.GetChildCount()];
            for (int i = 0; i < card.Body.GetChildCount(); i++) kids[i] = card.Body.GetChild(i);

            foreach (var k in kids)
            {
                if (SkipNames.Contains(k.Name)) continue;

                bool isTemplate = CardTemplateNames.Contains(k.Name);
                bool isExternal = !isTemplate;
                bool include = isTemplate || (isExternal && ShouldIncludeExternalNode(k));

                if (!include) continue;

                card.Body.RemoveChild(k);
                root.AddChild(k);
            }
            return t;
        }
        
        public static void DestroyTilt(TiltContainer tilt, Control body)
        {
            // 1. 提前移除并释放 Tilt
            var vp = tilt.GetNode<SubViewport>("SubViewport");
            tilt.GetParent()?.RemoveChild(tilt);
            tilt.QueueFree();

            // 2. 解包并移回所有子节点到 body
            Control root = (Control)vp.GetChild(0);
            PartWrapper.UnwrapParts(root);

            var movedChildren = new Node[root.GetChildCount()];
            for (int i = 0; i < root.GetChildCount(); i++)
                movedChildren[i] = root.GetChild(i);
            foreach (var child in movedChildren)
            {
                root.RemoveChild(child);
                body.AddChild(child);
            }

            // 3. 根据创建时保存的逻辑名称快照，严格恢复 body 的原始子节点顺序
            if (tilt.OriginalBodyNames != null && tilt.OriginalBodyNames.Length > 0)
            {
                // 收集当前 body 下所有子节点
                var bodyChildren = new List<Node>();
                foreach (var child in body.GetChildren())
                    bodyChildren.Add(child);

                var ordered = new List<Node>();

                foreach (string logicalName in tilt.OriginalBodyNames)
                {
                    // 先找原始名称，再找可能被 ShaderPart 包装后的名称
                    Node? match = bodyChildren.FirstOrDefault(n =>
                        n.Name == logicalName ||
                        n.Name.ToString().StartsWith(PartWrapper.SpPrefix + logicalName));

                    if (match != null)
                    {
                        ordered.Add(match);
                        bodyChildren.Remove(match);
                    }
                }

                // 快照中未包含的剩余节点（如动态添加的 VFX 等）保持原有相对顺序追加
                ordered.AddRange(bodyChildren);

                // 一次性重新构建子节点顺序
                for (int i = 0; i < ordered.Count; i++)
                {
                    Node n = ordered[i];
                    if (n.GetParent() == body)
                        body.RemoveChild(n);
                    body.AddChild(n);
                    body.MoveChild(n, i);
                }
            }
        }

        // ========================================================================
        // TiltContainer — 1200×1200 SubViewportContainer with 3D tilt shader
        // ========================================================================
        public partial class TiltContainer : SubViewportContainer
        {
            public string ProcessedCardId = "";
            internal NCard? _nCard;
            private const float MT = 16f, LS = 0.2f;
            private NCardHolder? _holder;

            /// <summary>
            /// 创建 Tilt 前 Body 的完整子节点顺序快照，用于销毁时还原。
            /// </summary>
            internal string[]? OriginalBodyNames;

            public override void _Process(double _)
            {
                if (_nCard is not NCard nc || !Config.GlobalDynamicEffect) return;
                UpdHolder();
                if (Material is not ShaderMaterial m || _nCard == null || _holder == null) return;

                float tx = 0, ty = 0;
                bool hv = _holder is NHandCardHolder { ZIndex: > 0 }
                    || (_holder.Hitbox is { IsEnabled: true } hb && Traverse.Create(hb).Field<bool>("_isHovered").Value);
                if (hv)
                {
                    var off = _nCard.GetGlobalMousePosition() - _nCard.GlobalPosition;
                    var sc = _nCard.GetGlobalTransform().Scale.Max(0.01f) * 256f;
                    tx = off.Y / sc.X * -MT; ty = off.X / sc.Y * MT;
                }
                tx = Mathf.Clamp(tx, -MT, MT); ty = Mathf.Clamp(ty, -MT, MT);
                float cx = (float)m.GetShaderParameter(XRotKey), cy = (float)m.GetShaderParameter(YRotKey);
                float curX = Mathf.Lerp(cx, tx, LS);
                float curY = Mathf.Lerp(cy, ty, LS);
                m.SetShaderParameter(XRotKey, curX);
                m.SetShaderParameter(YRotKey, curY);
            }
            private void UpdHolder()
            {
                NCardHolder? f = null;
                for (Node? cur = GetParent(); cur != null; cur = cur.GetParent())
                    if (cur is NCardHolder h) { f = h; break; }
                if (_holder == f) return; _holder = f;
                if (Material is ShaderMaterial m) { m.SetShaderParameter(XRotKey, 0f); m.SetShaderParameter(YRotKey, 0f); }
            }
        }
    }
}