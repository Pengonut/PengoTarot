
// PengoTarot/Powers/TarTemperanceReversedPower.cs
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Powers;

public sealed class TarTemperanceReversedPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || result.UnblockedDamage <= 0)
            return;

        Flash();

        int gold = result.UnblockedDamage * base.Amount;
        if (gold > 0)
        {
            await PowerCmd.Apply<PlanetGoldPower>(
                choiceContext, base.Owner, gold,
                base.Owner, null, silent: true);
            SfxCmd.Play("event:/sfx/ui/gold/gold_1");
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (base.Owner.Side != side)
            await PowerCmd.Remove(this);
    }
}