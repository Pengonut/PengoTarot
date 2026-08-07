#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 修复 EnchantmentModel / PowerModel / AfflictionModel 的 HoverTip
    /// 没有设置 CanonicalModel 的问题。
    /// 使用 Harmony 嵌套类模式，确保 PatchAll 100% 发现。
    /// </summary>
    public static class ModelHoverTipCanonicalPatch
    {
        [HarmonyPatch(typeof(EnchantmentModel), "get_HoverTip")]
        public static class Enchantment
        {
            [HarmonyPostfix]
            internal static void Postfix(EnchantmentModel __instance, ref HoverTip __result)
            {
                __result.SetCanonicalModel(__instance.CanonicalInstance);
            }
        }

        [HarmonyPatch(typeof(PowerModel), "GetDumbHoverTip")]
        public static class Power
        {
            [HarmonyPostfix]
            internal static void Postfix(PowerModel __instance, ref HoverTip __result)
            {
                __result.SetCanonicalModel(__instance);
            }
        }

        [HarmonyPatch(typeof(AfflictionModel), "get_HoverTip")]
        public static class Affliction
        {
            [HarmonyPostfix]
            internal static void Postfix(AfflictionModel __instance, ref HoverTip __result)
            {
                __result.SetCanonicalModel(__instance.CanonicalInstance);
            }
        }
    }
}
