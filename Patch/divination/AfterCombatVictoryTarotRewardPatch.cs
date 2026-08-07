#nullable enable

using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
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

            var players = runState.Players.ToList();
            TarotMarkerSystem.OnMarkedCombatVictory(coord, room, players);
        }
    }
}
