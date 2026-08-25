#nullable enable

using HarmonyLib;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;
using PengoTarot.Powers;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 精英标记占卜的战斗效果（进入被标记精英房间的战斗时生效）：
    /// - 战车(7)：给每个敌人挂 <see cref="TarChariotReversedPower"/>（未格挡伤害 → 玩家易伤）
    /// - 力量(8)：给每个敌人挂 <see cref="TarStrengthReversedPower"/>（未格挡伤害 → 玩家虚弱）
    /// - 隐者(9)：给每个敌人挂 <see cref="TarHermitReversedPower"/>，Amount = 最大生命×10%
    ///   （塔1 Plated Armor 机制：回合结束获得等量格挡，受到未格挡攻击伤害时减 1 层）。
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
        static void Postfix(IRunState runState, ICombatState? combatState, ref Task __result)
        {
            // Hook.BeforeCombatStart 本身会被 CombatManager await。把附加逻辑并入返回 Task，
            // 保证双方都在首回合/首次 checksum 前完成 Power 挂载；禁止 async void 越过同步边界。
            __result = ApplyAfterOriginalAsync(__result, runState, combatState);
        }

        private static async Task ApplyAfterOriginalAsync(
            Task originalTask, IRunState runState, ICombatState? combatState)
        {
            await originalTask;
            if (!RunManager.Instance.IsInProgress) return;
            if (combatState == null) return;
            if (runState.CurrentMapCoord is not { } coord) return;
            // 严格条件：只对「本幕」的精英房间生效（防止旧幕残留标记 + 跨幕坐标重叠时误伤当前幕房间）
            if (runState.CurrentMapPoint is not { PointType: MapPointType.Elite }) return;

            var flags = TarotMarkerSystem.GetMarkedFlagsAt(coord, runState.CurrentActIndex);
            bool chariot = flags.Contains(ChariotFlag);
            bool strength = flags.Contains(StrengthFlag);
            bool hermit = flags.Contains(HermitFlag);
            if (!chariot && !strength && !hermit) return;

            var context = new ThrowingPlayerChoiceContext();
            foreach (var enemy in combatState.Enemies)
            {
                if (chariot)
                {
                    if (enemy.GetPower<TarChariotReversedPower>() == null)
                        await PowerCmd.Apply<TarChariotReversedPower>(context, enemy, 1m, enemy, null);
                }
                if (strength)
                {
                    if (enemy.GetPower<TarStrengthReversedPower>() == null)
                        await PowerCmd.Apply<TarStrengthReversedPower>(context, enemy, 1m, enemy, null);
                }
                if (hermit)
                {
                    if (enemy.GetPower<TarHermitReversedPower>() == null)
                        await PowerCmd.Apply<TarHermitReversedPower>(
                            context, enemy, decimal.Round(enemy.MaxHp * HermitPlatingRatio), enemy, null);
                }
            }
        }
    }
}
