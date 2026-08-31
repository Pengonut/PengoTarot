#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Cards;
using PengoTarot.Data;

namespace PengoTarot.Patches;

/// <summary>
/// 长时间查看塔罗牌或星球牌时，在右上角展示当前无法接受对应附魔的牌。
/// 绘制层级只由 GlobalUi 中的兄弟节点顺序决定：预览容器始终紧邻 TopBar 之前。
/// </summary>
[HarmonyPatch]
internal static class UnavailableEnchantmentPreviewPatch
{
    [HarmonyPatch(typeof(NCardHolder), "DoCardHoverEffects")]
    [HarmonyPostfix]
    private static void CardHoverPostfix(NCardHolder __instance, bool isHovered)
    {
        if (__instance.GetParent()?.GetParent() is not NChooseACardSelectionScreen)
            return;

        if (isHovered)
            UnavailableEnchantmentPreview.NotifyHovered(__instance);
        else
            UnavailableEnchantmentPreview.NotifyUnhovered(__instance);
    }

    [HarmonyPatch(typeof(NChooseACardSelectionScreen), "SelectHolder")]
    [HarmonyPrefix]
    private static void CardSelectedPrefix() => UnavailableEnchantmentPreview.RetractImmediately();

    [HarmonyPatch(typeof(NChooseACardSelectionScreen), "OnSkipButtonReleased")]
    [HarmonyPrefix]
    private static void ReturnedPrefix() => UnavailableEnchantmentPreview.RetractImmediately();

    [HarmonyPatch(typeof(NChooseACardSelectionScreen), nameof(NChooseACardSelectionScreen.AfterOverlayHidden))]
    [HarmonyPostfix]
    private static void OverlayHiddenPostfix() => UnavailableEnchantmentPreview.RetractImmediately();

    [HarmonyPatch(typeof(NSettingsScreen), nameof(NSettingsScreen.OnSubmenuOpened))]
    [HarmonyPrefix]
    private static void SettingsOpenedPrefix() => UnavailableEnchantmentPreview.RetractImmediately();

    [HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuShown")]
    [HarmonyPrefix]
    private static void SettingsShownPrefix() => UnavailableEnchantmentPreview.RetractImmediately();

    [HarmonyPatch(typeof(NDeckEnchantSelectScreen), nameof(NDeckEnchantSelectScreen.ShowScreen))]
    [HarmonyPostfix]
    private static void EnchantScreenOpenedPostfix(EnchantmentModel enchantment) =>
        UnavailableEnchantmentPreview.ShowForEnchantment(enchantment);

    [HarmonyPatch(typeof(NCardGridSelectionScreen), nameof(NCardGridSelectionScreen.AfterOverlayHidden))]
    [HarmonyPostfix]
    private static void GridOverlayHiddenPostfix(NCardGridSelectionScreen __instance)
    {
        if (__instance is NDeckEnchantSelectScreen)
            UnavailableEnchantmentPreview.RetractImmediately();
    }
}

internal static partial class UnavailableEnchantmentPreview
{
    private const float HoverDelay = 1f;
    private const float LeaveDelay = 5f;
    private const float CardScale = 0.3f;
    private const float CardGap = 5f;
    private const float TopMargin = 108f;
    private const float RightMargin = 18f;
    private const float BottomMargin = 18f;
    private const float ColumnGap = 8f;
    private const float AnimationDuration = 0.38f;

    private sealed partial class PreviewController : Control
    {
        public override void _Process(double delta) => Tick((float)delta);
        public override void _ExitTree() => ControllerExited(this);
    }

    private sealed partial class InvalidCross : Control
    {
        private const string TexturePath = "res://images/ui/unavailable_enchantment_x.png";
        private const float HorizontalOverflow = 72f;
        private const float VerticalOverflow = 34f;
        private float _phase;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            // NCard 的视觉以节点原点为中心；恢复大尺寸，并以原点严格居中。
            Size = NCard.defaultSize + new Vector2(HorizontalOverflow * 2f, VerticalOverflow * 2f);
            Position = Size * -0.5f;
            var image = new TextureRect
            {
                MouseFilter = MouseFilterEnum.Ignore,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Texture = GD.Load<Texture2D>(TexturePath)
            };
            AddChild(image);
            image.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        }

