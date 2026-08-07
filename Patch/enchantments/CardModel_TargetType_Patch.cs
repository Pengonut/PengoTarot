// PengoTarot/Patches/CardModel_TargetType_Patch.cs
#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Enchantments;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(CardModel), "get_TargetType")]
    public static class CardModel_TargetType_Patch
    {
        static bool Prefix(CardModel __instance, ref TargetType __result)
        {
            if (__instance.Enchantment is PlanetMercuryEnchantment
                or PlanetVenusEnchantment
                or PlanetEarthEnchantment
                or PlanetMarsEnchantment)
            {
                __result = TargetType.AnyAlly;
                return false;
            }
            return true;
        }
    }
}