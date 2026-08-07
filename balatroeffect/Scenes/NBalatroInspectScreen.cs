#nullable enable

using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;	using MegaCrit.Sts2.Core.Localization.Fonts;using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace PengoTarot.BalatroEffect;

/// <summary>
/// Custom inspect screen with left 2/3 card display + right 1/3 Balatro effect debug panel.
/// Activated via a small entry button on the original NInspectCardScreen.
/// </summary>
public partial class NBalatroInspectScreen : Control, IScreenContext
{
	// ── Node names ──────────────────────────────────────────────
	private const string SliderName = "BalatroEffectsSlider";
	private const string AuthorPresetPath = "res://balatroeffect/Assets/author_preset.json";


	// ── Scene paths ─────────────────────────────────────────────
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
	private Control _cardArea = null!;
	private Control _panelArea = null!;
	private NSlider _slider = null!;
	private NButton _exportBtn = null!;
	private NButton _importBtn = null!;
	private NButton _menuBtn = null!;
	private PopupMenu _menuPopup = null!;
	private Control _enchantTickbox = null!;
	private Control _enchantTickVisuals = null!;
	/// <summary>切到附魔屏时置 true：Close 不返回原版检查界面。</summary>
	private bool _suppressOriginReturn;
	/// <summary>悬停缩放动画，避免快速进出时 tween 互相干扰。</summary>
	private readonly Dictionary<Control, Tween> _hoverTweens = new();
	// ── State ───────────────────────────────────────────────────
	private List<CardModel>? _cards;
	/// <summary>当前检查界面显示的卡列表（左右箭头遍历用），供批量操作读取。</summary>
	internal List<CardModel>? CurrentCards => _cards;
	private int _index;
	private bool _viewAllUpgraded;
	private Vector2 _cardPosition;
	private float _leftButtonX;
	private float _rightButtonX;
	private Tween? _openTween;
	private Tween? _cardTween;

	// References to the original inspect for returning
	private NInspectCardScreen? _originInspect;

	public static string[] AssetPaths => new[] { CardScenePath };

	public Control? DefaultFocusedControl => null;

	// ── Static helpers ──────────────────────────────────────────
	private static string Tr(string key) =>
		new LocString("gameplay_ui", "BAL_" + key).GetFormattedText() ?? key;

