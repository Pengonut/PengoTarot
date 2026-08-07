
#nullable enable

using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace PengoTarot.Network
{
    public struct ForcePlayerSyncMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
    {
        public SerializablePlayer player;
        public required RunLocation Location { get; set; }

        public bool ShouldBroadcast => true;
        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.Debug;
        public bool ShouldBuffer => false;
        public RunLocation location => Location;

        public void Serialize(PacketWriter writer)
        {
            writer.Write(player);
            writer.Write(Location);
        }

        public void Deserialize(PacketReader reader)
        {
            player = reader.Read<SerializablePlayer>();
            Location = reader.Read<RunLocation>();
        }
    }
}