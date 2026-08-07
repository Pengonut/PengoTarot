
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.HoverTips;

namespace PengoTarot.Enchantments;

public sealed class TarWorldUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Evoke)};

    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (base.Card == null)
            return;

        if (base.Card.Pile?.Type is PileType.None or PileType.Play)
            return;

        if (base.Card.Pile?.Type == PileType.Hand)
            return;

        await CardPileCmd.Add(base.Card, PileType.Hand);

        int currentCost = base.Card.EnergyCost.GetWithModifiers(CostModifiers.All);
        base.Card.EnergyCost.SetThisTurn(currentCost + 1);
    }
}