
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Extensions;
using PengoTarot.ConfigFW;
using PengoTarot.Enchantments;
using PengoTarot.Network;

namespace PengoTarot.Data
{
    public sealed class MerchantTarotEntry : MerchantEntry
    {
        private bool _used;
        private List<CardModel>? _pendingCards;
        private Dictionary<CardModel, TarotDef>? _pendingMap;

        public override bool IsStocked => !_used;
        public bool HasPurchased => _used;

        public MerchantTarotEntry(Player player) : base(player)
        {
            var rng = player.PlayerRng.Shops;
            // 塔罗包价格 = 基础价(175-200) + 本局价格偏移(默认购买后+50，女祭司-50抵消) + 教皇(-100)
            int baseMin = Math.Max(0, ConfigFloatingWindowRunData.TarotPriceMin);
            int baseMax = Math.Max(0, ConfigFloatingWindowRunData.TarotPriceMax);
            int offset = ConfigFloatingWindowRunData.TarotPriceOffset;
            int papal = ConfigFloatingWindowRunData.GetTarFlag(5) ? -100 : 0;  // 占卜-教皇：整体价降低100
            // 价格 = 基础价(175-200) + 本局价格偏移(默认购买后+50，女祭司-50抵消，最低不会为负) + 教皇(-100)
            _cost = rng.NextInt(baseMin, baseMax + 1) + offset + papal;
        }

        public override void CalcCost() { }

        protected override async Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost)
        {
            if (_used) return (false, 0);
            int goldCost = ignoreCost ? 0 : Cost;

            bool isMultiplayer = !RunManager.Instance.IsSingleplayerOrFakeMultiplayer && ModInitializer.TarotSync != null;

            if (isMultiplayer)
            {
                var success = await DoMultiplayerPurchase(goldCost, ignoreCost);
                if (success)
                {
                    _used = true;
                    AdjustPriceAfterPurchase();
                }
                return (success, goldCost);
            }
            else
            {
                bool success = await DoPurchaseWithCache(_player, goldCost, ignoreCost);
                if (success)
                {
                    _used = true;
                    AdjustPriceAfterPurchase();
                    return (true, goldCost);
                }
                return (false, 0);
            }
        }

        /// <summary>
        /// 购买塔罗包后的价格调整：
        /// 塔罗包默认行为：每次购买后整体价 +50（与愚者无关——愚者只控制卡包是否出现）；
        /// 占卜-女祭司：整体价 -50，开启后抵消默认 +50（默认配置下价格维持不变；关闭则 +50 涨价）。
        /// </summary>
        private static void AdjustPriceAfterPurchase()
        {
            ConfigFloatingWindowRunData.AdjustTarotPrice(50);  // 塔罗包默认：购买后涨价 +50
            if (ConfigFloatingWindowRunData.GetTarFlag(2)) ConfigFloatingWindowRunData.AdjustTarotPrice(-50);  // 女祭司：抵消
        }

        private async Task<bool> DoPurchaseWithCache(Player player, int goldCost, bool ignoreCost)
        {
            if (_pendingCards == null)
                await InitPendingCards(player, null);

            var selected = await ShowTarotSelection(player, canSkip: true);
            if (selected == null) return false;

            if (goldCost > 0 && !ignoreCost)
                await PlayerCmd.LoseGold(goldCost, player, GoldLossType.Spent);

            await TarotEffectExecutor.ExecuteEffectAndEnchant(_pendingMap![selected], player, isLocalBuyer: true);

            ClearPending();
            return true;
        }

        private async Task<bool> DoMultiplayerPurchase(int goldCost, bool ignoreCost)
        {
            List<string>? cachedIds = null;
            if (_pendingCards != null && _pendingMap != null)
                cachedIds = _pendingMap.Values.Select(d => d.Id).ToList();

            var msg = new TarotPurchaseRequestMessage
            {
                goldCost = goldCost,
                cachedDefIds = cachedIds,
                Location = default
            };
            ModInitializer.TarotSync!.SendMessage(msg);

            return await ExecuteNetworkedPurchase(_player, goldCost, ignoreCost, cachedIds, isLocalBuyer: true, canSkip: true);
        }

        public async Task<bool> ExecuteNetworkedPurchase(Player player, int goldCost, bool ignoreCostThis,
            List<string>? cachedDefIds, bool isLocalBuyer, bool canSkip)
        {
            if (_pendingCards == null)
            {
                if (cachedDefIds != null && cachedDefIds.Count > 0)
                {
                    _pendingCards = new List<CardModel>();
                    _pendingMap = new Dictionary<CardModel, TarotDef>();
                    foreach (var defId in cachedDefIds)
                    {
                        var def = TarotDeck.All.First(d => d.Id == defId);
                        var card = (CardModel)typeof(ModelDb)
                            .GetMethod("Card", Type.EmptyTypes)!
                            .MakeGenericMethod(def.CardType)
                            .Invoke(null, null)!;
                        _pendingCards.Add(card);
                        _pendingMap[card] = def;
                    }
                }
                else
                {
                    await InitPendingCards(player, null);
                }
            }

            var selected = await ShowTarotSelection(player, canSkip);
            if (selected == null)
                return false;

            if (isLocalBuyer && goldCost > 0 && !ignoreCostThis)
                await PlayerCmd.LoseGold(goldCost, player, GoldLossType.Spent);

            await TarotEffectExecutor.ExecuteEffectAndEnchant(_pendingMap![selected], player, isLocalBuyer);

            ClearPending();
            return true;
        }

