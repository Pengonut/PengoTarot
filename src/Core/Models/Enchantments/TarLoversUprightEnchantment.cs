
#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.Context;
namespace PengoTarot.Enchantments;

public sealed class TarLoversUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPile, AbstractModel? clonedBy)
    {
        if (card != base.Card || card.Pile?.Type != PileType.Hand || oldPile == PileType.Hand)
            return Task.CompletedTask;

        var other = FindOtherLoversCard(card);
        if (other != null)
            return CardPileCmd.Add(other, PileType.Hand);

        return Task.CompletedTask;
    }

    private CardModel? FindOtherLoversCard(CardModel self)
    {
        if (self?.Owner?.PlayerCombatState == null) return null;
        var allCards = self.Owner.PlayerCombatState.AllCards;
        return allCards.FirstOrDefault(c =>
            c != self &&
            c.Pile?.Type != PileType.Hand &&
            c.Pile?.Type != PileType.Play &&
            (c.Enchantment is TarLoversUprightEnchantment ||
            c.Enchantment is TarLoversReversedEnchantment));
    }
}