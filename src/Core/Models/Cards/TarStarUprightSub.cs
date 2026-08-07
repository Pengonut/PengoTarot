#nullable enable
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Cards;
using PengoTarot.Enchantments;

namespace PengoTarot.Cards;

public sealed class TarStarUprightSub : TarCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<TarStarUprightSubEnchantment>();
}
