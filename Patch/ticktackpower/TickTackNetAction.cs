// PengoTarot/GameActions/TickTackNetAction.cs
#nullable enable
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace PengoTarot.GameActions
{
    /// <summary>
    /// 网络序列化 TickTackPower 倒计时 tick。
    /// 携带目标玩家的 NetId，因为框架的 playerId 是提交者而非目标。
    /// </summary>
    public struct TickTackNetAction : INetAction
    {
        public ulong TargetPlayerNetId;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteULong(TargetPlayerNetId);
        }

        public void Deserialize(PacketReader reader)
        {
            TargetPlayerNetId = reader.ReadULong();
        }

        public GameAction ToGameAction(Player submitter)
        {
            // submitter 是发起 RequestEnqueue 的玩家（Host），不是 tick 目标。
            // 通过 CombatState 查找真正的目标玩家。
            var targetPlayer = LookupPlayer(submitter, TargetPlayerNetId);
            return new TickTackGameAction(targetPlayer);
        }

        private static Player LookupPlayer(Player anyPlayer, ulong targetNetId)
        {
            var combatState = anyPlayer.Creature?.CombatState;
            return combatState?.GetPlayer(targetNetId) ?? anyPlayer;
        }

        public override string ToString()
        {
            return $"TickTackNetAction targetNetId={TargetPlayerNetId}";
        }
    }
}
