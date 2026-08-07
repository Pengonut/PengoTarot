#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 每幕地图生成后应用占卜标记。
    /// 挂在 <see cref="Hook.ModifyGeneratedMapLate"/> 的 Postfix：
    /// - 该钩子在 State.Map 赋值、节点创建（NMapScreen.SetMap）之前被调用；
    /// - 同时覆盖「新建地图」（由 <see cref="Hook.ModifyGeneratedMap"/> 内部调用）与「读档恢复」（直接调用）两条路径；
    /// - 每次地图生成恰好执行一次。
    /// </summary>
    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyGeneratedMapLate))]
    public static class DivinationMarkerPatch
    {
        public static void Postfix(IRunState runState, ActMap map, int actIndex)
        {
            TarotMarkerSystem.ApplyMarkers(runState, map, actIndex);
        }
    }
}
