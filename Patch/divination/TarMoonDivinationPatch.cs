#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch.Card;

/// <summary>
/// 月亮（Moon, 索引18）难度开关效果：
/// 每次你的[gold]抽牌堆[/gold]打乱洗牌前，丢弃你的所有手牌，洗牌后额外抽等量的牌。
///
/// 实现原理：
/// - <see cref="CardPileCmd.Shuffle"/> 是战斗中洗牌的唯一入口（抽牌堆抽空自动洗牌、
///   以及 Reboot 卡等主动洗牌都走它；战斗开始的初始洗牌是另一条路径且那时手牌为空 → 无副作用）。
/// - Prefix 在月亮开启时把原方法替换为 <see cref="MoonShuffle"/>：
///   1. 丢弃当前手牌（进弃牌堆 → 随本次洗牌混进新抽牌堆）；
///   2. 调用原版 <see cref="CardPileCmd.Shuffle"/>（重入守卫 <see cref="_inMoonShuffle"/> 防递归，
///      正常触发 Hook.AfterShuffle 等监听）；
///   3. 用 <see cref="CardPileCmd.Draw"/> 抽回与丢弃等量的牌。
/// - 丢弃的手牌在洗牌前进入弃牌堆，因此新抽牌堆必然包含等量可抽的牌（除非被「弃置时返回手牌」等
///   效果带走，属正常游戏交互）。
/// - Prefix 不能 await，因此用「替换 __result + return false」的模式（参照恶魔 LoseMaxHp 补丁）。
///
/// 仅当「配置开启（GetTarFlag(18)）且当前在一局游戏中」时生效（主菜单/图鉴等不在跑局场景不生效）。
/// 只影响玩家的牌堆（怪物无牌堆）。
/// </summary>
public static class TarMoonDivinationPatch
{
    /// <summary>Moon 在 FlagNames 中的索引。</summary>
    private const int MoonFlagIndex = 18;

    /// <summary>重入守卫：防止 MoonShuffle 内部调用原版 Shuffle/Draw 时再次触发本效果。</summary>
    private static bool _inMoonShuffle;

    /// <summary>是否应生效：配置开启 且 当前在一局游戏中（主菜单/图鉴不生效）。</summary>
    private static bool ShouldApply()
        => ConfigFloatingWindowRunData.GetTarFlag(MoonFlagIndex)
           && RunManager.Instance.IsInProgress;

    /// <summary>
    /// 洗牌前丢弃所有手牌、洗牌后抽回等量牌。
    /// 目标：public static async Task Shuffle(PlayerChoiceContext, Player)
    /// </summary>
    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Shuffle))]
    public static class CardPileCmd_Shuffle_MoonPatch
    {
        [HarmonyPrefix]
        static bool Prefix(PlayerChoiceContext choiceContext, Player player, ref Task __result)
        {
            if (!ShouldApply() || _inMoonShuffle)
                return true; // 原逻辑（含 MoonShuffle 内部调用原版洗牌的重入放行）

            __result = MoonShuffle(choiceContext, player);
            return false; // 跳过原方法
        }

        /// <summary>月亮版洗牌：丢弃所有手牌 → 原版洗牌 → 抽回等量。</summary>
        private static async Task MoonShuffle(PlayerChoiceContext choiceContext, Player player)
        {
            // 与原版 Shuffle 一致：战斗已结束/即将结束时什么都不做（不丢牌）
            if (CombatManager.Instance.IsOverOrEnding)
                return;

            CardPile hand = PileType.Hand.GetPile(player);
            int count = hand.Cards.Count;

            // 守卫必须总是提前置位：count==0 时（如回合起始抽牌触发的洗牌、手牌为空）也要放行原版洗牌，
            // 不能把洗牌本身跳过，否则该回合会一张牌都抽不到。
            _inMoonShuffle = true;
            try
            {
                // 1. 丢弃所有手牌（进弃牌堆，随本次洗牌混进新抽牌堆）；手牌为空则跳过
                if (count > 0)
                    await CardCmd.Discard(choiceContext, hand.Cards.ToList());
                // 2. 原版洗牌（守卫使本 Prefix 直接放行 → 正常执行，含 Hook.AfterShuffle）总是执行
                await CardPileCmd.Shuffle(choiceContext, player);
                // 3. 洗牌后抽回等量；手牌为空时无需抽回
                if (count > 0 && !CombatManager.Instance.IsOverOrEnding)
                    await CardPileCmd.Draw(choiceContext, count, player);
            }
            finally
            {
                _inMoonShuffle = false;
            }
        }
    }
}
