
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarEmperorReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Retain) };
    public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {

        if (base.Card.Owner != player || base.Card.Pile?.Type != PileType.Hand)
            return Task.CompletedTask;

        var handPile = PileType.Hand.GetPile(player);
        var handCards = handPile.Cards.ToList();
        if (handCards.Count == 0) return Task.CompletedTask;

        int countToRetain = System.Math.Min(2, handCards.Count);
        var chosen = new List<CardModel>();
        var pool = new List<CardModel>(handCards);
        var rng = player.RunState.Rng;
        for (int i = 0; i < countToRetain; i++)
        {
            var card = rng.CombatCardSelection.NextItem(pool);
            chosen.Add(card!);
            pool.Remove(card!);
        }

        foreach (var card in chosen)
        {
            if (!card.Keywords.Contains(CardKeyword.Retain))
            {
                
                CardCmd.ApplyKeyword(card, CardKeyword.Retain);
            }
        }
        return Task.CompletedTask;
    }
}