        public override void _Process(double delta)
        {
            _phase = (_phase + (float)delta) % 3f;
            // 三秒一次完整明暗循环，峰值不透明度为 70%。
            var alpha = 0.35f * (1f - Mathf.Cos(_phase * Mathf.Tau / 3f));
            Modulate = new Color(1f, 1f, 1f, alpha);
        }
    }

    private sealed class PreviewEntry(NCard card)
    {
        public NCard Card { get; } = card;
        public Tween? Tween { get; set; }
    }

    private static readonly List<PreviewEntry> TypeEntries = [];
    private static readonly List<PreviewEntry> EnchantedEntries = [];
    private static PreviewController? _controller;
    private static NCardHolder? _hoveredHolder;
    private static float _hoverSeconds;
    private static float _unhoverSeconds;
    private static bool _shown;

    public static void NotifyHovered(NCardHolder holder)
    {
        if (holder.CardModel is not TarCard and not PlanetCard)
            return;
        if (!ReferenceEquals(_hoveredHolder, holder))
        {
            RetractImmediately();
            _hoveredHolder = holder;
            _hoverSeconds = 0f;
        }
        _unhoverSeconds = 0f;
        EnsureController();
    }

    public static void NotifyUnhovered(NCardHolder holder)
    {
        if (ReferenceEquals(_hoveredHolder, holder))
            _unhoverSeconds = 0.0001f;
    }

    public static void RetractImmediately()
    {
        _hoveredHolder = null;
        _hoverSeconds = 0f;
        _unhoverSeconds = 0f;
        RetractCards();
    }

    public static void ShowForEnchantment(EnchantmentModel enchantment)
    {
        _hoveredHolder = null;
        _hoverSeconds = 0f;
        _unhoverSeconds = 0f;
        ShowFor(enchantment);
    }

    private static void Tick(float delta)
    {
        if (_hoveredHolder == null || !GodotObject.IsInstanceValid(_hoveredHolder))
        {
            if (_shown)
            {
                _unhoverSeconds += delta;
                if (_unhoverSeconds >= LeaveDelay)
                    RetractImmediately();
            }
            return;
        }

        if (_unhoverSeconds > 0f)
        {
            _unhoverSeconds += delta;
            if (_unhoverSeconds >= LeaveDelay)
                RetractImmediately();
            return;
        }

        if (_shown)
            return;
        _hoverSeconds += delta;
        if (_hoverSeconds >= HoverDelay && _hoveredHolder.CardModel is { } card)
            ShowFor(card);
    }

