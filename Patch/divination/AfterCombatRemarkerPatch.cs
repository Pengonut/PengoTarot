#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 防御性补标：每次战斗胜利（完成房间）后检查当前幕地图标记。
    /// 某些读档类 mod 的破坏性代码可能让回档后地图标记完全消失；这里在不影响正常游玩的前提下
    /// （幂等：标记都在时无副作用、零开销），为这类玩家提供「房间完成后检查补标」的兜底。
    /// 触发点与塔罗奖励（<see cref="AfterCombatVictoryTarotRewardPatch"/>）一致：<see cref="Hook.AfterCombatVictory"/>。
    /// </summary>
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    public static class AfterCombatRemarkerPatch
    {
        static void Postfix(IRunState runState)
        {
            if (!RunManager.Instance.IsInProgress)
                return;
            TarotMarkerSystem.TryRemarker(runState);
        }
    }
}
