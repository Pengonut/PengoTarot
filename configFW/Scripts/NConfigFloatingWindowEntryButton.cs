#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using PengoTarot.Patches;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// configfloatingwindow 的入口悬浮按钮（选人界面与游戏过程共用同一场景）。
    /// 功能：
    ///  - 点击打开面板；
    ///  - 按住左键拖动（不超出屏幕边缘，拖动不误触发点击）；
    ///  - 贴边吸附：拖动停止且退出 hover 后，若距某边 < 自身尺寸一半，则自动贴边并把 70% 面积埋入屏幕边缘，
    ///    不透明度降至 20%；hover 时弹出恢复完整可见与不透明度，离开后缩回埋入。    ///  - 自动贴边隐藏：8 秒无鼠标操作（如手柄场景）后自动强制贴边埋入，复用同一套 dock 机制，避免入口常驻遮挡。    ///  - 位移/形变动画带速度曲线（Cubic/Back 缓动 + squash & stretch）。
    /// </summary>
    public partial class NConfigFloatingWindowEntryButton : NButton
    {
        /// <summary>超过该像素视为拖动，不触发 Released（防误开面板）。</summary>
        private const float DragThreshold = 6f;
        /// <summary>埋入比例（70% 面积移出屏幕）。</summary>
        private const float DockRatio = 0.7f;
        /// <summary>埋入后不透明度。</summary>
        private const float DockedAlpha = 0.2f;
        /// <summary>距边小于「尺寸一半」触发吸附。</summary>
        private const float DockTriggerRatio = 0.5f;
        /// <summary>无鼠标（手柄）时 8 秒后自动贴边隐藏（复用 dock 机制），避免入口常驻遮挡视线。</summary>
        private const float AutoHideDelay = 8f;

        private enum DockSide { None, Left, Right, Top, Bottom }

        // ── 跨场景/实例共享的位置缓存（内存，不落盘） ────────────
        // 选人界面与游戏过程的入口是不同实例，用静态字段让位置/埋入状态在它们之间保持。
        private static Vector2? _savedPos;
        private static DockSide _savedDockSide = DockSide.None;
        private static bool _savedDocked;

        private Control? _root;
        private bool _dragging;
        private Vector2 _dragOffset;
        private DockSide _dockSide = DockSide.None;
        private bool _isDocked;
        /// <summary>埋入状态下（扩展感应区）是否 hover。</summary>
        private bool _dockedHovered;
        private Tween? _hoverTween;
        private Tween? _dockTween;
        private Tween? _autoHideTween;

        public override void _Ready()
        {
            // NButton._Ready 会因类型检查抛异常，改为直接连接信号
            ConnectSignals();
            // 拖动超过阈值后 _isPressed 置 false，Release 不触发点击
            _ignoreDragThreshold = DragThreshold;
            _root = GetParent() as Control;

            // 恢复上次保存的位置/埋入状态（选人界面 ↔ 游戏过程共享）；
            // 无缓存时保持 .tscn 中定义的初始位置（不覆盖）。
            if (_root == null) return;
            ApplySavedState();

            Connect(NClickableControl.SignalName.Released,
                Callable.From<NClickableControl>(_ => ConfigFloatingWindow.OnEntryClicked()));

            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;

            // 8 秒无鼠标操作 → 自动贴边隐藏（手柄未适配前避免入口常驻遮挡视线）
            RestartAutoHide();
        }

        // ── 鼠标进出 ────────────────────────────────────────────
        private void OnMouseEntered()
        {
            if (_dragging) return;      // 拖动中忽略 hover
            if (_isDocked) return;      // 埋入态由 _Process 扩展感应接管
            CancelAutoHide();           // 鼠标进入：取消自动隐藏，恢复显示
            ScaleTo(1.05f);             // 自由：普通 hover 缩放
        }

        private void OnMouseExited()
        {
            if (_dragging) return;      // 拖动中不吸附、不变形
            if (_isDocked) return;      // 埋入态由 _Process 扩展感应接管
            RestartAutoHide();          // 鼠标离开：重新 8 秒计时
            ScaleTo(1f);                // 恢复 hover 缩放
            TrySnapAndDock();           // hover 结束：距离达标则自动贴边吸附（与是否拖动过无关）
        }

        // ── 输入处理（拖动） ────────────────────────────────────
        public override void _Input(InputEvent inputEvent)
        {
            // 面板打开时入口 root 被门面隐藏；Godot 的 _Input 不检查可见性，
            // 需手动早退，避免隐藏后仍可被全局输入拖动/点击。
            if (!IsVisibleInTree()) return;

            // 保留 NButton 的拖动阈值检测（CheckMouseDragThreshold）
            base._Input(inputEvent);

            if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left } mb)
            {
                if (mb.Pressed && GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                {
                    BeginDrag();
                }
                else if (!mb.Pressed && _dragging)
                {
                    _dragging = false;
                    SaveState();
                    // 松开时鼠标已不在按钮上（此前已 MouseExited）→ 立即吸附；
                    // 否则等 MouseExited（hover 结束）再吸附
                    if (!GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                        TrySnapAndDock();
                }
            }
            else if (inputEvent is InputEventMouseMotion && _dragging)
            {
                DragTo(GetGlobalMousePosition() - _dragOffset);
            }
        }

        // ── 埋入态扩展感应（每帧检测） ──────────────────────────
        public override void _Process(double delta)
        {
            // 隐藏（面板打开）时不处理埋入态 hover 感应
            if (!IsVisibleInTree()) return;
            // 埋入状态下，hover 感应区扩大到「唤出后的完整范围」，由这里手动判断
            if (!_isDocked || _root == null) return;
            bool hovered = GetDockHoverRect().HasPoint(GetGlobalMousePosition());
            if (hovered != _dockedHovered)
            {
                _dockedHovered = hovered;
                if (hovered) { PeekOut(); CancelAutoHide(); }   // hover：弹出并取消自动贴边计时
                else { DockIn(); }                              // 离开：缩回埋入（已隐藏，无需再计时）
            }
        }

        /// <summary>开始拖动：脱离 dock 状态、恢复完整可见、取消进行中的动画；同时开启地图标记总览（玩家在整理界面/看地图）。</summary>
        private void BeginDrag()
        {
            if (!IsVisibleInTree()) return;

            _dockTween?.Kill();
            _hoverTween?.Kill();
            _isDocked = false;
            _dockSide = DockSide.None;
            _dragging = true;
            Modulate = Colors.White;
            Scale = Vector2.One;
            CancelAutoHide();
            _dragOffset = GetGlobalMousePosition() - RootGlobalPos();
            // 以「拖动」作为总览开始的 trigger（而非自由/贴边状态）
            NMapPointMarkerIconPatch.SetOverview(true);
        }

        private Vector2 RootGlobalPos()
            => _root != null ? _root.GlobalPosition : GlobalPosition;

        /// <summary>移动父级（入口根节点）到目标位置，并 clamp 在屏幕内。</summary>
        private void DragTo(Vector2 target)
        {
            if (_root == null) return;
            var viewportSize = GetViewportRect().Size;
            var size = _root.Size;
            target.X = Mathf.Clamp(target.X, 0f, Mathf.Max(0f, viewportSize.X - size.X));
            target.Y = Mathf.Clamp(target.Y, 0f, Mathf.Max(0f, viewportSize.Y - size.Y));
            _root.GlobalPosition = target;
        }

        // ── 贴边吸附与埋入 ──────────────────────────────────────
        /// <summary>计算离入口最近的屏幕边（带距离输出，供吸附阈值判断用）。</summary>
        private DockSide FindNearestSide(out float minDist)
        {
            var viewportSize = GetViewportRect().Size;
            var pos = _root!.GlobalPosition;
            var size = _root.Size;

            float distLeft = pos.X;
            float distRight = viewportSize.X - (pos.X + size.X);
            float distTop = pos.Y;
            float distBottom = viewportSize.Y - (pos.Y + size.Y);

            if (distLeft <= distRight && distLeft <= distTop && distLeft <= distBottom)
            { minDist = distLeft; return DockSide.Left; }
            if (distRight <= distTop && distRight <= distBottom)
            { minDist = distRight; return DockSide.Right; }
            if (distTop <= distBottom)
            { minDist = distTop; return DockSide.Top; }
            minDist = distBottom; return DockSide.Bottom;
        }

        private void TrySnapAndDock()
        {
            if (_root == null) return;
            var size = _root.Size;
            DockSide side = FindNearestSide(out float minDist);

            // 触发阈值：左右用宽度一半，上下用高度一半
            float threshold = side is DockSide.Left or DockSide.Right
                ? size.X * DockTriggerRatio
                : size.Y * DockTriggerRatio;

            if (minDist >= threshold)
            {
                // 离所有边都够远 → 不吸附，保持自由
                _isDocked = false;
                _dockSide = DockSide.None;
                return;
            }

            _dockSide = side;
            _isDocked = true;
            DockIn();
        }

        private Vector2 GetDockPosition(DockSide side)
        {
            var viewportSize = GetViewportRect().Size;
            var size = _root!.Size;
            return side switch
            {
                DockSide.Left => new Vector2(-size.X * DockRatio, _root.GlobalPosition.Y),
                DockSide.Right => new Vector2(viewportSize.X - size.X * (1f - DockRatio), _root.GlobalPosition.Y),
                DockSide.Top => new Vector2(_root.GlobalPosition.X, -size.Y * DockRatio),
                DockSide.Bottom => new Vector2(_root.GlobalPosition.X, viewportSize.Y - size.Y * (1f - DockRatio)),
                _ => _root.GlobalPosition
            };
        }

        private Vector2 GetPeekPosition(DockSide side)
        {
            var viewportSize = GetViewportRect().Size;
            var size = _root!.Size;
            return side switch
            {
                DockSide.Left => new Vector2(0f, _root.GlobalPosition.Y),
                DockSide.Right => new Vector2(viewportSize.X - size.X, _root.GlobalPosition.Y),
                DockSide.Top => new Vector2(_root.GlobalPosition.X, 0f),
                DockSide.Bottom => new Vector2(_root.GlobalPosition.X, viewportSize.Y - size.Y),
                _ => _root.GlobalPosition
            };
        }

        /// <summary>埋入状态下 hover 的扩展感应矩形 = 唤出（peek）后的完整实际范围。</summary>
        private Rect2 GetDockHoverRect()
        {
            var peekPos = GetPeekPosition(_dockSide);
            return new Rect2(peekPos, _root!.Size);
        }

        /// <summary>埋入：平移到贴边埋入位置，透明度降至 20%，带轻微压缩形变；同时关闭地图标记总览（进入没入 = 结束 trigger）。</summary>
        private void DockIn()
        {
            if (_root == null || _dockSide == DockSide.None) return;
            NMapPointMarkerIconPatch.SetOverview(false);
            AnimateDock(GetDockPosition(_dockSide), DockedAlpha, 0.97f, 0.35f,
                Tween.TransitionType.Cubic, Tween.EaseType.Out);
        }

        /// <summary>弹出：平移到贴边完全可见位置，恢复不透明度，带回弹形变。</summary>
        private void PeekOut()
        {
            if (_root == null || _dockSide == DockSide.None) return;
            AnimateDock(GetPeekPosition(_dockSide), 1f, 1.08f, 0.3f,
                Tween.TransitionType.Back, Tween.EaseType.Out);
        }

        /// <summary>吸附/弹出的组合动画：位移 + 不透明度 + squash &amp; stretch 形变。</summary>
        private void AnimateDock(Vector2 targetPos, float targetAlpha, float peakScale, float duration,
            Tween.TransitionType trans, Tween.EaseType ease)
        {
            _dockTween?.Kill();
            _hoverTween?.Kill();
            PivotOffset = Size / 2f;
            _dockTween = CreateTween().SetParallel();

            // 位移（带速度曲线）
            _dockTween.TweenProperty(_root, "global_position", targetPos, duration)
                .SetTrans(trans).SetEase(ease);
            // 不透明度
            _dockTween.TweenProperty(this, "modulate:a", targetAlpha, duration)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            // 形变：从 peakScale 经 Back 缓动回落到 1（回弹/拉伸感）
            _dockTween.TweenProperty(this, "scale", Vector2.One, duration)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out)
                .From(Vector2.One * peakScale);

            // 动画结束（位置/埋入到位）后保存状态
            _dockTween.Finished += SaveState;
        }

        /// <summary>把当前位置/埋入状态存入静态缓存（跨实例恢复用）。</summary>
        private void SaveState()
        {
            if (_root == null) return;
            _savedPos = _root.GlobalPosition;
            _savedDockSide = _dockSide;
            _savedDocked = _isDocked;
        }

        /// <summary>
        /// 应用静态缓存里的位置/埋入状态（若有）。
        /// 供 _Ready 与门面在选人界面每次打开时调用，保证选人 ↔ 游戏位置始终同步。
        /// </summary>
        public void ApplySavedState()
        {
            if (_root == null || _savedPos is not Vector2 p) return;
            _dockTween?.Kill();
            _hoverTween?.Kill();
            _root.GlobalPosition = p;
            if (_savedDocked && _savedDockSide != DockSide.None)
            {
                _dockSide = _savedDockSide;
                _isDocked = true;
                Modulate = new Color(1f, 1f, 1f, DockedAlpha);
            }
            else
            {
                _dockSide = DockSide.None;
                _isDocked = false;
                Modulate = Colors.White;
            }
            Scale = Vector2.One;
        }

        // ── 自由状态下的 hover 缩放 ─────────────────────────────
        private void ScaleTo(float scale)
        {
            PivotOffset = Size / 2f;
            _hoverTween?.Kill();
            _hoverTween = CreateTween();
            _hoverTween.TweenProperty(this, "scale", Vector2.One * scale, 0.1f)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }

        // ── 自动贴边隐藏（8 秒无鼠标 → 距离达标则自动贴边埋入） ──
        /// <summary>重启 8 秒计时；到时无鼠标（手柄场景）且距边达标时自动贴边隐藏。</summary>
        private void RestartAutoHide()
        {
            if (_root == null) return;
            _autoHideTween?.Kill();
            _autoHideTween = CreateTween();
            _autoHideTween.TweenCallback(Callable.From(OnAutoHideTimer)).SetDelay(AutoHideDelay);
        }

        /// <summary>取消自动贴边计时（鼠标 hover/拖动时调用）。</summary>
        private void CancelAutoHide()
        {
            _autoHideTween?.Kill();
        }

        /// <summary>8 秒计时到：若尚未贴边，则按距离条件（距边 < 尺寸一半）尝试贴边隐藏；不满足则保持自由。</summary>
        private void OnAutoHideTimer()
        {
            if (_root == null || _isDocked) return;
            TrySnapAndDock();
        }
    }
}
