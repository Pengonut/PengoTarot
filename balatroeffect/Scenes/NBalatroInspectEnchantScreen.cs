// PengoTarot: 附魔特效检查界面（独立场景，与卡牌检查界面布局一致）。
// 左右箭头切换「附魔」；为每个附魔用方法1选一张可附魔的干净卡展示；
// 右侧面板直接绑定附魔配置（EnchantmentConfig），与卡牌编辑器完全解耦。
// 更多选项菜单暂时禁用（大部分效果无效）。取消「查看附魔特效」勾选 → 返回卡牌屏。

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace PengoTarot.BalatroEffect;

public partial class NBalatroInspectEnchantScreen : Control, IScreenContext
{
    private const string SliderName = "BalatroEffectsSlider";
    private static readonly string CardScenePath = "res://scenes/cards/card.tscn";

    // ── Fonts ───────────────────────────────────────────────────
    private static Font? _boldFont;
    private static Font BoldFont => _boldFont ??= LocaleFontUtil.GetLocaleFont(FontType.Bold) ?? GD.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres");
    private static Font? _regularFont;
    private static Font RegularFont => _regularFont ??= LocaleFontUtil.GetLocaleFont(FontType.Regular) ?? GD.Load<Font>("res://themes/kreon_regular_shared.tres");

    // ── Nodes ───────────────────────────────────────────────────
    private NCard _card = null!;
    private ColorRect _backstop = null!;
    private Control _hoverTipRect = null!;
    private NButton _leftButton = null!;
    private NButton _rightButton = null!;
    private NSlider _slider = null!;
    private NButton _exportBtn = null!;
    private NButton _importBtn = null!;
    private NButton _menuBtn = null!;
    private PopupMenu _menuPopup = null!;
    private Control _enchantTickbox = null!;
    private Control _enchantTickVisuals = null!;
    private Label _enchantNameLabel = null!;
    private readonly Dictionary<Control, Tween> _hoverTweens = new();

    // ── 附魔浏览状态 ────────────────────────────────────────────
    private List<EnchantmentModel> _enchantments = new();
    private int _index = -1;
    private string _separatelyHighlight = "";
    /// <summary>方法1：每个附魔选定的展示卡（CanEnchant），会话内缓存保证稳定显示。</summary>
    private readonly Dictionary<string, CardModel> _displayCardCache = new();
    private string CurrentEnchantmentId => _index >= 0 && _index < _enchantments.Count
        ? _enchantments[_index].Id.ToString() : "";

    // ── 返回卡牌屏的上下文 ──────────────────────────────────────
    private List<CardModel>? _returnCards;
    private int _returnIndex;
    private bool _returnViewAllUpgraded;
    private NInspectCardScreen? _returnOriginInspect;

    // ── 动画/布局 ───────────────────────────────────────────────
    private Vector2 _cardPosition;
    private float _leftButtonY;
    private float _rightButtonY;

    // ── 附魔序列图标列（卡与面板之间，9 格，选中居中放大、循环滚动） ──
    private const int ListSlotCount = 9;
    private const int ListCenterSlot = 4;
    private static readonly Vector2 ListSlotSize = new(55f, 55f);
    private VBoxContainer _enchantList = null!;
    private readonly List<EnchantSlot> _enchantSlots = new();

    private sealed class EnchantSlot
    {
        public Control Root = null!;
        public TextureRect Icon = null!;
        public int SlotIndex;
    }

    // ── Paginator（本屏自包含，绑定 EnchantmentConfig） ─────────
    private static List<EffectRegistry.EffectDef> _pgEffects = new();
    private static bool _pgInitialized;
    private int _pgCurrentIndex;
    private Label? _pgLabel;

    /// <summary>已打印过“被完全跳过”日志的附魔 id（避免重复打印同一附魔）。</summary>
    private static readonly HashSet<string> _loggedSkippedEnchantments = new();

    public Control? DefaultFocusedControl => null;

    private static string Tr(string key) =>
        new LocString("gameplay_ui", "BAL_" + key).GetFormattedText() ?? key;

    public static NBalatroInspectEnchantScreen? Create()
    {
        var scene = GD.Load<PackedScene>("res://balatroeffect/Scenes/balatro_inspect_enchant_screen.tscn");
        return scene.Instantiate<NBalatroInspectEnchantScreen>(PackedScene.GenEditState.Disabled);
    }

    // ── Godot lifecycle ─────────────────────────────────────────
    public override void _Ready()
    {
        LocExtension.Inject();
        // 先统一给所有文本控件应用语言字体（覆盖 tscn 里的 kreon override 与默认字体），
        // 随后 InitializeFromScene 里 AddThemeFontOverride(Bold/RegularFont) 会用语言 Bold/Regular 再次覆盖
        ApplyLocaleFontsToUi();
        InitializeFromScene();
    }

    /// <summary>给界面所有 Label/RichTextLabel 应用语言替换字体（游戏原版 ApplyLocaleFontSubstitution），消除日式异体字。</summary>
    private void ApplyLocaleFontsToUi()
    {
        foreach (Node child in FindChildren("*", "Label", owned: false, recursive: true))
            if (child is Label label)
                label.ApplyLocaleFontSubstitution(FontType.Regular, "font");
        foreach (Node child in FindChildren("*", "RichTextLabel", owned: false, recursive: true))
            if (child is RichTextLabel rtl)
                rtl.ApplyLocaleFontSubstitution(FontType.Regular, "normal_font");
    }

