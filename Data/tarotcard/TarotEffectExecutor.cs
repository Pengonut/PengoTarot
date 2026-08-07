#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Data
{
    /// <summary>
    /// 塔罗效果执行器：把「选中一张塔罗牌后执行其效果」的逻辑从商店塔罗包（MerchantTarotEntry）提取为公共工具，
    /// 供商店购买与塔罗奖励（TarotReward）复用，保证两处行为一致。
    /// 逻辑与原先 MerchantTarotEntry.ExecuteEffectAndEnchant / NotifyStateChange 完全一致，未做任何行为改动。
    /// </summary>
    public static class TarotEffectExecutor
    {
        /// <summary>
        /// 执行选中塔罗的效果：
        /// - 立即效果（命运之轮/高塔等）或 SUB 附魔变换：仅本地购买者（isLocalBuyer）执行，随后发全量同步；
        /// - 普通附魔效果：选出目标卡并附魔（多端一致，CardCmd.Enchant 自带同步）。
        /// </summary>
        public static async Task ExecuteEffectAndEnchant(TarotDef def, Player player, bool isLocalBuyer)
        {
            if (def.IsImmediateEffect)
            {
                // SUB（负片附魔变换）：从目标池生成候选并让玩家选牌变换
                if (def.Enchantment != null && def.Id.EndsWith("_SUB"))
                {
                    var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1, def.CardsToEnchant)
                    {
                        Cancelable = false,
                        RequireManualConfirmation = true
                    };

                    var optionsMap = new Dictionary<CardModel, List<CardModel>>();

                    CardTransformation BuildTransformation(CardModel card)
                    {
                        var targetPool = TarotImmediateEffects.GetTargetPool(def.Id);
                        var candidates = targetPool.AllCardIds
                            .Select(id => ModelDb.GetById<CardModel>(id))
                            .Where(c => c != null && c.Type == card.Type && c.Rarity == card.Rarity)
                            .Select(c => player.RunState.CreateCard(c, player))
                            .ToList();

                        foreach (var c in candidates)
                        {
                            CardCmd.Enchant(def.Enchantment.ToMutable(), c, 1m);
                        }
                        optionsMap[card] = candidates;

                        return new CardTransformation(card, candidates);
                    }

                    var chosenCards = await CardSelectCmd.FromDeckForTransformation(player, prefs, BuildTransformation);
                    if (chosenCards == null || !chosenCards.Any())
                        return;

                    if (isLocalBuyer)
                    {
                        var rng = player.PlayerRng.Shops;
                        foreach (var oldCard in chosenCards)
                        {
                            if (!optionsMap.TryGetValue(oldCard, out var candidates) || candidates.Count == 0)
                                continue;
                            var newCard = rng.NextItem(candidates)!;
                            await CardCmd.Transform(oldCard, newCard);
                        }
                        NotifyStateChange(player);
                    }
                }
                // 其余立即效果：仅本地购买者执行
                else if (isLocalBuyer)
                {
                    var rng = player.PlayerRng.Shops;
                    switch (def.Id)
                    {
                        case "WHEEL_OF_FORTUNE_UPRIGHT":
                            var allRelics = player.Relics.Where(r => r.Rarity != RelicRarity.Ancient).ToList();
                            RelicModel? targetRelic = null;
                            if (allRelics.Count > 0)
                            {
                                var nonStarter = allRelics.Where(r => r.Rarity != RelicRarity.Starter).ToList();
                                targetRelic = nonStarter.Count > 0 ? rng.NextItem(nonStarter) : rng.NextItem(allRelics);
                            }
                            await TarotImmediateEffects.WheelOfFortuneUpright(player, targetRelic);
                            NotifyStateChange(player);
                            break;

                        case "WHEEL_OF_FORTUNE_REVERSED":
                            var nonAncient = player.Relics.Where(r => r.Rarity != RelicRarity.Ancient).ToList();
                            if (nonAncient.Count >= 3)
                            {
                                var list = new List<RelicModel>(nonAncient);
                                var toRemove = new List<RelicModel>();
                                for (int i = 0; i < 2; i++)
                                {
                                    var relic = rng.NextItem(list);
                                    if (relic != null)
                                    {
                                        toRemove.Add(relic);
                                        list.Remove(relic);
                                    }
                                }
                                var toClone = list.Count > 0 ? rng.NextItem(list) : null;
                                await TarotImmediateEffects.WheelOfFortuneReversed(player, toRemove, toClone);
                                NotifyStateChange(player);
                            }
                            break;

                        case "TOWER_UPRIGHT":
                            await TarotImmediateEffects.TowerUpright(player);
                            NotifyStateChange(player);
                            break;

                        case "TOWER_REVERSED":
                            await TarotImmediateEffects.TowerReversed(player);
                            NotifyStateChange(player);
                            break;
                    }
                }
            }
            else
            {
                // 普通附魔：选目标卡并附魔（多端一致）
                var enchantment = def.Enchantment!;
                int targetCount = def.CardsToEnchant;
                int minSelect = (def.Id is "LOVERS_UPRIGHT" or "LOVERS_REVERSED") ? targetCount : 1;
                var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, minSelect, targetCount)
                {
                    Cancelable = false,
                    RequireManualConfirmation = true
                };
                var chosenCards = await CardSelectCmd.FromDeckForEnchantment(player, enchantment, targetCount, prefs);
                if (chosenCards != null)
                {
                    foreach (var card in chosenCards)
                    {
                        CardCmd.Enchant(enchantment.ToMutable(), card, 1m);
                        if (LocalContext.IsMe(player))
                            CardCmd.Preview(card);
                    }
                }
            }
        }

        /// <summary>非单机时向远端全量同步该玩家状态（立即效果只在本地执行，需同步差异）。</summary>
        public static void NotifyStateChange(Player player)
        {
            if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer && ModInitializer.TarotSync != null)
            {
                ModInitializer.TarotSync.SendForceSyncMessage(player.ToSerializable());
            }
        }
    }
}
