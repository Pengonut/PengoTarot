
#nullable enable

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using PengoTarot.ConfigFW;
using PengoTarot.UI;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(NMerchantInventory), "GetAllSlots")]
    public static class NMerchantInventory_GetAllSlots_Patch
    {
        static void Postfix(ref IEnumerable<NMerchantSlot> __result, NMerchantInventory __instance)
        {
            // 占卜-愚者未勾选：不把塔罗槽加入商人槽列表
            if (!ConfigFloatingWindowRunData.GetTarFlag(0)) return;

            var slotsContainer = __instance.GetNodeOrNull<Godot.Control>("%SlotsContainer");
            if (slotsContainer == null) return;

            var tarotNode = slotsContainer.GetNodeOrNull<NMerchantTarot>("MerchantTarot");
            if (tarotNode != null)
            {
                __result = __result.Concat(new[] { tarotNode });
            }
        }
    }
}