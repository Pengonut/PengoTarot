// Based on code from BalatroEffects by Indi (MIT License)
// PengoTarot: TiltContainer (3D perspective) + ShaderPartContainer (per-part bitmap render) + NextPass chain safety.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace PengoTarot.BalatroEffect
{
    public static class ShaderController
    {
        // Convenience aliases to wrapper constants (used in Inspect path)
        private static readonly HashSet<string> SkipNames = TiltWrapper.SkipNames;
        private static readonly HashSet<string> TextPartNames = PartWrapper.TextPartNames;
        private static readonly Dictionary<NCard, Action<CardModel?>> _handlers = new();
        /// <summary>卡牌 Model 的 EnchantmentChanged 处理器（附魔变化时重应用特效）。</summary>
        private static readonly Dictionary<NCard, (CardModel model, Action handler)> _enchHandlers = new();

        static ShaderController()
        {
            Config.PerformanceThrottled += OnPerformanceThrottled;
            // 附魔配置结构变化（部件/整卡/编辑模式）→ 全局重应用，保证运行时自动包裹即时生效
            EnchantmentConfig.Changed += () => RefreshAllCards();
        }

        // ── Lazy Tilt Infrastructure ───────────────────────────────────────────
        // Only create expensive SubViewport nodes when the mouse hovers over a
        // card, and destroy them shortly after the mouse leaves. This avoids
        // creating hundreds of SubViewports for cards that are just sitting in
        // hand/draw/discard piles with no tilt effect visible.
        //
        // _hoverCards  : cards registered for lazy tilt (non-FullCard, non-inspect)
        // _hoverWatchActive : whether we're hooked into SceneTree.ProcessFrame
        // TiltRemoveDelayMsec : grace period before destroying tilt after unhover
        private static readonly Dictionary<NCard, CardHoverState> _hoverCards = new();
        private static bool _hoverWatchActive;
        private const ulong TiltRemoveDelayMsec = 0;

        // ── Portrait intensity 每帧刷新（material 方式的插画不随 Slider 实时更新，需每帧同步） ──
        private static readonly Dictionary<NCard, (string Cid, Control? Portrait, Control? Ancient)> _portraitRefresh = new();
        private static bool _portraitRefreshActive;

        private sealed class CardHoverState
        {
            public string Cid = "";
            public TiltWrapper.TiltContainer? Tilt;
            public ulong LeaveTimeMsec; // 0 = hovered / not yet created
            /// <summary>True when per-part effects were pre-applied on Body (Path 3a).
            /// Tilt creation skips WrapParts; tilt destruction preserves ShaderPartContainers.</summary>
            public bool HasInlineEffects;
        }

        private static bool CardHasEffects(string cid)
        {
            // v7：含 fully（FullCardEffect）识别，避免 fully 卡被 ApplyShader 提前跳过
            return Config.HasEffect(cid);
        }

        // ========================================================================
        // PUBLIC
        // ========================================================================

        // ════════════════════════════════════════════════════════════════════════
        // ApplyShader — entry point for card visual effects
        // ════════════════════════════════════════════════════════════════════════
        // Four paths:
        //   1. Inspect screen  → ApplyShaderInspect (no tilt, direct per-part wrap)
        //   2. FullCard effect → immediate tilt + FullCard wrapper (persistent)
        //   3. Hand card       → lazy tilt: register for hover detection
        //   4. Other card      → immediate tilt (original behavior)
        public static void ApplyShader(NCard card)
        {
            // 卡已被释放（回收进池 / 界面关闭 / 移除）时直接跳过：
            // Model 克隆（MutableClone → DeepCloneFields → EnchantInternal）触发 EnchantmentChanged
            // 事件回调时可能带着已释放的 NCard，访问 card.Model / card.Body 会抛 ObjectDisposedException。
            if (!GodotObject.IsInstanceValid(card)) return;
            if (card?.Model == null) return;
            if (card.Body == null)
            {
                // Body 未就绪（初始化时序）：轮询等待就绪后重应用
                ScheduleReapply(card);
                return;
            }
            string cid = card.Model.Id.ToString();
            if (string.IsNullOrEmpty(cid)) return;

            // 附魔覆盖：把卡牌当前附魔的配置（若有）关联到该卡 id。
            // 预览（IsEnchantmentPreview）仅用附魔配置；运行时合并（卡效果为底，附魔覆盖）。
            Config.SetCardEnchantmentOverlay(cid, card.Model.Enchantment?.Id.ToString(), card.Model.IsEnchantmentPreview);

            // 卡初始化/复用未布局时 Portrait 尺寸为 0 → uv_scale = 0，插画 shader 不可见。
            // 轮询待布局完成后重应用（图鉴生成、战斗生成等首次 ApplyShader 场景）。
            if (card.Body.FindChild("Portrait", recursive: true, owned: false) is Control pn
                && pn.Size == Vector2.Zero)
            {
                ScheduleReapply(card);
                return;
            }

            //GD.Print($"[DBG] ApplyShader cid={cid} inInspect={InInspect(card)} body children: {string.Join(", ", card.Body.GetChildren().Select(c => c.Name))}");

            // Skip if this card has no effects AND tilt is off.
            // 但仍需清理可能残留的旧特效容器（如从有效果切到空 fully/separately 清空后）。
            if (!Config.GlobalDynamicEffect && !CardHasEffects(cid))
            {
                RemoveAllContainers(card.Body);
                CleanupInspect(card.Body);
                return;
            }

            if (_handlers.TryGetValue(card, out var old)) { card.ModelChanged -= old; _handlers.Remove(card); }
            Action<CardModel?> h = _ => ApplyShader(card);
            card.ModelChanged += h; _handlers[card] = h;

            // 附魔变化（附魔/移除）时重应用特效
            if (_enchHandlers.TryGetValue(card, out var oldEnch))
            {
                oldEnch.model.EnchantmentChanged -= oldEnch.handler;
                _enchHandlers.Remove(card);
            }
            if (card.Model != null)
            {
                Action eh = () => ApplyShader(card);
                card.Model.EnchantmentChanged += eh;
                _enchHandlers[card] = (card.Model, eh);
            }

            if (InInspect(card))
            {
                RemoveAllContainers(card.Body);
                if (Config.GetEffect(cid, "FullCard") > 0)
                    goto tiltPath;
                ApplyShaderInspect(card, cid);
                return;
            }

            RemoveAllContainers(card.Body);
            CleanupInspect(card.Body); // also clean stale ShaderPartContainers when card loses effects
tiltPath:

            var vfx = card.Body.GetNodeOrNull<Control>("CardVfxContainer");
            // 修改：不再 RemoveChild(vfx)，保留其在树中

            // ── Path 2: FullCard (persistent effect, immediate creation) ────
            int fullCard = Config.GetEffect(cid, "FullCard");
            if (fullCard > 0)
            {
                var tilt = TiltWrapper.CreateTilt(card, cid, fullCard: true);
                if (tilt == null)
                {
                    // 修改：仅用 MoveChild 确保 vfx 在最底层
                    if (vfx != null) card.Body.MoveChild(vfx, 0);
                    return;
                }
                card.Body.AddChild(tilt);
                // 修改：仅用 MoveChild 确保 vfx 在最底层
                if (vfx != null) card.Body.MoveChild(vfx, 0);
                FixGlowLayer(card.Body);

                if (tilt.Material is ShaderMaterial tm) { tm.SetShaderParameter(TiltWrapper.EffectModeKey, 0); tm.SetShaderParameter(TiltWrapper.IntensityKey, 1.0); }
                var fcCont = FullCardWrapper.CreateFullCard(card, cid, tilt, fullCard);
                card.Body.AddChild(fcCont);

                var vpFc = tilt.GetNode<SubViewport>("SubViewport");
                Control rootFc = (Control)vpFc.GetChild(0);
                PartWrapper.WrapParts(cid, rootFc);
                UpdatePortraitMaterial(card, cid, rootFc);
                return;
            }

            // ── Path 3: Non-inspect, non-FullCard cards ────────────────
            // 3a: Card has per-part effects → apply directly on Body, lazy tilt on hover
            if (CardHasEffects(cid))
            {
                WrapPartsInspect(cid, card.Body);
                UpdatePortraitInspect(card, cid, card.Body);

                _hoverCards[card] = new CardHoverState { Cid = cid, HasInlineEffects = true };
                EnsureHoverWatcher();
                // 修改：仅用 MoveChild 确保 vfx 在最底层
                if (vfx != null) card.Body.MoveChild(vfx, 0);
                return;
            }

            // 3b: No part effects — just dynamic/tilt: lazy creation on hover
            _hoverCards[card] = new CardHoverState { Cid = cid };
            EnsureHoverWatcher();
            // 修改：仅用 MoveChild 确保 vfx 在最底层
            if (vfx != null) card.Body.MoveChild(vfx, 0);
        }

        /// <summary>
        /// 轮询等待卡就绪（Body 非空且 Portrait 已布局）后重新 ApplyShader。
        /// 用于初始化时序：卡刚生成时 Body 可能为 null 或 Portrait 未布局。
        /// 带帧数上限（约 2 秒）避免永久循环。
        /// </summary>
        internal static void ScheduleReapply(NCard card)
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null) return;
            int attempts = 0;
            void Poll()
            {
                tree.ProcessFrame -= Poll;
                if (!GodotObject.IsInstanceValid(card)) return;
                if (card.Model == null || card.Body == null)
                {
                    // Model/Body 未就绪（如 OnReturnedFromPool 清空后）：等待就绪再重试
                    if (++attempts < 120) tree.ProcessFrame += Poll;
                    return;
                }
                var pn = card.Body.FindChild("Portrait", recursive: true, owned: false) as Control;
                if (pn != null && pn.Size == Vector2.Zero)
                {
                    if (++attempts < 120) tree.ProcessFrame += Poll;
                    return;
                }
                ApplyShader(card);
            }
            tree.ProcessFrame += Poll;
        }

        public static void CleanupCard(NCard card)
        {
            if (_handlers.TryGetValue(card, out var h)) { card.ModelChanged -= h; _handlers.Remove(card); }
            if (_enchHandlers.TryGetValue(card, out var eh)) { eh.model.EnchantmentChanged -= eh.handler; _enchHandlers.Remove(card); }
            _portraitRefresh.Remove(card);
            if (card.Body != null)
            {
                CleanupInspect(card.Body);
                RemoveAllContainers(card.Body);
            }
            _hoverCards.Remove(card);
            if (_hoverCards.Count == 0) UnhookHoverWatcher();
        }

        public static void RefreshAllCardsWithId(string cid)
        {
            var t = Engine.GetMainLoop() as SceneTree; if (t == null) return;
            foreach (var n in t.Root.GetChildren()) RNode(n, cid);
        }
        public static void RefreshAllCards()
        {
            var t = Engine.GetMainLoop() as SceneTree; if (t == null) return;
            RAll(t.Root);
        }

        /// <summary>
        /// 按节点宽高比设置背景星云材质的 uv_scale（短边归一化），避免宽扁背景块上星云被拉伸成椭圆。
        /// 轮询等待节点布局就绪（Size &gt; 0）后设置一次。
        /// </summary>
        internal static void ApplyStarcloudBgAspect(Control bg)
        {
            if (bg?.Material is not ShaderMaterial mat) return;
            var tree = bg.GetTree();
            if (tree == null) return;
            void Poll()
            {
                tree.ProcessFrame -= Poll;
                if (!GodotObject.IsInstanceValid(bg)) return;
                Vector2 s = bg.Size;
                float maxDim = Mathf.Max(s.X, s.Y);
                if (maxDim <= 0f) { tree.ProcessFrame += Poll; return; }
                mat.SetShaderParameter("uv_scale", new Vector2(s.X / maxDim, s.Y / maxDim));
            }
            tree.ProcessFrame += Poll;
        }

        // ========================================================================
        // CONTAINER LIFECYCLE
        // ========================================================================

        // ── Container Lifecycle ────────────────────────────────────────────────
        //
        // RemoveAllContainers: unwrap and destroy any TiltContainer,
        //   FullCardEffectContainer, or ShaderPartContainer nodes inside the card
        //   body. Restores original card children to Body. CardVfxContainer is
        //   preserved at z-index 0 (bottom-most layer).

        private static void RemoveAllContainers(Control body)
        {
            var vfx = body.GetNodeOrNull<Control>("CardVfxContainer");

            //GD.Print($"[DBG] RemoveAllContainers START on body, children: {string.Join(", ", body.GetChildren().Select(c => c.Name))}");

            foreach (var fc in body.GetChildren().OfType<FullCardWrapper.FullCardEffectContainer>().ToList())
            {
                //GD.Print($"[DBG] RemoveAllContainers: found FullCardEffectContainer, children before unwrap: {string.Join(", ", body.GetChildren().Select(c => c.Name))}");

                var fvp = fc.GetNodeOrNull<SubViewport>("SubViewport");
                if (fvp != null)
                {
                    var fkids = new Node[fvp.GetChildCount()];
                    for (int i = 0; i < fvp.GetChildCount(); i++) fkids[i] = fvp.GetChild(i);
                    foreach (var fk in fkids) { fvp.RemoveChild(fk); body.AddChild(fk); }
                }
                fc.GetParent()?.RemoveChild(fc); fc.QueueFree();

                //GD.Print($"[DBG] RemoveAllContainers: after fc unwrap, body children: {string.Join(", ", body.GetChildren().Select(c => c.Name))}");
            }

            foreach (var t in body.GetChildren().OfType<TiltWrapper.TiltContainer>().ToList())
            {
                //GD.Print($"[DBG] RemoveAllContainers: destroying TiltContainer, body children before destroy: {string.Join(", ", body.GetChildren().Select(c => c.Name))}");
                TiltWrapper.DestroyTilt(t, body);
                //GD.Print($"[DBG] RemoveAllContainers: after destroy, body children: {string.Join(", ", body.GetChildren().Select(c => c.Name))}");
            }

            if (vfx != null) body.MoveChild(vfx, 0);

            //GD.Print($"[DBG] RemoveAllContainers DONE, body children: {string.Join(", ", body.GetChildren().Select(c => c.Name))}");
        }

        // ========================================================================
        // PORTRAIT
        // ========================================================================

        /// <summary>登记需要每帧刷新 intensity 的插画材质（material 方式），避免 Slider 拖动时插画不更新。</summary>
        private static void RegisterPortraitRefresh(NCard card, string cid, Control? portrait, Control? ancient)
        {
            if (card == null) return;
            _portraitRefresh[card] = (cid, portrait, ancient);
            EnsurePortraitRefreshWatcher();
        }

        private static void EnsurePortraitRefreshWatcher()
        {
            if (_portraitRefreshActive) return;
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null) return;
            tree.ProcessFrame += OnPortraitRefreshProcessFrame;
            _portraitRefreshActive = true;
        }

        private static void OnPortraitRefreshProcessFrame()
        {
            if (_portraitRefresh.Count == 0)
            {
                var tree = Engine.GetMainLoop() as SceneTree;
                if (tree != null) tree.ProcessFrame -= OnPortraitRefreshProcessFrame;
                _portraitRefreshActive = false;
                return;
            }
            List<NCard>? toRemove = null;
            foreach (var (card, info) in _portraitRefresh)
            {
                if (!GodotObject.IsInstanceValid(card)) { (toRemove ??= new()).Add(card); continue; }
                double intensity = Config.GetIntensity(info.Cid);
                if (info.Portrait is { } pt && GodotObject.IsInstanceValid(pt)
                    && pt.Material is ShaderMaterial s1 && s1.Shader != null && IsOurs(s1))
                    s1.SetShaderParameter(TiltWrapper.IntensityKey, intensity);
                if (info.Ancient is { } an && GodotObject.IsInstanceValid(an)
                    && an.Material is ShaderMaterial s2 && s2.Shader != null && IsOurs(s2))
                    s2.SetShaderParameter(TiltWrapper.IntensityKey, intensity);
            }
            if (toRemove != null)
                foreach (var c in toRemove) _portraitRefresh.Remove(c);
        }

        private static void UpdatePortraitMaterial(NCard card, string cid, Control root)
        {
            Control? pn = root.FindChild("Portrait", recursive: true, owned: false) as Control
                       ?? root.FindChild("AncientPortrait", recursive: true, owned: false) as Control;
            if (pn == null) return;

            int e = Config.GetEffect(cid, "Portrait");
            Shader? sh = e > 0 ? EffectRegistry.GetShader(e) : null;
            if (sh == null) { RestoreMaterial(pn); return; }

            Vector2 gp = pn.Position + TiltWrapper.UvRefHalf;
            ShaderMaterial m;
            if (pn.Material is ShaderMaterial ex && ex.Shader == sh) m = ex;
            else
            {
                m = new ShaderMaterial { Shader = sh };
                m.SetShaderParameter(TiltWrapper.SeedKey, pn.GetHashCode() % 10000 / 10.0f);
                // EffectRegistry.ApplyNoiseTexture(m); // TODO: 噪声纹理功能未完成，暂时注释
                if (pn.Material != null && !IsOurs(pn.Material)) m.NextPass = pn.Material;
                pn.Material = m;
            }
            m.SetShaderParameter(TiltWrapper.EffectModeKey, e);
            m.SetShaderParameter(TiltWrapper.IntensityKey, Config.GetIntensity(cid));
            m.SetShaderParameter(TiltWrapper.UvOffsetKey, gp / TiltWrapper.UvRefSize);
            m.SetShaderParameter(TiltWrapper.UvScaleKey, pn.Size / TiltWrapper.UvRefSize);
            m.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, (gp - TiltWrapper.UvRefHalf) / TiltWrapper.UvRefSize);
            RegisterPortraitRefresh(card, cid, pn, null); // 每帧同步 intensity
        }

        private static void RestoreMaterial(Control? c)
        {
            if (c == null) return;
            if (c.Material is not ShaderMaterial sm || !IsOurs(sm)) return;
            c.Material = sm.NextPass;
        }
        private static bool IsOurs(Material mat)
        {
            if (mat is not ShaderMaterial sm || sm.Shader == null) return false;
            foreach (var d in EffectRegistry.AllEffects) if (d.Shader == sm.Shader) return true;
            return false;
        }

        // ════════════════════════════════════════════════════════════════════════
        // Lazy Tilt — hover-driven SubViewport lifecycle
        // ════════════════════════════════════════════════════════════════════════
        //
        // Flow:
        //   ApplyShader → register card in _hoverCards
        //   SceneTree.ProcessFrame → OnHoverProcess every frame
        //     hover detected  → CreateTiltContainer + WrapParts
        //     unhover + 0ms → DestroyTilt
        //   _hoverCards empty  → UnhookHoverWatcher (zero per-frame overhead)
        //
        // IsCardHovered: same hover logic as TiltContainer._Process — walks the
        //   parent chain to find NCardHolder, then checks hitbox._isHovered.

        private static bool IsCardHovered(NCard card, out NCardHolder? holder)
        {
            holder = null;
            for (Node? cur = card.GetParent(); cur != null; cur = cur.GetParent())
                if (cur is NCardHolder h) { holder = h; break; }
            if (holder == null) return false;
            return holder is NHandCardHolder { ZIndex: > 0 }
                || (holder.Hitbox is { IsEnabled: true } hb && Traverse.Create(hb).Field<bool>("_isHovered").Value);
        }

        // Hooks SceneTree.ProcessFrame once for ALL registered cards.
        private static void EnsureHoverWatcher()
        {
            if (_hoverWatchActive) return;
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null) return;
            tree.ProcessFrame += OnHoverProcess;
            _hoverWatchActive = true;
        }

        /// <summary>
        /// 修复 RareGlow / UncommonGlow 在 Body 上的层级：放在 CardVfxContainer (索引 0) 之后、TiltContainer 之前。
        /// </summary>
        private static void FixGlowLayer(Control body)
        {
            int insertAt = 1;
            foreach (var child in body.GetChildren())
            {
                if (child.Name == "RareGlow" || child.Name == "UncommonGlow")
                {
                    body.MoveChild(child, insertAt);
                    insertAt++;
                }
            }
        }

        // Unhooks when no cards need monitoring (zero overhead at idle).
        private static void UnhookHoverWatcher()
        {
            if (!_hoverWatchActive) return;
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree != null) tree.ProcessFrame -= OnHoverProcess;
            _hoverWatchActive = false;
        }

        // Called every frame via SceneTree.ProcessFrame.
        // For each registered card: if hovered & no tilt → create tilt.
        // If unhovered & tilt exists → start 0ms cooldown → destroy tilt.
        // Cards with HasInlineEffects: effects are already on Body children;
        // tilt creation skips WrapParts, and tilt destruction preserves
        // ShaderPartContainers (no UnwrapParts).
        private static void OnHoverProcess()
        {
            ulong now = Time.GetTicksMsec();
            var toRemove = new List<NCard>();

            foreach (var (card, state) in _hoverCards)
            {
                if (!GodotObject.IsInstanceValid(card) || card.Body == null)
                {
                    // Destroy tilt if card was disposed while hovered
                    if (state.Tilt != null && GodotObject.IsInstanceValid(state.Tilt))
                        state.Tilt.QueueFree();
                    toRemove.Add(card);
                    continue;
                }

                bool hovered = IsCardHovered(card, out _);

                if (hovered)
                {
                    state.LeaveTimeMsec = 0;
                    if (state.Tilt == null)
                    {
                        var body = card.Body;
                        var vfx = body.GetNodeOrNull<Control>("CardVfxContainer");
                        // 修改：不再 RemoveChild(vfx)

                        if (!state.HasInlineEffects)
                        {
                            var tilt = TiltWrapper.CreateTilt(card, state.Cid);
                            if (tilt != null)
                            {
                                body.AddChild(tilt); body.MoveChild(tilt, 0);
                                // 修改：仅用 MoveChild 确保 vfx 在最底层
                                if (vfx != null) body.MoveChild(vfx, 0);
                                FixGlowLayer(body);

                                var vp = tilt.GetNode<SubViewport>("SubViewport");
                                Control root = (Control)vp.GetChild(0);
                                PartWrapper.WrapParts(state.Cid, root);
                                UpdatePortraitMaterial(card, state.Cid, root);

                                state.Tilt = tilt;
                            }
                            else
                            {
                                // 修改：仅用 MoveChild 确保 vfx 在最底层
                                if (vfx != null) body.MoveChild(vfx, 0);
                            }
                        }
                        else
                        {
                            // Logical-name helper: "BalatroShaderPart_Frame" → "Frame"
                            string Ln(string nm) => nm.StartsWith(PartWrapper.SpPrefix)
                                ? nm.Substring(PartWrapper.SpPrefix.Length) : nm;

                            // Snapshot logical order before CreateTilt shuffles children
                            var logicalOrder = new List<string>();
                            foreach (var child in body.GetChildren())
                                logicalOrder.Add(Ln(child.Name));

                            var tilt = TiltWrapper.CreateTilt(card, state.Cid);
                            if (tilt != null)
                            {
                                body.AddChild(tilt); body.MoveChild(tilt, 0);
                                // 修改：仅用 MoveChild 确保 vfx 在最底层
                                if (vfx != null) body.MoveChild(vfx, 0);
                                FixGlowLayer(body);

                                var vp = tilt.GetNode<SubViewport>("SubViewport");
                                Control root = (Control)vp.GetChild(0);

                                // Move ShaderPartContainers from Body into root
                                var spList = new List<Node>();
                                foreach (var child in body.GetChildren())
                                    if (child.Name.ToString().StartsWith(PartWrapper.SpPrefix))
                                        spList.Add(child);
                                foreach (var sp in spList) { body.RemoveChild(sp); root.AddChild(sp); }

                                // Sort root children to match original logical order
                                var sorted = new List<Node>();
                                foreach (var ch in root.GetChildren()) sorted.Add(ch);
                                sorted.Sort((a, b) =>
                                    logicalOrder.IndexOf(Ln(a.Name))
                                        .CompareTo(logicalOrder.IndexOf(Ln(b.Name))));
                                foreach (var ch in root.GetChildren().ToList()) root.RemoveChild(ch);
                                foreach (var ch in sorted) root.AddChild(ch);

                                state.Tilt = tilt;
                            }
                            else
                            {
                                // 修改：仅用 MoveChild 确保 vfx 在最底层
                                if (vfx != null) body.MoveChild(vfx, 0);
                            }
                        }
                    }
                }
                else if (state.Tilt != null)
                {
                    if (state.LeaveTimeMsec == 0)
                        state.LeaveTimeMsec = now;
                    if (now - state.LeaveTimeMsec > TiltRemoveDelayMsec)
                    {
                        if (state.HasInlineEffects)
                        {
                            // Preserve ShaderPartContainers — move children back to Body intact
                            var body = card.Body;
                            var vfx2 = body.GetNodeOrNull<Control>("CardVfxContainer");
                            // 修改：不再 RemoveChild(vfx2)

                            var vp = state.Tilt.GetNode<SubViewport>("SubViewport");
                            Control root = (Control)vp.GetChild(0);
                            var kids = new Node[root.GetChildCount()];
                            for (int i = 0; i < root.GetChildCount(); i++) kids[i] = root.GetChild(i);
                            foreach (var k in kids) { root.RemoveChild(k); body.AddChild(k); }
                            state.Tilt.GetParent()?.RemoveChild(state.Tilt); state.Tilt.QueueFree();

                            // 修改：仅用 MoveChild 确保 vfx2 在最底层
                            if (vfx2 != null) body.MoveChild(vfx2, 0);
                        }
                        else
                        {
                            TiltWrapper.DestroyTilt(state.Tilt, card.Body);
                        }
                        state.Tilt = null;
                        state.LeaveTimeMsec = 0;
                    }
                }
            }

            foreach (var c in toRemove) _hoverCards.Remove(c);
            if (_hoverCards.Count == 0) UnhookHoverWatcher();
        }

        // ========================================================================
        // HELPERS
        // ========================================================================

        private static void RAll(Node n) { if (n is NCard c && c.Model != null) ApplyShader(c); foreach (Node ch in n.GetChildren()) RAll(ch); }
        private static void RNode(Node n, string cid) { if (n is NCard c && c.Model?.Id.ToString() == cid) ApplyShader(c); foreach (var ch in n.GetChildren()) RNode(ch, cid); }

        // ========================================================================
        // INSPECT PATH — skip TiltContainer for native text resolution.
        // Parts with effects are wrapped in ShaderPartContainer directly inside Body.
        // ========================================================================

        private static bool InInspect(NCard card)
        {
            for (Node? cur = card.GetParent(); cur != null; cur = cur.GetParent())
                if (cur.GetType().Name is "NInspectCardScreen" or "NBalatroInspectScreen" or "NBalatroInspectEnchantScreen") return true;
            return false;
        }

        private static void ApplyShaderInspect(NCard card, string cid)
        {
            var body = card.Body;
            RemoveAllContainers(body); // also clears TiltContainer from previous FullCard
            CleanupInspect(body);

            WrapPartsInspect(cid, body);
            UpdatePortraitInspect(card, cid, body);
        }

        private static void CleanupInspect(Control body)
        {
            // Collect ShaderPartContainers with their current indices
            var items = new List<(PartWrapper.ShaderPartContainer sc, int idx)>();
            var kids = new List<Node>();
            foreach (var child in body.GetChildren()) kids.Add(child);
            for (int i = 0; i < kids.Count; i++)
                if (kids[i] is PartWrapper.ShaderPartContainer sc && sc.Name.ToString().StartsWith(PartWrapper.SpPrefix))
                    items.Add((sc, i));

            // Unwrap in reverse order and restore to original position
            items.Reverse();
            foreach (var (sc, idx) in items)
            {
                var vp = sc.GetNodeOrNull<SubViewport>("SubViewport");
                if (vp?.GetChildCount() > 0)
                {
                    var inner = vp.GetChild(0).GetChildOrNull<Node>(0);
                    if (inner != null)
                    {
                        vp.GetChild(0).RemoveChild(inner);
                        if (inner is Control ic) ic.Position = sc.OriginalPosition;
                        body.AddChild(inner);
                        body.MoveChild(inner, idx);
                    }
                }
                sc.GetParent()?.RemoveChild(sc); sc.QueueFree();
            }
            // Also clean portrait material
            RestoreMaterial(body.FindChild("Portrait", recursive: true, owned: false) as Control);
            RestoreMaterial(body.FindChild("AncientPortrait", recursive: true, owned: false) as Control);

            //GD.Print($"[DBG] CleanupInspect DONE, body children: {string.Join(", ", body.GetChildren().Select(c => c.Name))}");
        }

        private static void WrapPartsInspect(string cid, Control body)
        {
            var orig = new List<Node>();
            foreach (var ch in body.GetChildren()) orig.Add(ch);
            var list = new List<Node>();
            foreach (var node in orig)
            {
                if (node is not Control c) { list.Add(node); continue; }
                string nm = c.Name;
                if (SkipNames.Contains(nm) || nm == "PortraitCanvasGroup" || TextPartNames.Contains(nm)) { list.Add(c); continue; }
                if (Config.AllPartNames.Contains(nm) && Config.GetEffect(cid, nm) > 0)
                {
                    Vector2 gp = c.Position + TiltWrapper.UvRefHalf;
                    var sc = PartWrapper.CreateShaderPart(cid, nm, c, gp, c.Size);
                    body.RemoveChild(c); c.Position = Vector2.Zero;
                    sc.GetNode<SubViewport>("SubViewport").GetChild(0).AddChild(c);
                    list.Add(sc);
                }
                else list.Add(c);
            }
            foreach (var ch in body.GetChildren()) body.RemoveChild(ch);
            foreach (var ch in list) body.AddChild(ch);
        }

        private static void UpdatePortraitInspect(NCard card, string cid, Control body)
        {
            // 同时处理 Portrait 与 AncientPortrait：先古卡可见的是 AncientPortrait，普通卡可见的是 Portrait
            Control? portrait = body.FindChild("Portrait", recursive: true, owned: false) as Control;
            Control? ancient = body.FindChild("AncientPortrait", recursive: true, owned: false) as Control;
            if (portrait == null && ancient == null) return;
            int e = Config.GetEffect(cid, "Portrait");
            Shader? sh = e > 0 ? EffectRegistry.GetShader(e) : null;
            if (sh == null)
            {
                if (portrait != null) RestoreMaterial(portrait);
                if (ancient != null) RestoreMaterial(ancient);
                return;
            }
            if (portrait != null) ApplyPortraitShader(portrait, cid, e, sh);
            if (ancient != null) ApplyPortraitShader(ancient, cid, e, sh);
            RegisterPortraitRefresh(card, cid, portrait, ancient); // 每帧同步 intensity
        }

        private static void ApplyPortraitShader(Control pn, string cid, int e, Shader sh)
        {
            Vector2 gp = pn.Position + TiltWrapper.UvRefHalf;
            ShaderMaterial m;
            if (pn.Material is ShaderMaterial ex && ex.Shader == sh) m = ex;
            else { m = new ShaderMaterial { Shader = sh }; m.SetShaderParameter(TiltWrapper.SeedKey, pn.GetHashCode() % 10000 / 10.0f); /* EffectRegistry.ApplyNoiseTexture(m); TODO: 噪声纹理功能未完成，暂时注释 */ if (pn.Material != null && !IsOurs(pn.Material)) m.NextPass = pn.Material; pn.Material = m; }
            m.SetShaderParameter(TiltWrapper.EffectModeKey, e); m.SetShaderParameter(TiltWrapper.IntensityKey, Config.GetIntensity(cid));
            m.SetShaderParameter(TiltWrapper.UvOffsetKey, gp / TiltWrapper.UvRefSize); m.SetShaderParameter(TiltWrapper.UvScaleKey, pn.Size / TiltWrapper.UvRefSize);
            m.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, (gp - TiltWrapper.UvRefHalf) / TiltWrapper.UvRefSize);
        }




        /// <summary>
        /// 性能限制触发时的回调。延迟一帧后清理所有卡牌上的特效容器，
        /// 避免在可能正在遍历场景树的调用栈中直接移除节点。
        /// </summary>
        private static void OnPerformanceThrottled()
        {
            //GD.PrintErr("[PengoTarot] Performance throttle triggered! All effects will be disabled and cleaned up.");
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree != null)
            {
                // 使用一次性 ProcessFrame 回调确保在安全时机执行清理
                tree.ProcessFrame += PerformCleanupAfterThrottle;
            }
        }

        private static void PerformCleanupAfterThrottle()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree != null)
                tree.ProcessFrame -= PerformCleanupAfterThrottle;

            // 清空所有悬停状态
            _hoverCards.Clear();
            if (_hoverWatchActive)
            {
                UnhookHoverWatcher();
            }

            // 遍历整个场景树，移除所有卡牌上的特效容器
            if (tree != null)
            {
                foreach (var node in tree.Root.GetChildren())
                    CleanupAllCardContainersRecursive(node);
            }
        }

        /// <summary>
        /// 递归遍历节点树，若遇到 NCard 则移除其所有 Shader 容器。
        /// </summary>
        private static void CleanupAllCardContainersRecursive(Node node)
        {
            if (node is NCard card && card.Body != null)
            {
                // 移除所有特效容器（Tilt、FullCard、ShaderPart）
                RemoveAllContainers(card.Body);
                CleanupInspect(card.Body);
                // 清理事件订阅
                if (_handlers.TryGetValue(card, out var handler))
                {
                    card.ModelChanged -= handler;
                    _handlers.Remove(card);
                }
            }

            foreach (Node child in node.GetChildren())
                CleanupAllCardContainersRecursive(child);
        }
    }
}