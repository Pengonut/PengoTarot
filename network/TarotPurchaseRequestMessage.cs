
#nullable enable

using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Network
{
    public struct TarotPurchaseRequestMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
    {
        public int goldCost;
        public List<string>? cachedDefIds;
        public required RunLocation Location { get; set; }

        public bool ShouldBroadcast => true;
        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.VeryDebug;
        public bool ShouldBuffer => true;
        public RunLocation location => Location;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteInt(goldCost);
            bool hasCache = cachedDefIds != null;
            writer.WriteBool(hasCache);
            if (hasCache)
            {
                writer.WriteInt(cachedDefIds!.Count, 8);
                foreach (var id in cachedDefIds)
                    writer.WriteString(id);
            }
            writer.Write(Location);
        }

        public void Deserialize(PacketReader reader)
        {
            goldCost = reader.ReadInt();
            if (reader.ReadBool())
            {
                int count = reader.ReadInt(8);
                cachedDefIds = new List<string>(count);
                for (int i = 0; i < count; i++)
                    cachedDefIds.Add(reader.ReadString());
            }
            Location = reader.Read<RunLocation>();
        }
    }
}