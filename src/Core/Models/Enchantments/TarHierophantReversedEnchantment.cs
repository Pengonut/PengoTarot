
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.CardSelection;

namespace PengoTarot.Enchantments;

public sealed class TarHierophantReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        return base.CanEnchant(card);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        var player = Card.Owner;
        var hand = PileType.Hand.GetPile(player);
        var rng = player.RunState.Rng.CombatCardSelection;

        var downgradeableHand = hand.Cards
            .Where(c => c.IsUpgraded || (c is Wither w && GetWitherFakeLevel(w) > 0))
            .OrderBy(c => c is Wither ? 1 : 0)
            .ToList();

        if (downgradeableHand.Count == 0)
            return;

        var prompt = new LocString("gameplay_ui", "CHOOSE_CARD_DOWNGRADE_HEADER");
        var prefs = new CardSelectorPrefs(prompt, 0, downgradeableHand.Count)
        {
            Cancelable = false,
            RequireManualConfirmation = false
        };
        var chosen = (await CardSelectCmd.FromHand(
            choiceContext, player, prefs,
            c => c.IsUpgraded || (c is Wither w && GetWitherFakeLevel(w) > 0),
            this
        )).ToList();

        foreach (var card in chosen)
        {
            if (card is Wither wither)
                DowngradeWither(wither);
            else
                CardCmd.Downgrade(card);
        }

        var drawPile = PileType.Draw.GetPile(player);
        var discardPile = PileType.Discard.GetPile(player);
        var upgradeable = drawPile.Cards
            .Concat(discardPile.Cards)
            .Where(c => c.IsUpgradable && c is not Wither)
            .ToList();

        foreach (var _ in chosen)
        {
            if (upgradeable.Count == 0) break;
            int take = Math.Min(3, upgradeable.Count);
            var picked = upgradeable.TakeRandom(take, rng).ToList();

            foreach (var card in picked)
            {
                CardCmd.Upgrade(card);
                CardCmd.Preview(card);
            }

            upgradeable.RemoveAll(c => picked.Contains(c));
        }

        await Task.CompletedTask;
    }

    private static int GetWitherFakeLevel(Wither wither)
    {
        var field = typeof(Wither).GetField("_fakeUpgradeLevel",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (int)field!.GetValue(wither)!;
    }

    private static void DowngradeWither(Wither wither)
    {
        int current = GetWitherFakeLevel(wither);
        if (current <= 0) return;

        var field = typeof(Wither).GetField("_fakeUpgradeLevel",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(wither, 0);

        var damageVar = wither.DynamicVars["Damage"];
        damageVar.UpgradeValueBy(-3m * current);
    }
}