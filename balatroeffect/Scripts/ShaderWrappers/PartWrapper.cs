// PengoTarot: ShaderPartContainer (per-part bitmap render) wrapping & unwrapping.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace PengoTarot.BalatroEffect
{
    public static partial class PartWrapper
    {
        public const string SpPrefix = "BalatroShaderPart_";
        /// <summary>Text parts render poorly through SubViewports — skip wrapping.</summary>
        public static readonly HashSet<string> TextPartNames = new() { "TitleLabel", "DescriptionLabel" };

        // ========================================================================
        // PUBLIC
        // ========================================================================

        public static void WrapParts(string cid, Control root)
        {
            //GD.Print($"[DBG] WrapParts on {root.Name}, before: {string.Join(", ", root.GetChildren().Select(c => c.Name))}");
            
            var orig = new List<Node>();
            foreach (var ch in root.GetChildren()) orig.Add(ch);
            var list = new List<Node>();
            foreach (var node in orig)
            {
                if (node is not Control c) { list.Add(node); continue; }
                string nm = c.Name;
                if (TiltWrapper.SkipNames.Contains(nm) || nm == "PortraitCanvasGroup" || TextPartNames.Contains(nm)) { list.Add(c); continue; }
                if (Config.AllPartNames.Contains(nm) && Config.GetEffect(cid, nm) > 0)
                {
                    Vector2 gp = c.Position + root.Position;
                    var sc = CreateShaderPart(cid, nm, c, gp, c.Size);
                    root.RemoveChild(c); c.Position = Vector2.Zero;
                    sc.GetNode<SubViewport>("SubViewport").GetChild(0).AddChild(c);
                    list.Add(sc);
                }
                else list.Add(c);
            }
            foreach (var ch in root.GetChildren()) root.RemoveChild(ch);
            foreach (var ch in list) root.AddChild(ch);

            //GD.Print($"[DBG] WrapParts after rebuild, list order: {string.Join(", ", list.Select(n => n.Name))}, root children now: {string.Join(", ", root.GetChildren().Select(c => c.Name))}");
        }

        public static void UnwrapParts(Control root)
        {
            //GD.Print($"[DBG] UnwrapParts on {root.Name}, before: {string.Join(", ", root.GetChildren().Select(c => c.Name))}");

            var items = new List<(ShaderPartContainer c, int idx)>();
            var kids = new List<Node>();
            foreach (var ch in root.GetChildren()) kids.Add(ch);
            for (int i = 0; i < kids.Count; i++)
                if (kids[i].Name.ToString().StartsWith(SpPrefix) && kids[i] is ShaderPartContainer sc)
                    items.Add((sc, i));
            items.Reverse();
            foreach (var (sc, idx) in items)
            {
                var vp = sc.GetNode<SubViewport>("SubViewport");
                Node inner = vp.GetChild(0).GetChild(0);
                vp.GetChild(0).RemoveChild(inner);
                if (inner is Control ctrl) ctrl.Position = sc.OriginalPosition;
                root.RemoveChild(sc); root.AddChild(inner); root.MoveChild(inner, idx);
                sc.QueueFree();
            }
            //GD.Print($"[DBG] UnwrapParts after restore, root children: {string.Join(", ", root.GetChildren().Select(c => c.Name))}");
        }

        public static ShaderPartContainer CreateShaderPart(string cid, string pn, Control pc, Vector2 gp, Vector2 sz)
        {
            int em = Config.GetEffect(cid, pn);
            Shader? sh = EffectRegistry.GetShader(em) ?? EffectRegistry.GetShader(1);

            var m = new ShaderMaterial { Shader = sh };
            m.SetShaderParameter(TiltWrapper.SeedKey, pc.GetHashCode() % 10000 / 10.0f);
            // EffectRegistry.ApplyNoiseTexture(m); // TODO: 噪声纹理功能未完成，暂时注释
            m.SetShaderParameter(TiltWrapper.EffectModeKey, em);
            m.SetShaderParameter(TiltWrapper.IntensityKey, Config.GetIntensity(cid));
            m.SetShaderParameter(TiltWrapper.UvOffsetKey, gp / TiltWrapper.UvRefSize);
            m.SetShaderParameter(TiltWrapper.UvScaleKey, sz / TiltWrapper.UvRefSize);
            m.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, (gp - TiltWrapper.UvRefHalf) / TiltWrapper.UvRefSize);

            var sc = new ShaderPartContainer
            {
                Name = SpPrefix + pn, Material = m, Size = sz, Position = pc.Position,
                MouseFilter = Control.MouseFilterEnum.Ignore, Stretch = true,
                PartName = pn, CardId = cid, OriginalPosition = pc.Position
            };
            var vp = new SubViewport { Name = "SubViewport", TransparentBg = true, Disable3D = true, Size = (Vector2I)sz };
            vp.AddChild(new Control());
            sc.AddChild(vp);
            return sc;
        }

        // ========================================================================
        // ShaderPartContainer — dynamic effect switching + tilt shimmer pass-through
        // ========================================================================
        public partial class ShaderPartContainer : SubViewportContainer
        {
            public string? PartName, CardId;
            public Vector2 OriginalPosition;
            private TiltWrapper.TiltContainer? _tilt;

            public override void _Process(double _)
            {
                if (Material is not ShaderMaterial m || string.IsNullOrEmpty(CardId) || string.IsNullOrEmpty(PartName)) return;
                if (_tilt == null || !GodotObject.IsInstanceValid(_tilt))
                {
                    _tilt = null;
                    for (Node? cur = GetParent(); cur != null; cur = cur.GetParent())
                        if (cur is TiltWrapper.TiltContainer tc) { _tilt = tc; break; }
                }

                // Reset rotation when no tilt parent exists (e.g. after unhover).
                // Stale x_rot/y_rot would cause perspective_warp_uv in modes 1-5
                // to incorrectly clip/scale even though the card is not tilted.
                if (_tilt == null)
                {
                    m.SetShaderParameter(TiltWrapper.XRotKey, 0f);
                    m.SetShaderParameter(TiltWrapper.YRotKey, 0f);
                }

                int e = Config.GetEffect(CardId, PartName);
                if (e == 0) { m.SetShaderParameter(TiltWrapper.EffectModeKey, 0); return; }

                Shader? ts = EffectRegistry.GetShader(e);
                if (ts != null && m.Shader != ts)
                {
                    var nm = new ShaderMaterial { Shader = ts };
                    nm.SetShaderParameter(TiltWrapper.SeedKey, m.GetShaderParameter(TiltWrapper.SeedKey));
                    // EffectRegistry.ApplyNoiseTexture(nm); // TODO: 噪声纹理功能未完成，暂时注释
                    nm.SetShaderParameter(TiltWrapper.EffectModeKey, e);
                    nm.SetShaderParameter(TiltWrapper.IntensityKey, Config.GetIntensity(CardId));
                    nm.SetShaderParameter(TiltWrapper.UvOffsetKey, m.GetShaderParameter(TiltWrapper.UvOffsetKey));
                    nm.SetShaderParameter(TiltWrapper.UvScaleKey, m.GetShaderParameter(TiltWrapper.UvScaleKey));
                    nm.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, m.GetShaderParameter(TiltWrapper.FoilAltUvOffsetKey));
                    Material = nm; m = nm;
                }
                else { m.SetShaderParameter(TiltWrapper.EffectModeKey, e); m.SetShaderParameter(TiltWrapper.IntensityKey, Config.GetIntensity(CardId)); }

                // Card-play rotation animation takes priority over tilt shimmer
                var playRot = CardPlayTracker.GetPlayRotation(CardId);
                if (playRot.HasValue)
                {
                    m.SetShaderParameter(TiltWrapper.XRotKey, playRot.Value.xRot);
                    m.SetShaderParameter(TiltWrapper.YRotKey, playRot.Value.yRot);
                }
                else if (_tilt?.Material is ShaderMaterial tm && GodotObject.IsInstanceValid(_tilt))
                { m.SetShaderParameter(TiltWrapper.XRotKey, tm.GetShaderParameter(TiltWrapper.XRotKey)); m.SetShaderParameter(TiltWrapper.YRotKey, tm.GetShaderParameter(TiltWrapper.YRotKey)); }
            }
        }
    }
}
