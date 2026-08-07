#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// 游戏开始后（NRun 就绪）注入只读入口悬浮窗（所有界面显示、可拖动）。
    /// </summary>
    [HarmonyPatch(typeof(NRun), "_Ready")]
    public static class NRunEntryPatch
    {
        public static void Postfix(NRun __instance)
        {
            ConfigFloatingWindow.OnRunReady(__instance);
        }
    }
}
