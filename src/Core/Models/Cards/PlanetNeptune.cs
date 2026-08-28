#nullable enable
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Cards;
using PengoTarot.Enchantments;

namespace PengoTarot.Cards;

public sealed class PlanetNeptune : PlanetCard
{
    public PlanetNeptune() : base(CardType.Attack)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromEnchantment<PlanetNeptuneEnchantment>();
}
