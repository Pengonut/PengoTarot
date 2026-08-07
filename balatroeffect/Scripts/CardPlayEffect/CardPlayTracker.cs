// PengoTarot: tracks card play timestamps so shader effects can apply
// a 360° rotation animation from drag-start through play-finish with easing.

#nullable enable

using System.Collections.Generic;
using Godot;

namespace PengoTarot.BalatroEffect
{
    /// <summary>
    /// Tracks card play lifecycle: drag start → play finish → fade-out.
    /// Applies a rotation effect with fast ease-in attack and slow ease-out release.
    /// </summary>
    public static class CardPlayTracker
    {
        /// <summary>Fast ramp-up duration in seconds.</summary>
        public const float AttackTime = 0.12f;

        /// <summary>Slow fade-out duration after play ends.</summary>
        public const float ReleaseTime = 0.40f;

        /// <summary>Peak rotation amplitude in degrees.</summary>
        public const float RotationAmplitude = 15.0f;

        /// <summary>Base angular speed in rad/s.</summary>
        public const float BaseSpeed = 2.0f;

        private static readonly Dictionary<string, PlayState> _states = new();

        /// <summary>待延迟到下一帧刷新的卡 id（去重，避免 ProcessFrame 事件堆积）。</summary>
        private static readonly HashSet<string> _pendingRefresh = new();

        /// <summary>是否已排队一个 ProcessFrame 刷新回调。</summary>
        private static bool _refreshScheduled;

        private sealed class PlayState
        {
            public float DragStartTime;
            public float? PlayEndTime;
            public int AxisMode;
        }

        private static float Now => Time.GetTicksMsec() / 1000.0f;

        /// <summary>Called when player starts dragging a card from hand.</summary>
        public static void MarkDragStarted(string cardId)
        {
            _states[cardId] = new PlayState
            {
                DragStartTime = Now,
                AxisMode = (int)(Time.GetTicksMsec() & 3) // 0–3, time-based
            };
            DeferRefresh(cardId);
        }

        /// <summary>Called when the card play finishes (success or cancel).</summary>
        public static void MarkPlayFinished(string cardId)
        {
            if (_states.TryGetValue(cardId, out var state) && !state.PlayEndTime.HasValue)
            {
                state.PlayEndTime = Now;
                DeferRefresh(cardId);
            }
        }

        /// <summary>
        /// 延迟到下一帧刷新指定卡的 shader 特效。
        /// 出牌/消耗发生在游戏本体 OnPlayWrapper 的同步调用栈内，此时场景树节点
        /// （如卡牌消耗特效 NCardExhaustQuickVfx）可能正在创建/销毁，立即遍历并重排
        /// 会让特效节点在 _ExitTree 时访问已失效的 viewport 而崩溃。
        /// 延迟一帧等状态稳定后再刷新，避免时序竞争。
        /// 同一帧的多个卡 id 合并到一次 ProcessFrame 回调统一处理（与 OnPerformanceThrottled 同模式）。
        /// </summary>
        private static void DeferRefresh(string cardId)
        {
            _pendingRefresh.Add(cardId);
            if (_refreshScheduled) return;
            _refreshScheduled = true;

            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null)
            {
                // 拿不到场景树（极端情况）：立即刷新兜底，避免特效不生效
                FlushPendingRefresh();
                return;
            }
            tree.ProcessFrame += FlushPendingRefresh;
        }

        /// <summary>ProcessFrame 回调：统一刷新所有排队中的卡 id。</summary>
        private static void FlushPendingRefresh()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree != null) tree.ProcessFrame -= FlushPendingRefresh;
            _refreshScheduled = false;

            if (_pendingRefresh.Count == 0) return;
            var cids = new List<string>(_pendingRefresh);
            _pendingRefresh.Clear();
            foreach (var cid in cids)
                ShaderController.RefreshAllCardsWithId(cid);
        }

        /// <summary>
        /// Returns (x_rot, y_rot) with easing applied, or null if outside the effect window.
        /// </summary>
        public static (float xRot, float yRot)? GetPlayRotation(string cardId)
        {
            if (!_states.TryGetValue(cardId, out var state))
                return null;

            float now = Now;
            float elapsed = now - state.DragStartTime;

            // Cleanup: well past fade-out
            if (state.PlayEndTime.HasValue && now > state.PlayEndTime.Value + ReleaseTime)
            {
                _states.Remove(cardId);
                return null;
            }

            float intensity = ComputeIntensity(elapsed, state);

            int axisMode = state.AxisMode;
            float angle = elapsed * BaseSpeed * intensity;
            float amp = RotationAmplitude * intensity;
            float wobble = Mathf.Sin(angle * 2.3f) * 0.25f; // 高频微扰

            // 计算原始旋转值
            (float x, float y) = axisMode switch
            {
                0 => ( Mathf.Sin(angle) * amp,  Mathf.Cos(angle) * amp),               // X-Y circle CW
                1 => ( Mathf.Cos(angle) * amp,  Mathf.Sin(angle) * amp),               // X-Y circle CCW
                2 => ( Mathf.Sin(angle) * amp, (Mathf.Sin(angle) + wobble) * amp),     // diagonal ＋45° + wobble
                3 => ( Mathf.Sin(angle) * amp, (Mathf.Sin(-angle) + wobble) * amp),    // diagonal －45° + wobble
                _ => (0f, 0f)
            };

            // 整体缩放 0.6 倍
            const float scale = 0.6f;
            return (x * scale, y * scale);
        }

        private static float ComputeIntensity(float elapsed, PlayState state)
        {
            // Phase 1: fast ease-in attack
            if (elapsed < AttackTime)
            {
                float t0 = elapsed / AttackTime;
                return SmoothStep(t0);
            }

            // Phase 2: sustain (while dragging/playing)
            if (!state.PlayEndTime.HasValue)
                return 1.0f;

            // Phase 3: slow ease-out release after play ends
            float releaseElapsed = Now - state.PlayEndTime.Value;
            float t1 = 1.0f - Mathf.Min(releaseElapsed / ReleaseTime, 1.0f);
            return SmoothStep(t1);
        }

        /// <summary>Smoothstep: 3t² - 2t³</summary>
        private static float SmoothStep(float t)
        {
            return t * t * (3.0f - 2.0f * t);
        }
    }
}

