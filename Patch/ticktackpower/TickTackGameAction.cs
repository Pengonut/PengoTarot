// PengoTarot/GameActions/TickTackGameAction.cs
#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace PengoTarot.GameActions
{

    public sealed class TickTackGameAction : GameAction
    {
        private readonly Player _player;

        public override ulong OwnerId => _player.NetId;

        public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

        public TickTackGameAction(Player player)
        {
            _player = player;
        }

        protected override async Task ExecuteAction()
        {
            var creature = _player.Creature;
            if (creature == null) return;

            var powers = creature.GetPowerInstances<Powers.TickTackPower>().ToList();
            bool anyReachedZero = false;

            foreach (var power in powers)
            {
                if (!power.IsMutable || power.Amount <= 0) continue;
                await PowerCmd.Decrement(power);
                if (power.Amount <= 0)
                    anyReachedZero = true;
            }

            if (anyReachedZero)
            {
                SfxCmd.Play("event:/sfx/debuff");
                PlayerCmd.EndTurn(_player, canBackOut: false);
            }
        }

        public override INetAction ToNetAction()
        {
            return new TickTackNetAction { TargetPlayerNetId = _player.NetId };
        }

        public override string ToString()
        {
            return $"TickTackGameAction playerNetId={_player.NetId}";
        }
    }
}
