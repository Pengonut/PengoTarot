#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.ConfigFW;
using PengoTarot.Data.Divination;

namespace PengoTarot.Data
{
    /// <summary>
    /// 塔罗奖励（自定义 Reward 栏类型）：标记类占卜的战斗完成后发放到奖励栏。
    /// 点击后按来源占卜弹出其「正位 + 逆位」供选择（availabilityCheck 不满足的那张不生成；
    /// 若正逆均未生成，则兜底一张命运之轮正，保证至少可选）。
    /// 选中后复用 <see cref="TarotEffectExecutor.ExecuteEffectAndEnchant"/> 执行对应塔罗效果。
    /// </summary>
    /// <remarks>
    /// 自定义 RewardType 使用高位动态枚举值（借鉴 RitsuLib DynamicEnumValueMinter 思路，简化为常量）：
    /// 值 = 0x4000_0000 + flagIndex，完全避开原版 RewardType 的 0~6。
    /// 存档/多人通过 RewardType 反查来源占卜（见 <see cref="TryGetFlagFromRewardType"/>）。
    /// </remarks>
    public sealed class TarotReward : Reward
    {
        /// <summary>动态 RewardType 高位区间起点（原版枚举仅用 0~6）。</summary>
        private const int DynamicRewardTypeFloor = 0x4000_0000;

        /// <summary>flagIndex → 塔罗牌 Id 前缀（标记类占卜发奖励；恋人/命运之轮不发）。</summary>
        private static readonly Dictionary<int, string> TarotIdPrefixByFlag = new()
        {
            { 7, "CHARIOT" },
            { 8, "STRENGTH" },
            { 9, "HERMIT" },
            { 11, "JUSTICE" },
            { 12, "HANGED_MAN" },
            { 13, "DEATH" },
        };

        /// <summary>兜底塔罗（来源占卜的正位/逆位均不可用时，无条件给一张）。</summary>
        private const string FallbackTarotId = "WHEEL_OF_FORTUNE_UPRIGHT";

        private readonly int _flagIndex;

        /// <summary>来源占卜的难度开关索引（见 <see cref="NConfigFloatingWindow.FlagNames"/>）。</summary>
        public int FlagIndex => _flagIndex;

        public TarotReward(Player player, int flagIndex) : base(player)
        {
            _flagIndex = flagIndex;
        }

        /// <summary>flagIndex → 动态 RewardType（存档/多人据此反查来源占卜）。</summary>
        public static RewardType GetRewardTypeForFlag(int flagIndex) => (RewardType)(DynamicRewardTypeFloor + flagIndex);

        /// <summary>从动态 RewardType 反查来源占卜；非塔罗奖励类型返回 false。</summary>
        public static bool TryGetFlagFromRewardType(RewardType type, out int flagIndex)
        {
            int v = (int)type - DynamicRewardTypeFloor;
            if (TarotIdPrefixByFlag.ContainsKey(v))
            {
                flagIndex = v;
                return true;
            }
            flagIndex = 0;
            return false;
        }

        protected override RewardType RewardType => GetRewardTypeForFlag(_flagIndex);

        public override int RewardsSetIndex => 9;

        public override bool IsPopulated => true;

        public override LocString Description => new("gameplay_ui", "BAL_CFW_TAROT_REWARD_DESC");

        protected override string? IconPath => TarotMarkerSystem.GetMarkerIconPath(_flagIndex);

        public override void Populate()
        {
        }

        public override void MarkContentAsSeen()
        {
        }

        /// <summary>点击奖励：弹出该占卜的正位/逆位选择（不可跳过），选中后执行对应塔罗效果。</summary>
        protected override async Task<bool> OnSelect()
        {
            var player = Player;
            var defs = CollectDefs(player);
            if (defs.Count == 0)
                return false;

            var cards = new List<CardModel>();
            foreach (var def in defs)
            {
                var card = (CardModel)typeof(ModelDb)
                    .GetMethod("Card", Type.EmptyTypes)!
                    .MakeGenericMethod(def.CardType)
                    .Invoke(null, null)!;
                cards.Add(card);
            }

            var context = new SilentPlayerChoiceContext();
            // 允许跳过（不选则视为放弃本次塔罗奖励）
            var selected = await CardSelectCmd.FromChooseACardScreen(context, cards, player, canSkip: true);
            if (selected == null)
                return false;

            var chosenDef = defs[cards.IndexOf(selected)];
            await TarotEffectExecutor.ExecuteEffectAndEnchant(chosenDef, player, isLocalBuyer: true);
            return true;
        }

        /// <summary>收集可选塔罗：来源占卜的正位 + 逆位（availabilityCheck 过滤），全无则兜底命运之轮正（强制）。</summary>
        private List<TarotDef> CollectDefs(Player player)
        {
            var result = new List<TarotDef>();
            if (TarotIdPrefixByFlag.TryGetValue(_flagIndex, out var prefix))
            {
                AddIfAvailable(result, player, prefix + "_UPRIGHT");
                AddIfAvailable(result, player, prefix + "_REVERSED");
            }
            if (result.Count == 0)
            {
                // 兜底：无条件给一张命运之轮正（保证至少可选）
                var fallback = TarotDeck.All.FirstOrDefault(d => d.Id == FallbackTarotId);
                if (fallback != null)
                    result.Add(fallback);
            }
            return result;
        }

        private static void AddIfAvailable(List<TarotDef> list, Player player, string id)
        {
            var def = TarotDeck.All.FirstOrDefault(d => d.Id == id);
            if (def == null)
                return;
            if (def.AvailabilityCheck != null && !def.AvailabilityCheck(player))
                return;
            list.Add(def);
        }

        public override SerializableReward ToSerializable()
            => new() { RewardType = GetRewardTypeForFlag(_flagIndex) };
    }
}
