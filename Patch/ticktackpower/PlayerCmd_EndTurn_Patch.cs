#nullable enable

using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(PlayerCmd))]
    public static class PlayerCmd_EndTurn_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerCmd.EndTurn))]
        public static bool Prefix(Player player, bool canBackOut, Func<Task>? actionDuringEnemyTurn)
        {
            var cm = CombatManager.Instance;
            if (cm.IsPlayerReadyToEndTurn(player))
            {
                // 玩家已经 ready，只更新 canBackOut 状态
                // 通过反射触发 PlayerEndedTurn 事件，让 UI 刷新
                var evt = AccessTools.Field(typeof(CombatManager), "PlayerEndedTurn")
                    ?.GetValue(cm) as Action<Player, bool>;
                evt?.Invoke(player, canBackOut);

                return false; // 跳过原方法，避免任何额外副作用
            }
            // 玩家未 ready，正常执行原 EndTurn 逻辑
            return true;
        }
    }
}