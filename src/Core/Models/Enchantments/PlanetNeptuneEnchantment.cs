// PengoTarot/Enchantments/PlanetNeptuneEnchantment.cs
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetNeptuneEnchantment : EnchantmentModel
    {
        public override bool HasExtraCardText => true;

        public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

        public override bool CanEnchant(CardModel card)
        {
            if (!base.CanEnchant(card)) return false;
            if (card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly) return false;
            return true;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card != base.Card) return;
            if (base.Card.CombatState == null) return;

            foreach (var player in base.Card.CombatState.Players)
            {
                var clone = PlanetMarsPower.CreateCloneForPlayer(base.Card, player);
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Discard, player));
            }
        }
    }
}
