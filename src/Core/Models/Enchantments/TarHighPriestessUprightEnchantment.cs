
#nullable enable
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
namespace PengoTarot.Enchantments;

public sealed class TarHighPriestessUprightEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Ethereal) };

    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return !card.Keywords.Contains(CardKeyword.Ethereal);
    }

    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Ethereal);
    }
}