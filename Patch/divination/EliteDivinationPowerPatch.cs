#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;
using PengoTarot.Powers;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 精英标记占卜的战斗效果（进入被标记精英房间的战斗时生效）：
    /// - 战车(7)：给每个敌人挂 <see cref="TarChariotReversedPower"/>（未格挡伤害 → 玩家易伤）
    /// - 力量(8)：给每个敌人挂 <see cref="TarStrengthReversedPower"/>（未格挡伤害 → 玩家虚弱）
    /// - 隐者(9)：给每个敌人挂游戏自带 <see cref="PlatingPower"/>，Amount = 最大生命×10%
    ///   （PlatingPower 自带第 1 回合开始时获得等量格挡，与青蛙骑士同款）。
    ///
    /// 挂在 <see cref="Hook.BeforeCombatStart"/> Postfix：此时敌人已生成并加入 combatState、战斗未开始，
    /// 两端确定性执行（同幕标记同坐标，多人可复现）。
    /// 必须 async void（fire-and-forget）：PowerCmd.Apply 内有依赖主循环 tick 的等待，
    /// 在 Harmony Postfix 里同步阻塞会死锁（参照 TarTemperanceDivinationPatch）。
    /// </summary>
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
    public static class EliteDivinationPowerPatch
    {
        private const int ChariotFlag = 7;
        private const int StrengthFlag = 8;
        private const int HermitFlag = 9;

        /// <summary>隐者：覆甲/格挡 = 最大生命的该比例。</summary>
        private const decimal HermitPlatingRatio = 0.1m;

        [HarmonyPostfix]
        static async void Postfix(IRunState runState, ICombatState? combatState)
        {
            if (!RunManager.Instance.IsInProgress) return;
            if (combatState == null) return;
            if (runState.CurrentMapCoord is not { } coord) return;

            var flags = TarotMarkerSystem.GetMarkedFlagsAt(coord);
            bool chariot = flags.Contains(ChariotFlag);
            bool strength = flags.Contains(StrengthFlag);
            bool hermit = flags.Contains(HermitFlag);
            if (!chariot && !strength && !hermit) return;

            var context = new ThrowingPlayerChoiceContext();
            foreach (var enemy in combatState.Enemies)
            {
                if (chariot)
                    await PowerCmd.Apply<TarChariotReversedPower>(context, enemy, 1m, enemy, null);
                if (strength)
                    await PowerCmd.Apply<TarStrengthReversedPower>(context, enemy, 1m, enemy, null);
                if (hermit)
                    await PowerCmd.Apply<PlatingPower>(
                        context, enemy, decimal.Round(enemy.MaxHp * HermitPlatingRatio), enemy, null);
            }
        }
    }
}
