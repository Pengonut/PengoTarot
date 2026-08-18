#nullable enable

using HarmonyLib;
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
    /// 普通房间标记占卜的战斗效果（进入被标记普通房间的战斗时生效）：
    /// - 正义(11)：给每个玩家挂 <see cref="TarJusticeReversedPower"/>（攻击牌获得侵蚀，打出后消耗）
    /// - 倒吊人(12)：给每个玩家挂 <see cref="TarHangedManReversedPower"/>（技能牌获得侵蚀，打出后消耗）
    /// - 死神(13)：给每个玩家挂 <see cref="TarDeathReversedPower"/>（打出能力牌立即结束回合）
    ///
    /// 挂在 <see cref="Hook.BeforeCombatStart"/> Postfix：此时玩家已加入 combatState、战斗未开始，
    /// 两端确定性执行（同幕标记同坐标，多人可复现）。
    /// 必须 async void（fire-and-forget）：PowerCmd.Apply 内有依赖主循环 tick 的等待，
    /// 在 Harmony Postfix 里同步阻塞会死锁（参照 TarTemperanceDivinationPatch）。
    /// </summary>
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
    public static class NormalDivinationPowerPatch
    {
        private const int JusticeFlag = 11;
        private const int HangedManFlag = 12;
        private const int DeathFlag = 13;

        [HarmonyPostfix]
        static async void Postfix(IRunState runState, ICombatState? combatState)
        {
            if (!RunManager.Instance.IsInProgress) return;
            if (combatState == null) return;
            if (runState.CurrentMapCoord is not { } coord) return;
            // 严格条件：只对「本幕」的普通战斗房间生效（防止旧幕残留标记 + 跨幕坐标重叠时误伤当前幕房间）
            if (runState.CurrentMapPoint is not { PointType: MapPointType.Monster }) return;

            var flags = TarotMarkerSystem.GetMarkedFlagsAt(coord, runState.CurrentActIndex);
            bool justice = flags.Contains(JusticeFlag);
            bool hanged = flags.Contains(HangedManFlag);
            bool death = flags.Contains(DeathFlag);
            if (!justice && !hanged && !death) return;

            var context = new ThrowingPlayerChoiceContext();
            foreach (var player in combatState.Players)
            {
                if (justice)
                    await PowerCmd.Apply<TarJusticeReversedPower>(context, player.Creature, 1m, player.Creature, null);
                if (hanged)
                    await PowerCmd.Apply<TarHangedManReversedPower>(context, player.Creature, 1m, player.Creature, null);
                if (death)
                    await PowerCmd.Apply<TarDeathReversedPower>(context, player.Creature, 1m, player.Creature, null);
            }
        }
    }
}
