#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using PengoTarot.ConfigFW;
using PengoTarot.Data.Divination;
using PengoTarot.Powers;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 地图节点占卜标记视觉。
    /// 图层（低 → 高）：
    ///   1. 连接线（dagger 尖细特效，节点内 z_index=-1，低于节点图标，展示图标归属）；
    ///   2. 节点图标（地图点）；
    ///   3. 外侧旋转图标 + 恋人图标：挂到 TheMap/Points 之后（高于所有地图节点、随地图滚动、
    ///      仍位于 NMapScreen 内部，低于 NGame 上层 UI 如设置界面）；
    ///   4. sparkle 金光常驻（节点内，Icon 之上）。
    /// 交互：hover 或总览（入口悬浮窗未贴边）展开图标；hover 时公转、总览时静止于相位；
    /// 恋人不公转、固定左下角。全部由 _Process 驱动（Progress 统一控制展开/收回）。
    /// </summary>
    public static class NMapPointMarkerIconPatch
    {
        /// <summary>节点实际尺寸（IconContainer 为 56×56，中心即节点中心）。</summary>
        public const float OrbitSize = 64f;
        /// <summary>图标显示尺寸（enchantment 源图 64px → 缩放到此；用户要求调大）。</summary>
        public const float IconSize = 48f;
        /// <summary>绕节点中心的公转半径（原 96，用户要求减到 0.8 倍）。</summary>
        public const float OrbitRadius = 76.8f;
        /// <summary>公转角速度（弧度/秒；原 0.6，用户要求降到 0.8 倍 → 0.48；配合 SmoothStep 启停缓动）。</summary>
        public const float OrbitSpeed = 0.48f;
        /// <summary>图标可见阈值：p 低于该值直接归 0（消除收起尾声的极小图标残留）。</summary>
        public const float MinVisible = 0.06f;
        /// <summary>图标初始相位（3π/4 = 左下角，便于查看；而非最右侧）。</summary>
        public const float InitialPhase = 3f * Mathf.Pi / 4f;
        /// <summary>稀有度金色闪光 sparkle 的缩放（原发射范围约 120×170，缩放适配 56px 节点）。</summary>
        public const float SparkleScale = 0.32f;
        /// <summary>sparkle 不透明度系数（场景自带 modulate 0.75，此处经 SelfModulate 叠加）。</summary>
        public const float SparkleAlpha = 1.0f;
        /// <summary>图标浮出/收回动画时长（秒）。</summary>
        public const float PopDuration = 0.35f;
        /// <summary>恋人图标距容器左下边缘的距离（减小 = 更靠左下外侧）。</summary>
        public const float LoversOffset = 12f;
        /// <summary>恋人占卜的难度开关索引。</summary>
        public const int LoversFlag = 6;
        /// <summary>恋人图标尺寸比例（其他图标的 0.8 倍）。</summary>
        public const float LoversScale = 0.8f;
        /// <summary>图标源图基础边长（enchantment 图标 64×64）；Sprite2D 用它换算显示尺寸。</summary>
        public const float IconBaseTexSize = 64f;
        /// <summary>粘滞阈值：鼠标离开节点后，距节点中心超过该距离才收起图标（保持展开不旋转）。</summary>
        public const float StickyThreshold = 130f;
        /// <summary>图标被 hover 时的放大倍数。</summary>
        public const float IconHoverScale = 1.05f;

        // ── 连接线（soul_beam 完整线状特效） ────────────────────
        /// <summary>连接线场景（游戏本体 Kin Priest 灵魂光束线状特效，含 Beam + StaticParticles）。</summary>
        private const string BeamScenePath = "res://scenes/vfx/monsters/kin_priest_beam_vfx.tscn";
        /// <summary>
        /// 连接线长度比例（BeamHolder.scale.x）。精确计算：soul_beam.png 宽 500 × Beam.Scale.x(0.76) × scale = 光束长 380×scale；
        /// 目标长度 = 节点中心到图标中心 OrbitRadius(76.8) → scale = 76.8/380 ≈ 0.2。
        /// </summary>
        private const float BeamLengthScale = 0.2f;
        /// <summary>
        /// 连接线粗细缩放（BeamHolder.scale.y）。精确计算：soul_beam.png 高 20 × Beam.Scale.y(1.6) × scale = 光束高 32×scale；
        /// 目标约 5px → scale = 5/32 ≈ 0.16。
        /// </summary>
        private const float BeamThinScale = 0.16f;
        /// <summary>光束默认朝 -x 方向（Beam offset 为负），需旋转 -π 使其指向目标角；若朝向相反改为 +π。</summary>
        private const float BeamAngleOffset = -Mathf.Pi;

        /// <summary>单个公转图标的视觉状态（图标 + 连接线 + 数字角标 + 失效切换）。</summary>
        internal sealed class OrbitItem
        {
            /// <summary>图标容器(Control，鼠标hit)，全局层。</summary>
            public Control Icon = null!;
            /// <summary>显示(Sprite2D)，缩放绕中心。</summary>
            public Sprite2D Sprite = null!;
            /// <summary>连接线（节点内，置底）。</summary>
            public Node2D? Stem;
            /// <summary>占卜难度开关索引。</summary>
            public int Flag;
            /// <summary>初始相位（圆上分布）。</summary>
            public float Phase;
            /// <summary>数字角标（右下角黑底白字；恋人不建，失效后隐藏）。</summary>
            public Label? Badge;
            /// <summary>上次显示的角标数字（变化时刷新文本）。</summary>
            public int LastShown;
            /// <summary>上次的失效状态（失效时切逆位图并停角标）。</summary>
            public bool WasExpired;
        }

        /// <summary>单个节点的标记视觉状态。</summary>
        internal sealed class OrbitState
        {
            /// <summary>旋转组（非恋人图标）。</summary>
            public readonly List<OrbitItem> OrbitItems = new();
            /// <summary>恋人项：图标容器 + 显示 + 占卜索引（不旋转、固定左下角、不加连接线）。</summary>
            public (Control Icon, Sprite2D Sprite, int Flag)? LoversItem;
            public float Center;
            /// <summary>公转累计时间（旋转随 Progress 加速/减速）。</summary>
            public float Time;
            /// <summary>鼠标正在节点上（展开 + 旋转）。</summary>
            public bool Hovered;
            /// <summary>鼠标刚离开节点但距离未超阈值 → 保持展开、不旋转（此时可看图标 hovertip）。</summary>
            public bool Sticky;
            /// <summary>总览模式（入口悬浮窗未贴边时全局展开显示）。</summary>
            public bool Overview;
            /// <summary>展开进度 0（收起）→ 1（完全浮出）。</summary>
            public float Progress;
        }

        private static readonly ConditionalWeakTable<NNormalMapPoint, OrbitState> _states = new();

        /// <summary>活跃节点集合（总览模式遍历用）。</summary>
        private static readonly HashSet<NNormalMapPoint> _activeNodes = new();

        /// <summary>当前被鼠标 hover 的图标（用于 hover 放大）。</summary>
        private static readonly HashSet<Control> _iconHovered = new();

        internal static void RegisterNode(NNormalMapPoint node) => _activeNodes.Add(node);
        internal static void UnregisterNode(NNormalMapPoint node) => _activeNodes.Remove(node);

        /// <summary>
        /// 总览模式开关：入口悬浮窗自由（未贴边）时为 true，全局展开所有被标记节点的标记图标
        /// （静止在各自相位，不公转，便于规划路线）；贴边/埋入时恢复只显示 sparkle。
        /// </summary>
        public static void SetOverview(bool on)
        {
            foreach (var node in _activeNodes)
            {
                if (!GodotObject.IsInstanceValid(node)) continue;
                if (TryGetState(node, out var st))
                    st.Overview = on;
            }
        }

        internal static bool TryGetState(NNormalMapPoint node, out OrbitState state)
            => _states.TryGetValue(node, out state!);

        /// <summary>节点退出场景树：反注册并清理全局层图标与连接线（避免换幕残留孤儿节点）。</summary>
        internal static void OnExit(NNormalMapPoint node)
        {
            UnregisterNode(node);
            if (!_states.TryGetValue(node, out var state)) return;
            _states.Remove(node);
            foreach (var item in state.OrbitItems)
            {
                if (GodotObject.IsInstanceValid(item.Icon)) { _iconHovered.Remove(item.Icon); item.Icon.QueueFree(); }   // sprite/badge 是 icon 子节点，随 icon 释放
                if (GodotObject.IsInstanceValid(item.Stem)) item.Stem.QueueFree();
            }
            if (state.LoversItem is { } li)
            {
                if (GodotObject.IsInstanceValid(li.Icon)) { _iconHovered.Remove(li.Icon); li.Icon.QueueFree(); }
            }
        }

        // ── 地图全局图标层（高于所有节点，低于 UI） ──────────────
        /// <summary>图标层节点名（挂 TheMap 下、Points 之后）。</summary>
        private const string MarkerLayerName = "PengoTarotMarkerLayer";

        /// <summary>
        /// 获取（惰性创建）地图全局图标层：TheMap/Points 之后 → 渲染高于所有地图节点；
        /// 位于 NMapScreen（地图屏幕）内部，NGame 上层 UI（设置界面等）仍在其上。
        /// </summary>
        private static Control? GetOrCreateMarkerLayer()
        {
            var mapScreen = NMapScreen.Instance;
            if (mapScreen == null || !GodotObject.IsInstanceValid(mapScreen)) return null;
            var theMap = mapScreen.GetNodeOrNull<Control>("TheMap");
            if (theMap == null) return null;

            var layer = theMap.GetNodeOrNull<Control>(MarkerLayerName);
            if (layer == null)
            {
                layer = new Control { Name = MarkerLayerName, MouseFilter = Control.MouseFilterEnum.Ignore };
                theMap.AddChild(layer);
                var points = theMap.GetNodeOrNull<Control>("Points");
                if (points != null)
                    theMap.MoveChild(layer, points.GetIndex() + 1);  // Points 之后 = 高于所有地图点
            }
            return layer;
        }

        // ── 几何工具 ────────────────────────────────────────────
        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseIn(float t) => t * t;

        private static bool HasIcon(int flag)
        {
            string p = TarotMarkerSystem.GetMarkerIconPath(flag);
            return !string.IsNullOrEmpty(p) && ResourceLoader.Exists(p);
        }

        /// <summary>
        /// 创建图标 = Control 容器（hit 区，接收鼠标 hover）+ 内部 Sprite2D（显示图，缩放绕中心）。
        /// Control 负责定位与鼠标事件，Sprite2D 负责显示与缩放中心（避免 TextureRect 从左上角缩放）。
        /// </summary>
        private static (Control Icon, Sprite2D Sprite) CreateIcon(int flag)
        {
            var tex = GD.Load<Texture2D>(TarotMarkerSystem.GetMarkerIconPath(flag));
            var sprite = new Sprite2D
            {
                Name = $"TarotMarkerSprite{flag}",
                Texture = tex,
            };
            sprite.Position = new Vector2(IconSize / 2f, IconSize / 2f);   // 放 Control 中心
            var icon = new Control
            {
                Name = $"TarotMarker{flag}",
                CustomMinimumSize = new Vector2(IconSize, IconSize),
                MouseFilter = Control.MouseFilterEnum.Stop,                 // 接收鼠标 hover
            };
            icon.AddChild(sprite);
            return (icon, sprite);
        }

        /// <summary>
        /// 创建右下角数字角标（白字 + 黑描边、无底框，参照原版卡牌 HandIndex / 能力图标数字样式）。
        /// 作为 icon 的子节点定位在右下角，随图标展开一起出现。
        /// </summary>
        private static Label CreateBadge(int flag)
        {
            var label = new Label
            {
                Name = $"TarotBadge{flag}",
                Text = "0",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            // 原版数字角标样式：白字 + 黑色描边（outline），不设背景框
            label.AddThemeFontSizeOverride("font_size", 17);
            label.AddThemeColorOverride("font_color", Colors.White);
            label.AddThemeColorOverride("font_outline_color", Colors.Black);
            label.AddThemeConstantOverride("outline_size", 5);
            // 右下角（icon 尺寸 IconSize×IconSize，角标贴在右下内侧、略靠右）
            label.Position = new Vector2(IconSize - 18f, IconSize - 22f);
            // 缩放 pivot 精确对齐图标（sprite）中心 = icon 中心 − 角标 Position
            // （PivotOffset 是相对角标左上角的偏移，必须补偿 Position 才是图标中心）
            // → 角标绕图标中心与 sprite 同中心缩放，真正绑死（而非各自绕自己中心）
            label.PivotOffset = new Vector2(IconSize / 2f, IconSize / 2f) - label.Position;
            return label;
        }

        /// <summary>把图标纹理切换为逆位版本（资源缺失时保持原图）。</summary>
        private static void ApplyReversedTexture(Sprite2D sprite, int flag)
        {
            string p = TarotMarkerSystem.GetReversedMarkerIconPath(flag);
            if (string.IsNullOrEmpty(p) || !ResourceLoader.Exists(p))
                return;
            sprite.Texture = GD.Load<Texture2D>(p);
        }

        // ── 占卜 hovertip ──────────────────────────────────────
        /// <summary>flagIndex → 本地化 NAME（大写，对应 BAL_CFW_FLAG_&lt;NAME&gt;_DESC 键）。</summary>
        private static readonly string[] _flagNames =
        {
            "FOOL", "MAGICIAN", "HIGHPRIESTESS", "EMPRESS", "EMPEROR", "HIEROPHANT",
            "LOVERS", "CHARIOT", "STRENGTH", "HERMIT", "WHEELOFFORTUNE", "JUSTICE",
            "HANGEDMAN", "DEATH", "TEMPERANCE", "DEVIL", "TOWER", "STAR", "MOON", "SUN", "JUDGEMENT", "WORLD",
        };

        private static string GetFlagName(int flag)
            => flag >= 0 && flag < _flagNames.Length ? _flagNames[flag] : string.Empty;

        /// <summary>标记占卜在动态描述之外追加的状态/关键词 hovertip（易伤/虚弱/隐者逆/消耗等词条说明，随描述一起堆叠显示）。</summary>
        internal static IHoverTip[] ExtraTipsForFlag(int flag)
            => flag switch
            {
                7 => new IHoverTip[] { HoverTipFactory.FromPower<VulnerablePower>() },
                8 => new IHoverTip[] { HoverTipFactory.FromPower<WeakPower>() },
                9 => new IHoverTip[] { HoverTipFactory.FromPower<TarHermitReversedPower>() },
                11 => new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) },
                12 => new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) },
                _ => Array.Empty<IHoverTip>(),
            };

        /// <summary>鼠标移到图标上：显示该占卜的详细效果 hovertip（读 gameplay_ui 表 BAL_CFW_FLAG_&lt;NAME&gt;_DESC）。</summary>
        private static void ShowIconTip(Control icon, int flag)
        {
            string name = GetFlagName(flag);
            if (string.IsNullOrEmpty(name)) return;
            // 标记占卜：地图 hovertip 用动态描述 LocString（SmartFormat 条件，已注入 Expired/Completed）；非标记类用静态描述键
            var mapDesc = ConfigFloatingWindowLoc.BuildMapDescription(flag);
            Texture2D? tex = null;
            string p = TarotMarkerSystem.GetMarkerIconPath(flag);
            if (!string.IsNullOrEmpty(p) && ResourceLoader.Exists(p))
                tex = GD.Load<Texture2D>(p);
            HoverTip main = mapDesc != null
                ? new HoverTip(mapDesc, tex)
                : new HoverTip(new LocString("gameplay_ui", "BAL_CFW_FLAG_" + name + "_DESC"), tex);
            // 标记占卜：动态描述 + 关键词/状态词条 hovertip 一起堆叠显示
            var extras = ExtraTipsForFlag(flag);
            NHoverTipSet? tip = extras.Length > 0
                ? NHoverTipSet.CreateAndShow(icon, TipsWithExtras(main, extras))
                : NHoverTipSet.CreateAndShow(icon, main);
            if (tip != null)
            {
                // 布局完成后重新对齐到图标旁（否则默认停在屏幕左上角）
                HoverTipAlignment alignment = HoverTip.GetHoverTipAlignment(icon);
                Callable.From(() => tip.SetAlignment(icon, alignment)).CallDeferred();
            }
        }

        /// <summary>描述 tip 后追加额外词条 tip，组合为多 tip 列表。</summary>
        private static IEnumerable<IHoverTip> TipsWithExtras(IHoverTip main, IReadOnlyList<IHoverTip> extras)
        {
            yield return main;
            foreach (var e in extras) yield return e;
        }

        /// <summary>鼠标离开图标：移除该图标的 hovertip。</summary>
        private static void HideIconTip(Control icon)
        {
            NHoverTipSet.Remove(icon);
        }

        /// <summary>
        /// 创建连接线（soul_beam 完整线状特效）：挂节点 IconContainer，用「置入顺序」置底
        /// （MoveChild 到容器最前 = 渲染最底层，低于节点图标；同父级其他节点 ZIndex 均为 0，不用 z_index）。
        /// 不调用其 Fire()（那是自动播放后隐藏），由我们自己控制出现/消失：初始隐藏，
        /// 展开时显示光束 Beam + 流动粒子 StaticParticles（LocalCoords 改 true 跟随地图滚动）。
        /// </summary>
        private static Node2D? CreateBeamStem(Control container)
        {
            var scene = GD.Load<PackedScene>(BeamScenePath);
            if (scene == null) return null;
            var vfx = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
            var holder = vfx.GetNode<Node2D>("BeamHolder");
            var staticParticles = vfx.GetNode<GpuParticles2D>("BeamHolder/StaticParticles");
            // local_coords 默认 false → 地图滚动时粒子滞留，改 true 跟随节点
            staticParticles.LocalCoords = true;
            staticParticles.Emitting = false;
            staticParticles.Visible = false;
            holder.Visible = false;                    // 初始隐藏（由我们控制显隐）
            vfx.Position = new Vector2(32f, 32f);      // 节点中心，光束从中心向外延伸
            container.AddChild(vfx);
            container.MoveChild(vfx, 0);               // 置入最前 = 渲染最底层（靠顺序，不靠 z_index）
            return vfx;
        }

        internal static void OnReady(NNormalMapPoint node)
        {
            var point = node.Point;
            if (point == null) return;

            // 用「含失效」的显示查询：失效的占卜仍需显示（逆位图），只是不计数/不触发战斗效果
            var flags = TarotMarkerSystem.GetDisplayedFlagsAt(point.coord);
            if (flags.Count == 0) return;

            var container = node.GetNodeOrNull<Control>("%IconContainer");
            if (container == null) return;

            float center = OrbitSize / 2f;
            var state = new OrbitState { Center = center };

            // 1) 稀有度金色闪光 sparkle（节点内，Icon 之上，常驻）
            // 注：必须用 GD.Load 而非 PreloadManager.Cache（该场景不在游戏预加载清单，Cache 找不到）
            var sparkleScene = GD.Load<PackedScene>("res://scenes/vfx/card_sparkles_vfx.tscn");
            if (sparkleScene != null)
            {
                var sparkle = sparkleScene.Instantiate<GpuParticles2D>(PackedScene.GenEditState.Disabled);
                // 关键：local_coords 默认 false（粒子在世界空间模拟）→ 地图滚动时已发射的星星会滞留在原地；
                // 改为 true 让粒子在本地空间模拟，跟随节点一起滚动
                sparkle.LocalCoords = true;
                sparkle.Position = new Vector2(center, center);
                sparkle.Scale = Vector2.One * SparkleScale;
                sparkle.SelfModulate = new Color(1f, 1f, 1f, SparkleAlpha);
                container.AddChild(sparkle);
            }

            // 2) 地图全局图标层（高于所有节点）
            var layer = GetOrCreateMarkerLayer();
            if (layer == null) return;

            // 3) 收集图标：非恋人（旋转组）与恋人（特殊）
            var orbitFlags = new List<int>();
            bool hasLovers = false;
            foreach (int flag in flags)
            {
                if (flag == LoversFlag) { hasLovers = true; continue; }
                if (HasIcon(flag)) orbitFlags.Add(flag);
            }

            int displayCount = orbitFlags.Count;
            int placed = 0;
            foreach (int flag in orbitFlags)
            {
                var (icon, sprite) = CreateIcon(flag);
                sprite.Scale = Vector2.Zero;             // 初始收起
                layer.AddChild(icon);                    // 全局层（高于所有节点）
                var stem = CreateBeamStem(container);    // 连接线（节点内，置底）
                var item = new OrbitItem
                {
                    Icon = icon,
                    Sprite = sprite,
                    Stem = stem,
                    Flag = flag,
                    Phase = InitialPhase + Mathf.Tau * placed / displayCount,  // 从左下起始，圆上均匀分布
                };
                if (TarotMarkerSystem.IsExpired(flag))
                {
                    // 已失效：直接切逆位图、不建角标（读档恢复的场景）
                    item.WasExpired = true;
                    ApplyReversedTexture(sprite, flag);
                }
                else
                {
                    // 未失效：建右下角数字角标（初始为该占卜当前完成进度；图标收起时隐藏）
                    item.Badge = CreateBadge(flag);
                    item.LastShown = TarotMarkerSystem.GetProgressForDisplay(flag);
                    item.Badge.Text = item.LastShown.ToString();
                    item.Badge.Visible = false;   // 初始收起，由 _Process 随图标展开显示
                    icon.AddChild(item.Badge);
                }
                state.OrbitItems.Add(item);
                // 图标 hover → 放大 + 显示该占卜的详细效果 hovertip
                icon.MouseEntered += () => { _iconHovered.Add(icon); ShowIconTip(icon, flag); };
                icon.MouseExited += () => { _iconHovered.Remove(icon); HideIconTip(icon); };
                placed++;
            }

            // 4) 恋人：不旋转、固定左下角、不加连接线；作为 %IconContainer 子节点（与光束同机制）
            //    → 被容器连带缩放，与地图图标完全同中心同幅度地播放呼吸动画
            if (hasLovers)
            {
                var (lovers, loversSprite) = CreateIcon(LoversFlag);
                loversSprite.Scale = Vector2.Zero;
                float left = LoversOffset - OrbitSize / 2f;
                float down = OrbitSize / 2f - LoversOffset;
                // 容器中心局部（与光束 Position 同基准 OrbitSize/2）+ 左下偏移 - icon 半尺寸 = icon 左上角
                lovers.Position = new Vector2(OrbitSize / 2f, OrbitSize / 2f) + new Vector2(left, down)
                                  - new Vector2(IconSize / 2f, IconSize / 2f);
                container.AddChild(lovers);          // 加容器末尾 = 渲染在 Icon/sparkle 之上
                state.LoversItem = (lovers, loversSprite, LoversFlag);
                lovers.MouseEntered += () => { _iconHovered.Add(lovers); ShowIconTip(lovers, LoversFlag); };
                lovers.MouseExited += () => { _iconHovered.Remove(lovers); HideIconTip(lovers); };
            }

            // 5) hover 挂钩：进入 → 展开+旋转；离开 → 进入粘滞（保持展开不旋转，距离超阈值才收起，由 _Process 检测）
            node.MouseEntered += () => { state.Hovered = true; state.Sticky = false; };
            node.MouseExited += () => { state.Hovered = false; state.Sticky = true; };

            _states.AddOrUpdate(node, state);
            RegisterNode(node);
        }

        internal static void OnProcess(NNormalMapPoint node, double delta)
        {
            if (!TryGetState(node, out var state)) return;

            // 粘滞：鼠标离开节点后保持展开不旋转，直到距节点中心超过阈值才收起
            if (state.Hovered || state.Sticky)
            {
                float dist = (node.GetGlobalRect().GetCenter() - node.GetGlobalMousePosition()).Length();
                if (dist > StickyThreshold)
                {
                    state.Hovered = false;
                    state.Sticky = false;
                }
            }

            // 进度驱动：hover / 粘滞 / 总览展开，否则收回
            bool expand = state.Hovered || state.Sticky || state.Overview;
            if (expand && state.Progress < 1f)
                state.Progress = Mathf.Min(1f, state.Progress + (float)delta / PopDuration);
            else if (!expand && state.Progress > 0f)
                state.Progress = Mathf.Max(0f, state.Progress - (float)delta / PopDuration);
            // 旋转：
            //  - hover 时转（含总览下 hover）；粘滞（离开节点未超距离）保持展开但不旋转；
            //  - 普通收起（非粘滞非总览）过程中减速停止；纯总览静止读图。
            bool inOverview = state.Overview;
            bool rotating = state.Hovered || (!inOverview && !state.Sticky && state.Progress > 0f);
            if (rotating)
                state.Time += (float)delta * Mathf.SmoothStep(0f, 1f, state.Progress);

            if (state.Progress <= 0f) return;

            float p = state.Hovered ? EaseOut(state.Progress) : EaseIn(state.Progress);
            // 收起尾声直接归 0，避免极小图标残留
            float iconScale = p < MinVisible ? 0f : p;
            // 节点中心（全局）：节点无旋转缩放，GetGlobalRect 中心即节点中心
            Vector2 nodeCenter = node.GetGlobalRect().GetCenter();

            // 旋转组：图标绕节点中心公转（自身正立），连接线跟随方向；右下角数字角标随展开显示、随完成/失效更新
            foreach (var item in state.OrbitItems)
            {
                if (!GodotObject.IsInstanceValid(item.Icon)) continue;
                float angle = state.Time * OrbitSpeed + item.Phase;
                Vector2 iconCenter = nodeCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * OrbitRadius;
                item.Icon.GlobalPosition = iconCenter - new Vector2(IconSize / 2f, IconSize / 2f);  // Control 左上角定位
                // 收起（隐形）时禁用鼠标交互，避免在空位复现 hovertip
                bool visible = iconScale > 0f;
                item.Icon.MouseFilter = visible ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
                float hov = _iconHovered.Contains(item.Icon) ? IconHoverScale : 1f;
                item.Sprite.Scale = Vector2.One * (IconSize / IconBaseTexSize) * iconScale * hov;    // Sprite 绕中心缩放 + hover 放大
                item.Sprite.Rotation = 0f;
                UpdateStem(item.Stem, angle, p);

                // 失效切换：达成目标 → 切逆位图 + 停止显示数字角标（图标仍显示）
                bool expired = TarotMarkerSystem.IsExpired(item.Flag);
                if (expired && !item.WasExpired)
                {
                    item.WasExpired = true;
                    ApplyReversedTexture(item.Sprite, item.Flag);
                    if (item.Badge != null)
                        item.Badge.Visible = false;
                }
                // 未失效：数字角标与图标同中心同步缩放（Visible 随图标出现，Scale 从图标中心一起放大/缩小）；完成数变化时刷新（普通类每满 RewardInterval 个自动归零）
                if (!expired && item.Badge != null)
                {
                    int shown = TarotMarkerSystem.GetProgressForDisplay(item.Flag);
                    if (shown != item.LastShown)
                    {
                        item.LastShown = shown;
                        item.Badge.Text = shown.ToString();
                    }
                    item.Badge.Visible = visible;
                    item.Badge.Scale = Vector2.One * (iconScale * hov);
                }
            }

            // 恋人：不旋转，固定左下角（位置已在 OnReady 用容器局部坐标设好）；不加连接线
            if (state.LoversItem is { } li && GodotObject.IsInstanceValid(li.Icon))
            {
                bool visible = iconScale > 0f;
                li.Icon.MouseFilter = visible ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
                li.Icon.PivotOffset = new Vector2(IconSize / 2f, IconSize / 2f);   // 缩放锚点 = 恋人自身中心
                float hov = _iconHovered.Contains(li.Icon) ? IconHoverScale : 1f;
                // 容器呼吸缩放 C 与地图图标 hover 缩放 H（%Icon 1→1.45）都只作用于 sprite 视觉，hitbox 不随之变化
                float c = 1f, h = 1f;
                if (node.GetNodeOrNull<Control>("%IconContainer") is { } iconContainer && iconContainer.Scale.X != 0f)
                    c = iconContainer.Scale.X;
                if (node.GetNodeOrNull<TextureRect>("%Icon") is { } mapIcon)
                    h = mapIcon.Scale.X;
                // hitbox（Control）反向补偿容器缩放 → 命中区恒为 IconSize，不随节点放大/缩小
                li.Icon.Scale = Vector2.One * (1f / c);
                // sprite 视觉：展开动画 + 恋人 0.8 倍 + hover 放大(h) + 容器呼吸(c) 全部体现在显示上
                li.Sprite.Scale = Vector2.One * (IconSize / IconBaseTexSize) * iconScale * hov * LoversScale * c * h;
                li.Sprite.Rotation = 0f;
            }
        }

        /// <summary>更新连接线（soul_beam）：由我们控制显隐与朝向/长度；展开时显示光束 + 流动粒子。</summary>
        private static void UpdateStem(Node2D? vfx, float angle, float p)
        {
            if (vfx == null || !GodotObject.IsInstanceValid(vfx)) return;
            // 反向补偿 %IconContainer 的原版呼吸缩放（NNormalMapPoint._Process: Sin*0.25+1.2）
            // → 光束大小不随地图节点放大/缩小（container.Scale × vfx.Scale = 1 恒定）
            vfx.Scale = vfx.GetParent() is Control c ? Vector2.One / c.Scale : Vector2.One;
            var holder = vfx.GetNode<Node2D>("BeamHolder");
            var staticParticles = vfx.GetNode<GpuParticles2D>("BeamHolder/StaticParticles");
            bool show = p > MinVisible;
            holder.Visible = show;
            staticParticles.Visible = show;
            staticParticles.Emitting = show;
            if (!show) return;
            vfx.Rotation = angle + BeamAngleOffset;                          // 光束指向目标角
            holder.Scale = new Vector2(p * BeamLengthScale, BeamThinScale);  // 长度/粗细随展开
        }
    }

    /// <summary>NNormalMapPoint._Ready 后创建占卜标记视觉（sparkle + 全局图标 + 连接线）。</summary>
    [HarmonyPatch(typeof(NNormalMapPoint), "_Ready")]
    public static class NMapPointMarkerIconReadyPatch
    {
        public static void Postfix(NNormalMapPoint __instance)
        {
            NMapPointMarkerIconPatch.OnReady(__instance);
        }
    }

    /// <summary>NNormalMapPoint._Process 每帧驱动图标浮出/公转（hover 时节点仍处理帧，动画不停止）。</summary>
    [HarmonyPatch(typeof(NNormalMapPoint), "_Process")]
    public static class NMapPointMarkerOrbitPatch
    {
        public static void Postfix(NNormalMapPoint __instance, double delta)
        {
            NMapPointMarkerIconPatch.OnProcess(__instance, delta);
        }
    }

    /// <summary>NNormalMapPoint 退出场景树时反注册并清理（总览遍历 / 孤儿节点清理）。</summary>
    [HarmonyPatch(typeof(NNormalMapPoint), "_ExitTree")]
    public static class NMapPointMarkerIconExitPatch
    {
        public static void Postfix(NNormalMapPoint __instance)
        {
            NMapPointMarkerIconPatch.OnExit(__instance);
        }
    }
}
