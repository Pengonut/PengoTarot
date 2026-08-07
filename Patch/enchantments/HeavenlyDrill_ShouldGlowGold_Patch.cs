#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Cards;
using PengoTarot.Enchantments;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(HeavenlyDrill), "get_ShouldGlowGoldInternal")]
    public static class HeavenlyDrill_ShouldGlowGoldInternal_Patch
    {
        static bool Prefix(HeavenlyDrill __instance, ref bool __result)
        {
            var enchantment = __instance.Enchantment as TarStarReversedEnchantment;
            if (enchantment == null)
                return true; 

            int threshold = __instance.DynamicVars["Energy"].IntValue; 

            if (enchantment.IsSwappedToStarX)
            {
                
                __result = __instance.Owner.PlayerCombatState!.Stars >= threshold;
            }
            else 
            {
                
                __result = __instance.Owner.PlayerCombatState!.Energy >= threshold;
            }

            return false; 
        }
    }
}