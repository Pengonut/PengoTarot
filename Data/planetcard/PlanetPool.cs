#nullable enable
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Cards;

namespace PengoTarot.Data
{
    public sealed class PlanetPool : CardPoolModel
    {
        public override string Title => "planet";
        public override string EnergyColorName => "colorless";
        public override string CardFrameMaterialPath => "card_frame_quest";
        public override Color DeckEntryCardColor => new Color("E8D086");
        public override Color EnergyOutlineColor => new Color("4A3B2C");
        public override bool IsColorless => false;

        protected override CardModel[] GenerateAllCards()
        {
            return new CardModel[]
            {
                ModelDb.Card<PlanetMercury>(),
                ModelDb.Card<PlanetVenus>(),
                ModelDb.Card<PlanetEarth>(),
                ModelDb.Card<PlanetMars>(),
                ModelDb.Card<PlanetJupiter>(),
                ModelDb.Card<PlanetSaturn>(),
                ModelDb.Card<PlanetUranus>(),
                ModelDb.Card<PlanetNeptune>(),
                ModelDb.Card<PlanetPluto>(),
                ModelDb.Card<PlanetX>(),
                ModelDb.Card<PlanetCeres>(),
                ModelDb.Card<PlanetEris>(),
            };
        }
    }
}