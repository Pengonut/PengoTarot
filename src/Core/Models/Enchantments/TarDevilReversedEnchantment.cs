#nullable enable
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Enchantments;

public sealed class TarDevilReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    private decimal _hpLostSinceLastPlay;
    private int _appliedReductions; // 已添加的 AddUntilPlayed(-1) 层数

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, 
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Card?.Owner?.Creature)
            return Task.CompletedTask;

        _hpLostSinceLastPlay += result.UnblockedDamage;
        int targetReductions = (int)(_hpLostSinceLastPlay / 3m);
        int toAdd = targetReductions - _appliedReductions;

        for (int i = 0; i < toAdd; i++)
        {
            base.Card!.EnergyCost.AddUntilPlayed(-1);
        }
        _appliedReductions = targetReductions;

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == base.Card)
        {
            _hpLostSinceLastPlay = 0m;
            _appliedReductions = 0;
            // AddUntilPlayed 的修正会在打出后自动移除，无需额外清理
        }
        return Task.CompletedTask;
    }

}