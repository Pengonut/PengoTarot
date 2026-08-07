/*

#nullable enable

using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.addons.mega_text;
using PengoTarot.Cards;

namespace PengoTarot.Patch.VisualVanilla
{
    [HarmonyPatch(typeof(NInspectCardScreen), "SetCard")]
    public static class InspectVanillaStylePatch
    {
        private static readonly HashSet<NInspectCardScreen> _initialized = new();
        private static bool _currentIsTarot;
        private static bool _currentIsPlanet;

        public static void Postfix(NInspectCardScreen __instance, int index, List<CardModel> ____cards)
        {
            if (____cards is null || index < 0 || index >= ____cards.Count) return;

            var model = ____cards[index];
            bool isTarot = model is TarCard;
            bool isPlanet = model is PlantCard;
            bool isModCard = isTarot || isPlanet;
            _currentIsTarot = isTarot;
            _currentIsPlanet = isPlanet;

            var upgradeNode = __instance.GetNodeOrNull<NTickbox>("%Upgrade");
            if (upgradeNode == null) return;

            var label = upgradeNode.GetNodeOrNull<Label>("ShowUpgradeLabel");
            var controllerIcon = upgradeNode.GetNodeOrNull<Control>("ControllerIcon");

            if (isModCard)
            {
                if (!_initialized.Contains(__instance))
                {
                    _initialized.Add(__instance);
                    var conns = upgradeNode.GetSignalConnectionList("Toggled");
                    foreach (var dict in conns)
                        upgradeNode.Disconnect("Toggled", (Callable)dict["callable"]);

                    upgradeNode.Connect("Toggled", Callable.From<NTickbox>(nt =>
                    {
                        bool ticked = nt.IsTicked;
                        if (_currentIsTarot)
                        {
                            VanillaStyleConfig.TarotVanilla = ticked;
                            RefreshAllModCards(isTarot: true);
                        }
                        else if (_currentIsPlanet)
                        {
                            VanillaStyleConfig.PlanetVanilla = ticked;
                            RefreshAllModCards(isTarot: false);
                        }
                        CardLibraryVanillaPatch.SyncLibraryToggle();
                        var tr = Traverse.Create(__instance);
                        var cards = tr.Field<List<CardModel>>("_cards").Value;
                        var idx = tr.Field<int>("_index").Value;
                        if (cards != null && idx >= 0 && idx < cards.Count)
                        {
                            var card = __instance.GetNodeOrNull<NCard>("Card");
                            if (card != null)
                            {
                                card.Model = null!;
                                card.Model = cards[idx];
                                card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
                            }
                        }
                    }));
                }

                upgradeNode.Visible = true;
                upgradeNode.MouseFilter = Control.MouseFilterEnum.Stop;

                bool isVanilla = isTarot ? VanillaStyleConfig.TarotVanilla : VanillaStyleConfig.PlanetVanilla;
                if (label is MegaLabel megaLabel)
                    megaLabel.SetTextAutoSize(isTarot
                        ? new LocString("gameplay_ui", "VANILLA_STYLE_TAROT").GetFormattedText()
                        : new LocString("gameplay_ui", "VANILLA_STYLE_PLANET").GetFormattedText());
                else if (label != null)
                    label.Text = isTarot
                        ? new LocString("gameplay_ui", "VANILLA_STYLE_TAROT").GetFormattedText()
                        : new LocString("gameplay_ui", "VANILLA_STYLE_PLANET").GetFormattedText();

                upgradeNode.SetBlockSignals(true);
                upgradeNode.IsTicked = isVanilla;
                upgradeNode.SetBlockSignals(false);

                if (controllerIcon != null)
                    controllerIcon.Visible = false;
            }
            else
            {
                if (controllerIcon != null)
                    controllerIcon.Visible = true;
            }
        }

        public static void RefreshAllModCards(bool isTarot)
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null) return;
            RefreshNode(tree.Root, isTarot);
        }

        private static void RefreshNode(Node node, bool isTarot)
        {
            if (node is NCard card && card.Model != null)
            {
                bool match = isTarot ? card.Model is TarCard : card.Model is PlantCard;
                if (match)
                    card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
            }
            foreach (Node child in node.GetChildren())
                RefreshNode(child, isTarot);
        }
    }
}

*/