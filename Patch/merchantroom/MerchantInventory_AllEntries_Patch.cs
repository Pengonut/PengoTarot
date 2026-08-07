using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using PengoTarot.ConfigFW;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(MerchantInventory), "get_AllEntries")]
    public static class MerchantInventory_AllEntries_Patch
    {
        static void Postfix(ref IEnumerable<MerchantEntry> __result, MerchantInventory __instance)
        {
            // 占卜-愚者未勾选：不把塔罗卡包加入商人商品列表
            if (!ConfigFloatingWindowRunData.GetTarFlag(0)) return;

            var tarotEntry = TarotEntryHolder.GetEntry(__instance);
            if (tarotEntry != null)
                __result = __result.Concat(new[] { tarotEntry });
        }
    }
}