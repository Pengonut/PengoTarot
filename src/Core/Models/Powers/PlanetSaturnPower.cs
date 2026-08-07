// PengoTarot/Powers/PlanetSaturnPower.cs
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Powers
{
    /// <summary>
    /// Debuff placed on an enemy by Planet Saturn enchantment.
    /// Sets a damage floor: future attack damage against this enemy cannot be lower than the power's Amount.
    /// Only the highest value overwrites; lower values are ignored.
    /// Removed at the end of the player side turn.
    /// </summary>
    public sealed class PlanetSaturnPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override Task AfterDamageReceived(
            PlayerChoiceContext choiceContext, Creature target, DamageResult result,
            ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            // 同步逻辑，无需 async：避免 CS1998（async 方法缺少 await）
            if (!props.IsPoweredAttack()) return Task.CompletedTask;
            Flash();
            return Task.CompletedTask;
        }
        
        public override decimal ModifyDamageAdditive(
            Creature? target, decimal amount, ValueProp props,
            Creature? dealer, CardModel? cardSource
#if STS2_AT_LEAST_0_110_0
            , CardPlay? cardPlay
#endif
            )
        {
            if (target != Owner) return 0m;
            if (!props.IsPoweredAttack()) return 0m;
            if (amount >= Amount) return 0m;
            return Amount - amount;
        }

        public override async Task AfterSideTurnEnd(
            PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (Owner.Side != side)
                await PowerCmd.Remove(this);
        }
    }
}