#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;
using PengoTarot.Powers;

namespace PengoTarot.Patch.Card;

/// <summary>
/// 节制（Temperance, 索引14）难度开关效果：
/// 1. 当节制启用时，玩家每次打出一张牌，使自身获得 1 层「节制-逆」。
///    「节制-逆」= <see cref="TarTemperanceReversedPower"/>：本回合内每受到 1 点未被格挡的伤害，
///    获得 1 金币（层数叠加），敌方回合结束时移除。
/// 2. 战斗胜利时不再获得金币奖励（普通/精英/Boss 的基础金币奖励；药水/卡/遗物保留；
///    节制逆受伤金币、事件、商店、遗物效果等其它途径发放的金币不受影响）。
///
/// 仅当「配置开启（GetTarFlag(14)）且当前在一局游戏中」时生效（主菜单/图鉴等不在跑局场景不生效）。
/// </summary>
public static class TarTemperanceDivinationPatch
{
    /// <summary>Temperance 在 FlagNames 中的索引。</summary>
    private const int TemperanceFlagIndex = 14;

    /// <summary>是否应生效：配置开启 且 当前在一局游戏中（主菜单/图鉴不生效）。</summary>
    private static bool ShouldApply()
        => ConfigFloatingWindowRunData.GetTarFlag(TemperanceFlagIndex)
           && RunManager.Instance.IsInProgress;

    /// <summary>
    /// 每次出牌后：若出牌者是我方玩家且节制开关开启，给该玩家叠加 1 层节制逆。
    ///
    /// 必须用 async void（fire-and-forget）：PowerCmd.Apply 内部有 Cmd.CustomScaledWait 等
    /// 依赖主循环 tick 的等待，在 Harmony Postfix 里同步阻塞（GetAwaiter().GetResult()）会死锁。
    /// choiceContext 来自 Hook.AfterCardPlayed 原方法参数，多人下由它负责同步本次 power 应用。
    /// </summary>
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
    public static class Hook_AfterCardPlayed_TemperancePatch
    {
        [HarmonyPostfix]
        static async void Postfix(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var owner = cardPlay.Card.Owner.Creature;
            if (!owner.IsPlayer || !ShouldApply())
                return;

            await PowerCmd.Apply<TarTemperanceReversedPower>(
                choiceContext, owner, 1m, owner, cardPlay.Card);
        }
    }

    /// <summary>
    /// 战斗胜利金币奖励拦截：patch RewardsSet.GenerateRewardsFor（战斗/宝库房间默认奖励生成）。
    /// 该方法只在普通(Monster)/精英(Elite)/Boss 三种战斗房间添加 GoldReward，其余奖励（药水/卡/遗物）保留；
    /// 教程奖励与 combatRoom.ExtraRewards 不经过该方法 → 不受影响（ExtraRewards 属「其它途径」应保留）。
    /// 注意：不要用 CombatRoom.GoldProportion=0，它只拦普通房间（Elite/Boss 分支无条件加金币）。
    /// </summary>
    [HarmonyPatch(typeof(RewardsSet), "GenerateRewardsFor",
        typeof(Player), typeof(AbstractRoom))]
    public static class RewardsSet_GenerateRewardsFor_TemperancePatch
    {
        [HarmonyPostfix]
        static void Postfix(ref List<Reward> __result)
        {
            if (!ShouldApply())
                return;

            __result.RemoveAll(r => r is GoldReward);
        }
    }
}
