// PengoTarot/Powers/PlanetGoldPower.cs
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace PengoTarot.Powers
{
    /// <summary>
    /// Buff that gives gold at the end of combat.
    /// Applied by Jupiter and TarTemperanceReversed powers as they trigger.
    /// </summary>
    public sealed class PlanetGoldPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override Task AfterCombatEnd(CombatRoom room)
        {
            Flash();
            if (Amount > 0 && Owner.Player != null)
                room.AddExtraReward(Owner.Player, new GoldReward(Amount, Owner.Player));
            return Task.CompletedTask;
        }
    }
}