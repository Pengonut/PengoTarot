// PengoTarot/Enchantments/PlanetJupiterEnchantment.cs
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetJupiterEnchantment : EnchantmentModel
    {
        public override bool HasExtraCardText => true;
        public override bool ShouldGlowRed => true;

        public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

        public override bool CanEnchant(CardModel card)
        {
            if (!base.CanEnchant(card)) return false;
            if (card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly) return false;
            return true;
        }

        public override async Task AfterDamageGiven(
            PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
            ValueProp props, Creature target, CardModel? cardSource)
        {
            if (cardSource != base.Card) return;
            if (!props.IsPoweredAttack()) return;

            if (target.GetPower<PlanetJupiterHitMarkerPower>() == null)
            {
                if (target.GetPower<PlanetJupiterPower>() == null)
                {
                    var jupiterPower = (PlanetJupiterPower)ModelDb.Power<PlanetJupiterPower>().ToMutable();
                    await PowerCmd.Apply(choiceContext, jupiterPower, target, 1m, dealer, base.Card);
                }
                else
                {
                    var jupiterPower = (PlanetJupiterPower)ModelDb.Power<PlanetJupiterPower>().ToMutable();
                    await PowerCmd.Apply(choiceContext, jupiterPower, target, 1m, dealer, base.Card);
                }
            }

            await PowerCmd.Apply<PlanetJupiterHitMarkerPower>(
                choiceContext, target, 1m, dealer, base.Card);
        }

        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
        {
            if (base.Card.CombatState != null)
            {
                foreach (var enemy in base.Card.CombatState.Enemies)
                {
                    var hitMarker = enemy.GetPower<PlanetJupiterHitMarkerPower>();
                    if (hitMarker == null) continue;

                    await PowerCmd.Remove(hitMarker);
                }
            }
            await PowerCmd.Apply<TickTackPower>(
                choiceContext, base.Card.Owner.Creature, 10m,
                base.Card.Owner.Creature, base.Card);
        }
    }
}