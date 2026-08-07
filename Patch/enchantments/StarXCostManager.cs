
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Nodes.Cards;
using System;

namespace PengoTarot.Enchantments
{
    
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.ResolveEnergyXValue))]
    public static class CardModel_ResolveEnergyXValue_Patch
    {
        static bool Prefix(CardModel __instance, ref int __result)
        {
            if (__instance.Enchantment is TarStarReversedEnchantment enchantment && enchantment.IsSwappedToStarX)
            {
                __result = __instance.ResolveStarXValue();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.ResolveStarXValue))]
    public static class CardModel_ResolveStarXValue_Patch
    {
        static bool Prefix(CardModel __instance, ref int __result)
        {
            if (__instance.Enchantment is TarStarReversedEnchantment enchantment && enchantment.IsSwappedToEnergyX)
            {
                __result = __instance.ResolveEnergyXValue();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CardModel), "get_HasStarCostX")]
    public static class CardModel_HasStarCostX_Patch
    {
        static bool Prefix(CardModel __instance, ref bool __result)
        {
            if (__instance.Enchantment is TarStarReversedEnchantment enchantment)
            {
                if (enchantment.IsSwappedToStarX)
                {
                    __result = true;
                    return false;
                }
                if (enchantment.IsSwappedToEnergyX)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    
    [HarmonyPatch(typeof(CardModel), "GetStarCostWithModifiers")]
    public static class CardModel_GetStarCostWithModifiers_Patch
    {
        static bool Prefix(CardModel __instance, ref int __result)
        {
            if (__instance.Enchantment is TarStarReversedEnchantment enchantment && enchantment.IsSwappedToEnergyX)
            {
                __result = 0;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NCard), "UpdateStarCostVisuals")]
    public static class NCard_UpdateStarCostVisuals_StarX_Patch
    {
        static void Postfix(NCard __instance)
        {
            CardModel? model = __instance.Model;
            
            if (model?.Enchantment is TarStarReversedEnchantment enchantment && enchantment.IsSwappedToEnergyX)
            {
                var starIcon = Traverse.Create(__instance).Field<TextureRect>("_starIcon").Value;
                var starLabel = Traverse.Create(__instance).Field<MegaLabel>("_starLabel").Value;
                if (starIcon != null) starIcon.Visible = false;
                if (starLabel != null) starLabel.Text = "";
            }
        }
    }

    public static class StarXReflectionHelper
    {
        private static readonly FieldInfo _baseStarCostField =
            AccessTools.Field(typeof(CardModel), "_baseStarCost");
        private static readonly FieldInfo _starCostSetField =
            AccessTools.Field(typeof(CardModel), "_starCostSet");
        private static readonly FieldInfo _starCostChangedField =
            AccessTools.Field(typeof(CardModel), "StarCostChanged");

        public static void SetStarCost(CardModel card, int cost)
        {
            card.AssertMutable();
            _baseStarCostField.SetValue(card, cost);
            _starCostSetField.SetValue(card, true);

            var handler = (System.Action?)_starCostChangedField.GetValue(card);
            handler?.Invoke();
        }
    }

    
    public static class RuntimeEnergyXCostHelper
    {
        private static readonly FieldInfo CostsXField =
            AccessTools.Field(typeof(CardEnergyCost), "<CostsX>k__BackingField");
        private static readonly FieldInfo BaseField =
            AccessTools.Field(typeof(CardEnergyCost), "_base");
        private static readonly FieldInfo CardField =
            AccessTools.Field(typeof(CardEnergyCost), "_card");
        private static readonly MethodInfo InvokeEnergyCostChangedMethod =
            AccessTools.Method(typeof(CardModel), "InvokeEnergyCostChanged");

        public static void SetCostsX(CardEnergyCost cost, bool value)
        {
            var card = CardField.GetValue(cost) as CardModel;
            if (card == null) return;

            card.AssertMutable();
            CostsXField.SetValue(cost, value);

            if (value)
                BaseField.SetValue(cost, 0);
            else
                BaseField.SetValue(cost, cost.Canonical);

            InvokeEnergyCostChangedMethod.Invoke(card, null);
        }
    }
}