        public static async Task<bool> HandleRemoteTarotPurchase(Player player, int goldCost, List<string>? cachedDefIds)
        {
            return await ExecuteNetworkedPurchaseStatic(player, goldCost, cachedDefIds, false, false);
        }

        private static async Task<bool> ExecuteNetworkedPurchaseStatic(Player player, int goldCost,
            List<string>? cachedDefIds, bool isLocalBuyer, bool canSkip)
        {
            List<CardModel> pendingCards;
            Dictionary<CardModel, TarotDef> pendingMap;

            if (cachedDefIds != null && cachedDefIds.Count > 0)
            {
                pendingCards = new List<CardModel>();
                pendingMap = new Dictionary<CardModel, TarotDef>();
                foreach (var defId in cachedDefIds)
                {
                    var def = TarotDeck.All.First(d => d.Id == defId);
                    var card = (CardModel)typeof(ModelDb)
                        .GetMethod("Card", Type.EmptyTypes)!
                        .MakeGenericMethod(def.CardType)
                        .Invoke(null, null)!;
                    pendingCards.Add(card);
                    pendingMap[card] = def;
                }
            }
            else
            {
                var rng = player.PlayerRng.Shops;
                var defs = DrawTarotPackOptions(player, rng);
                pendingCards = new List<CardModel>();
                pendingMap = new Dictionary<CardModel, TarotDef>();
                foreach (var def in defs)
                {
                    var card = (CardModel)typeof(ModelDb)
                        .GetMethod("Card", Type.EmptyTypes)!
                        .MakeGenericMethod(def.CardType)
                        .Invoke(null, null)!;
                    pendingCards.Add(card);
                    pendingMap[card] = def;
                }
            }

            var context = new SilentPlayerChoiceContext();
            var selected = await CardSelectCmd.FromChooseACardScreen(context, pendingCards, player, canSkip);

            if (selected == null)
                return false;

            if (isLocalBuyer && goldCost > 0)
                await PlayerCmd.LoseGold(goldCost, player, GoldLossType.Spent);

            await TarotEffectExecutor.ExecuteEffectAndEnchant(pendingMap[selected], player, isLocalBuyer);
            return true;
        }

        private async Task<CardModel?> ShowTarotSelection(Player player, bool canSkip)
        {
            var context = new SilentPlayerChoiceContext();
            return await CardSelectCmd.FromChooseACardScreen(context, _pendingCards!, player, canSkip);
        }

        // 塔罗效果执行已提取到 TarotEffectExecutor（Data/tarotcard/TarotEffectExecutor.cs），商店与塔罗奖励共用。

        /// <summary>塔罗包抽取选项：占卜-愚者基础 1 张；占卜-魔术师 +2 → 3 张。皇后/皇帝决定是否含逆位/角色专属。</summary>
        private static List<TarotDef> DrawTarotPackOptions(Player player, Rng rng)
        {
            int count = 1 + (ConfigFloatingWindowRunData.GetTarFlag(1) ? 2 : 0);  // 愚者基础 1；魔术师 +2 → 3
            bool reversed = ConfigFloatingWindowRunData.GetTarFlag(3);             // 占卜-皇后：加入逆位
            bool charSpecific = ConfigFloatingWindowRunData.GetTarFlag(4);         // 占卜-皇帝：加入角色专属
            return TarotDeck.DrawUnique(player, rng, count, reversed, charSpecific);
        }

        private Task InitPendingCards(Player player, List<string>? defIds)
        {
            var rng = player.PlayerRng.Shops;
            List<TarotDef> defs;
            if (defIds != null)
                defs = defIds.Select(id => TarotDeck.All.First(d => d.Id == id)).ToList();
            else
                defs = DrawTarotPackOptions(player, rng);

            _pendingCards = new List<CardModel>();
            _pendingMap = new Dictionary<CardModel, TarotDef>();
            foreach (var def in defs)
            {
                var card = (CardModel)typeof(ModelDb)
                    .GetMethod("Card", Type.EmptyTypes)!
                    .MakeGenericMethod(def.CardType)
                    .Invoke(null, null)!;
                _pendingCards.Add(card);
                _pendingMap[card] = def;
            }
            return Task.CompletedTask;
        }

        private void ClearPending()
        {
            _pendingCards = null;
            _pendingMap = null;
        }

        protected override void ClearAfterPurchase() { }
        protected override void RestockAfterPurchase(MerchantInventory? inventory) { }
    }
}