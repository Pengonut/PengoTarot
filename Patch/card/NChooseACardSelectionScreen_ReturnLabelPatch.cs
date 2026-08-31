#nullable enable
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using PengoTarot.Cards;

namespace PengoTarot.Patches;

/// <summary>
/// 塔罗牌与星球牌三选一界面的取消操作实际会返回来源界面，
/// 因此将共用按钮的“跳过”文案替换为更符合行为的“返回”。
/// </summary>
[HarmonyPatch(typeof(NChooseACardSelectionScreen), nameof(NChooseACardSelectionScreen._Ready))]
public static class NChooseACardSelectionScreenReturnLabelPatch
{
    [HarmonyPostfix]
    private static void Postfix(NChooseACardSelectionScreen __instance)
    {
        var cards = Traverse.Create(__instance)
            .Field("_cards")
            .GetValue<IReadOnlyList<CardModel>>();

        if (cards == null || cards.Count == 0 ||
            !cards.All(card => card is TarCard or PlanetCard))
        {
            return;
        }

        var skipButton = __instance.GetNode<NChoiceSelectionSkipButton>("SkipButton");
        var label = Traverse.Create(skipButton).Field("_label").GetValue<MegaLabel>();
        label.SetTextAutoSize(
            new LocString("gameplay_ui", "PENGO_TAROT_RETURN_BUTTON").GetFormattedText());
    }
}
