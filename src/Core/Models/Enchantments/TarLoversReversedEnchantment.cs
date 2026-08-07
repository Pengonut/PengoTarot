
#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;

namespace PengoTarot.Enchantments;

public sealed class TarLoversReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card))
            return false;
        if (card.EnergyCost.CostsX)
            return false;
        if (card.HasStarCostX)
            return false;
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != base.Card)
            return;

        var other = FindOtherLoversCard(base.Card);
        if (other != null && other.Pile?.Type != PileType.Hand)
        {
            await CardPileCmd.Add(other, PileType.Hand);
            
            int currentCost = (int)other.EnergyCost.GetWithModifiers(CostModifiers.All);
            other.EnergyCost.SetThisTurn(currentCost + 1);
        }
    }

    private CardModel? FindOtherLoversCard(CardModel self)
    {
        if (self?.Owner?.PlayerCombatState == null) return null;
        var allCards = self.Owner.PlayerCombatState.AllCards;
        return allCards.FirstOrDefault(c =>
            c != self &&
            c.Pile?.Type != PileType.Hand &&
            (c.Enchantment is TarLoversUprightEnchantment ||
            c.Enchantment is TarLoversReversedEnchantment));
    }
}