#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patches;

/// <summary>
/// 命运之轮占卜：每主动往牌组加入四张牌，额外复制第四张。
/// 额外复制本身不推进计数；开局构筑初始牌组时也不计数。
/// </summary>
public static class TarWheelOfFortuneDivinationPatch
{
    private const int FlagIndex = 10;
    // 复制牌本身也会经过五轮书式 Hook 并计数，因此内部使用 5 周期；
    // 每名玩家新局从 1 开始，对玩家而言仍是每主动加入 4 张触发一次。
    private const int CardInterval = 5;
    private const string ShadowName = "PengoTarotWheelCardShadow";
    private static readonly Vector2 ShadowOffset = new(33f, -44f);

    private sealed class SelectionState
    {
        public readonly List<CardModel> SelectionOrder = new();
    }

    private static readonly ConditionalWeakTable<NSimpleCardSelectScreen, SelectionState> SelectionStates = new();

    private static bool IsEnabled()
        => RunManager.Instance.IsInProgress
           && ConfigFloatingWindowRunData.GetTarFlag(FlagIndex);

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardChangedPiles))]
    private static class AfterCardChangedPilesPatch
    {
        [HarmonyPostfix]
        private static void Postfix(CardModel card, ref Task __result)
        {
            __result = ResolvePileChange(__result, card);
        }
    }

    private static async Task ResolvePileChange(Task originalTask, CardModel card)
    {
        await originalTask;
        // 严格采用原版五轮书的判定：Hook 结束时牌位于该玩家的 Deck 即计数。
        // 复制牌也会推进一次计数（4 -> 5），因此不会形成递归触发。
        if (card.Owner.Creature.IsDead
            || card.Pile?.Type != PileType.Deck || !IsEnabled())
            return;

        int count = ConfigFloatingWindowRunData.RecordWheelOfFortuneCard(card.Owner.NetId);
        Log.Info($"[PengoTarot] [WheelOfFortune] player={card.Owner.NetId} " +
                 $"card={card.Id} count={count}");
        if (count % CardInterval == 0)
        {
            CardModel copy = card.Owner.RunState.CloneCard(card);
            // 原版宾邦也通过 clonedBy 标记复制来源，并展示入牌预览。
            CardPileAddResult result = await CardPileCmd.Add(
                copy, PileType.Deck, CardPilePosition.Bottom, card);
            CardCmd.PreviewCardPileAdd(result);
            Log.Info($"[PengoTarot] [WheelOfFortune] copied={result.cardAdded.Id} " +
                     $"success={result.success}");
        }

        RefreshVisibleChoiceShadows();
    }

    [HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.RefreshOptions))]
    private static class RewardScreenRefreshPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NCardRewardSelectionScreen __instance)
            => Callable.From(() => UpdateRewardScreen(__instance)).CallDeferred();
    }

    [HarmonyPatch(typeof(NMerchantCard), nameof(NMerchantCard.FillSlot))]
    private static class MerchantCardFillSlotPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NMerchantCard __instance)
            => Callable.From(() => UpdateMerchantCard(__instance)).CallDeferred();
    }

    [HarmonyPatch(typeof(NSimpleCardSelectScreen), "OnCardClicked")]
    private static class SimpleSelectionClickedPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NSimpleCardSelectScreen __instance, CardModel card)
        {
            // 只有 CardCreationResult 网格才表示“选中后加入牌组”；普通牌组选择不显示。
            if (Traverse.Create(__instance).Field("_cardResults").GetValue() == null)
                return;

            var selected = Traverse.Create(__instance).Field("_selectedCards")
                .GetValue<HashSet<CardModel>>();
            var state = SelectionStates.GetOrCreateValue(__instance);
            if (selected.Contains(card))
            {
                if (!state.SelectionOrder.Contains(card))
                    state.SelectionOrder.Add(card);
            }
            else
            {
                state.SelectionOrder.Remove(card);
            }
            state.SelectionOrder.RemoveAll(candidate => !selected.Contains(candidate));
            UpdateSimpleSelectionScreen(__instance, state);
        }
    }

    [HarmonyPatch(typeof(NSimpleCardSelectScreen), "ConnectSignalsAndInitGrid")]
    private static class SimpleSelectionReadyPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NSimpleCardSelectScreen __instance)
            => Callable.From(() => UpdateSimpleSelectionScreen(
                __instance, SelectionStates.GetOrCreateValue(__instance))).CallDeferred();
    }

    [HarmonyPatch(typeof(NCardGridSelectionScreen), nameof(NCardGridSelectionScreen.CardsSelected))]
    private static class PreserveSimpleSelectionOrderPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NCardGridSelectionScreen __instance,
            ref Task<IEnumerable<CardModel>> __result)
        {
            if (__instance is NSimpleCardSelectScreen simple
                && SelectionStates.TryGetValue(simple, out var state))
            {
                __result = ReorderSelectedCards(__result, state);
            }
        }
    }

    private static async Task<IEnumerable<CardModel>> ReorderSelectedCards(
        Task<IEnumerable<CardModel>> originalTask, SelectionState state)
    {
        var selected = (await originalTask).ToHashSet();
        return state.SelectionOrder.Where(selected.Contains).ToList();
    }

    private static void UpdateRewardScreen(NCardRewardSelectionScreen screen)
    {
        if (!GodotObject.IsInstanceValid(screen)) return;
        var cardRow = Traverse.Create(screen).Field("_cardRow").GetValue<Control>();
        var holders = cardRow?.GetChildren().OfType<NGridCardHolder>().ToList();
        if (holders == null) return;
        if (holders.Count == 0) return;
        ulong netId = holders[0].CardModel.Owner.NetId;
        bool show = IsEnabled()
                    && (ConfigFloatingWindowRunData.GetWheelOfFortuneCardCount(netId) + 1) % CardInterval == 0;
        foreach (var holder in holders)
            SetShadow(holder, show);
    }

    private static void UpdateSimpleSelectionScreen(NSimpleCardSelectScreen screen, SelectionState state)
    {
        if (!GodotObject.IsInstanceValid(screen)) return;
        if (Traverse.Create(screen).Field("_cardResults").GetValue() == null)
            return;
        var grid = Traverse.Create(screen).Field("_grid").GetValue<NCardGrid>();
        var holders = grid?.CurrentlyDisplayedCardHolders.ToList();
        if (holders == null || holders.Count == 0) return;

        int baseCount = ConfigFloatingWindowRunData.GetWheelOfFortuneCardCount(
            holders[0].CardModel.Owner.NetId);
        var fixedShadows = new HashSet<CardModel>();
        int simulatedCount = baseCount;
        foreach (CardModel selectedCard in state.SelectionOrder)
        {
            simulatedCount++;
            if (simulatedCount % CardInterval != 0) continue;
            fixedShadows.Add(selectedCard);
            simulatedCount++; // 触发后复制牌也像五轮书一样推进计数。
        }
        bool nextSelectionCopies = (simulatedCount + 1) % CardInterval == 0;

        foreach (var holder in holders)
        {
            bool selected = state.SelectionOrder.Contains(holder.CardModel);
            bool show = IsEnabled()
                        && (fixedShadows.Contains(holder.CardModel)
                            || (nextSelectionCopies && !selected));
            SetShadow(holder, show);
        }
    }

    private static void SetShadow(NGridCardHolder holder, bool visible)
        => SetShadow(holder, holder.CardNode, holder.CardModel, visible);

    private static void UpdateMerchantCard(NMerchantCard merchantCard)
    {
        if (!GodotObject.IsInstanceValid(merchantCard)) return;
        var traverse = Traverse.Create(merchantCard);
        var holder = traverse.Field("_cardHolder").GetValue<Control>();
        var cardNode = traverse.Field("_cardNode").GetValue<NCard>();
        if (holder == null || cardNode?.Model == null) return;

        bool show = IsEnabled()
                    && (ConfigFloatingWindowRunData.GetWheelOfFortuneCardCount(
                            cardNode.Model.Owner.NetId) + 1) % CardInterval == 0;
        SetShadow(holder, cardNode, cardNode.Model, show);
    }

    private static void SetShadow(Node holder, NCard? cardNode, CardModel? cardModel, bool visible)
    {
        var existing = holder.GetNodeOrNull<NCard>(ShadowName);
        if (!visible)
        {
            existing?.QueueFree();
            return;
        }
        if (existing != null && existing.Model == cardModel)
            return;
        if (existing != null)
        {
            holder.RemoveChild(existing);
            existing.QueueFree();
        }

        if (cardNode == null || cardModel == null) return;
        NCard? shadow = NCard.Create(cardModel);
        if (shadow == null) return;
        shadow.Name = ShadowName;
        IgnoreMouseRecursively(shadow);
        shadow.Modulate = new Color(0.7f, 0.7f, 0.7f, 1f);
        shadow.Position = cardNode.Position + ShadowOffset;
        holder.AddChild(shadow);
        holder.MoveChild(shadow, 0);
        shadow.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
    }

    private static void IgnoreMouseRecursively(Node node)
    {
        if (node is Control control)
            control.MouseFilter = Control.MouseFilterEnum.Ignore;
        foreach (Node child in node.GetChildren())
            IgnoreMouseRecursively(child);
    }

    private static void RefreshVisibleChoiceShadows()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree?.Root == null) return;
        foreach (var node in tree.Root.FindChildren("NCardRewardSelectionScreen", "", true, false))
            if (node is NCardRewardSelectionScreen rewardScreen) UpdateRewardScreen(rewardScreen);
        foreach (var node in tree.Root.FindChildren("*", "", true, false))
            if (node is NMerchantCard merchantCard) UpdateMerchantCard(merchantCard);
    }
}