    private void InitializeFromScene()
    {
        _backstop = GetNode<ColorRect>("Backstop");
        GetNode<NButton>("BackstopHit").Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => Close()));

        // Card（与卡牌屏一致：居中、2x）
        var cardAnchor = GetNode<Control>("%CardAnchor");
        var cardScene = GD.Load<PackedScene>(CardScenePath);
        _card = cardScene.Instantiate<NCard>(PackedScene.GenEditState.Disabled);
        _card.Name = "Card";
        _card.SetAnchorsPreset(LayoutPreset.Center);
        _card.Scale = Vector2.One * 2f;
        _cardPosition = _card.Position;
        cardAnchor.AddChild(_card);

        _hoverTipRect = GetNode<Control>("%HoverTipRect");

        _leftButton = GetNode<NButton>("%LeftArrow");
        _leftButtonY = _leftButton.Position.Y;
        _leftButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => OnLeftButtonReleased()));
        AddHoverScale(_leftButton, _leftButton);

        _rightButton = GetNode<NButton>("%RightArrow");
        _rightButtonY = _rightButton.Position.Y;
        _rightButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => OnRightButtonReleased()));
        AddHoverScale(_rightButton, _rightButton);

        // 附魔序列图标列（9 格动态生成）
        InitializeEnchantList();

        // 附魔勾选框（附魔屏中默认勾选；点击取消 → 返回卡牌屏）
        _enchantTickbox = GetNode<Control>("%EnchantTickbox");
        _enchantTickVisuals = _enchantTickbox.GetNode<Control>("TickEnchant/TickboxVisuals");
        _enchantTickbox.MouseFilter = Control.MouseFilterEnum.Stop;
        _enchantTickbox.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(evt =>
        {
            if (evt is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
                OnEnchantTickClicked();
        }));
        var tickLabel = _enchantTickbox.GetNode<Label>("Label");
        // 对齐原版「查看升级」：MegaLabel.RefreshFont 用 FontType.Regular（zhs=Noto 黑体）；非 CJK 回退 kreon_bold
        tickLabel.AddThemeFontOverride("font", LocaleFontUtil.GetLocaleFont(FontType.Regular) ?? BoldFont);
        tickLabel.Text = Tr("VIEW_ENCHANTMENTS");
        SetTickState(_enchantTickVisuals, true);

        // 附魔名称标签（tscn 中定义，置于卡左侧）
        _enchantNameLabel = GetNode<Label>("%EnchantNameLabel");
        _enchantNameLabel.Text = "";
        _enchantNameLabel.Visible = false;

        // 面板
        var panelVBox = GetNode<VBoxContainer>("PanelArea/PanelMargin/PanelVBox");
        var effectHeader = panelVBox.GetNode<Label>("EffectHeader");
        effectHeader.AddThemeFontOverride("font", BoldFont);
        effectHeader.Text = Tr("LABEL_EFFECT");
        var intensityHeader = panelVBox.GetNode<Label>("IntensityHeader");
        intensityHeader.AddThemeFontOverride("font", BoldFont);
        intensityHeader.Text = Tr("LABEL_INTENSITY");
        var partsHeader = panelVBox.GetNode<Label>("PartsHeader");
        partsHeader.AddThemeFontOverride("font", BoldFont);
        partsHeader.Text = Tr("LABEL_PARTS");
        var partsHint = panelVBox.GetNode<Label>("PartsHint");
        partsHint.AddThemeFontOverride("font", RegularFont);
        partsHint.Text = Tr("HINT_CHECK_PARTS");

        // 部件行（整行可点）
        var partsGrid = panelVBox.GetNode<GridContainer>("PartsGrid");
        foreach (var child in partsGrid.GetChildren())
        {
            if (child is HBoxContainer row && row.Name.ToString().StartsWith("Row_"))
            {
                string partKey = row.Name.ToString().Substring(4);
                var label = row.GetNodeOrNull<Label>("Label");
                if (label != null) label.Text = Tr("PART_" + partKey.ToUpperInvariant());
                Control? tick = null;
                foreach (var rc in row.GetChildren())
                    if (rc is Control c && c.Name.ToString().StartsWith("Tick")) { tick = c; break; }
                if (tick != null)
                {
                    string partName = partKey;
                    row.MouseFilter = Control.MouseFilterEnum.Stop;
                    row.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(evt =>
                    {
                        if (evt is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
                            OnPartClicked(partName);
                    }));
                    tick.MouseFilter = Control.MouseFilterEnum.Ignore;
                    if (label != null) label.MouseFilter = Control.MouseFilterEnum.Ignore;
                    var hv = tick.GetNodeOrNull<Control>("TickboxVisuals");
                    if (hv != null) AddHoverScale(row, hv, 1.15f);
                }
            }
        }

        // 滑条
        _slider = GetNode<NSlider>("%SliderCenter/Slider");
        _slider.Name = SliderName;
        _slider.ValueChanged += v =>
        {
            string enchId = CurrentEnchantmentId;
            if (enchId != "") EnchantmentConfig.SetIntensity(enchId, (float)v / 100.0);
        };
        if (_slider.GetNodeOrNull<Control>("%Handle") is Control handle)
            AddHoverScale(_slider, handle, 1.15f);

        // 复制 / 粘贴（绑定附魔配置）
        _exportBtn = GetNode<NButton>("%ExportBtn");
        _exportBtn.GetNode<Label>("Label").Text = Tr("BTN_COPY_CARD");
        _exportBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => OnCopyCard()));
        AddHoverScale(_exportBtn, _exportBtn);

        _importBtn = GetNode<NButton>("%ImportBtn");
        _importBtn.GetNode<Label>("Label").Text = Tr("BTN_PASTE_CARD");
        _importBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => OnPasteCard()));
        AddHoverScale(_importBtn, _importBtn);

        // 更多选项：目前只有「清空当前附魔效果」
        _menuBtn = GetNode<NButton>("%MenuBtnHolder");
        _menuBtn.GetNode<Label>("Label").Text = Tr("MENU_TITLE");
        AddHoverScale(_menuBtn, _menuBtn);
        _menuPopup = new PopupMenu { Name = "MenuPopup" };
        _menuPopup.AddThemeFontSizeOverride("font_size", 28);
        _menuPopup.AddThemeColorOverride("font_disabled_color", new Color(0.35f, 0.35f, 0.35f));
        _menuPopup.AddItem(Tr("MENU_CLEAR_ENCHANT"), 0);
        _menuPopup.Connect(PopupMenu.SignalName.IdPressed, Callable.From<int>(OnEnchantMenuAction));
        _menuBtn.AddChild(_menuPopup);
        if (LocaleFontUtil.GetLocaleFont(FontType.Regular) is Font locFont)
            _menuPopup.AddThemeFontOverride("font", locFont);
        _menuBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ =>
        {
            UpdateEnchantMenuAvailability();
            var gpos = _menuBtn.GlobalPosition;
            var gsize = _menuBtn.Size;
            _menuPopup.Popup(new Rect2I((int)gpos.X, (int)(gpos.Y + gsize.Y), 0, 0));
        }));

        // Paginator
        var pgLeftBtn = GetNode<NButton>("%PgLeftBtn");
        var pgRightBtn = GetNode<NButton>("%PgRightBtn");
        var pgLabel = GetNode<Label>("%PgLabel");
        pgLeftBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => OnPaginatorNavigate(-1)));
        pgRightBtn.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => OnPaginatorNavigate(1)));
        AddHoverScale(pgLeftBtn, pgLeftBtn);
        AddHoverScale(pgRightBtn, pgRightBtn);
        InitPaginator(pgLabel);

        // 编辑模式按钮
        WireModeButton("%NormalModeBtn", Config.ModeNormal);
        WireModeButton("%SeparatelyModeBtn", Config.ModeSeparately);
        WireModeButton("%FullyModeBtn", Config.ModeFully);

        // 背景星云（全屏垫底）：按实际尺寸设 uv_scale 防拉伸
        ShaderController.ApplyStarcloudBgAspect(GetNodeOrNull<ColorRect>("CardAreaBg"));

        Visible = false;
        _leftButton.Disable();
        _rightButton.Disable();
    }

    // ── Hover scale ─────────────────────────────────────────────
    private void AddHoverScale(Control trigger, Control target, float scale = 1.04f)
    {
        trigger.MouseEntered += () => HoverScaleTo(target, scale);
        trigger.MouseExited += () => HoverScaleTo(target, 1.0f);
    }

    private void HoverScaleTo(Control target, float scale)
    {
        if (_hoverTweens.TryGetValue(target, out var old) && GodotObject.IsInstanceValid(old))
            old.Kill();
        target.PivotOffset = target.Size / 2f;
        var t = target.CreateTween();
        t.TweenProperty(target, "scale", Vector2.One * scale, 0.1f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _hoverTweens[target] = t;
    }

    // ── Open / Close ────────────────────────────────────────────
    public void Open(List<CardModel>? returnCards, int returnIndex, bool returnViewAllUpgraded,
        NInspectCardScreen? returnOriginInspect)
    {
        _returnCards = returnCards;
        _returnIndex = returnIndex;
        _returnViewAllUpgraded = returnViewAllUpgraded;
        _returnOriginInspect = returnOriginInspect;

        BuildEnchantList();
        if (_separatelyHighlight == "")
            _separatelyHighlight = Config.AllPartNames.Length > 0 ? Config.AllPartNames[0] : "";

        Visible = true;
        MouseFilter = Control.MouseFilterEnum.Stop;

        SetEnchantment(0);

        // Animate in（与卡牌屏一致）
        _card.Scale = Vector2.One * 1.75f;
        _card.Modulate = StsColors.transparentBlack;
        _leftButton.Modulate = StsColors.transparentBlack;
        _rightButton.Modulate = StsColors.transparentBlack;

        var openTween = CreateTween().SetParallel();
        openTween.TweenProperty(_backstop, "modulate:a", 0.9f, 0.25);
        openTween.TweenProperty(this, "modulate:a", 1f, 0.25)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo).From(0f);
        openTween.TweenProperty(_leftButton, "position:y", _leftButtonY, 0.25)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
            .From(_leftButtonY - 100f).SetDelay(0.1);
        openTween.TweenProperty(_leftButton, "modulate", Colors.White, 0.25).SetDelay(0.1);
        openTween.TweenProperty(_rightButton, "position:y", _rightButtonY, 0.25)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
            .From(_rightButtonY + 100f).SetDelay(0.1);
        openTween.TweenProperty(_rightButton, "modulate", Colors.White, 0.25).SetDelay(0.1);

        var cardTween = CreateTween().SetParallel();
        cardTween.TweenProperty(_card, "modulate", Colors.White, 0.25);
        cardTween.TweenProperty(_card, "scale", Vector2.One * 2f, 0.15)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Spring).SetDelay(0.1);

        ActiveScreenContext.Instance.Update();
        NHotkeyManager.Instance!.AddBlockingScreen(this);
        _rightButton.Enable();
        _leftButton.Enable();
        NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.cancel, Close);
        NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.pauseAndBack, Close);
        NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.left, OnLeftButtonReleased);
        NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.right, OnRightButtonReleased);
    }

    public void Close()
    {
        if (!Visible) return;
        MouseFilter = Control.MouseFilterEnum.Ignore;
        _leftButton.MouseFilter = Control.MouseFilterEnum.Ignore;
        _rightButton.MouseFilter = Control.MouseFilterEnum.Ignore;
        _rightButton.Disable();
        _leftButton.Disable();
        NHoverTipSet.Clear();

        // 立即创建并淡入卡牌屏，与本屏淡出交叉进行，避免返回时出现空档闪烁
        ReturnToCardScreen();

        var t = CreateTween().SetParallel();
        t.TweenProperty(_backstop, "modulate:a", 0f, 0.25);
        t.TweenProperty(_leftButton, "modulate:a", 0f, 0.1);
        t.TweenProperty(_rightButton, "modulate:a", 0f, 0.1);
        t.TweenProperty(_card, "modulate", StsColors.transparentWhite, 0.1);
        t.Chain().TweenCallback(Callable.From(() =>
        {
            Visible = false;
            ActiveScreenContext.Instance.Update();
            QueueFree();
        }));

        NHotkeyManager.Instance!.RemoveHotkeyPressedBinding(MegaInput.cancel, Close);
        NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.pauseAndBack, Close);
        NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.left, OnLeftButtonReleased);
        NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.right, OnRightButtonReleased);
        NHotkeyManager.Instance.RemoveBlockingScreen(this);
    }

    /// <summary>返回卡牌检查界面（清理附魔预览 overlay，避免残留到同 id 的真实卡牌）。</summary>
    private void ReturnToCardScreen()
    {
        Config.ClearAllEnchantOverlays();
        if (_returnCards is { Count: > 0 })
        {
            var screen = NBalatroInspectScreen.Create();
            if (screen != null)
            {
                var tree = Engine.GetMainLoop() as SceneTree;
                tree?.Root.AddChild(screen);
                screen.Open(_returnCards, _returnIndex, _returnViewAllUpgraded, _returnOriginInspect);
            }
        }
    }

    // ── 附魔列表构建（方法1） ────────────────────────────────────
    private void BuildEnchantList()
    {
        _enchantments.Clear();
        _displayCardCache.Clear();
        var candidates = new List<EnchantmentModel>();
        foreach (var ench in ModelDb.DebugEnchantments)
        {
            // 排除测试用 mock 附魔
            string ns = ench.GetType().Namespace ?? "";
            if (ns.Contains(".Mocks", StringComparison.Ordinal)) continue;
            var card = FindDisplayCard(ench);
            if (card == null)
            {
                // 完全跳过的附魔：打印一行日志，同一附魔只打一次
                string enchId = ench.Id.ToString();
                if (_loggedSkippedEnchantments.Add(enchId))
                    GD.Print($"[PengoTarot] 附魔 {enchId} 被跳过：找不到任何可附魔的卡牌");
                continue;
            }
            candidates.Add(ench);
            _displayCardCache[ench.Id.ToString()] = card;
        }

        // 稳定排序：原版 → 塔罗主牌(0-21 正逆交替) → 负片(Sub 正逆交替) → 星球 → 其他mod
        _enchantments = candidates
            .Select((e, idx) => (e, idx))
            .OrderBy(t => GetPengoTarotOrderKey(t.e))
            .ThenBy(t => t.idx)
            .Select(t => t.e)
            .ToList();
    }

    // ── 我们mod附魔的排序（原版之后、其他mod之前） ──────────────
    // 塔罗主牌 0-21（命运之轮/高塔无附魔），正逆交替
    private static readonly (string, int)[] TarotMajorOrder = new (string, int)[]
    {
        ("Fool", 0), ("Magician", 1), ("HighPriestess", 2), ("Empress", 3), ("Emperor", 4),
        ("Hierophant", 5), ("Lovers", 6), ("Chariot", 7), ("Strength", 8), ("Hermit", 9),
        ("Justice", 11), ("HangedMan", 12), ("Death", 13), ("Temperance", 14), ("Devil", 15),
        ("Star", 17), ("Moon", 18), ("Sun", 19), ("Judgement", 20), ("World", 21)
    };
    // 负片(Sub)附魔：恶魔/星星/月亮/太阳/世界，正逆交替
    private static readonly (string, int)[] SubOrder = new (string, int)[]
    {
        ("Devil", 0), ("Star", 1), ("Moon", 2), ("Sun", 3), ("World", 4)
    };
    // 星球牌附魔（顺序同 PlanetDeck）
    private static readonly (string, int)[] PlanetOrder = new (string, int)[]
    {
        ("Mercury", 0), ("Venus", 1), ("Earth", 2), ("Mars", 3), ("Jupiter", 4),
        ("Saturn", 5), ("Uranus", 6), ("Neptune", 7), ("Pluto", 8), ("X", 9),
        ("Ceres", 10), ("Eris", 11)
    };

    /// <summary>给附魔算排序键：0=原版，1=塔罗主牌，2=负片，3=星球，4=其他mod。</summary>
    private static int GetPengoTarotOrderKey(EnchantmentModel ench)
    {
        string ns = ench.GetType().Namespace ?? "";
        if (!ns.StartsWith("PengoTarot.", StringComparison.Ordinal)) return 0; // 原版在最前
        string name = ench.GetType().Name;
        bool isSub = name.Contains("Sub", StringComparison.Ordinal);

        if (isSub)
        {
            // 负片(Sub)：恶魔/星星/月亮/太阳/世界，正逆交替
            foreach (var (subName, idx) in SubOrder)
                if (name.StartsWith("Tar" + subName, StringComparison.Ordinal))
                    return 2_000_000 + idx * 2 + (name.Contains("Reversed", StringComparison.Ordinal) ? 1 : 0);
            return 4_000_000;
        }

        // 塔罗主牌 0-21，正逆交替
        foreach (var (majorName, num) in TarotMajorOrder)
            if (name.StartsWith("Tar" + majorName, StringComparison.Ordinal))
                return 1_000_000 + num * 2 + (name.Contains("Reversed", StringComparison.Ordinal) ? 1 : 0);

        // 星球牌附魔
        foreach (var (planet, idx) in PlanetOrder)
            if (name == "Planet" + planet + "Enchantment")
                return 3_000_000 + idx;

        return 4_000_000;
    }

    /// <summary>
    /// 安全调用 <see cref="EnchantmentModel.CanEnchant"/>：canonical 模板卡先 clone 成 mutable 再判定。
    /// 兼容在 CanEnchant 内访问 <c>card.Owner</c> 的第三方 mod（如 HextechRunesSponsorPack 的
    /// CanEnchantPrefix），避免在模板卡上触发 CanonicalModelException。
    /// </summary>
    private static bool SafeCanEnchant(EnchantmentModel ench, CardModel card)
    {
        if (!card.IsCanonical) return ench.CanEnchant(card);
        var clone = (CardModel)card.MutableClone();
        return ench.CanEnchant(clone);
    }

    private CardModel? FindDisplayCard(EnchantmentModel ench)
    {
        // 优先当前切换列表里的卡（卡牌屏上下文）：以当前停留索引为中心交替向两边找最近的可用卡
        var nearest = FindNearestDisplayCard(ench);
        if (nearest != null) return nearest;
        // 兜底：全卡第一张可附魔的
        foreach (var c in ModelDb.AllCards)
            if (SafeCanEnchant(ench, c)) return c;
        return null;
    }

    /// <summary>
    /// 在当前卡牌屏序列 <see cref="_returnCards"/> 中，以 <see cref="_returnIndex"/> 为起点
    /// 向前向后交替寻找（0, +1, -1, +2, -2...）最近的可用展示卡。
    /// </summary>
    private CardModel? FindNearestDisplayCard(EnchantmentModel ench)
    {
        if (_returnCards is not { Count: > 0 }) return null;
        int count = _returnCards.Count;
        int start = Math.Clamp(_returnIndex, 0, count - 1);
        for (int dist = 0; dist < count; dist++)
        {
            if (dist == 0)
            {
                if (SafeCanEnchant(ench, _returnCards[start])) return _returnCards[start];
            }
            else
            {
                int forward = start + dist;
                if (forward < count && SafeCanEnchant(ench, _returnCards[forward]))
                    return _returnCards[forward];
                int backward = start - dist;
                if (backward >= 0 && SafeCanEnchant(ench, _returnCards[backward]))
                    return _returnCards[backward];
            }
        }
        return null;
    }

    // ── 附魔导航 ────────────────────────────────────────────────
    private void SetEnchantment(int index)
    {
        if (_enchantments.Count == 0)
        {
            _index = -1;
            _leftButton.Visible = false; _leftButton.MouseFilter = Control.MouseFilterEnum.Ignore;
            _rightButton.Visible = false; _rightButton.MouseFilter = Control.MouseFilterEnum.Ignore;
            if (_enchantNameLabel != null) _enchantNameLabel.Visible = false;
            return;
        }
        _index = WrapIndex(index);

        // 移除无法显示的附魔（失败即从列表剔除），保证前后导航都顺畅、不再需要二次点击
        int guard = 0;
        while (guard < _enchantments.Count)
        {
            if (TryShowEnchantment(_index))
            {
                _card.Visible = true;
                UpdateArrowVisibility();
                RefreshEnchantList();
                return;
            }
            RemoveEnchantmentAt(_index); // _index 保持不变，指向被移除项的下一个
            if (_enchantments.Count == 0) break;
            if (_index >= _enchantments.Count) _index = 0; // 到尾则从头继续清理剩余失败项
            guard++;
        }

        // 全部无法显示：隐藏卡片与提示
        _index = -1;
        _card.Visible = false;
        if (_enchantNameLabel != null) _enchantNameLabel.Visible = false;
        _leftButton.Visible = false; _leftButton.MouseFilter = Control.MouseFilterEnum.Ignore;
        _rightButton.Visible = false; _rightButton.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    /// <summary>从附魔列表与展示卡缓存中移除指定索引的附魔（无法显示时调用）。</summary>
    private void RemoveEnchantmentAt(int idx)
    {
        if (idx < 0 || idx >= _enchantments.Count) return;
        string id = _enchantments[idx].Id.ToString();
        _enchantments.RemoveAt(idx);
        _displayCardCache.Remove(id);
    }

    private void UpdateArrowVisibility()
    {
        // 循环滚动：只要多于一个附魔，上下箭头始终可用
        bool show = _enchantments.Count > 1;
        _leftButton.Visible = show;
        _leftButton.MouseFilter = show ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
        _rightButton.Visible = show;
        _rightButton.MouseFilter = show ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
    }

    /// <summary>循环取模索引（附魔数 &gt; 0 时）。</summary>
    private int WrapIndex(int i)
    {
        if (_enchantments.Count == 0) return 0;
        return ((i % _enchantments.Count) + _enchantments.Count) % _enchantments.Count;
    }

    /// <summary>动态生成 9 格附魔序列图标列（选中居中放大、距离衰减、点击跳转、悬停放大变亮）。</summary>
    private void InitializeEnchantList()
    {
        _enchantList = GetNode<VBoxContainer>("%EnchantList");
        for (int i = 0; i < ListSlotCount; i++)
        {
            var root = new Control { Name = "Slot" + i };
            root.CustomMinimumSize = ListSlotSize;
            root.MouseFilter = Control.MouseFilterEnum.Stop;
            root.PivotOffset = ListSlotSize / 2f;

            var icon = new TextureRect { Name = "Icon" };
            icon.SetAnchorsPreset(LayoutPreset.FullRect);
            icon.MouseFilter = Control.MouseFilterEnum.Ignore;
            icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            icon.TextureFilter = TextureFilterEnum.Linear;
            root.AddChild(icon);

            var slot = new EnchantSlot { Root = root, Icon = icon, SlotIndex = i };
            int slotIdx = i;
            root.GuiInput += evt =>
            {
                if (evt is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
                    OnListSlotClicked(slotIdx);
            };
            // 悬停：放大 + 恢复全亮（取消变暗/降透明）；离开恢复距离衰减
            root.MouseEntered += () =>
            {
                slot.Root.Modulate = Colors.White;
                HoverScaleTo(slot.Root, 1.12f);
            };
            root.MouseExited += () =>
            {
                if (_hoverTweens.TryGetValue(slot.Root, out var old) && GodotObject.IsInstanceValid(old))
                { old.Kill(); _hoverTweens.Remove(slot.Root); }
                RefreshEnchantList();
            };

            _enchantList.AddChild(root);
            _enchantSlots.Add(slot);
        }
    }

    /// <summary>点击图标列某格 → 跳到对应附魔（选中后会居中），并播放与箭头一致的卡滑动动画与切卡音效。</summary>
    private void OnListSlotClicked(int slotIndex)
    {
        if (_enchantments.Count == 0) return;
        int real = WrapIndex(_index - ListCenterSlot + slotIndex);
        if (real == _index) return; // 点击当前选中格：无反馈
        SetEnchantment(real);
        // 卡滑动动画：点上方图标 → 卡从上方滑入，点下方图标 → 从下方滑入
        _card.Modulate = Colors.White;
        _card.Position = _cardPosition;
        float fromY = slotIndex < ListCenterSlot ? -120f : 120f;
        var t = CreateTween().SetParallel();
        t.TweenProperty(_card, "position", _cardPosition, 0.25)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
            .From(_cardPosition + new Vector2(0f, fromY));
        PlayUiClickSfx();
    }

    /// <summary>刷新图标列：显示窗 = _index-4.._index+4（循环），中间选中 1.05 放大，两侧按距离衰减。</summary>
    private void RefreshEnchantList()
    {
        if (_enchantSlots.Count == 0) return;
        bool visible = _enchantments.Count > 0;
        if (_enchantList != null) _enchantList.Visible = visible;
        if (!visible) return;
        for (int i = 0; i < _enchantSlots.Count; i++)
        {
            var slot = _enchantSlots[i];
            int real = WrapIndex(_index - ListCenterSlot + i);
            slot.Icon.Texture = _enchantments[real].Icon;
            int d = Math.Abs(i - ListCenterSlot);
            bool sel = d == 0;
            // 未选中基准 80% 亮/透明，越远越低（0.8^d）
            float v = sel ? 1f : Mathf.Pow(0.8f, d);
            slot.Root.Modulate = new Color(v, v, v, v);
            slot.Root.Scale = sel ? Vector2.One * 1.05f : Vector2.One;
        }
    }

    /// <summary>尝试显示第 idx 个附魔；成功 true，无展示卡或应用异常 false（调用方自动跳过）。</summary>
    private bool TryShowEnchantment(int idx)
    {
        if (idx < 0 || idx >= _enchantments.Count) return false;
        _index = idx;
        var ench = _enchantments[idx];
        if (!_displayCardCache.TryGetValue(ench.Id.ToString(), out var baseCard) || baseCard == null)
            return false;
        try
        {
            var clone = (CardModel)baseCard.MutableClone();
            var mutableEnch = ench.ToMutable();
            clone.EnchantInternal(mutableEnch, 1m);
            clone.IsEnchantmentPreview = true;

            try
            {
                // 部分附魔（如死亡=免费打出）的 OnEnchant 依赖战斗上下文，在裸克隆上可能抛空引用。
                // 预览时容忍失败：仍展示附魔（图标/特效/名称），只是数值不体现 OnEnchant 效果。
                mutableEnch.ModifyCard();
            }
            catch (Exception e2)
            {
                GD.PrintErr($"[PengoTarot] 附魔 {ench.Id} 预览 ModifyCard 失败（仅影响数值显示）：\n{e2}");
            }

            _card.Model = clone;
            _card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);

            // 附魔名称标签
            UpdateEnchantName(mutableEnch);

            // 面板绑定当前附魔
            UpdatePaginatorTarget(CurrentEnchantmentId);
            RefreshPanelState();

            // 应用附魔特效（overlay 预览模式：仅附魔配置）
            ShaderController.ApplyShader(_card);
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"[PengoTarot] 附魔预览失败 {ench.Id}：\n{e}");
            return false;
        }
    }

    /// <summary>更新附魔名称标签（tscn 中定义，置于卡左侧）。</summary>
    private void UpdateEnchantName(EnchantmentModel ench)
    {
        if (_enchantNameLabel == null) return;
        _enchantNameLabel.Text = ench.Title.GetFormattedText() ?? "";
        _enchantNameLabel.Visible = true;
    }

    private void OnRightButtonReleased()
    {
        if (_rightButton.Visible)
        {
            SetEnchantment(_index + 1);
            _card.Modulate = Colors.White;
            _card.Position = _cardPosition;
            var t = CreateTween().SetParallel();
            t.TweenProperty(_card, "position", _cardPosition, 0.25)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
                .From(_cardPosition + new Vector2(0f, 100f));
        }
    }

    private void OnLeftButtonReleased()
    {
        if (_leftButton.Visible)
        {
            SetEnchantment(_index - 1);
            _card.Modulate = Colors.White;
            _card.Position = _cardPosition;
            var t = CreateTween().SetParallel();
            t.TweenProperty(_card, "position", _cardPosition, 0.25)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
                .From(_cardPosition + new Vector2(0f, -100f));
        }
    }

    /// <summary>播放与 NButton 点击一致的 ui_click 音效。图标列是自定义 Control（无 NButton.OnPress），需手动播放，保证与箭头点击反馈一致。</summary>
    private static void PlayUiClickSfx()
    {
        SfxCmd.Play("event:/sfx/ui/clicks/ui_click");
    }

    private void OnEnchantTickClicked()
    {
        SetTickState(_enchantTickVisuals, false);
        Close();
    }

    /// <summary>更多选项菜单动作：目前只有「清空当前附魔效果」。</summary>
    private void OnEnchantMenuAction(int id)
    {
        string enchId = CurrentEnchantmentId;
        if (enchId == "") return;
        switch (id)
        {
            case 0:
                EnchantmentConfig.ClearEffect(enchId);
                RefreshPanelState();
                if (_card?.Model != null)
                    ShaderController.ApplyShader(_card); // Changed 已触发全局重应用，这里兜底预览卡
                break;
        }
    }

    /// <summary>更新更多选项菜单项可用性（当前附魔无效果时禁用「清空」）。</summary>
    private void UpdateEnchantMenuAvailability()
    {
        string enchId = CurrentEnchantmentId;
        _menuPopup.SetItemDisabled(0, enchId == "" || !EnchantmentConfig.HasEffect(enchId));
    }

    // ── 面板刷新 ────────────────────────────────────────────────
    public void RefreshPanelState()
    {
        string enchId = CurrentEnchantmentId;
        if (enchId == "") return;

        _slider?.SetBlockSignals(true);
        if (_slider != null) _slider.Value = EnchantmentConfig.GetIntensity(enchId) * 100.0;
        _slider?.SetBlockSignals(false);

        string editMode = EnchantmentConfig.GetEditMode(enchId);
        UpdateModeButtonStyles(editMode);

        bool separately = editMode == Config.ModeSeparately;
        bool fully = editMode == Config.ModeFully;
        foreach (var part in Config.AllPartNames)
        {
            int effect = EnchantmentConfig.GetEffect(enchId, part);
            SetTickState(GetNodeOrNull<Control>("%Tick" + part)?.GetNodeOrNull<Control>("TickboxVisuals"), effect > 0);

            var visuals = GetNodeOrNull<Control>("%Tick" + part)?.GetNodeOrNull<Control>("TickboxVisuals");
            if (visuals == null) continue;
            bool dimmed = separately ? part != _separatelyHighlight : fully;
            visuals.Modulate = dimmed ? new Color(0.7f, 0.7f, 0.7f, 0.7f) : Colors.White;
            var label = (GetNodeOrNull<Control>("%Tick" + part)?.GetParent() as HBoxContainer)?.GetNodeOrNull<Label>("Label");
            if (label != null) label.Modulate = dimmed ? new Color(0.7f, 0.7f, 0.7f, 0.7f) : Colors.White;
        }

        UpdateButtonAvailability(_exportBtn, EnchantmentConfig.HasEffect(enchId));
        UpdateButtonAvailability(_importBtn, Config.IsValidCardPresetJson(DisplayServer.ClipboardGet() ?? ""));
    }

    private static void UpdateButtonAvailability(NButton btn, bool valid)
    {
        btn.Modulate = valid ? Colors.White : Color.FromHsv(0f, 0.1f, 0.72f);
    }

    // ── 部件 / 编辑模式 ─────────────────────────────────────────
    private void OnPartClicked(string partName)
    {
        string enchId = CurrentEnchantmentId;
        if (enchId == "") return;
        string cardId = _card.Model?.Id.ToString() ?? "";

        switch (EnchantmentConfig.GetEditMode(enchId))
        {
            case Config.ModeFully:
                return;
            case Config.ModeSeparately:
                bool isChecked = EnchantmentConfig.GetEffect(enchId, partName) > 0;
                if (isChecked && _separatelyHighlight == partName)
                    EnchantmentConfig.ClearEffect(enchId, partName);
                else
                    _separatelyHighlight = partName;
                SetPaginatorToMode(EnchantmentConfig.GetEffect(enchId, _separatelyHighlight));
                RefreshPanelState();
                ShaderController.RefreshAllCardsWithId(cardId);
                return;
            default:
                bool checkedState = EnchantmentConfig.GetEffect(enchId, partName) > 0;
                EnchantmentConfig.SetEffect(enchId, partName, checkedState ? 0 : _pgEffects[_pgCurrentIndex].Mode);
                RefreshPanelState();
                ShaderController.RefreshAllCardsWithId(cardId);
                return;
        }
    }

    private void OnEditModeSelected(string mode)
    {
        string enchId = CurrentEnchantmentId;
        if (enchId == "") return;
        EnchantmentConfig.SetEditMode(enchId, mode);
        if (mode == Config.ModeSeparately)
        {
            _separatelyHighlight = Config.AllPartNames.Length > 0 ? Config.AllPartNames[0] : "";
            SetPaginatorToMode(EnchantmentConfig.GetEffect(enchId, _separatelyHighlight));
        }
        RefreshPanelState();
        ShaderController.RefreshAllCardsWithId(_card.Model?.Id.ToString() ?? "");
    }

    private void OnCopyCard()
    {
        string enchId = CurrentEnchantmentId;
        if (enchId == "") return;
        DisplayServer.ClipboardSet(EnchantmentConfig.ExportCardPreset(enchId));
    }

    private void OnPasteCard()
    {
        string enchId = CurrentEnchantmentId;
        if (enchId == "") return;
        string? clipboard = DisplayServer.ClipboardGet();
        if (!string.IsNullOrEmpty(clipboard) && EnchantmentConfig.ImportCardPreset(enchId, clipboard))
        {
            UpdatePaginatorTarget(enchId);
            RefreshPanelState();
            ShaderController.RefreshAllCardsWithId(_card.Model?.Id.ToString() ?? "");
        }
    }

    // ── Paginator ───────────────────────────────────────────────
    private void InitPaginator(Label label)
    {
        _pgLabel = label;
        if (_pgInitialized) return;
        EffectRegistry.Initialize();
        _pgEffects.Clear();
        foreach (var def in EffectRegistry.AllEffects) _pgEffects.Add(def);
        _pgInitialized = true;
    }

    private void UpdatePaginatorTarget(string enchId)
    {
        int currentMode = EnchantmentConfig.GetCardEffectMode(enchId);
        int idx = _pgEffects.FindIndex(d => d.Mode == currentMode);
        _pgCurrentIndex = idx < 0 ? 0 : idx;
        UpdatePaginatorLabel();
    }

    private void SetPaginatorToMode(int mode)
    {
        int idx = _pgEffects.FindIndex(d => d.Mode == mode);
        if (idx < 0) return;
        _pgCurrentIndex = idx;
        UpdatePaginatorLabel();
    }

    private void OnPaginatorNavigate(int delta)
    {
        string enchId = CurrentEnchantmentId;
        if (enchId == "") return;
        int newIndex = _pgCurrentIndex + delta;
        if (newIndex < 0) newIndex = _pgEffects.Count - 1;
        else if (newIndex >= _pgEffects.Count) newIndex = 0;
        _pgCurrentIndex = newIndex;
        UpdatePaginatorLabel();
        int mode = _pgEffects[_pgCurrentIndex].Mode;
        EnchantmentConfig.SetCardEffectMode(enchId, mode);
        ApplyCurrentEffectToCheckedParts();
        RefreshPanelState();
        ShaderController.RefreshAllCardsWithId(_card.Model?.Id.ToString() ?? "");
    }

    private void UpdatePaginatorLabel()
    {
        if (_pgLabel == null || !_pgInitialized) return;
        string key = "BAL_" + _pgEffects[_pgCurrentIndex].LocKey;
        string? text = LocManager.Instance?.GetTable("gameplay_ui").GetRawText(key);
        _pgLabel.Text = !string.IsNullOrEmpty(text) ? text : _pgEffects[_pgCurrentIndex].LocKey;
    }

    /// <summary>切换 effect 后应用到勾选部件（按编辑模式分派）。</summary>
    private void ApplyCurrentEffectToCheckedParts()
    {
        string enchId = CurrentEnchantmentId;
        if (enchId == "") return;
        int effect = _pgEffects[_pgCurrentIndex].Mode;
        switch (EnchantmentConfig.GetEditMode(enchId))
        {
            case Config.ModeFully:
                EnchantmentConfig.SetEffect(enchId, "FullCard", effect);
                break;
            case Config.ModeSeparately:
                string part = _separatelyHighlight;
                if (string.IsNullOrEmpty(part)) part = Config.AllPartNames.Length > 0 ? Config.AllPartNames[0] : "";
                if (string.IsNullOrEmpty(part)) break;
                EnchantmentConfig.SetEffect(enchId, part, effect);
                break;
            default:
                foreach (var p in Config.AllPartNames)
                    if (EnchantmentConfig.GetEffect(enchId, p) > 0)
                        EnchantmentConfig.SetEffect(enchId, p, effect);
                break;
        }
    }

    // ── 编辑模式按钮样式 ────────────────────────────────────────
    private void WireModeButton(string nodePath, string mode)
    {
        var label = GetNodeOrNull<Label>(nodePath);
        if (label == null) return;
        label.Text = Tr(ModeLocKey(mode));
        label.MouseFilter = Control.MouseFilterEnum.Stop;
        label.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(evt =>
        {
            if (evt is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
                OnEditModeSelected(mode);
        }));
        AddHoverScale(label, label, 1.06f);
    }

    private static string ModeLocKey(string mode) => mode switch
    {
        Config.ModeNormal => "MODE_NORMAL",
        Config.ModeSeparately => "MODE_SEPARATELY",
        _ => "MODE_FULLY"
    };

    private void UpdateModeButtonStyles(string editMode)
    {
        SetModeButtonStyle(GetNodeOrNull<Label>("%NormalModeBtn"), editMode == Config.ModeNormal);
        SetModeButtonStyle(GetNodeOrNull<Label>("%SeparatelyModeBtn"), editMode == Config.ModeSeparately);
        SetModeButtonStyle(GetNodeOrNull<Label>("%FullyModeBtn"), editMode == Config.ModeFully);
    }

    private static void SetModeButtonStyle(Label? label, bool active)
    {
        if (label == null) return;
        label.Modulate = active ? Colors.White : new Color(0.8f, 0.8f, 0.8f, 0.8f);
        if (active)
        {
            label.AddThemeColorOverride("font_outline_color", new Color(0.144f, 0.331f, 0.36f, 1f));
            label.AddThemeConstantOverride("outline_size", 12);
        }
        else
        {
            label.RemoveThemeColorOverride("font_outline_color");
            label.RemoveThemeConstantOverride("outline_size");
        }
    }

    private static void SetTickState(Control? visuals, bool ticked)
    {
        if (visuals == null) return;
        var t = visuals.GetNodeOrNull<Control>("Ticked");
        var nt = visuals.GetNodeOrNull<Control>("NotTicked");
        if (t != null) t.Visible = ticked;
        if (nt != null) nt.Visible = !ticked;
    }
}