	public static NBalatroInspectScreen? Create()
	{
		var scene = GD.Load<PackedScene>("res://balatroeffect/Scenes/balatro_inspect_screen.tscn");
		return scene.Instantiate<NBalatroInspectScreen>(PackedScene.GenEditState.Disabled);
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
		// Backstop（全屏黑色视觉垫底，Ignore 鼠标不挡交互）
		_backstop = GetNode<ColorRect>("Backstop");
		// BackstopHit（左 2/3 透明交互层）：点击空白关闭，右侧面板不受影响
		GetNode<NButton>("BackstopHit").Connect(NClickableControl.SignalName.Released,
			Callable.From<NButton>(_ => Close()));

		// Card area
		_cardArea = GetNode<Control>("%CardArea");
		_hoverTipRect = GetNode<Control>("%HoverTipRect");

		// Card (instantiated from card.tscn and added to CardAnchor)
		var cardAnchor = GetNode<Control>("%CardAnchor");
		var cardScene = GD.Load<PackedScene>(CardScenePath);
		_card = cardScene.Instantiate<NCard>(PackedScene.GenEditState.Disabled);
		_card.Name = "Card";
		_card.SetAnchorsPreset(LayoutPreset.Center);
		_card.Scale = Vector2.One * 2f;
		_cardPosition = _card.Position;
		cardAnchor.AddChild(_card);

		// Arrows
		_leftButton = GetNode<NButton>("%LeftArrow");
		_leftButtonX = _leftButton.Position.X;
		_leftButton.Connect(NClickableControl.SignalName.Released,
			Callable.From<NButton>(_ => OnLeftButtonReleased()));
		AddHoverScale(_leftButton, _leftButton);

		_rightButton = GetNode<NButton>("%RightArrow");
		_rightButtonX = _rightButton.Position.X;
		_rightButton.Connect(NClickableControl.SignalName.Released,
			Callable.From<NButton>(_ => OnRightButtonReleased()));
		AddHoverScale(_rightButton, _rightButton);

		// 附魔勾选框（默认未勾选；勾选 → 进入附魔检查界面）
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
		SetTickState(_enchantTickVisuals, false);

		// Panel area
		_panelArea = GetNode<Control>("%PanelArea");

		// Panel VBox - set fonts and localized text on header labels
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

		// Part rows — 整行可点：点击行内任意位置（勾选框/文字/空白）toggle；hover 整行放大勾选框
		var partsGrid = panelVBox.GetNode<GridContainer>("PartsGrid");
		foreach (var child in partsGrid.GetChildren())
		{
			if (child is HBoxContainer row && row.Name.ToString().StartsWith("Row_"))
			{
				string partKey = row.Name.ToString().Substring(4); // "Portrait" etc.
				var label = row.GetNodeOrNull<Label>("Label");
				if (label != null)
					label.Text = Tr("PART_" + partKey.ToUpperInvariant());

				// Find the tickbox container in this row ("TickPortrait", etc.)
				Control? tick = null;
				foreach (var rc in row.GetChildren())
					if (rc is Control c && c.Name.ToString().StartsWith("Tick"))
						{ tick = c; break; }

				if (tick != null)
				{
					string partName = partKey; // preserve for closure
					var hv = tick.GetNodeOrNull<Control>("TickboxVisuals");

					// 整行可点：点击行内任意位置 → OnPartClicked（按编辑模式分派；图标由 RefreshPanelState 刷新）
					row.MouseFilter = Control.MouseFilterEnum.Stop;
					row.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(evt =>
					{
						if (evt is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
							InspectScreen.OnPartClicked(partName);
					}));

					// 子节点不再拦截鼠标（避免重复触发），统一交给 Row
					tick.MouseFilter = Control.MouseFilterEnum.Ignore;
					if (label != null) label.MouseFilter = Control.MouseFilterEnum.Ignore;

					// Hover：鼠标在整行任意位置时，仅勾选框放大，文本不变
					if (hv != null)
						AddHoverScale(row, hv, 1.15f);
				}
			}
		}

		// Slider (inside SliderCenter HBoxContainer, 0.618 width via stretch_ratio)
		_slider = GetNode<NSlider>("%SliderCenter/Slider");
		_slider.Name = SliderName;
		_slider.ValueChanged += v => Config.SetIntensity(InspectScreen.CurrentCardId, (float)v / 100.0);
		// 仅放大可拖动的手柄（Handle），轨道/滑动条本身不放大
		if (_slider.GetNodeOrNull<Control>("%Handle") is Control handle)
			AddHoverScale(_slider, handle, 1.15f);

		// Buttons (pre-built in scene)
		_exportBtn = GetNode<NButton>("%ExportBtn");
		_exportBtn.GetNode<Label>("Label").Text = Tr("BTN_COPY_CARD");
		_exportBtn.Connect(NClickableControl.SignalName.Released,
			Callable.From<NButton>(_ => InspectScreen.OnCopyCard()));
		AddHoverScale(_exportBtn, _exportBtn);

		_importBtn = GetNode<NButton>("%ImportBtn");
		_importBtn.GetNode<Label>("Label").Text = Tr("BTN_PASTE_CARD");
		_importBtn.Connect(NClickableControl.SignalName.Released,
			Callable.From<NButton>(_ => InspectScreen.OnPasteCard()));
		AddHoverScale(_importBtn, _importBtn);

		_menuBtn = GetNode<NButton>("%MenuBtnHolder");
		_menuBtn.GetNode<Label>("Label").Text = Tr("MENU_TITLE");
		AddHoverScale(_menuBtn, _menuBtn);
		_menuPopup = new PopupMenu { Name = "MenuPopup" };
		_menuPopup.AddThemeFontSizeOverride("font_size", 28);
		_menuPopup.AddThemeColorOverride("font_disabled_color", new Color(0.35f, 0.35f, 0.35f)); // 无效项更深的颜色
		_menuPopup.AddItem(Tr("MENU_CLEAR"), 0);
		_menuPopup.AddItem(Tr("MENU_RESET"), 1);
		_menuPopup.AddItem(Tr("MENU_APPLY_TO_VISIBLE"), 2);
		_menuPopup.AddItem(Tr("MENU_EXPORT_GLOBAL"), 3);
		_menuPopup.AddItem(Tr("MENU_IMPORT_GLOBAL"), 4);
		_menuPopup.AddItem(Tr("MENU_LOAD_AUTHOR_GLOBAL"), 5);
		_menuPopup.Connect(PopupMenu.SignalName.IdPressed,
			Callable.From<int>(id => InspectScreen.OnMenuAction(id)));
		_menuBtn.AddChild(_menuPopup);
		if (LocaleFontUtil.GetLocaleFont(FontType.Regular) is Font locFont)
			_menuPopup.AddThemeFontOverride("font", locFont);
		_menuBtn.Connect(NClickableControl.SignalName.Released,
			Callable.From<NButton>(_ =>
			{
				UpdateMenuAvailability();
				var gpos = _menuBtn.GlobalPosition;
				var gsize = _menuBtn.Size;
				_menuPopup.Popup(new Rect2I((int)gpos.X, (int)(gpos.Y + gsize.Y), 0, 0));
			}));

		// Paginator - wire scene-embedded buttons directly (no C# node creation)
		var pgLeftBtn = GetNode<NButton>("%PgLeftBtn");
		var pgRightBtn = GetNode<NButton>("%PgRightBtn");
		var pgLabel = GetNode<Label>("%PgLabel");
		pgLeftBtn.Connect(NClickableControl.SignalName.Released,
			Callable.From<NButton>(_ => InspectScreen.OnPaginatorNavigate(-1)));
		pgRightBtn.Connect(NClickableControl.SignalName.Released,
			Callable.From<NButton>(_ => InspectScreen.OnPaginatorNavigate(1)));
		AddHoverScale(pgLeftBtn, pgLeftBtn);
		AddHoverScale(pgRightBtn, pgRightBtn);
		InspectScreen.InitPaginator(pgLabel);

		// Edit mode buttons (Normal | Separately | Fully)
		WireModeButton("%NormalModeBtn", Config.ModeNormal);
		WireModeButton("%SeparatelyModeBtn", Config.ModeSeparately);
		WireModeButton("%FullyModeBtn", Config.ModeFully);

		// 背景星云（全屏垫底）：按实际尺寸设 uv_scale 防拉伸
		ShaderController.ApplyStarcloudBgAspect(GetNodeOrNull<ColorRect>("CardAreaBg"));

		// Hide initially until Open() is called
		Visible = false;
		_leftButton.Disable();
		_rightButton.Disable();
	}

