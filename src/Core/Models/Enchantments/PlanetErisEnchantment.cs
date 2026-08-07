// PengoTarot/Enchantments/PlanetErisEnchantment.cs
#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetErisEnchantment : EnchantmentModel
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
                var erisCards = player.PlayerCombatState!.AllCards
                    .Where(c => c.Enchantment is PlanetErisEnchantment)
                    .ToList();

                foreach (var card in erisCards)
                {
                    foreach (var targetPlayer in base.Card.CombatState.Players)
                    {
                        var clone = PlanetMarsPower.CreateCloneForPlayer(card, targetPlayer);
                        CardCmd.ClearEnchantment(clone);
                        CardCmd.PreviewCardPileAdd(
                            await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, targetPlayer));
                    }
                }
            }
        }
    }
}
