#nullable enable

using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// 角色选择界面浮动面板的开关状态同步消息（主机 → 所有客户端）。
    /// 客户端据此打开/关闭只读面板，以跟随主机的画面内容。
    /// </summary>
    public struct ConfigFloatingWindowStateMessage : INetMessage, IPacketSerializable
    {
        /// <summary>消息格式版本（预留校验；两端同版本安全，Serialize/Deserialize 头部对齐）。</summary>
        public int version;
        /// <summary>true = 主机打开面板；false = 主机关闭面板。</summary>
        public bool isOpen;

        public bool ShouldBroadcast => true;
        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.VeryDebug;
        public bool ShouldBuffer => false;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteInt(version, 8);
            writer.WriteBool(isOpen);
        }

        public void Deserialize(PacketReader reader)
        {
            version = reader.ReadInt(8);
            isOpen = reader.ReadBool();
        }
    }
}
