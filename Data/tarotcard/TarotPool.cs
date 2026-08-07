using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Cards;
namespace PengoTarot.Data
{
    public sealed class TarotPool : CardPoolModel
    {
        public override string Title => "tarot";
        public override string EnergyColorName => "colorless";
        public override string CardFrameMaterialPath => "card_frame_quest";
        public override Color DeckEntryCardColor => new Color("E8D086");
        public override Color EnergyOutlineColor => new Color("4A3B2C");
        public override bool IsColorless => false;
        protected override CardModel[] GenerateAllCards()
        {
            return new CardModel[]
            {
                ModelDb.Card<TarFoolUpright>(),
                ModelDb.Card<TarFoolReversed>(),
                ModelDb.Card<TarMagicianUpright>(),
                ModelDb.Card<TarMagicianReversed>(),
                ModelDb.Card<TarHighPriestessUpright>(),
                ModelDb.Card<TarHighPriestessReversed>(),
                ModelDb.Card<TarEmpressUpright>(),
                ModelDb.Card<TarEmpressReversed>(),
                ModelDb.Card<TarEmperorUpright>(),
                ModelDb.Card<TarEmperorReversed>(),
                ModelDb.Card<TarHierophantUpright>(),
                ModelDb.Card<TarHierophantReversed>(),
                ModelDb.Card<TarLoversUpright>(),
                ModelDb.Card<TarLoversReversed>(),
                ModelDb.Card<TarChariotUpright>(),
                ModelDb.Card<TarChariotReversed>(),
                ModelDb.Card<TarStrengthUpright>(),
                ModelDb.Card<TarStrengthReversed>(),
                ModelDb.Card<TarHermitUpright>(),
                ModelDb.Card<TarHermitReversed>(),
                ModelDb.Card<TarWheelOfFortuneUpright>(),
                ModelDb.Card<TarWheelOfFortuneReversed>(),
                ModelDb.Card<TarJusticeUpright>(),
                ModelDb.Card<TarJusticeReversed>(),
                ModelDb.Card<TarHangedManUpright>(),
                ModelDb.Card<TarHangedManReversed>(),
                ModelDb.Card<TarDeathUpright>(),
                ModelDb.Card<TarDeathReversed>(),
                ModelDb.Card<TarTemperanceUpright>(),
                ModelDb.Card<TarTemperanceReversed>(),
                ModelDb.Card<TarDevilUpright>(),
                ModelDb.Card<TarDevilReversed>(),
                ModelDb.Card<TarTowerUpright>(),
                ModelDb.Card<TarTowerReversed>(),
                ModelDb.Card<TarStarUpright>(),
                ModelDb.Card<TarStarReversed>(),
                ModelDb.Card<TarMoonUpright>(),
                ModelDb.Card<TarMoonReversed>(),
                ModelDb.Card<TarSunUpright>(),
                ModelDb.Card<TarSunReversed>(),
                ModelDb.Card<TarJudgementUpright>(),
                ModelDb.Card<TarJudgementReversed>(),
                ModelDb.Card<TarWorldUpright>(),
                ModelDb.Card<TarWorldReversed>(),
                
                ModelDb.Card<TarDevilUprightSub>(),
                ModelDb.Card<TarDevilReversedSub>(),
                ModelDb.Card<TarStarUprightSub>(),
                ModelDb.Card<TarStarReversedSub>(),
                ModelDb.Card<TarMoonUprightSub>(),
                ModelDb.Card<TarMoonReversedSub>(),
                ModelDb.Card<TarSunUprightSub>(),
                ModelDb.Card<TarSunReversedSub>(),
                ModelDb.Card<TarWorldUprightSub>(),
                ModelDb.Card<TarWorldReversedSub>(),
            };
        }
    }
}