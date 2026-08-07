
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarEmpressReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    protected override void OnEnchant()
    {
        base.Card.EnergyCost.UpgradeBy(-1);
        Status = EnchantmentStatus.Normal;
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card == base.Card)
            return Status == EnchantmentStatus.Disabled;
        return true;
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card == base.Card && Status == EnchantmentStatus.Normal && card.Pile?.Type == PileType.Discard)
        {
            Status = EnchantmentStatus.Disabled;
        }
        return Task.CompletedTask;
    }
}