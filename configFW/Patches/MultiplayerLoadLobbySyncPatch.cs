#nullable enable

using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Saves;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// 多人「读档继续」的配置同步入口：
    /// LoadRunLobby 创建（主机/客机）时注册配置消息 handler，CleanUp 时注销。
    /// 客机因此能收到主机在 SetUpSavedMultiplayer 之后广播的本局配置（ConfigFloatingWindowDataMessage）
    /// 并覆盖本机 RunData，实现整局共享主机配置。
    /// 只 patch 主构造函数（客户端构造函数链式调用主构造函数），一个 Postfix 覆盖主机+客机两条创建路径。
    /// 仿 RitsuLib RunSavedDataStartRunLobbyCtorPatch 的思路。
    /// </summary>
    [HarmonyPatch(typeof(LoadRunLobby), MethodType.Constructor,
        new Type[] { typeof(INetGameService), typeof(ILoadRunLobbyListener), typeof(SerializableRun) })]
    public static class LoadRunLobbyCtorSyncPatch
    {
        public static void Postfix(LoadRunLobby __instance)
        {
            ConfigFloatingWindow.RegisterSyncForLoadLobby(__instance.NetService);
        }
    }

    /// <summary>LoadRunLobby 关闭（读档屏幕退出/开局完成）时注销配置消息 handler。</summary>
    [HarmonyPatch(typeof(LoadRunLobby), nameof(LoadRunLobby.CleanUp))]
    public static class LoadRunLobbyCleanUpSyncPatch
    {
        public static void Postfix(LoadRunLobby __instance)
        {
            ConfigFloatingWindow.OnLoadLobbyClosed(__instance.NetService);
        }
    }
}
