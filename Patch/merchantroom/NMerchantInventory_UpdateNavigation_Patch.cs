
#nullable enable

using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using PengoTarot.ConfigFW;
using PengoTarot.UI;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(NMerchantInventory), "UpdateNavigation")]
    public static class NMerchantInventory_UpdateNavigation_Patch
    {
        static void Postfix(NMerchantInventory __instance)
        {
            // 占卜-愚者未勾选：不参与塔罗槽导航
            if (!ConfigFloatingWindowRunData.GetTarFlag(0)) return;

            var slotsContainer = __instance.GetNodeOrNull<Control>("%SlotsContainer");
            if (slotsContainer == null) return;

            var tarotNode = slotsContainer.GetNodeOrNull<NMerchantTarot>("MerchantTarot");
            if (tarotNode == null || !tarotNode.Visible) return;

            var removalNode = __instance.GetNodeOrNull<NMerchantCardRemoval>("%MerchantCardRemoval");

            
            
            
            if (removalNode != null && removalNode.Visible)
            {
                tarotNode.FocusNeighborLeft = removalNode.GetPath();
                removalNode.FocusNeighborRight = tarotNode.GetPath();
            }
            else
            {
                
                tarotNode.FocusNeighborLeft = tarotNode.GetPath();
            }

            
            tarotNode.FocusNeighborRight = tarotNode.GetPath();

            
            
            var characterContainer = __instance.GetNodeOrNull<Control>("%CharacterCards");
            var characterSlots = characterContainer?.GetChildren().OfType<NMerchantSlot>().Where(s => s.Visible).ToList()
                                 ?? new List<NMerchantSlot>();
            if (characterSlots.Count > 0)
            {
                tarotNode.FocusNeighborTop = characterSlots[0].GetPath();
                
                foreach (var slot in characterSlots)
                {
                    if (slot.FocusNeighborBottom == null || slot.FocusNeighborBottom == slot.GetPath())
                    {
                        slot.FocusNeighborBottom = tarotNode.GetPath();
                    }
                }
            }
            else
            {
                tarotNode.FocusNeighborTop = tarotNode.GetPath();
            }

            
            tarotNode.FocusNeighborBottom = tarotNode.GetPath();
        }
    }
}