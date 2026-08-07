using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(CardModel), "get_OverlayPath")]
    public static class CardModel_OverlayPath_Patch
    {
        static void Postfix(ref string __result, CardModel __instance)
        {
            if (__instance.Pool is TarotPool)
            {
                __result = "res://scenes/cards/overlays/tarot_overlay.tscn";
            }
        }
    }
}