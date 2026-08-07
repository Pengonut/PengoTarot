#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// 角色选择界面入口按钮注入：OnSubmenuOpened 时注入/刷新（submenu 单例复用，靠 HasNode 防重）。
    /// </summary>
    [HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuOpened")]
    public static class CharacterSelectEntryPatch
    {
        public static void Postfix(NCharacterSelectScreen __instance)
        {
            ConfigFloatingWindow.OnCharacterSelectOpened(__instance);
        }
    }

    /// <summary>
    /// 角色选择界面关闭时清理面板与消息 handler。
    /// </summary>
    [HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuClosed")]
    public static class CharacterSelectClosePatch
    {
        public static void Postfix(NCharacterSelectScreen __instance)
        {
            ConfigFloatingWindow.OnCharacterSelectClosed(__instance);
        }
    }
}
