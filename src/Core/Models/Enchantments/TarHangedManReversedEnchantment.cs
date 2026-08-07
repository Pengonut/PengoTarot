
#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace PengoTarot.Enchantments;

public sealed class TarHangedManReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return card.Keywords.Contains(CardKeyword.Exhaust);
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };

#if STS2_AT_LEAST_0_110_0
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card, bool isAutoPlay, ResourceInfo resources,
        CardLocation cardLocation)
    {
        if (card == base.Card)
        {
            cardLocation.pileType = PileType.Discard;
            return cardLocation;
        }

        return cardLocation;
    }
#else
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card, bool isAutoPlay, ResourceInfo resources,
        PileType currentPileType, CardPilePosition currentPosition)
    {
        if (card == base.Card)
            return (PileType.Discard, currentPosition);

        return (currentPileType, currentPosition);
    }
#endif

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (base.Card == null) return;

        var handPile = PileType.Hand.GetPile(base.Card.Owner);
        var otherCards = handPile.Cards.Where(c => c != base.Card).ToList();
        if (otherCards.Count > 0)
        {
            var toExhaust = base.Card.Owner.RunState.Rng.CombatCardSelection.NextItem(otherCards);
            if (toExhaust != null)
                await CardCmd.Exhaust(choiceContext, toExhaust);
        }
    }
}