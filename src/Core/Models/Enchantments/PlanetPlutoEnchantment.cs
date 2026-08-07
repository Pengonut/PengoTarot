// PengoTarot/Enchantments/PlanetPlutoEnchantment.cs
#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetPlutoEnchantment : EnchantmentModel
    {
        private bool _usedThisCombat;

        public override bool HasExtraCardText => true;

        public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Skill;

        public override bool CanEnchant(CardModel card)
        {
            if (!base.CanEnchant(card)) return false;
            if (card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly) return false;
            return true;
        }

        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
        {
            if (_usedThisCombat) return;
            _usedThisCombat = true;
            Status = EnchantmentStatus.Disabled;

            if (base.Card.CombatState == null) return;

            foreach (var player in base.Card.CombatState.Players.Where(p => p != base.Card.Owner))
            {
                var plutoCards = player.PlayerCombatState!.AllCards
                    .Where(c => c.Enchantment is PlanetPlutoEnchantment)
                    .ToList();

                foreach (var card in plutoCards)
                {
                    await CardPileCmd.Add(card, PileType.Hand);
                    card.EnergyCost.SetThisTurn(0);
                }
            }
        }
    }
}
