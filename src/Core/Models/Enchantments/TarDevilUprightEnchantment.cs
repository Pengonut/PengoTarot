
#nullable enable
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace PengoTarot.Enchantments;

public sealed class TarDevilUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override bool ShouldGlowRed => true;

    protected override void OnEnchant()
    {
        base.Card.EnergyCost.UpgradeBy(-1);
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card.Owner != base.Card.Owner)
            return true;
        if (base.Card.Pile?.Type != PileType.Hand)
            return true;
        if (autoPlayType != AutoPlayType.None || card == base.Card)
            return true;
        if (card.Enchantment is TarDevilUprightEnchantment)
            return true;
        if (card is Enthralled)
            return true;
        return false;
    }
}