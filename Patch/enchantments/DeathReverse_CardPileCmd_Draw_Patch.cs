
#nullable enable
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using PengoTarot.Enchantments;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Commands.CardPileCmd), "CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot")]
    public static class CardPileCmd_CheckIfDraw_Patch
    {
        static bool Prefix(Player player, ref bool __result)
        {
            var hand = PileType.Hand.GetPile(player);
            if (hand.Cards.Any(c => c.Enchantment is TarDeathReversedEnchantment))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}