	// ── Hover scale（参考入口按钮悬停放大；无淡出效果） ──────────
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

	// ── 按钮/菜单可用性 ─────────────────────────────────────────
	private static void UpdateButtonAvailability(NButton btn, bool valid)
	{
		// 无效：饱和度降至 0.1、亮度降低一点；有效：恢复原样
		btn.Modulate = valid ? Colors.White : Color.FromHsv(0f, 0.1f, 0.72f);
	}

	private void UpdateMenuAvailability()
	{
		bool authorExists = Godot.FileAccess.FileExists(AuthorPresetPath);
		string clipboard = DisplayServer.ClipboardGet() ?? "";
		_menuPopup.SetItemDisabled(0, !Config.HasEffect(InspectScreen.CurrentCardId)); // 清空当前卡
		_menuPopup.SetItemDisabled(1, !authorExists);                                  // 恢复作者预设
		_menuPopup.SetItemDisabled(2, CurrentCards == null || CurrentCards.Count == 0);// 应用到当前列表
		_menuPopup.SetItemDisabled(3, !Config.HasAnyEffect());                         // 导出全局
		_menuPopup.SetItemDisabled(4, !Config.IsValidPresetJson(clipboard));           // 导入全局
		_menuPopup.SetItemDisabled(5, !authorExists);                                  // 作者全局预设
	}

