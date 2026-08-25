
#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.Data;
using PengoTarot.Data.Divination;

namespace PengoTarot.Network
{
    public class TarotSynchronizer : IDisposable
    {
        private readonly RunLocationTargetedMessageBuffer _messageBuffer;
        private readonly INetGameService _gameService;
        private readonly IPlayerCollection _playerCollection;
        private readonly ulong _localPlayerId;

        public TarotSynchronizer(RunLocationTargetedMessageBuffer messageBuffer,
            INetGameService gameService, IPlayerCollection playerCollection, ulong localPlayerId)
        {
            _messageBuffer = messageBuffer;
            _gameService = gameService;
            _playerCollection = playerCollection;
            _localPlayerId = localPlayerId;
            _messageBuffer.RegisterMessageHandler<TarotPurchaseRequestMessage>(HandleTarotPurchaseRequest);
            _messageBuffer.RegisterMessageHandler<ForcePlayerSyncMessage>(HandleForcePlayerSyncMessage);
            _messageBuffer.RegisterMessageHandler<TarotMarkerStateMessage>(HandleTarotMarkerStateMessage);
        }

        public void Dispose()
        {
            _messageBuffer.UnregisterMessageHandler<TarotPurchaseRequestMessage>(HandleTarotPurchaseRequest);
            _messageBuffer.UnregisterMessageHandler<ForcePlayerSyncMessage>(HandleForcePlayerSyncMessage);
            _messageBuffer.UnregisterMessageHandler<TarotMarkerStateMessage>(HandleTarotMarkerStateMessage);
        }

        public void SendMessage(TarotPurchaseRequestMessage msg)
        {
            msg.Location = _messageBuffer.CurrentLocation;
            _gameService.SendMessage(msg);
        }

        public void SendForceSyncMessage(SerializablePlayer serializablePlayer)
        {
            var msg = new ForcePlayerSyncMessage
            {
                player = serializablePlayer,
                Location = _messageBuffer.CurrentLocation
            };
            _gameService.SendMessage(msg);
        }

        /// <summary>仅主机广播标记完整快照；客户端永远不反向覆盖主机。</summary>
        public void BroadcastMarkerState()
        {
            if (_gameService.Type != NetGameType.Host)
                return;

            _gameService.SendMessage(new TarotMarkerStateMessage
            {
                json = TarotMarkerSystem.ToJson().ToJsonString(),
                Location = _messageBuffer.CurrentLocation,
            });
        }

        private void HandleTarotPurchaseRequest(TarotPurchaseRequestMessage message, ulong senderId)
        {
            if (senderId == _localPlayerId) return;

            var player = _playerCollection.GetPlayer(senderId);
            if (player == null) return;

            TaskHelper.RunSafely(MerchantTarotEntry.HandleRemoteTarotPurchase(player, message.goldCost, message.cachedDefIds));
        }

        private void HandleForcePlayerSyncMessage(ForcePlayerSyncMessage message, ulong senderId)
        {
            if (senderId == _localPlayerId) return;

            var player = _playerCollection.GetPlayer(senderId);
            if (player == null) return;

            player.SyncWithSerializedPlayer(message.player);
        }

        private void HandleTarotMarkerStateMessage(TarotMarkerStateMessage message, ulong senderId)
        {
            if (senderId == _localPlayerId || _gameService.Type != NetGameType.Client)
                return;
            if (_gameService is not INetClientGameService clientService
                || clientService.NetClient == null
                || senderId != clientService.NetClient.HostNetId)
                return;

            try
            {
                TarotMarkerSystem.FromJson(JsonNode.Parse(message.json) as JsonObject);
            }
            catch
            {
                // 损坏/版本不匹配的快照不能清空现有状态；等待下一次主机快照自愈。
            }
        }
    }
}
