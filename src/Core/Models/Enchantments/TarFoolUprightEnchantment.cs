
#nullable enable
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarFoolUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    private bool _triggered;

#if STS2_AT_LEAST_0_110_0
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card, bool isAutoPlay, ResourceInfo resources,
        CardLocation cardLocation)
    {
        if (card == base.Card && !_triggered && (card.Type == CardType.Attack || card.Type == CardType.Skill))
        {
            _triggered = true;
            Status = EnchantmentStatus.Disabled;
            cardLocation.pileType = PileType.Hand;
            cardLocation.position = CardPilePosition.Bottom;
            return cardLocation;
        }
        return cardLocation;
    }
#else
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card, bool isAutoPlay, ResourceInfo resources,
        PileType currentPileType, CardPilePosition currentPosition)
    {
        if (card == base.Card && !_triggered && (card.Type == CardType.Attack || card.Type == CardType.Skill))
        {
            _triggered = true;
            Status = EnchantmentStatus.Disabled;
            return (PileType.Hand, CardPilePosition.Bottom);
        }
        return (currentPileType, currentPosition);
    }
#endif


    public override async System.Threading.Tasks.Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != base.Card || _triggered || base.Card?.Owner == null)
            return;

        if (base.Card.Type != CardType.Power)
            return; 

        _triggered = true;
        Status = EnchantmentStatus.Disabled;

        var clone = base.Card.CreateClone();
        if (clone != null)
        {
            CardCmd.ClearEnchantment(clone);
            CardCmd.Enchant<TarFoolUprightEnchantment>(clone, 1m);
            if (clone.Enchantment is TarFoolUprightEnchantment cloneEnchant)
            {
                cloneEnchant._triggered = true;
                cloneEnchant.Status = EnchantmentStatus.Disabled;
            }
            await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, base.Card.Owner);
        }
    }
}