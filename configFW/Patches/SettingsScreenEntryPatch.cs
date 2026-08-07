#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// 设置界面入口按钮注入：仅在 ShowInSettingsOnly 模式下生效。
    /// _Ready 时注入入口悬浮按钮；OnSubmenuClosed/OnSubmenuHidden 时清理。
    /// （不能用 _ExitTree：submenu stack 只是隐藏而非从树移除，_ExitTree 不会触发。）
    /// </summary>
    [HarmonyPatch(typeof(NSettingsScreen), "_Ready")]
    public static class SettingsScreenReadyPatch
    {
        public static void Postfix(NSettingsScreen __instance)
        {
            ConfigFloatingWindow.OnSettingsScreenOpened(__instance);
        }
    }

    /// <summary>
    /// 设置界面关闭时清理入口（涵盖返回按钮关闭、切换到主菜单等路径）。
    /// </summary>
    [HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuClosed")]
    public static class SettingsScreenClosePatch
    {
        public static void Postfix()
        {
            ConfigFloatingWindow.OnSettingsScreenClosed();
        }
    }

    /// <summary>
    /// 设置界面被其他 submenu 遮挡时也清理入口（OnSubmenuHidden）。
    /// </summary>
    [HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuHidden")]
    public static class SettingsScreenHiddenPatch
    {
        public static void Postfix()
        {
            ConfigFloatingWindow.OnSettingsScreenClosed();
        }
    }

    /// <summary>
    /// 设置界面从隐藏恢复时重新注入入口（submenu stack 只隐藏/显示，不会重新 _Ready）。
    /// </summary>
    [HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuShown")]
    public static class SettingsScreenShownPatch
    {
        public static void Postfix(NSettingsScreen __instance)
        {
            ConfigFloatingWindow.OnSettingsScreenOpened(__instance);
        }
    }
}