	// ── Open / Close ────────────────────────────────────────────
	public void Open(List<CardModel> cards, int index, bool viewAllUpgraded = false,
		NInspectCardScreen? origin = null)
	{
		_cards = cards;
		_originInspect = origin;
		_viewAllUpgraded = viewAllUpgraded;

		Visible = true;
		MouseFilter = Control.MouseFilterEnum.Stop;

		SetCard(index);

		// Animate in
		_card.Scale = Vector2.One * 1.75f;
		_card.Modulate = StsColors.transparentBlack;
		_leftButton.Modulate = StsColors.transparentBlack;
		_rightButton.Modulate = StsColors.transparentBlack;

		_openTween?.Kill();
		_openTween = CreateTween().SetParallel();
		_openTween.TweenProperty(_backstop, "modulate:a", 0.9f, 0.25);
		_openTween.TweenProperty(this, "modulate:a", 1f, 0.25)
			.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo).From(0f);
		_openTween.TweenProperty(_leftButton, "position:x", _leftButtonX, 0.25)
			.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_leftButtonX + 100f).SetDelay(0.1);
		_openTween.TweenProperty(_leftButton, "modulate", Colors.White, 0.25).SetDelay(0.1);
		_openTween.TweenProperty(_rightButton, "position:x", _rightButtonX, 0.25)
			.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_rightButtonX - 100f).SetDelay(0.1);
		_openTween.TweenProperty(_rightButton, "modulate", Colors.White, 0.25).SetDelay(0.1);

		_cardTween?.Kill();
		_cardTween = CreateTween().SetParallel();
		_cardTween.TweenProperty(_card, "modulate", Colors.White, 0.25);
		_cardTween.TweenProperty(_card, "scale", Vector2.One * 2f, 0.15)
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

		_openTween?.Kill();
		_openTween = CreateTween().SetParallel();
		_openTween.TweenProperty(_backstop, "modulate:a", 0f, 0.25);
		_openTween.TweenProperty(_leftButton, "modulate:a", 0f, 0.1);
		_openTween.TweenProperty(_rightButton, "modulate:a", 0f, 0.1);
		_openTween.TweenProperty(_card, "modulate", StsColors.transparentWhite, 0.1);
		_openTween.Chain().TweenCallback(Callable.From(() =>
		{
			Visible = false;
			ActiveScreenContext.Instance.Update();

			// Return to original inspect with current card index（切到附魔屏时抑制）
			if (!_suppressOriginReturn && _originInspect != null && _cards != null)
			{
				_originInspect.Open(_cards, _index, _viewAllUpgraded);
			}

			QueueFree();
		}));

		NHotkeyManager.Instance!.RemoveHotkeyPressedBinding(MegaInput.cancel, Close);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.pauseAndBack, Close);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.left, OnLeftButtonReleased);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.right, OnRightButtonReleased);
		NHotkeyManager.Instance.RemoveBlockingScreen(this);
	}

	// ── Card navigation ─────────────────────────────────────────
	private void OnRightButtonReleased()
	{
		if (_rightButton.Visible && _cards != null)
		{
			SetCard(_index + 1);
			_card.Modulate = Colors.White;
			_openTween?.Kill();
			_openTween = CreateTween().SetParallel();
			_openTween.TweenProperty(_card, "position", _cardPosition, 0.25)
				.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(_cardPosition + new Vector2(100f, 0f));
		}
	}

	private void OnLeftButtonReleased()
	{
		if (_leftButton.Visible && _cards != null)
		{
			SetCard(_index - 1);
			_card.Modulate = Colors.White;
			_openTween?.Kill();
			_openTween = CreateTween().SetParallel();
			_openTween.TweenProperty(_card, "position", _cardPosition, 0.25)
				.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(_cardPosition + new Vector2(-100f, 0f));
		}
	}

	// ── 查看附魔特效 ────────────────────────────────────────────
	private void OnEnchantTickClicked()
	{
		SetTickState(_enchantTickVisuals, true);
		SwitchToEnchant();
	}

	/// <summary>进入附魔检查界面：关闭自身（抑制返回原版），带上下文打开附魔屏。</summary>
	private void SwitchToEnchant()
	{
		if (_cards == null || _cards.Count == 0)
		{
			SetTickState(_enchantTickVisuals, false);
			return;
		}
		_suppressOriginReturn = true;
		var cards = _cards;
		int index = _index;
		bool viewAllUpgraded = _viewAllUpgraded;
		var origin = _originInspect;
		Close();

		var screen = NBalatroInspectEnchantScreen.Create();
		if (screen != null)
		{
			var tree = Engine.GetMainLoop() as SceneTree;
			tree?.Root.AddChild(screen);
			screen.Open(cards, index, viewAllUpgraded, origin);
		}
	}

	private void SetCard(int index)
	{
		if (_cards == null) return;

		_index = Math.Clamp(index, 0, _cards.Count - 1);

		_leftButton.Visible = _index > 0;
		_leftButton.MouseFilter = _leftButton.Visible ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
		_rightButton.Visible = _index < _cards.Count - 1;
		_rightButton.MouseFilter = _rightButton.Visible ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;

		var model = _cards[_index];
		var displayModel = (CardModel)model.MutableClone();

		if (_viewAllUpgraded && !model.IsUpgraded && model.IsUpgradable)
		{
			displayModel.UpgradePreviewType = CardUpgradePreviewType.Deck;
			displayModel.UpgradeInternal();
			_card.Model = displayModel;
			_card.ShowUpgradePreview();
		}
		else
		{
			_card.Model = displayModel;
			_card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
		}

		NHoverTipSet.Clear();
		NHoverTipSet.CreateAndShow(this, displayModel.HoverTips)
			?.SetAlignment(_hoverTipRect, HoverTip.GetHoverTipAlignment(this));

		// Update InspectScreen state
		InspectScreen.CurrentCardId = model.Id.ToString();
		InspectScreen.UpdatePaginatorTarget(InspectScreen.CurrentCardId);
		// 切卡：Separately 模式默认高亮第一个部件（每张卡独立）
		if (Config.GetEditMode(InspectScreen.CurrentCardId) == Config.ModeSeparately)
			InspectScreen.SeparatelyHighlight = Config.AllPartNames.Length > 0 ? Config.AllPartNames[0] : "";

		// Refresh slider & parts checkboxes
		RefreshPanelState();

		// Apply shader
		ShaderController.ApplyShader(_card);
	}

	public void RefreshPanelState()
	{
		// Slider
		_slider?.SetBlockSignals(true);
		if (_slider != null)
			_slider.Value = Config.GetIntensity(InspectScreen.CurrentCardId) * 100.0;
		_slider?.SetBlockSignals(false);

		// Edit mode 按钮描边高亮
		string editMode = Config.GetEditMode(InspectScreen.CurrentCardId);
		UpdateModeButtonStyles(editMode);

		// Parts tickboxes（勾选态 + 模式相关不透明度，文本与勾选框同步明暗）
		bool separately = editMode == Config.ModeSeparately;
		bool fully = editMode == Config.ModeFully;
		foreach (var part in Config.AllPartNames)
		{
			int effect = Config.GetEffect(InspectScreen.CurrentCardId, part);
			SetTickState("%Tick" + part, effect > 0);

			var visuals = GetNodeOrNull<Control>("%Tick" + part)?.GetNodeOrNull<Control>("TickboxVisuals");
			if (visuals == null) continue;

			bool dimmed;
			if (separately)
				// Separately：高亮 100%，其余 70% 不透明度 + 降 30% 亮度
				dimmed = part != InspectScreen.SeparatelyHighlight;
			else if (fully)
				// Fully：整卡效果，部件行不可用（点击无效）
				dimmed = true;
			else
				dimmed = false;

			visuals.Modulate = dimmed ? new Color(0.7f, 0.7f, 0.7f, 0.7f) : Colors.White;

			// 文本同步变暗，与勾选框一致
			var label = (GetNodeOrNull<Control>("%Tick" + part)?.GetParent() as HBoxContainer)?.GetNodeOrNull<Label>("Label");
			if (label != null)
				label.Modulate = dimmed ? new Color(0.7f, 0.7f, 0.7f, 0.7f) : Colors.White;
		}

		// Copy/Paste 可用性：当前卡无有效配置 → 复制置灰；剪贴板无有效预设 → 粘贴置灰
		UpdateButtonAvailability(_exportBtn, Config.HasEffect(InspectScreen.CurrentCardId));
		UpdateButtonAvailability(_importBtn, Config.IsValidCardPresetJson(DisplayServer.ClipboardGet() ?? ""));
	}

	private void WireModeButton(string nodePath, string mode)
	{
		var label = GetNodeOrNull<Label>(nodePath);
		if (label == null) return;
		label.Text = Tr(ModeLocKey(mode)); // 应用本地化文本，避免显示原始 key
		label.MouseFilter = Control.MouseFilterEnum.Stop;
		label.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(evt =>
		{
			if (evt is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
				InspectScreen.OnEditModeSelected(mode);
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
		// 未选中的模式降到 80% 不透明度 + 降 20% 亮度，区分更明显
		label.Modulate = active ? Colors.White : new Color(0.8f, 0.8f, 0.8f, 0.8f);
		if (active)
		{
			// 带描边字体（同 EffectHeader 样式）
			label.AddThemeColorOverride("font_outline_color", new Color(0.144f, 0.331f, 0.36f, 1f));
			label.AddThemeConstantOverride("outline_size", 12);
		}
		else
		{
			label.RemoveThemeColorOverride("font_outline_color");
			label.RemoveThemeConstantOverride("outline_size");
		}
	}

	private void SetTickState(string nodePath, bool ticked)
	{
		var container = GetNodeOrNull<Control>(nodePath);
		if (container == null) return;
		var visuals = container.GetNodeOrNull<Control>("TickboxVisuals");
		if (visuals == null) return;
		SetTickState(visuals, ticked);
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
