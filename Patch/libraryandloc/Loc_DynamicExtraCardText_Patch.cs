
#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(EnchantmentModel), "get_DynamicExtraCardText")]
    public static class EnchantmentModel_DynamicExtraCardText_Patch
    {
        static void Postfix(ref LocString? __result, EnchantmentModel __instance)
        {
            if (__result != null && __instance.HasExtraCardText)
            {
                __result.Add("energyPrefix", EnergyIconHelper.GetPrefix(__instance));
            }
        }
    }
}