// PengoTarot/Powers/PlanetJupiterPower.cs
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Powers
{
    /// <summary>
    /// Debuff placed on an enemy by Planet Jupiter enchantment.
    /// When the enemy takes attack damage this turn, applies PlanetGoldPower to all players.
    /// Removed at the end of the player side turn.
    /// </summary>
    public sealed class PlanetJupiterPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override async Task AfterDamageGiven(
            PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
            ValueProp props, Creature target, CardModel? cardSource)
        {
            if (target != Owner) return;
            if (dealer == null) return;
            if (!props.IsPoweredAttack()) return;
            if (result.UnblockedDamage + result.OverkillDamage <= 0) return;

            Flash();

            var goldAmount = ( result.UnblockedDamage + result.OverkillDamage ) * Amount;
            foreach (var player in CombatState.Players)
            {
                await PowerCmd.Apply<PlanetGoldPower>(
                    choiceContext, player.Creature, goldAmount,
                    dealer, null, silent: true);
            }

            SfxCmd.Play("event:/sfx/ui/gold/gold_1");
        }
        public override async Task AfterSideTurnEnd(
            PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (Owner.Side != side)
                await PowerCmd.Remove(this);
        }
    }
}