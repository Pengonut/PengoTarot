
#nullable enable
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using PengoTarot.ConfigFW;
using PengoTarot.Data;
using PengoTarot.UI;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(NMerchantInventory), "Initialize")]
    public static class NMerchantInventory_Initialize_Patch
    {
        static void Postfix(NMerchantInventory __instance, MerchantInventory inventory)
        {
            // 占卜-愚者未勾选：不挂载塔罗 UI 槽
            if (!ConfigFloatingWindowRunData.GetTarFlag(0)) return;

            var tarotEntry = TarotEntryHolder.GetEntry(inventory);
            if (tarotEntry != null)
            {
                var slot = new NMerchantTarot();
                slot.Name = "MerchantTarot";
                slot.Scale = new Vector2(0.65f, 0.65f);
                slot.Position = new Vector2(1520, 678);
                var slotsContainer = __instance.GetNode<Control>("%SlotsContainer");
                slotsContainer.AddChild(slot);
                slot.Initialize(__instance);
                slot.FillSlot(tarotEntry);

                
                var removalNode = __instance.GetNodeOrNull<Control>("%MerchantCardRemoval");

                
                if (removalNode != null)
                {
                    slot.FocusNeighborLeft = slot.GetPathTo(removalNode);
                    removalNode.FocusNeighborRight = removalNode.GetPathTo(slot);
                }

                
                NMerchantSlot? topSlot = null;
                var characterCards = __instance.GetNodeOrNull<Control>("%CharacterCards");
                if (characterCards?.GetChildCount() > 0)
                    topSlot = characterCards.GetChildren()
                        .OfType<NMerchantSlot>()
                        .LastOrDefault(s => s.Visible);

                if (topSlot == null)
                {
                    var colorlessCards = __instance.GetNodeOrNull<Control>("%ColorlessCards");
                    if (colorlessCards?.GetChildCount() > 0)
                        topSlot = colorlessCards.GetChildren()
                            .OfType<NMerchantSlot>()
                            .LastOrDefault(s => s.Visible);
                }

                if (topSlot != null)
                    slot.FocusNeighborTop = slot.GetPathTo(topSlot);
                else
                    slot.FocusNeighborTop = "."; 

                
                slot.FocusNeighborBottom = ".";
                slot.FocusNeighborRight = ".";
            }

            
            var others = new[] { "%ColorlessCards", "%Relics", "%Potions" };
            foreach (var name in others)
            {
                var container = __instance.GetNodeOrNull<Control>(name);
                if (container != null)
                    container.Position = new Vector2(container.Position.X - 80, container.Position.Y);
            }
            var removal = __instance.GetNodeOrNull<Control>("%MerchantCardRemoval");
            if (removal != null)
                removal.Position = new Vector2(removal.Position.X - 80, removal.Position.Y);
        }
    }
}