#nullable enable

using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace PengoTarot.BalatroEffect
{
    /// <summary>
    /// Adds a "全局动态效果 / Global Dynamic Effect" toggle checkbox to the
    /// card library sidebar, under all pool filters. Duplicates the existing
    /// %Upgrades tickbox and repurposes it.
    /// </summary>
    [HarmonyPatch(typeof(NCardLibrary))]
    public static class GlobalDynamicPatch
    {
        private static readonly HashSet<NCardLibrary> _initialized = new();

        [HarmonyPatch("_Ready"), HarmonyPostfix]
        private static void Ready_Postfix(NCardLibrary __instance)
        {
            if (_initialized.Contains(__instance)) return;
            _initialized.Add(__instance);

            var bottomVBox = __instance.GetNodeOrNull<VBoxContainer>("Sidebar/MarginContainer/BottomVBox");
            var upgrades = __instance.GetNodeOrNull<NTickbox>("%Upgrades");
            if (bottomVBox == null || upgrades == null) return;

            // Duplicate the Upgrades tickbox (clean copy, no signal duplication)
            var dup = upgrades.Duplicate() as Control;
            if (dup == null) return;

            dup.Name = "GlobalDynamicEffect";
            dup.UniqueNameInOwner = true;
            bottomVBox.AddChild(dup);
            bottomVBox.MoveChild(dup, upgrades.GetIndex() + 1);

            // Set label text via LocString
            var label = dup.FindChild("Label", recursive: true, owned: false) as Label;
            if (label != null)
            {
                string text = new LocString("gameplay_ui", "BAL_GLOBAL_DYNAMIC_EFFECT").GetFormattedText();
                if (!string.IsNullOrEmpty(text))
                    label.Text = text;
            }

            // Set initial state and wire toggle
            if (dup is NTickbox tickbox)
            {
                tickbox.SetBlockSignals(true);
                tickbox.IsTicked = Config.GlobalDynamicEffect;
                tickbox.SetBlockSignals(false);

                tickbox.Connect("Toggled", Callable.From<NTickbox>(nt =>
                {
                    Config.GlobalDynamicEffect = nt.IsTicked;
                }));
            }
        }
    }
}
