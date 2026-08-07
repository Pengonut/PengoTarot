#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Enchantments;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 当一张卡牌被设为免费（SetToFreeThisCombat）时，如果它正被愚者逆附魔（TarFoolReversedEnchantment）附着，则自动将
    /// 附魔的 IgnoreGenerationCost 设为 true，使其以及未来所有克隆品都永久免费。这主要为癫狂之触药水等一次性资源保留合理的无限。
    /// </summary>
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.SetToFreeThisCombat))]
    public static class CardModel_SetToFreeThisCombat_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(CardModel __instance)
        {
            if (__instance.Enchantment is TarFoolReversedEnchantment foolEnchant)
            {
                foolEnchant.IgnoreGenerationCost = true;
            }
        }
    }
}