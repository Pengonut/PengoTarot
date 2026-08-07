// PengoTarot/Powers/PlanetVenusPower.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Powers
{
    public sealed class PlanetVenusPower : PowerModel
    {
        public Player? PairedPlayer { get; set; }

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            new[] { new StringVar("PairedName") };

        public override Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            if (PairedPlayer != null)
                ((StringVar)DynamicVars["PairedName"]).StringValue =
                    PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, PairedPlayer.NetId);
            return Task.CompletedTask;
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side != CombatSide.Player || PairedPlayer == null) return;
            if (!participants.Contains(base.Owner)) return;

            var discardPile = PileType.Discard.GetPile(PairedPlayer);
            var topCards = discardPile.Cards.Take(5).ToList();
            if (topCards.Count == 0) return;

            var prompt = new LocString("gameplay_ui", "PLANET_VENUS_SELECTION_PROMPT");
            var prefs = new CardSelectorPrefs(prompt, 0, topCards.Count);

            var selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                topCards,
                base.Owner.Player!,
                prefs
            )).ToList();

            Flash();

            if (selected.Count == 0) return;

            foreach (var card in selected)
            {
                await CardPileCmd.Add(card, PileType.Hand);
            }
        }
    }
}