    private static PreviewController? EnsureController()
    {
        var globalUi = NRun.Instance?.GlobalUi;
        var topBar = globalUi?.TopBar;
        if (globalUi == null || topBar == null)
            return null;
        if (GodotObject.IsInstanceValid(_controller) && _controller!.GetParent() == globalUi)
            return _controller;

        _controller = new PreviewController
        {
            Name = "PengoUnavailableEnchantmentPreview",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _controller.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        globalUi.AddChild(_controller);
        globalUi.MoveChild(_controller, topBar.GetIndex());
        return _controller;
    }

    private static void ShowFor(CardModel hoveredCard)
    {
        var enchantment = FindEnchantment(hoveredCard);
        if (enchantment != null)
            ShowFor(enchantment);
    }

    private static void ShowFor(EnchantmentModel enchantment)
    {
        var player = FindLocalPlayer();
        var controller = EnsureController();
        if (player == null || controller == null)
            return;

        ClearEntries(false);
        var deck = player.Piles.FirstOrDefault(pile => pile.Type == PileType.Deck);
        if (deck == null)
            return;

        var typeCards = DeduplicateByTitle(deck.Cards.Where(card => IsCardTypeRejected(enchantment, card)));
        var enchantedCards = DeduplicateByTitle(deck.Cards.Where(card => card.Enchantment != null));
        CreateColumn(typeCards, TypeEntries);
        CreateColumn(enchantedCards, EnchantedEntries);
        LayoutColumn(TypeEntries, 0);
        LayoutColumn(EnchantedEntries, 1);
        _shown = true;
    }

    private static EnchantmentModel? FindEnchantment(CardModel card)
    {
        var type = card.GetType();
        if (card is TarCard)
            return TarotDeck.All.FirstOrDefault(def => def.CardType == type)?.Enchantment;
        if (card is PlanetCard)
            return PlanetDeck.All.FirstOrDefault(def => def.CardType == type)?.Enchantment;
        return null;
    }

    private static Player? FindLocalPlayer() =>
        RunManager.Instance.DebugOnlyGetState()?.Players.FirstOrDefault(LocalContext.IsMe);

    private static List<CardModel> DeduplicateByTitle(IEnumerable<CardModel> cards) =>
        cards.GroupBy(card => card.Title, StringComparer.CurrentCulture).Select(group => group.First()).ToList();

    private static bool IsCardTypeRejected(EnchantmentModel enchantment, CardModel card) =>
        !enchantment.CanEnchantCardType(card.Type)
        // 未附魔牌的其余资格限制（稀有度、关键词、AoE 等）也归入左列；
        // 已附魔本身造成的拒绝则只归入右列，避免把整列重复过去。
        || (card.Enchantment == null && !enchantment.CanEnchant(card))
        // 原版特殊牌型通常会被基础规则拒绝，但必须尊重 Harmony 后的最终裁决：
        // 高塔占卜启用时，进阶之灾虽然是 Curse，CanEnchant 仍会返回 true。
        || (card.Type is CardType.None or CardType.Status or CardType.Curse or CardType.Quest
            && !enchantment.CanEnchant(card));

    private static void CreateColumn(IEnumerable<CardModel> models, List<PreviewEntry> entries)
    {
        foreach (var model in models)
        {
            var card = NCard.Create(model);
            if (card == null)
                continue;
            card.MouseFilter = Control.MouseFilterEnum.Ignore;
            card.Scale = Vector2.One * CardScale;
            _controller!.AddChild(card);
            card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
            card.AddChild(new InvalidCross());
            entries.Add(new PreviewEntry(card));
        }
    }

    private static void LayoutColumn(List<PreviewEntry> entries, int column)
    {
        if (_controller == null || entries.Count == 0)
            return;
        var viewport = _controller.GetViewportRect().Size;
        var size = NCard.defaultSize * CardScale;
        var x = viewport.X - RightMargin - size.X * 0.5f - (1 - column) * (size.X + ColumnGap);
        var available = Mathf.Max(viewport.Y - TopMargin - BottomMargin - size.Y, 0f);
        var spacing = entries.Count <= 1
            ? 0f
            : Mathf.Min(size.Y + CardGap, available / (entries.Count - 1));

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var target = new Vector2(x, TopMargin + size.Y * 0.5f + spacing * i);
            // 从屏幕上方依次向下弹落，不从右侧横向滑入。
            entry.Card.Position = new Vector2(target.X, -size.Y * 0.5f - i * CardGap);
            entry.Tween = entry.Card.CreateTween();
            entry.Tween.TweenProperty(entry.Card, "position", target, AnimationDuration)
                .SetDelay(i * 0.035f)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        }
    }

    private static void RetractCards()
    {
        if (!_shown && TypeEntries.Count == 0 && EnchantedEntries.Count == 0)
            return;
        _shown = false;
        var viewportWidth = _controller?.GetViewportRect().Size.X ?? 1920f;
        foreach (var entry in TypeEntries.Concat(EnchantedEntries))
        {
            if (!GodotObject.IsInstanceValid(entry.Card))
                continue;
            entry.Tween?.Kill();
            entry.Tween = entry.Card.CreateTween();
            entry.Tween.TweenProperty(entry.Card, "position:x", viewportWidth + NCard.defaultSize.X, AnimationDuration)
                .SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
            entry.Tween.TweenCallback(Callable.From(entry.Card.QueueFree));
        }
        TypeEntries.Clear();
        EnchantedEntries.Clear();
    }

    private static void ClearEntries(bool animate)
    {
        if (animate)
        {
            RetractCards();
            return;
        }
        foreach (var entry in TypeEntries.Concat(EnchantedEntries))
            if (GodotObject.IsInstanceValid(entry.Card))
                entry.Card.QueueFree();
        TypeEntries.Clear();
        EnchantedEntries.Clear();
        _shown = false;
    }

    private static void ControllerExited(PreviewController controller)
    {
        if (!ReferenceEquals(_controller, controller))
            return;
        TypeEntries.Clear();
        EnchantedEntries.Clear();
        _controller = null;
        _hoveredHolder = null;
        _shown = false;
        _hoverSeconds = 0f;
        _unhoverSeconds = 0f;
    }
}
