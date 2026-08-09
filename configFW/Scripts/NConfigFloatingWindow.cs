#nullable enable

using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using PengoTarot.BalatroEffect;
using PengoTarot.Patches;
using PengoTarot.Powers;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// configfloatingwindow 的浮动面板（场景驱动）。
    /// 美术参考 balatro_inspect：黑色 shader 底 + 中央 2/3×2/3 蓝色半透明面板，点击面板外黑色区域返回原界面。
    /// 布局：左侧竖向 2 个大按钮（塔罗/星球，占位）+ 右侧 3 列 7/8/7 共 22 个难度开关。
    /// editable=true（选人界面）可编辑并即时写 JSON；editable=false（游戏过程/远端）只读。
    /// </summary>
    public partial class NConfigFloatingWindow : Control, IScreenContext
    {
        private const string ScenePath = "res://configFW/Scenes/configfloatingwindow.tscn";

        // 左侧大按钮图标：未勾选用普通，勾选用 up
        private const string TarotIcon = "res://configFW/Scenes/images/tarot.webp";
        private const string TarotUpIcon = "res://configFW/Scenes/images/tarot_up.webp";
        private const string PlanetIcon = "res://configFW/Scenes/images/planet.webp";
        private const string PlanetUpIcon = "res://configFW/Scenes/images/planet_up.webp";

        private static Font? _boldFont;
        private static Font BoldFont => _boldFont ??= GD.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres");

        /// <summary>按当前语言返回游戏官方的替换字体（zhs→Noto 简体 / jpn→CJK JP / kor→韩文，字形正确且 glyph 图集已被游戏本体预热）；非 CJK 返回 null。</summary>
        private static Font? GetLocaleFont(FontType type)
        {
            if (LocManager.Instance != null && FontManager.NeedsFontSubstitution(LocManager.Instance.Language))
                return FontManager.GetSubstituteFont(LocManager.Instance.Language, type);
            return null;
        }

        // ── 提示字体 glyph 预热：避免首次 hover 时逐字生成字形图集导致的卡顿 ──
        private static bool _hintFontsWarmedUp;
        private static RichTextLabel? _warmLabel;
        private static Queue<string>? _warmChunks;

        /// <summary>在游戏启动时调度一次提示文本字体 glyph 预热（后台逐帧绘制透明标签，不阻塞、无视觉影响）。
        /// ⚠️ 预热是纯优化：只在帧循环里收集/绘制文本，任何异常都静默忽略，绝不影响游戏启动（防 Steam Deck 等启动时序差异下 LocException）。</summary>
        public static void ScheduleHintFontsWarmUp()
        {
            if (_hintFontsWarmedUp) return;
            try
            {
                if (Engine.GetMainLoop() is not SceneTree tree) return;
                _hintFontsWarmedUp = true;
                // 不在 Initialize 同步栈里做本地化查询（BAL_CFW_* 键可能尚未注入）；首帧再构建
                tree.ProcessFrame += OnWarmUpFrame;
            }
            catch (Exception)
            {
                _hintFontsWarmedUp = true;   // 标记避免反复重试
            }
        }

        /// <summary>每帧绘制一段预热文本（几乎透明但参与绘制 → 生成字形图集），绘制完换下一段，全部完成即释放。</summary>
        private static void OnWarmUpFrame()
        {
            try
            {
                if (Engine.GetMainLoop() is not SceneTree tree) return;
                if (_warmChunks == null)
                {
                    // 首帧再收集文本：此时本地化表已稳定，BAL_CFW_* 键可用
                    _warmChunks = BuildWarmUpChunks();
                }
                if (_warmChunks.Count == 0)
                {
                    tree.ProcessFrame -= OnWarmUpFrame;
                    _warmLabel?.QueueFree();
                    _warmLabel = null;
                    _warmChunks = null;
                    return;
                }
                var root = tree.Root;
                if (root == null) return;
                if (_warmLabel == null)
                {
                    _warmLabel = new RichTextLabel
                    {
                        BbcodeEnabled = true,
                        AutowrapMode = TextServer.AutowrapMode.Word,
                        MouseFilter = Control.MouseFilterEnum.Ignore,
                        Modulate = new Color(1, 1, 1, 0.01f),   // 几乎透明但参与绘制 → glyph 进图集
                        Position = new Vector2(0, 0),
                        Size = new Vector2(1100, 520),
                    };
                    _warmLabel.AddThemeFontOverride("normal_font", GetLocaleFont(FontType.Regular) ?? BoldFont);
                    _warmLabel.AddThemeFontSizeOverride("normal_font_size", 24);
                    root.AddChild(_warmLabel);
                }
                _warmLabel.Text = _warmChunks.Dequeue();
            }
            catch (Exception)
            {
                // 预热失败（本地化缺失/字体缺失/节点时序等）静默终止：这是纯优化，绝不能影响游戏
                _warmChunks = new Queue<string>();   // 空队列 → 下帧走 count==0 分支清理退出
            }
        }

        /// <summary>收集配置界面全部可能显示的文本并按字符数分块（每块一屏内完整绘制，避免被视口裁剪导致 glyph 未生成）。
        /// 用 GetIfExists 安全读取：键缺失返回 null 而非抛 LocException（预热路径可能早于注入链）。</summary>
        private static Queue<string> BuildWarmUpChunks()
        {
            ConfigFloatingWindowLoc.Inject();   // 幂等：确保 BAL_CFW_* 键已注入
            var all = new List<string> { SafeLoc("CFW_DEFAULT_HINT") };
            foreach (string name in FlagNames)
                all.Add(SafeLoc("CFW_FLAG_" + name + "_DESC"));
            var progress = LocString.GetIfExists("gameplay_ui", "BAL_CFW_PROGRESS_LINE");
            if (progress != null)
            {
                progress.Add("Expired", false);
                progress.Add("Count", 3);
                all.Add(progress.GetFormattedText() ?? string.Empty);
            }

            var chunks = new Queue<string>();
            var chunk = new List<string>();
            int chars = 0;
            foreach (string text in all)
            {
                if (chars > 0 && chars + text.Length > 350)
                {
                    chunks.Enqueue(string.Join("\n\n", chunk));
                    chunk.Clear();
                    chars = 0;
                }
                chunk.Add(text);
                chars += text.Length;
            }
            if (chunk.Count > 0)
                chunks.Enqueue(string.Join("\n\n", chunk));
            return chunks;
        }

        /// <summary>安全读取本地化文本：键缺失时返回键名（不抛 LocException；预热/启动路径专用）。</summary>
        private static string SafeLoc(string key)
        {
            var ls = LocString.GetIfExists("gameplay_ui", "BAL_" + key);
            return ls?.GetFormattedText() ?? key;
        }

        private ColorRect _backstop = null!;
        private NButton _backstopHit = null!;
        private Tween? _openTween;

        /// <summary>客户端只读模式：禁用点击关闭与热键。</summary>
        private bool _remoteMode;
        /// <summary>是否可编辑（选人界面 true，游戏过程/远端 false）。</summary>
        private bool _editable;
        /// <summary>淡出关闭中（防 Close 被重复调用：避免二次 QueueFree / 重复移除热键）。</summary>
        private bool _closing;

        /// <summary>生成的开关按钮（统一管理 Enable/Disable）。</summary>
        private readonly List<Control> _toggleButtons = new();
        /// <summary>按钮当前开关状态。</summary>
        private readonly Dictionary<Control, bool> _toggleState = new();
        /// <summary>底部提示行（hover 按钮时显示对应描述）。</summary>
        private RichTextLabel _hintLabel = null!;
        /// <summary>右侧难度网格（塔罗开关控制显隐）。</summary>
        private GridContainer _rightGrid = null!;
        /// <summary>右侧 22 个难度按钮（塔罗开关动画显隐用）。</summary>
        private readonly List<Control> _rightButtons = new();
        /// <summary>右侧按钮的开关动画 tween（快速开关时先 Kill 旧的）。</summary>
        private readonly List<Tween> _rightTweens = new();
        /// <summary>中央面板（hover 标记占卜开关时，右上角额外词条 hovertip 的定位锚点）。</summary>
        private Control? _panel;

        /// <summary>左侧塔罗大按钮（RefreshFromRunData 用）。</summary>
        private Control? _tarotBtn;
        /// <summary>左侧星球大按钮（RefreshFromRunData 用）。</summary>
        private Control? _planetBtn;
        /// <summary>右侧难度按钮 → 索引（RefreshFromRunData 用）。</summary>
        private readonly Dictionary<Control, int> _rightButtonIndices = new();

        /// <summary>当前 hover 中有额外词条 hovertip 的开关按钮（用于切换开关后立即刷新「塔罗包」等动态词条）。</summary>
        private Control? _hoveredTipButton;

        // ── 底部设置入口位置 toggle（节点在 .tscn 中定义，完全独立于本局配置） ──
        private HBoxContainer _settingsToggle = null!;
        private Control _settingsToggleTick = null!;
        /// <summary>左下角随机 TIPS 标签（仅当前语言有内容时显示）。</summary>
        private Label _tipLabel = null!;
        /// <summary>本次启动打开配置界面的次数（用于解锁「感谢」TIPS，进程启动即清零）。</summary>
        private static int _configOpensThisLaunch;

        /// <summary>右侧 22 个难度按钮的名字，按标准塔罗大阿卡纳顺序（对应描述本地化键 BAL_CFW_FLAG_&lt;NAME&gt;_DESC）。</summary>
        private static readonly string[] FlagNames =
        {
            "Fool", "Magician", "HighPriestess", "Empress", "Emperor",
            "Hierophant", "Lovers", "Chariot", "Strength", "Hermit",
            "WheelOfFortune", "Justice", "HangedMan", "Death", "Temperance",
            "Devil", "Tower", "Star", "Moon", "Sun", "Judgement", "World"
        };

        public Control? DefaultFocusedControl => null;

        public static NConfigFloatingWindow? Create()
        {
            var scene = GD.Load<PackedScene>(ScenePath);
            return scene.Instantiate<NConfigFloatingWindow>(PackedScene.GenEditState.Disabled);
        }

        public override void _Ready()
        {
            _backstop = GetNode<ColorRect>("Backstop");
            _backstopHit = GetNode<NButton>("BackstopHit");
            // 点击面板外 → 关闭面板（由静态门面统一管理状态与多人广播）
            _backstopHit.Connect(NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ => ConfigFloatingWindow.ClosePanel()));

            var panelVBox = GetNode<VBoxContainer>("CenterPanel/PanelMargin/PanelVBox");
            var title = panelVBox.GetNode<Label>("TitleLabel");
            title.AddThemeFontOverride("font", GetLocaleFont(FontType.Bold) ?? BoldFont);
            title.Text = Tr("CFW_TITLE");

            _hintLabel = panelVBox.GetNode<RichTextLabel>("HintLabel");
            _hintLabel.AddThemeFontOverride("normal_font", GetLocaleFont(FontType.Regular) ?? BoldFont);
            // 直接作为 VBox 子节点满宽 + 居中（不用 CenterContainer：实测其布局会把文本压成竖直细条）
            _hintLabel.VerticalAlignment = VerticalAlignment.Center;
            _hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _hintLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _hintLabel.BbcodeEnabled = true;   // 支持 [gold] 等游戏色标（经 ToLabelBbcode 转换）
            _hintLabel.Text = ToLabelBbcode(Tr("CFW_DEFAULT_HINT"));

            var body = panelVBox.GetNode<HBoxContainer>("Body");
            var leftColumn = body.GetNode<VBoxContainer>("LeftColumn");
            _rightGrid = body.GetNode<GridContainer>("RightGrid");
            _panel = GetNodeOrNull<Control>("CenterPanel");

            BindLeftButtons(leftColumn);
            BindRightButtons(_rightGrid);
            // 鼠标移出整个按钮区域（左列 + 右网格）才恢复默认提示，避免按钮间间隙闪默认文本
            body.MouseFilter = Control.MouseFilterEnum.Stop;
            body.MouseExited += () => _hintLabel.Text = ToLabelBbcode(Tr("CFW_DEFAULT_HINT"));
            // 塔罗总开关未启用：右侧保持占位，但不可见、不可交互、无 hover
            bool tarotOn = ConfigFloatingWindowRunData.TarotEnabled;
            if (!tarotOn)
            {
                foreach (var b in _rightButtons)
                {
                    b.MouseFilter = Control.MouseFilterEnum.Ignore;
                    if (b is NButton nb) nb.Disable();
                    b.Modulate = new Color(b.Modulate.R, b.Modulate.G, b.Modulate.B, 0f);
                }
            }

            // 背景星云（全屏垫底）：按实际尺寸设 uv_scale 防拉伸
            ShaderController.ApplyStarcloudBgAspect(GetNodeOrNull<ColorRect>("CardAreaBg"));

            // ── 底部设置入口位置 toggle ──
            BindSettingsToggle();

            // ── 左下角随机 TIPS ──
            BindTipLabel();

            Visible = false;
        }

        /// <summary>绑定底部 SettingsToggle（节点在 .tscn 中已定义）。此开关完全独立于本局配置，仅读写全局 JSON。</summary>
        private void BindSettingsToggle()
        {
            _settingsToggle = GetNode<HBoxContainer>("%SettingsToggle");
            _settingsToggleTick = _settingsToggle.GetNode<Control>("TickIcon");

            // 本地化标签文本（tscn 无 font override → 需按语言替换字体，否则中文走系统 fallback 显示异体字）
            var label = _settingsToggle.GetNode<Label>("Label");
            label.AddThemeFontOverride("font", GetLocaleFont(FontType.Regular) ?? BoldFont);
            label.Text = Tr("CFW_SETTINGS_TOGGLE");

            // hover 与点击范围一致：仅「文本」与「勾选框图标」区域响应（放大图标/切换），空白区域不响应
            _settingsToggleTick.MouseFilter = Control.MouseFilterEnum.Pass;
            label.MouseFilter = Control.MouseFilterEnum.Pass;
            _settingsToggleTick.MouseEntered += () => ScaleTickIcon(_settingsToggleTick, 1.1f);
            _settingsToggleTick.MouseExited += () => ScaleTickIcon(_settingsToggleTick, 1.0f);
            label.MouseEntered += () => ScaleTickIcon(_settingsToggleTick, 1.1f);
            label.MouseExited += () => ScaleTickIcon(_settingsToggleTick, 1.0f);

            // 点击切换：仅当点击落在文本或勾选框图标区域内才生效（避免整行大片空白误触）
            _settingsToggle.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(ev =>
            {
                if (ev is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
                {
                    var pos = _settingsToggle.GetLocalMousePosition();
                    if (!_settingsToggleTick.GetRect().HasPoint(pos) && !label.GetRect().HasPoint(pos))
                        return;
                    ToggleSettingsOnly();
                }
            }));

            // 初始化状态：纯读全局 JSON，不依赖 RunData
            ApplySettingsToggleVisual(ConfigFloatingWindowConfig.ShowInSettingsOnly);
        }

        /// <summary>配置界面左下角随机 TIPS：每次打开抽一条；查看 ≥<see cref="ConfigFloatingWindowLoc.ThanksUnlockOpens"/> 次后「感谢」内容并入随机池。</summary>
        private void BindTipLabel()
        {
            _tipLabel = GetNodeOrNull<Label>("TipLabel");
            if (_tipLabel == null) return;
            _configOpensThisLaunch++;
            _tipLabel.AddThemeFontOverride("font", GetLocaleFont(FontType.Regular) ?? BoldFont);
            string lang = LocManager.Instance?.Language ?? "en";
            var main = ConfigFloatingWindowLoc.GetMainTips(lang);
            if (main.Count == 0)
            {
                _tipLabel.Visible = false;   // 当前语言无 TIPS 内容（仅中文有）
                return;
            }
            _tipLabel.Text = "TIPS：" + PickRandomTip(main, lang);
        }

        /// <summary>最近抽过的 TIPS（最多 <see cref="RecentTipCount"/> 条，抽选时剔除避免重复）。</summary>
        private static readonly List<string> _recentTips = new();
        private const int RecentTipCount = 10;

        private static string PickRandomTip(IReadOnlyList<string> main, string lang)
        {
            var thanks = ConfigFloatingWindowLoc.GetThanksTips(lang);
            var pool = new List<string>(main);
            if (_configOpensThisLaunch >= ConfigFloatingWindowLoc.ThanksUnlockOpens)
                pool.AddRange(thanks);

            // 最近 10 条不重复；候选被排除到空（池太小）时回退全池，避免无解
            var candidates = new List<string>();
            foreach (string t in pool)
                if (!_recentTips.Contains(t))
                    candidates.Add(t);
            if (candidates.Count == 0)
                candidates = pool;

            string pick = candidates[Random.Shared.Next(candidates.Count)];
            _recentTips.Add(pick);
            while (_recentTips.Count > RecentTipCount)
                _recentTips.RemoveAt(0);
            return pick;
        }

        private void ToggleSettingsOnly()
        {
            // 此开关完全独立：不依赖 _editable，不写 RunData，不广播
            bool next = !ConfigFloatingWindowConfig.ShowInSettingsOnly;
            ConfigFloatingWindowConfig.SetShowInSettingsOnly(next);
            ApplySettingsToggleVisual(next);
        }

        private void ApplySettingsToggleVisual(bool isOn)
        {
            if (_settingsToggleTick == null) return;
            var visuals = _settingsToggleTick.GetNodeOrNull<Control>("TickboxVisuals");
            if (visuals == null) return;
            var ticked = visuals.GetNodeOrNull<Control>("Ticked");
            var notTicked = visuals.GetNodeOrNull<Control>("NotTicked");
            if (ticked != null) ticked.Visible = isOn;
            if (notTicked != null) notTicked.Visible = !isOn;
        }

        private static void ScaleTickIcon(Control target, float scale)
        {
            target.PivotOffset = target.Size / 2f;
            target.CreateTween()
                .TweenProperty(target, "scale", Vector2.One * scale, 0.1f)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }

        private static string Tr(string key) =>
            new LocString("gameplay_ui", "BAL_" + key).GetFormattedText() ?? key;

        // ── 左侧 2 个大按钮（塔罗/星球，节点在 tscn 中静态建立） ──
        private void BindLeftButtons(VBoxContainer column)
        {
            _tarotBtn = column.GetNodeOrNull<Control>("TarotBtn");
            _planetBtn = column.GetNodeOrNull<Control>("PlanetBtn");
            BindLeftToggle(_tarotBtn,
                ConfigFloatingWindowRunData.TarotEnabled,
                v =>
                {
                    ConfigFloatingWindowConfig.SetTarotEnabled(v);
                    ConfigFloatingWindowRunData.SetTarotEnabled(v);
                    ConfigFloatingWindow.BroadcastConfig();
                    // 开启大塔罗时刷新右侧按钮状态（_Ready 中 TarotEnabled=false 导致 _toggleState 全 false）
                    if (v)
                    {
                        foreach (var kv in _rightButtonIndices)
                            _toggleState[kv.Key] = ConfigFloatingWindowRunData.GetTarFlag(kv.Value);
                    }
                    // 塔罗总开关：动画显示/隐藏右侧全部难度选项
                    AnimateRightButtons(v);
                },
                "CFW_TAROT_DESC",
                TarotIcon, TarotUpIcon);
            BindLeftToggle(column.GetNodeOrNull<Control>("PlanetBtn"),
                ConfigFloatingWindowRunData.PlanetEnabled,
                v =>
                {
                    ConfigFloatingWindowConfig.SetPlanetEnabled(v);
                    ConfigFloatingWindowRunData.SetPlanetEnabled(v);
                    ConfigFloatingWindow.BroadcastConfig();
                },
                "CFW_PLANET_DESC",
                PlanetIcon, PlanetUpIcon);
        }

        /// <summary>按勾选状态切换大按钮图标（勾选时用 up 图标）。</summary>
        private static void ApplyBigButtonIcon(Control? btn, bool isUp, string? normalIcon, string? upIcon)
        {
            if (btn == null) return;
            var icon = btn.GetNodeOrNull<TextureRect>("Icon");
            if (icon == null) return;
            string path = isUp ? upIcon! : normalIcon!;
            if (string.IsNullOrEmpty(path)) return;
            var tex = GD.Load<Texture2D>(path);
            if (tex != null) icon.Texture = tex;
        }

        private void BindLeftToggle(Control? btn, bool isOn, Action<bool> onToggled, string hintKey,
            string? normalIcon = null, string? upIcon = null)
        {
            if (btn == null) return;
            if (btn is NButton nb)
            {
                nb.Connect(NClickableControl.SignalName.Released,
                    Callable.From<NButton>(_ =>
                    {
                        bool next = !_toggleState[btn];
                        _toggleState[btn] = next;
                        onToggled(next);
                        ApplyToggleVisual(btn, next);
                        ApplyBigButtonIcon(btn, next, normalIcon, upIcon);
                    }));
            }
            AddHover(btn, hintKey);

            _toggleState[btn] = isOn;
            _toggleButtons.Add(btn);
            ApplyToggleVisual(btn, isOn);
            ApplyBigButtonIcon(btn, isOn, normalIcon, upIcon);
        }

        // ── 右侧 22 个难度开关（3 列，节点在 tscn 中静态建立） ──
        private void BindRightButtons(GridContainer grid)
        {
            int i = 0;
            foreach (Node child in grid.GetChildren())
            {
                if (child is not Control btn) continue;
                if (!btn.Name.ToString().StartsWith("Flag")) continue;

                int idx = i;
                _rightButtonIndices[btn] = idx;
                if (idx < FlagNames.Length)
                    // 本地化键为全大写（BAL_CFW_FLAG_<NAME>_DESC），FlagNames 是 PascalCase，需转大写匹配
                    AddHover(btn, $"CFW_FLAG_{FlagNames[idx].ToUpperInvariant()}_DESC");

                // 有额外词条 hovertip 的开关（战车→易伤、力量→虚弱、隐者→隐者逆、节制→节制-逆、正义/倒吊人→消耗）：
                // hover 时从面板右上角顶点开始、向右下方向创建额外词条 hovertip
                if (ExtraTipsForConfigFlag(idx).Length > 0)
                {
                    btn.MouseEntered += () => { _hoveredTipButton = btn; ShowRightTopExtras(idx, btn); };
                    btn.MouseExited += () => { if (_hoveredTipButton == btn) _hoveredTipButton = null; HideRightTopExtras(btn); };
                }

                if (btn is NButton nb)
                {
                    nb.Connect(NClickableControl.SignalName.Released,
                        Callable.From<NButton>(_ =>
                        {
                            bool next = !_toggleState[btn];
                            _toggleState[btn] = next;
                            ConfigFloatingWindowConfig.SetDifficultyFlag(idx, next);
                            ConfigFloatingWindowRunData.SetDifficultyFlag(idx, next);
                            ConfigFloatingWindow.BroadcastConfig();
                            ApplyToggleVisual(btn, next);
                            // 切换开关后，若鼠标仍停在该开关上，立即刷新右上角的动态词条（「塔罗包」内容随开关状态变化）
                            if (_hoveredTipButton == btn)
                                ShowRightTopExtras(idx, btn);
                        }));
                }

                _toggleState[btn] = ConfigFloatingWindowRunData.GetTarFlag(idx);
                _toggleButtons.Add(btn);
                _rightButtons.Add(btn);
                ApplyToggleVisual(btn, _toggleState[btn]);
                i++;
            }
        }

        /// <summary>按钮 hover：放大 1.05 倍 + 显示描述；局内（只读）额外取消亮度/不透明度降低。</summary>
        /// <summary>把游戏 bbcode 色标（[gold]/[red]/[blue]/[purple]/[green]）转为 RichTextLabel 可渲染的 [color=#...]（色值取 StsColors），并水平居中。</summary>
        private static string ToLabelBbcode(string text)
        {
            if (text == null) return string.Empty;
            return "[center]" + text
                .Replace("[gold]", "[color=#EFC851]").Replace("[/gold]", "[/color]")
                .Replace("[red]", "[color=#FF5555]").Replace("[/red]", "[/color]")
                .Replace("[blue]", "[color=#87CEEB]").Replace("[/blue]", "[/color]")
                .Replace("[purple]", "[color=#EE82EE]").Replace("[/purple]", "[/color]")
                .Replace("[green]", "[color=#7FFF00]").Replace("[/green]", "[/color]")
                + "[/center]";
        }

        /// <summary>flag 按钮 hover 提示：标记占卜用动态描述（游戏内追加「当前已完成X」/「已失效」），其余走静态本地化。</summary>
        private string FlagHintText(string hintKey)
        {
            const string prefix = "CFW_FLAG_";
            const string suffix = "_DESC";
            if (hintKey.StartsWith(prefix) && hintKey.EndsWith(suffix))
            {
                string upper = hintKey.Substring(prefix.Length, hintKey.Length - prefix.Length - suffix.Length);
                for (int i = 0; i < FlagNames.Length; i++)
                {
                    if (string.Equals(FlagNames[i], upper, StringComparison.OrdinalIgnoreCase))
                    {
                        if (ConfigFloatingWindowLoc.IsMarkedDivination(i))
                            return ConfigFloatingWindowLoc.BuildSettingsDescription(i);
                        break;
                    }
                }
            }
            return Tr(hintKey);
        }

        private void AddHover(Control btn, string hintKey)
        {
            btn.MouseEntered += () =>
            {
                _hintLabel.Text = ToLabelBbcode(FlagHintText(hintKey));
                ScaleTo(btn, 1.05f);   // hover 仅轻微放大：不改亮度也不加描边（避免与开关开/关的亮度动画冲突）
            };
            btn.MouseExited += () =>
            {
                // 不在此恢复默认文本（按钮间有间隙会闪烁）；由 Body 整体移出时恢复
                ScaleTo(btn, 1.0f);
                // 关闭/不可用状态下不恢复 modulate（避免淡出中的按钮被 hover 重新点亮）
                if (btn.MouseFilter != Control.MouseFilterEnum.Ignore)
                    ApplyToggleVisual(btn, _toggleState[btn]);
            };
        }

        /// <summary>配置面板 hover 时右上角显示的额外词条 hovertip：标记占卜复用地图词条（易伤/虚弱/隐者逆/消耗），节制-逆单独提供。</summary>
        /// <summary>配置面板 hover 时右上角显示的额外词条 hovertip 列表。顺序与开关描述文本一致（自上而下）：原版房间词条（精英/敌人）在前、效果词条在后、效果子词条（如隐者逆带的格挡）最后。</summary>
        private static IHoverTip[] ExtraTipsForConfigFlag(int flag) => flag switch
        {
            0 => new IHoverTip[] { TarotPackTip() },   // 愚者：塔罗包
            1 => new IHoverTip[] { TarotPackTip() },   // 魔术师：塔罗包
            2 => new IHoverTip[] { TarotPackTip() },   // 女祭司：塔罗包
            3 => new IHoverTip[] { TarotPackTip() },   // 皇后：塔罗包
            4 => new IHoverTip[] { TarotPackTip() },   // 皇帝：塔罗包
            5 => new IHoverTip[] { TarotPackTip() },   // 教皇：塔罗包
            6 => new IHoverTip[] { RoomTip("ROOM_ELITE") },   // 恋人：精英
            7 => new IHoverTip[] { RoomTip("ROOM_ELITE"), HoverTipFactory.FromPower<VulnerablePower>() },   // 战车：精英、易伤
            8 => new IHoverTip[] { RoomTip("ROOM_ELITE"), HoverTipFactory.FromPower<WeakPower>() },   // 力量：精英、虚弱
            9 => new IHoverTip[] { RoomTip("ROOM_ELITE"), HoverTipFactory.FromPower<TarHermitReversedPower>(), HoverTipFactory.Static(StaticHoverTip.Block) },   // 隐者：精英、隐者逆、格挡
            11 => new IHoverTip[] { RoomTip("ROOM_ENEMY"), HoverTipFactory.FromKeyword(CardKeyword.Exhaust) },   // 正义：敌人、消耗
            12 => new IHoverTip[] { RoomTip("ROOM_ENEMY"), HoverTipFactory.FromKeyword(CardKeyword.Exhaust) },   // 倒吊人：敌人、消耗
            13 => new IHoverTip[] { RoomTip("ROOM_ENEMY") },   // 死神：敌人
            14 => new IHoverTip[] { HoverTipFactory.FromPower<TarTemperanceReversedPower>() },   // 节制：节制-逆
            16 => new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust), HoverTipFactory.FromCard<AscendersBane>() },   // 高塔：消耗、进阶之灾
            _ => Array.Empty<IHoverTip>(),
        };

        /// <summary>游戏原版房间类型词条（static_hover_tips 表，如 ROOM_ELITE 精英 / ROOM_ENEMY 敌人）。</summary>
        private static IHoverTip RoomTip(string prefix)
            => new HoverTip(
                new LocString("static_hover_tips", prefix + ".title"),
                new LocString("static_hover_tips", prefix + ".description"));

        /// <summary>「塔罗包」信息词条（愚者~教皇 6 个开关 hover 时右上角显示）。内容随开关状态动态变化，
        /// 用 SmartFormat 条件语法（{BoolVar:true值|false值}）在本地化文本里做判断：
        /// 愚者未开→（商店中未启用）；魔术师→抽取 3/1；教皇→价格 75~100/175~200；女祭司→涨幅 0/+50。</summary>
        private static IHoverTip TarotPackTip()
        {
            ConfigFloatingWindowLoc.Inject();   // 幂等：确保 BAL_CFW_TAROT_PACK_* 键已注入
            var desc = new LocString("gameplay_ui", "BAL_CFW_TAROT_PACK_DESC");
            desc.Add("FoolOn", ConfigFloatingWindowRunData.GetTarFlag(0));
            desc.Add("MagicianOn", ConfigFloatingWindowRunData.GetTarFlag(1));
            desc.Add("HierophantOn", ConfigFloatingWindowRunData.GetTarFlag(5));
            desc.Add("PriestessOn", ConfigFloatingWindowRunData.GetTarFlag(2));
            return new HoverTip(
                new LocString("gameplay_ui", "BAL_CFW_TAROT_PACK_TITLE"),
                desc);
        }

        /// <summary>鼠标移到有额外词条 hovertip 的开关上：从配置面板右上角顶点开始、向右下方向创建额外词条 hovertip（易伤/虚弱/隐者逆/节制-逆/消耗）。
        /// 面板已在 tscn 左右对称预留固定边距（约 380px，略大于 hovertip 宽度），SetAlignment(Right) 不会因超屏被拉偏。</summary>
        private void ShowRightTopExtras(int flag, Control owner)
        {
            var extras = ExtraTipsForConfigFlag(flag);
            if (extras.Length == 0 || _panel == null) return;

            NHoverTipSet.Remove(owner);   // 防御：同 owner 旧 tip 先清（重复 hover 覆盖）
            var tip = NHoverTipSet.CreateAndShow(owner, extras);
            if (tip == null) return;

            // 布局完成后对齐到面板右上角（否则默认停在屏幕左上角），tip 垂直向右下排列；再往右移 5px
            var panel = _panel;
            Callable.From(() =>
            {
                if (GodotObject.IsInstanceValid(tip) && GodotObject.IsInstanceValid(panel))
                {
                    tip.SetAlignment(panel, HoverTipAlignment.Right);
                    tip.GlobalPosition += new Vector2(10f, 0f);
                }
            }).CallDeferred();
        }

        private void HideRightTopExtras(Control owner) => NHoverTipSet.Remove(owner);

        private void ScaleTo(Control target, float scale)
        {
            target.PivotOffset = target.Size / 2f;
            target.CreateTween()
                .TweenProperty(target, "scale", Vector2.One * scale, 0.1f)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }

        private void ApplyToggleVisual(Control btn, bool isOn)
        {
            // 背景色全部去掉：Bg 设为透明，只留图标
            var bg = btn.GetNodeOrNull<Panel>("Bg");
            if (bg != null)
            {
                bg.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0) });
            }

            btn.Modulate = GetBaseModulate(isOn);
        }

        private Color GetBaseModulate(bool isOn)
        {
            // 打开：正常亮度；未打开：70% 亮度 + 70% 不透明度
            Color mod = isOn ? Colors.White : new Color(0.7f, 0.7f, 0.7f, 0.7f);
            // 只读：整体再压暗
            if (!_editable)
                mod = new Color(mod.R * 0.65f, mod.G * 0.65f, mod.B * 0.65f, mod.A * 0.9f);
            return mod;
        }

        /// <summary>塔罗总开关动画：开启时右侧 22 项从 0→21 依次上滑淡入，关闭时依次下滑淡出并隐藏禁用。</summary>
        private void AnimateRightButtons(bool show)
        {
            // 快速开关时先终止上一轮动画，避免 tween 叠加
            foreach (var tw in _rightTweens)
                if (GodotObject.IsInstanceValid(tw)) tw.Kill();
            _rightTweens.Clear();

            _rightGrid.Visible = true;
            if (show)
            {
                // 网格刚显示，等一帧布局完成后 Position 才有效
                var tree = GetTree();
                void Start()
                {
                    tree.ProcessFrame -= Start;
                    if (!GodotObject.IsInstanceValid(this)) return;
                    PlayRightButtonsShow();
                }
                tree.ProcessFrame += Start;
            }
            else
            {
                PlayRightButtonsHide();
            }
        }

        private void PlayRightButtonsShow()
        {
            int n = _rightButtons.Count;
            const float stagger = 0.045f;
            for (int i = 0; i < n; i++)
            {
                var btn = _rightButtons[i];
                if (btn == null || !GodotObject.IsInstanceValid(btn)) continue;
                var target = GetBaseModulate(_toggleState[btn]);
                btn.Visible = true;
                btn.MouseFilter = Control.MouseFilterEnum.Stop;
                // 只读（客机/局内）不启用：动画回调会覆盖 Open 里的 Disable，导致只读仍可点击切换
                if (_editable && btn is NButton nb) nb.Enable();
                btn.PivotOffset = btn.Size / 2f;
                btn.Scale = Vector2.One * 0.92f;
                btn.Modulate = new Color(target.R, target.G, target.B, 0f);
                var t = btn.CreateTween();
                _rightTweens.Add(t);
                t.TweenInterval(i * stagger);
                t.Parallel().TweenProperty(btn, "modulate", target, 0.32f)
                    .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
                t.Parallel().TweenProperty(btn, "scale", Vector2.One, 0.35f)
                    .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            }
        }

        private void PlayRightButtonsHide()
        {
            int n = _rightButtons.Count;
            const float stagger = 0.04f;
            for (int i = 0; i < n; i++)
            {
                var btn = _rightButtons[i];
                if (btn == null || !GodotObject.IsInstanceValid(btn)) continue;
                // 动画一开始就屏蔽 hover，避免淡出中被鼠标点亮
                btn.MouseFilter = Control.MouseFilterEnum.Ignore;
                btn.PivotOffset = btn.Size / 2f;
                var t = btn.CreateTween();
                _rightTweens.Add(t);
                t.TweenInterval((n - 1 - i) * stagger);
                t.Parallel().TweenProperty(btn, "modulate:a", 0f, 0.2f)
                    .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                t.Parallel().TweenProperty(btn, "scale", Vector2.One * 0.92f, 0.24f)
                    .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                // 不逐按钮隐藏（避免 GridContainer 重排错位）；也不隐藏网格（保持占位，避免左侧大按钮被排版到中间）
                t.Chain().TweenCallback(Callable.From(() =>
                {
                    if (btn is NButton nb) nb.Disable();
                }));
            }
        }

        // ── Open / Close ─────────────────────────────────────────
        /// <summary>
        /// 从本局配置（RunData）重新读取并刷新显示。
        /// 多人下客机收到主机广播（ConfigFloatingWindowDataMessage）后由门面调用，实现实时跟随。
        /// </summary>
        public void RefreshFromRunData()
        {
            // 先终止进行中的右侧显隐动画，避免与本次刷新竞争（快速广播下 tween 叠加/卡在中间态）
            foreach (var tw in _rightTweens)
                if (GodotObject.IsInstanceValid(tw)) tw.Kill();
            _rightTweens.Clear();

            // 右侧难度按钮
            foreach (var kv in _rightButtonIndices)
            {
                bool v = ConfigFloatingWindowRunData.GetTarFlag(kv.Value);
                _toggleState[kv.Key] = v;
                ApplyToggleVisual(kv.Key, v);
            }

            // 左侧塔罗大按钮
            if (_tarotBtn != null)
            {
                bool v = ConfigFloatingWindowRunData.TarotEnabled;
                _toggleState[_tarotBtn] = v;
                ApplyToggleVisual(_tarotBtn, v);
                ApplyBigButtonIcon(_tarotBtn, v, TarotIcon, TarotUpIcon);

                // 塔罗总开关：控制右侧显隐（不隐藏网格，保持占位避免左侧大按钮居中）
                if (v) AnimateRightButtons(true);
                else
                {
                    foreach (var b in _rightButtons)
                    {
                        b.MouseFilter = Control.MouseFilterEnum.Ignore;
                        if (b is NButton nb) nb.Disable();
                        b.Modulate = new Color(b.Modulate.R, b.Modulate.G, b.Modulate.B, 0f);
                    }
                }
            }

            // 左侧星球大按钮
            if (_planetBtn != null)
            {
                bool v = ConfigFloatingWindowRunData.PlanetEnabled;
                _toggleState[_planetBtn] = v;
                ApplyToggleVisual(_planetBtn, v);
                ApplyBigButtonIcon(_planetBtn, v, PlanetIcon, PlanetUpIcon);
            }

            // 只读模式下禁止交互（不覆盖 editable 时的启用状态）
            if (!_editable)
            {
                foreach (var b in _toggleButtons)
                    if (b is NButton nb) nb.Disable();
            }
        }

        /// <summary>打开面板。remote=true 客户端只读（跟随主机）；editable=true 允许编辑并即时写 JSON。</summary>
        public void Open(bool remote, bool editable)
        {
            _remoteMode = remote;
            _editable = editable;
            Visible = true;
            MouseFilter = Control.MouseFilterEnum.Stop;

            if (remote)
            {
                _backstopHit.MouseFilter = Control.MouseFilterEnum.Ignore;
            }

            // 按 editable 统一设置交互与视觉
            foreach (var btn in _toggleButtons)
            {
                if (btn is NButton nb)
                {
                    if (editable) nb.Enable();
                    else nb.Disable();
                }
                ApplyToggleVisual(btn, _toggleState[btn]);
            }

            // 塔罗总开关关闭时，右侧保持占位但不可见、不可交互、无 hover
            if (!ConfigFloatingWindowRunData.TarotEnabled)
            {
                foreach (var b in _rightButtons)
                {
                    b.MouseFilter = Control.MouseFilterEnum.Ignore;
                    if (b is NButton nb) nb.Disable();
                    b.Modulate = new Color(b.Modulate.R, b.Modulate.G, b.Modulate.B, 0f);
                }
            }

            _openTween?.Kill();
            _openTween = CreateTween().SetParallel();
            _openTween.TweenProperty(_backstop, "modulate:a", 0.9f, 0.25);
            _openTween.TweenProperty(this, "modulate:a", 1f, 0.25)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo).From(0f);

            ActiveScreenContext.Instance.Update();
            if (!remote)
            {
                NHotkeyManager.Instance!.AddBlockingScreen(this);
                NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.cancel, CloseLocal);
                NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.pauseAndBack, CloseLocal);
            }
        }

        /// <summary>关闭面板（动画淡出 + 释放）。淡出期间重复调用直接返回，防二次 QueueFree/重复移除热键。</summary>
        public void Close()
        {
            if (_closing || !Visible) return;
            _closing = true;

            MouseFilter = Control.MouseFilterEnum.Ignore;
            _backstopHit.MouseFilter = Control.MouseFilterEnum.Ignore;
            if (!_remoteMode)
            {
                NHotkeyManager.Instance!.RemoveHotkeyPressedBinding(MegaInput.cancel, CloseLocal);
                NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.pauseAndBack, CloseLocal);
                NHotkeyManager.Instance.RemoveBlockingScreen(this);
            }

            _openTween?.Kill();
            _openTween = CreateTween().SetParallel();
            _openTween.TweenProperty(_backstop, "modulate:a", 0f, 0.25);
            _openTween.TweenProperty(this, "modulate:a", 0f, 0.1);
            _openTween.Chain().TweenCallback(Callable.From(() =>
            {
                Visible = false;
                ActiveScreenContext.Instance.Update();
                QueueFree();
            }));
        }

        private void CloseLocal()
        {
            ConfigFloatingWindow.ClosePanel();
        }
    }
}
