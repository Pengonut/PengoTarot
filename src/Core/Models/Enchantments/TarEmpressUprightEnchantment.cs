
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarEmpressUprightEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, new DynamicVar("Times", 1)) };

    public override bool HasExtraCardText => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new DynamicVar("PlayCount", 0) };

    public override int EnchantPlayCount(int originalPlayCount)
    {
        if (DynamicVars["PlayCount"].IntValue >= 2)
            return originalPlayCount + 1;
        return originalPlayCount;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != Card)
            return Task.CompletedTask;

        int current = DynamicVars["PlayCount"].IntValue;
        DynamicVars["PlayCount"].BaseValue = current + 1;
        RecalculateValues();
        return Task.CompletedTask;
    }
}