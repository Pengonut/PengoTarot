#nullable enable

using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// 本局配置分发消息（主机 → 所有客户端）。
    /// 在开始新多人局时由主机广播，客户端收到后覆盖本机运行时配置，确保整局共享主机配置。
    /// </summary>
    public struct ConfigFloatingWindowDataMessage : INetMessage, IPacketSerializable
    {
        /// <summary>数据格式版本（预留校验；两端同版本安全，Serialize/Deserialize 头部对齐）。</summary>
        public int version;
        public bool tarot;
        public bool planet;
        public int priceMin;
        public int priceMax;
        public bool[] flags;

        public bool ShouldBroadcast => true;
        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.VeryDebug;
        public bool ShouldBuffer => false;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteInt(version, 8);
            writer.WriteBool(tarot);
            writer.WriteBool(planet);
            writer.WriteInt(priceMin);
            writer.WriteInt(priceMax);
            int count = flags?.Length ?? 0;
            writer.WriteInt(count, 8);
            for (int i = 0; i < count; i++)
                writer.WriteBool(flags![i]);
        }

        public void Deserialize(PacketReader reader)
        {
            version = reader.ReadInt(8);
            tarot = reader.ReadBool();
            planet = reader.ReadBool();
            priceMin = reader.ReadInt();
            priceMax = reader.ReadInt();
            int count = reader.ReadInt(8);
            flags = new bool[count];
            for (int i = 0; i < count; i++)
                flags[i] = reader.ReadBool();
        }

        /// <summary>从运行时配置构建。</summary>
        public static ConfigFloatingWindowDataMessage FromRunData()
        {
            int n = ConfigFloatingWindowConfig.DifficultyFlagCount;
            var flags = new bool[n];
            for (int i = 0; i < n; i++)
                flags[i] = ConfigFloatingWindowRunData.GetTarFlag(i);
            return new ConfigFloatingWindowDataMessage
            {
                version = ConfigFloatingWindowRunData.CurrentVersion,
                tarot = ConfigFloatingWindowRunData.TarotEnabled,
                planet = ConfigFloatingWindowRunData.PlanetEnabled,
                priceMin = ConfigFloatingWindowRunData.TarotPriceMin,
                priceMax = ConfigFloatingWindowRunData.TarotPriceMax,
                flags = flags,
            };
        }

        /// <summary>应用到运行时配置。</summary>
        public void ApplyToRunData()
        {
            ConfigFloatingWindowRunData.Apply(tarot, planet, priceMin, priceMax, flags);
        }
    }
}
