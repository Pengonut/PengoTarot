#nullable enable

using System;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using PengoTarot.Data;

namespace PengoTarot.UI
{
    public partial class NMerchantTarot : NMerchantSlot
    {
        private Sprite2D _visual = null!;
        private Control _costContainer = null!;
        private MerchantTarotEntry _entry = null!;
        private bool _isHovered;
        private Tween? _hoverTween;
        private NMerchantInventory? _merchantRug;
        private bool _ignoreMouseRelease;

        
        private bool _isAnimating;
        private bool _hasPlayedFirstAnimation;   
        private AudioStreamPlayer? _audioPlayer;

        private static readonly Vector2 _hoverScale = Vector2.One * 0.8f;
        private static readonly Vector2 _smallScale = Vector2.One * 0.65f;

        public override MerchantEntry Entry => _entry;
        protected override CanvasItem Visual => _visual;

        public override void _Ready()
        {
            
            int imageIndex = GD.RandRange(1, 4);   
            var texture = ResourceLoader.Load<Texture2D>(
                $"res://images/rooms/merchant_room/TarotEntry{imageIndex}.png");

            _visual = new Sprite2D();
            _visual.Name = "Visual";
            _visual.Scale = new Vector2(0.559943f, 0.559943f);
            _visual.Position = new Vector2(1.53845f, -1.53851f);
            _visual.Texture = texture;

            var shadow = new Sprite2D();
            shadow.Modulate = new Color(0, 0, 0, 0.25f);
            shadow.ShowBehindParent = true;
            shadow.Texture = texture;
            shadow.Offset = new Vector2(16, 16);
            _visual.AddChild(shadow);
            AddChild(_visual);

            
            var costContainer = new HBoxContainer();
            costContainer.Name = "Cost";
            costContainer.LayoutMode = 0;
            costContainer.Alignment = BoxContainer.AlignmentMode.Center;
            costContainer.AddThemeConstantOverride("separation", 6);
            costContainer.Position = new Vector2(-55, 185);
            _costContainer = costContainer;
            AddChild(_costContainer);

            var goldIcon = new TextureRect();
            goldIcon.CustomMinimumSize = new Vector2(54, 54);
            goldIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            goldIcon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            goldIcon.Texture = ResourceLoader.Load<Texture2D>(
                "res://images/atlases/ui_atlas.sprites/top_bar/top_bar_gold.tres");
            _costContainer.AddChild(goldIcon);

            var font = ResourceLoader.Load<FontVariation>(
                "res://themes/kreon_bold_glyph_space_two.tres");
            _costLabel = new MegaLabel();
            _costLabel.Name = "CostLabel";
            _costLabel.AddThemeFontOverride("font", font);
            _costLabel.AddThemeFontSizeOverride("font_size", 39);
            _costLabel.Text = "99";
            _costLabel.AddThemeConstantOverride("line_spacing", 0);
            _costLabel.AddThemeConstantOverride("outline_size", 15);
            _costLabel.AddThemeColorOverride("font_outline_color",
                new Color(0.0666667f, 0, 0, 0.431373f));
            _costLabel.AddThemeConstantOverride("shadow_outline_size", 0);
            _costLabel.AddThemeColorOverride("font_shadow_color",
                new Color(0.223529f, 0.223529f, 0.223529f, 0));
            _costLabel.MinFontSize = 40;
            _costContainer.AddChild(_costLabel);

            
            _hitbox = new NClickableControl();
            _hitbox.LayoutMode = 0;
            _hitbox.Name = "Hitbox";
            _hitbox.SetPosition(new Vector2(-157.0f, -126.0f));
            _hitbox.SetSize(new Vector2(308, 300));
            _hitbox.PivotOffset = new Vector2(56, 56);
            AddChild(_hitbox);

            
            Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
            Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
            _hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnFocus));
            _hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));
            _hitbox.Connect(NClickableControl.SignalName.MousePressed,
                Callable.From<InputEvent>(OnMousePressed));
            _hitbox.Connect(NClickableControl.SignalName.MouseReleased,
                Callable.From<InputEvent>(OnMouseReleased));

            Scale = _smallScale;

            
            _hitbox.MouseFilter = MouseFilterEnum.Ignore;
            _hitbox.SetDeferred("mouse_filter", (int)MouseFilterEnum.Stop);

            _audioPlayer = new AudioStreamPlayer();
            _audioPlayer.Bus = "SFX";
            string audioPath = "res://audio/tarot_open.ogg";
            if (ResourceLoader.Exists(audioPath))
            {
                _audioPlayer.Stream = ResourceLoader.Load<AudioStream>(audioPath);
                _audioPlayer.VolumeDb = 15f;
            }
            else
            {
                GD.PushWarning($"找不到音频: {audioPath}");
            }
            AddChild(_audioPlayer);
        }

        public override void _GuiInput(InputEvent inputEvent)
        {
            if (_isAnimating) return;
            if (inputEvent.IsActionPressed(MegaInput.select))
            {
                AcceptEvent();
                TaskHelper.RunSafely(OnSelected());
            }
#if STS2_AT_LEAST_0_110_0
            else if (inputEvent.IsActionPressed(MegaInput.confirm))
#else
            else if (inputEvent.IsActionPressed(MegaInput.accept))
#endif
            {
                AcceptEvent();
                OnPreview();   
            }
        }

        private void OnFocus()
        {
            if (_isAnimating) return;
            _isHovered = true;
            _hoverTween?.Kill();
            Scale = _hoverScale;
            CreateHoverTip();
            EmitSignal(SignalName.Hovered, this);
        }

        private void OnUnfocus()
        {
            if (_isAnimating) return;
            _isHovered = false;
            _hoverTween?.Kill();
            _hoverTween = CreateTween();
            _hoverTween.TweenProperty(this, "scale", _smallScale, 0.5)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
            ClearHoverTip();
            EmitSignal(SignalName.Unhovered, this);
        }

        private void OnMousePressed(InputEvent inputEvent)
        {
            _ignoreMouseRelease = false;
        }

        private void OnMouseReleased(InputEvent inputEvent)
        {
            if (_isAnimating) return;
            if (_isHovered && !_ignoreMouseRelease && inputEvent is InputEventMouseButton mb)
            {
                if (mb.ButtonIndex == MouseButton.Left)
                    TaskHelper.RunSafely(OnSelected());
                else
                    OnPreview();
            }
        }

        private async Task OnSelected()
        {
            if (_isAnimating) return;
            if (!Entry.IsStocked) return;

            ClearHoverTip();

            if (!_hasPlayedFirstAnimation)
            {
                _hasPlayedFirstAnimation = true;
                await PlayOpenAnimation();
            }

            
            if (_entry is MerchantTarotEntry tarotEntry && tarotEntry.HasPurchased)
                return;

            
            await OnTryPurchase(_merchantRug?.Inventory);

            UpdateVisual();
            if (Entry.IsStocked && _isHovered)
                CreateHoverTip();
        }

        
        private async Task PlayOpenAnimation()
        {
            _isAnimating = true;
            _hitbox.MouseFilter = MouseFilterEnum.Ignore;

            _isHovered = false;
            _hoverTween?.Kill();

            Vector2 size = GetRect().Size;
            PivotOffset = size / 2;

            Vector2 startScale = Scale;
            _audioPlayer?.Play();

            Tween rotateTween = CreateTween();
            rotateTween.SetParallel(false);
            rotateTween.TweenProperty(this, "rotation_degrees", 1f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", -2f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", 3f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", -5f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", 8f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", -13f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", 21f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", -34f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", 19f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", -7f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", 3f, 0.05f).SetEase(Tween.EaseType.InOut);
            rotateTween.TweenProperty(this, "rotation_degrees", 0f, 0.05f).SetEase(Tween.EaseType.InOut);

            Tween scaleTween = CreateTween();
            scaleTween.TweenProperty(this, "scale", startScale * 1.1f, 0.4f).SetEase(Tween.EaseType.Out);
            scaleTween.TweenProperty(this, "scale", startScale, 0.1f).SetEase(Tween.EaseType.In);

            await ToSignal(rotateTween, "finished");
            if (scaleTween.IsRunning())
                await ToSignal(scaleTween, "finished");

            Scale = startScale;
            RotationDegrees = 0;
            PivotOffset = Vector2.Zero;

            _isAnimating = false;
            _hitbox.MouseFilter = MouseFilterEnum.Stop;
        }

        protected override void OnPreview() { }

        protected override async Task OnTryPurchase(MerchantInventory? inventory)
        {
            await _entry.OnTryPurchaseWrapper(inventory);
        }

        protected override void CreateHoverTip()
        {
            var title = new LocString("merchant_room", "TAROT_PILE_ENTRY.title");
            var desc = new LocString("merchant_room", "TAROT_PILE_ENTRY.description");
            var tipSet = NHoverTipSet.CreateAndShow(this, new HoverTip(title, desc));
            if (tipSet != null)
            {
                tipSet.GlobalPosition = GlobalPosition + new Vector2(-180, -280);
            }
        }

        protected override void UpdateVisual()
        {
            if (!IsInstanceValid(this) || !GodotObject.IsInstanceValid(_costContainer)) return;

            if (Entry is MerchantTarotEntry tarotEntry && tarotEntry.HasPurchased)
            {
                _costContainer.Visible = false;
                _hitbox.MouseFilter = MouseFilterEnum.Stop;
                _visual.Modulate = tarotEntry.IsStocked ? Colors.White : new Color(0.3f, 0.3f, 0.3f, 1);
                base.FocusMode = FocusModeEnum.None;
            }
            else if (Entry.IsStocked)
            {
                _costContainer.Visible = true;
                _costLabel.SetTextAutoSize(Entry.Cost.ToString());
                _costLabel.Modulate = Entry.EnoughGold ? new Color("ffd9b3") : new Color(0.9f, 0.25f, 0.25f, 1f);
                _hitbox.MouseFilter = MouseFilterEnum.Stop;
                _visual.Modulate = Colors.White;
                base.FocusMode = FocusModeEnum.All;
            }
            else
            {
                _costContainer.Visible = false;
                _hitbox.MouseFilter = MouseFilterEnum.Ignore;
                _visual.Modulate = new Color(0.3f, 0.3f, 0.3f, 1);
                base.FocusMode = FocusModeEnum.None;
            }
        }

        public void FillSlot(MerchantTarotEntry entry)
        {
            _entry = entry;
            _entry.EntryUpdated += UpdateVisual;
            _entry.PurchaseFailed += OnPurchaseFailed;
            _entry.PurchaseCompleted += OnSuccessfulPurchase;
            UpdateVisual();
        }

        private void OnSuccessfulPurchase(PurchaseStatus status, MerchantEntry entry)
        {
            UpdateVisual();
        }

        public new void Initialize(NMerchantInventory rug)
        {
            _merchantRug = rug;
            base.Initialize(rug);
        }

        public override void _ExitTree()
        {
            if (_entry != null)
            {
                _entry.EntryUpdated -= UpdateVisual;
                _entry.PurchaseFailed -= OnPurchaseFailed;
                _entry.PurchaseCompleted -= OnSuccessfulPurchase;
            }
            if (_merchantRug?.Inventory?.Player != null)
            {
                _merchantRug.Inventory.Player.GoldChanged -= UpdateVisual;
            }
        }
    }
}