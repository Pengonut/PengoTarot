#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Cards
{
    public abstract class PlanetCard : CardModel
    {
        protected override int CanonicalEnergyCost => -1;
        public override IEnumerable<CardKeyword> CanonicalKeywords => Enumerable.Empty<CardKeyword>();

        protected PlanetCard(CardType type) : base(0, type, CardRarity.Ancient, TargetType.None)
        {
        }

        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            return Task.CompletedTask;
        }

        protected override void OnUpgrade()
        {
        }

        public override bool HasBuiltInOverlay => false;
    }
}