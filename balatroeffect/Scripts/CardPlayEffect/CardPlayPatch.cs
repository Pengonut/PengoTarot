// PengoTarot: patches card play lifecycle for shader rotation effect.
// Start:  NPlayerHand.StartCardPlay (drag start, v107+)
// End:    CardModel.OnPlayWrapper Postfix (fade-out)
// Cancel: NCardPlay.Cleanup (cancel fade-out)

#nullable enable

using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace PengoTarot.BalatroEffect
{
    public static class CardPlayPatch
    {
        private static readonly PropertyInfo? CardProp =
            AccessTools.Property(typeof(NCardPlay), "Card");

        /// <summary>Player starts dragging a card → begin rotation effect.</summary>
        [HarmonyPatch(typeof(NPlayerHand), "StartCardPlay")]
        public static class StartCardPlay_Patch
        {
            public static void Prefix(NHandCardHolder holder)
            {
                var model = holder.CardNode?.Model;
                if (model != null)
                    CardPlayTracker.MarkDragStarted(model.Id.ToString());
            }
        }

        /// <summary>Card successfully played → begin fade-out.</summary>
        [HarmonyPatch(typeof(CardModel), "OnPlayWrapper")]
        public static class OnPlayWrapper_Patch
        {
            public static void Postfix(CardModel __instance)
            {
                CardPlayTracker.MarkPlayFinished(__instance.Id.ToString());
            }
        }

        /// <summary>Card play cancelled → begin fade-out.</summary>
        [HarmonyPatch(typeof(NCardPlay), "Cleanup")]
        public static class Cleanup_Patch
        {
            public static void Postfix(NCardPlay __instance)
            {
                var model = CardProp?.GetValue(__instance) as CardModel;
                if (model != null)
                    CardPlayTracker.MarkPlayFinished(model.Id.ToString());
            }
        }
    }
}
