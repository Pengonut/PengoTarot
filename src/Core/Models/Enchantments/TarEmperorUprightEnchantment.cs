
#nullable enable
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.Context;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
namespace PengoTarot.Enchantments;

public sealed class TarEmperorUprightEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Retain) };

    public override bool HasExtraCardText => true;

    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Retain);
    }
}