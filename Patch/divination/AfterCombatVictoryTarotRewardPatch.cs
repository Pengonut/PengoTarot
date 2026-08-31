#nullable enable

using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 战斗胜利后发放塔罗奖励：若当前战斗房间被「标记类」占卜标记，则记录完成
    /// （<see cref="TarotMarkerSystem.OnMarkedCombatVictory"/>）并按其规则发放 <see cref="TarotReward"/>。
    /// </summary>
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    public static class AfterCombatVictoryTarotRewardPatch
    {
        static void Postfix(IRunState runState, ICombatState? combatState, CombatRoom room)
        {
            if (!RunManager.Instance.IsInProgress)
                return;
            if (runState.CurrentMapCoord is not { } coord)
                return;

            Log.Info($"[PengoTarot] [DivinationReward] victory peer={RunManager.Instance.NetService.Type} " +
                     $"act={runState.CurrentActIndex} coord={coord} room={room.RoomType}");

            var players = runState.Players.ToList();
            TarotMarkerSystem.OnMarkedCombatVictory(coord, runState.CurrentActIndex, room, players);
            // 战斗胜利由所有 peer 确定性复现。此处若广播递增后的主机快照，
            // 客机可能先应用快照、随后又执行本地 Postfix，导致同一场战斗计数两次。
            // 主机快照只用于地图初始化、读档和重连等非战斗恢复路径。
        }
    }
}
