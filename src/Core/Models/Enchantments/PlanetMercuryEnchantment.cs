// PengoTarot/Enchantments/PlanetMercuryEnchantment.cs
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetMercuryEnchantment : EnchantmentModel
    {
        public override bool HasExtraCardText => true;
        public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Power;

        public override bool CanEnchant(CardModel card)
        {
            if (!base.CanEnchant(card)) return false;
            if (card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly) return false;
            return true;
        }

        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
        {
            if (cardPlay?.Target?.Player == null) return;

            var power = ModelDb.Power<PlanetMercuryPower>().ToMutable();
            if (power is PlanetMercuryPower mercuryPower)
                mercuryPower.PairedPlayer = cardPlay.Target.Player;

            await PowerCmd.Apply(choiceContext, power,
                base.Card.Owner.Creature, 1m,
                base.Card.Owner.Creature, base.Card);
        }
    }
}