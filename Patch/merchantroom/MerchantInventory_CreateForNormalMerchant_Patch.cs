using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using PengoTarot.ConfigFW;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(MerchantInventory), "CreateForNormalMerchant")]
    public static class MerchantInventory_CreateForNormalMerchant_Patch
    {
        static void Postfix(Player player, ref MerchantInventory __result)
        {
            // 占卜-愚者未勾选：不创建塔罗卡包 entry
            if (!ConfigFloatingWindowRunData.GetTarFlag(0)) return;

            var tarotEntry = new MerchantTarotEntry(player);
            TarotEntryHolder.SetEntry(__result, tarotEntry);
        }
    }
}