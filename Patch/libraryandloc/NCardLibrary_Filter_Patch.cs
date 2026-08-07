
#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary._Ready))]
    public static class NCardLibrary_TarotFilter_Patch
    {
        private static readonly FieldInfo _lastHoveredControlField =
            typeof(NCardLibrary).GetField("_lastHoveredControl", BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly MethodInfo _updateCardPoolFilterMethod =
            typeof(NCardLibrary).GetMethod("UpdateCardPoolFilter", BindingFlags.NonPublic | BindingFlags.Instance)!;

        static void Postfix(NCardLibrary __instance)
        {
            
            var poolFiltersField = typeof(NCardLibrary).GetField("_poolFilters",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (poolFiltersField == null) return;

            var filters = poolFiltersField.GetValue(__instance)
                as Dictionary<NCardPoolFilter, Func<CardModel, bool>>;
            if (filters == null) return;

            
            var ancientsFilter = __instance.GetNode<NCardPoolFilter>("%AncientsPool");
            if (ancientsFilter != null && filters.ContainsKey(ancientsFilter))
            {
                filters[ancientsFilter] = c => c.Rarity == CardRarity.Ancient && c.Pool is not TarotPool && c.Pool is not PlanetPool;
            }

            
            var templateFilter = ancientsFilter;
            if (templateFilter == null) return;

            var parent = templateFilter.GetParent();
            var tarotFilter = (NCardPoolFilter)templateFilter.Duplicate();
            tarotFilter.Name = "TarotPool";

            
            tarotFilter.Loc = new LocString("card_library", "POOL_TAROT_TIP");

            
            var imageNode = tarotFilter.GetNode<TextureRect>("Image");
            if (imageNode != null)
            {
                
                imageNode.Material = (Material)imageNode.Material.Duplicate();

                var iconPath = "res://images/ui/filter_icons/tarot_filter_icon.png";
                var iconTexture = ResourceLoader.Load<Texture2D>(iconPath);
                if (iconTexture != null)
                    imageNode.Texture = iconTexture;
            }

            
            parent.AddChild(tarotFilter);

            
            filters[tarotFilter] = c => c.Pool is TarotPool && !c.GetType().Name.EndsWith("Sub");

            
            var updateCallable = Callable.From(
                (Action<NCardPoolFilter>)Delegate.CreateDelegate(
                    typeof(Action<NCardPoolFilter>), __instance, _updateCardPoolFilterMethod));
            tarotFilter.Connect(NCardPoolFilter.SignalName.Toggled, updateCallable);

            
            tarotFilter.Connect(Control.SignalName.FocusEntered, Callable.From(() =>
            {
                _lastHoveredControlField.SetValue(__instance, tarotFilter);
            }));

            
            var planetFilter = (NCardPoolFilter)templateFilter.Duplicate();
            planetFilter.Name = "PlanetPool";

            
            planetFilter.Loc = new LocString("card_library", "POOL_PLANET_TIP");

            
            var planetImageNode = planetFilter.GetNode<TextureRect>("Image");
            if (planetImageNode != null)
            {
                
                planetImageNode.Material = (Material)planetImageNode.Material.Duplicate();

                var planetIconPath = "res://images/enchantments/planet_earth_enchantment.png";
                var planetIconTexture = ResourceLoader.Load<Texture2D>(planetIconPath);
                if (planetIconTexture != null)
                    planetImageNode.Texture = planetIconTexture;
            }

            
            parent.AddChild(planetFilter);

            
            filters[planetFilter] = c => c.Pool is PlanetPool && !c.GetType().Name.EndsWith("Sub");

            
            var planetUpdateCallable = Callable.From(
                (Action<NCardPoolFilter>)Delegate.CreateDelegate(
                    typeof(Action<NCardPoolFilter>), __instance, _updateCardPoolFilterMethod));
            planetFilter.Connect(NCardPoolFilter.SignalName.Toggled, planetUpdateCallable);

            
            planetFilter.Connect(Control.SignalName.FocusEntered, Callable.From(() =>
            {
                _lastHoveredControlField.SetValue(__instance, planetFilter);
            }));
        }
    }
}