// PengoTarot/Enchantments/PlanetSaturnEnchantment.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetSaturnEnchantment : EnchantmentModel
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

            int effectiveDamage = result.TotalDamage + result.OverkillDamage;
            if (effectiveDamage <= 0) return;

            var existing = target.GetPower<PlanetSaturnPower>();
            if (existing != null && existing.Amount >= effectiveDamage)
                return;

            if (existing != null)
                await PowerCmd.Remove(existing);

            var power = (PlanetSaturnPower)ModelDb.Power<PlanetSaturnPower>().ToMutable();
            await PowerCmd.Apply(choiceContext, power, target, effectiveDamage,
                base.Card.Owner.Creature, base.Card);
        }
        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
        {
            await PowerCmd.Apply<TickTackPower>(
                choiceContext, base.Card.Owner.Creature, 10m,
                base.Card.Owner.Creature, base.Card);
        }
    }
}
