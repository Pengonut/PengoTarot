
#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.Data;

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
        }

        public void Dispose()
        {
            _messageBuffer.UnregisterMessageHandler<TarotPurchaseRequestMessage>(HandleTarotPurchaseRequest);
            _messageBuffer.UnregisterMessageHandler<ForcePlayerSyncMessage>(HandleForcePlayerSyncMessage);
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
    }
}