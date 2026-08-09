
#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(PowerModel), "get_PackedIconPath")]
    public static class PowerModel_PackedIconPath_Patch
    {
        private const string GoldIconPath = "res://images/packed/sprite_fonts/gold_icon.png";

        static bool Prefix(PowerModel __instance, ref string __result)
        {
            if (__instance is TarTemperanceReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_temperance_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarChariotReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_chariot_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarStrengthReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_strength_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarJusticeReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_justice_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarHangedManReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_hanged_man_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarDeathReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_death_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarHermitReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_hermit_reversed_enchantment.png");
                return false;
            }
            if (__instance is PlanetGoldPower)
            {
                __result = GoldIconPath;
                return false;
            }
            if (__instance is TickTackPower)
            {
                __result = ImageHelper.GetImagePath(
                    "atlases/power_atlas.sprites/borrowed_time_power.tres");
                return false;
            }
            if (__instance is PlanetEarthPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_earth_enchantment.png");
                return false;
            }
            if (__instance is PlanetJupiterPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_jupiter_enchantment.png");
                return false;
            }
            if (__instance is PlanetMarsPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_mars_enchantment.png");
                return false;
            }
            if (__instance is PlanetMercuryPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_mercury_enchantment.png");
                return false;
            }
            if (__instance is PlanetSaturnPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_saturn_enchantment.png");
                return false;
            }
            if (__instance is PlanetVenusPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_venus_enchantment.png");
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PowerModel), "get_ResolvedBigIconPath")]
    public static class PowerModel_BigIconPath_Patch
    {
        private const string GoldIconPath = "res://images/packed/sprite_fonts/gold_icon.png";

        static bool Prefix(PowerModel __instance, ref string __result)
        {
            if (__instance is TarTemperanceReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_temperance_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarChariotReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_chariot_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarStrengthReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_strength_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarJusticeReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_justice_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarHangedManReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_hanged_man_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarDeathReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_death_reversed_enchantment.png");
                return false;
            }
            if (__instance is TarHermitReversedPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/tar_hermit_reversed_enchantment.png");
                return false;
            }
            if (__instance is PlanetGoldPower)
            {
                __result = GoldIconPath;
                return false;
            }
            if (__instance is TickTackPower)
            {
                __result = ImageHelper.GetImagePath(
                    "atlases/power_atlas.sprites/borrowed_time_power.tres");
                return false;
            }
            if (__instance is PlanetEarthPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_earth_enchantment.png");
                return false;
            }
            if (__instance is PlanetJupiterPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_jupiter_enchantment.png");
                return false;
            }
            if (__instance is PlanetMarsPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_mars_enchantment.png");
                return false;
            }
            if (__instance is PlanetMercuryPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_mercury_enchantment.png");
                return false;
            }
            if (__instance is PlanetSaturnPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_saturn_enchantment.png");
                return false;
            }
            if (__instance is PlanetVenusPower)
            {
                __result = ImageHelper.GetImagePath(
                    "enchantments/planet_venus_enchantment.png");
                return false;
            }
            return true;
        }
    }
}