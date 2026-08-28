#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using PengoTarot.Cards;

namespace PengoTarot.Patch.VisualVanilla
{
    /// <summary>
    /// When viewing Tarot/Planet cards in the card library, repurpose the
    /// "查看升级" checkbox as a vanilla-style toggle. "查看数据" is hidden
    /// for mod pools. Original behaviour is fully preserved for other pools.
    /// </summary>
    [HarmonyPatch(typeof(NCardLibrary))]
    public static class CardLibraryVanillaPatch
    {
        private static readonly HashSet<NCardLibrary> _handlerAdded = new();
        internal static bool CurrentIsTarot;
        internal static bool CurrentIsPlanet;

        /// <summary>
        /// FilterCards has two overloads; specify the two-parameter one that
        /// DisplayCards actually calls. Runs sync after _cards is populated.
        /// </summary>
        [HarmonyPatch(typeof(NCardLibraryGrid), "FilterCards", typeof(Func<CardModel, bool>), typeof(List<SortingOrders>)), HarmonyPostfix]
        static void GridFilterCards_Postfix(NCardLibraryGrid __instance)
        {
            for (Node? cur = __instance.GetParent(); cur != null; cur = cur.GetParent())
            {
                if (cur is NCardLibrary lib)
                {
                    UpdateToggleState(lib);
                    return;
                }
            }
        }

        internal static void UpdateToggleState(NCardLibrary library)
        {
            var viewUpgrades = library.GetNodeOrNull<NTickbox>("%Upgrades");
            var viewStats = library.GetNodeOrNull<NTickbox>("%Stats");
            if (viewUpgrades == null) return;

            var tr = Traverse.Create(library);
            var grid = tr.Field("_grid").GetValue();
            var visibleCards = grid != null
                ? Traverse.Create(grid).Property("VisibleCards").GetValue() as IEnumerable<CardModel>
                : null;
            var cards = visibleCards?.ToList();
            bool allTarot = cards != null && cards.Count > 0 && cards.All(c => c is TarCard);
            bool allPlanet = cards != null && cards.Count > 0 && cards.All(c => c is PlanetCard);
            bool isModPool = allTarot || allPlanet;

            CurrentIsTarot = allTarot;
            CurrentIsPlanet = allPlanet;

            if (!_handlerAdded.Contains(library))
            {
                _handlerAdded.Add(library);
                viewUpgrades.Connect("Toggled", Callable.From<NTickbox>(nt =>
                {
                    if (!CurrentIsTarot && !CurrentIsPlanet) return;
                    bool ticked = nt.IsTicked;
                    if (CurrentIsTarot)
                        VanillaStyleConfig.TarotVanilla = ticked;
                    else
                        VanillaStyleConfig.PlanetVanilla = ticked;

                    ResetIsShowingUpgrades(library);
                    AnimateRefresh(library);
                }));
            }

            if (isModPool)
            {
                viewUpgrades.Visible = true;
                viewUpgrades.MouseFilter = Control.MouseFilterEnum.Stop;

                bool isVanilla = allTarot ? VanillaStyleConfig.TarotVanilla : VanillaStyleConfig.PlanetVanilla;
                viewUpgrades.SetBlockSignals(true);
                viewUpgrades.IsTicked = isVanilla;
                viewUpgrades.SetBlockSignals(false);
                ResetIsShowingUpgrades(library);

                var label = viewUpgrades.FindChild("Label", recursive: true, owned: false) as Label;
                if (label != null)
                    label.Text = allTarot
                        ? new LocString("gameplay_ui", "VANILLA_STYLE_TAROT").GetFormattedText()
                        : new LocString("gameplay_ui", "VANILLA_STYLE_PLANET").GetFormattedText();

                if (viewStats != null)
                {
                    viewStats.Visible = false;
                    viewStats.MouseFilter = Control.MouseFilterEnum.Ignore;
                }
            }
            else
            {
                var label = viewUpgrades.FindChild("Label", recursive: true, owned: false) as Label;
                if (label != null)
                    label.Text = new LocString("card_library", "VIEW_UPGRADES").GetRawText();

                if (viewStats != null)
                {
                    viewStats.Visible = true;
                    viewStats.MouseFilter = Control.MouseFilterEnum.Stop;
                }
            }
        }

        private static void ResetIsShowingUpgrades(NCardLibrary library)
        {
            var tr = Traverse.Create(library);
            var grid = tr.Field("_grid").GetValue();
            if (grid != null)
                Traverse.Create(grid).Property("IsShowingUpgrades").SetValue(false);
        }

        private static async void AnimateRefresh(NCardLibrary library)
        {
            var grid = Traverse.Create(library).Field("_grid").GetValue();
            if (grid == null) return;

            var animateOut = grid.GetType().GetMethod("AnimateOut", Type.EmptyTypes);
            if (animateOut != null)
            {
                var t = animateOut.Invoke(grid, null);
                if (t is System.Threading.Tasks.Task task)
                    await task;
            }

            var displayCards = typeof(NCardLibrary).GetMethod("DisplayCards",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (displayCards != null)
            {
                var t = displayCards.Invoke(library, null);
                if (t is System.Threading.Tasks.Task task)
                    await task;
            }
        }

        /// <summary>Called from inspect toggle to sync library checkbox state.</summary>
        internal static void SyncLibraryToggle()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null) return;
            var lib = tree.Root.FindChild("CardLibrary", recursive: true, owned: false) as NCardLibrary;
            if (lib != null) UpdateToggleState(lib);
        }
    }
}
