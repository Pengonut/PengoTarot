#nullable enable

using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Network
{
    /// <summary>主机权威的占卜地图标记完整快照。</summary>
    public struct TarotMarkerStateMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
    {
        public string json;
        public required RunLocation Location { get; set; }

        public bool ShouldBroadcast => true;
        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.VeryDebug;
        public bool ShouldBuffer => true;
        public RunLocation location => Location;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteString(json ?? "{}");
            writer.Write(Location);
        }

        public void Deserialize(PacketReader reader)
        {
            json = reader.ReadString();
            Location = reader.Read<RunLocation>();
        }
    }
}
