// PengoTarot/Enchantments/PlanetUranusEnchantment.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetUranusEnchantment : EnchantmentModel
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, new DynamicVar("Times", 1)) };

        public override bool HasExtraCardText => true;

        public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

        public override bool CanEnchant(CardModel card)
        {
            if (!base.CanEnchant(card)) return false;
            if (card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly) return false;
            return true;
        }

#if STS2_AT_LEAST_0_110_0
        public override CardLocation ModifyCardPlayResultLocation(
            CardModel card, bool isAutoPlay, ResourceInfo resources,
            CardLocation cardLocation)
        {
            if (card == base.Card)
            {
                cardLocation.pileType = PileType.None;
                return cardLocation;
            }
            return cardLocation;
        }
#else
        public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
            CardModel card, bool isAutoPlay, ResourceInfo resources,
            PileType currentPileType, CardPilePosition currentPosition)
        {
            if (card == base.Card)
            {
                return (PileType.None, CardPilePosition.Bottom);
            }
            return (currentPileType, currentPosition);
        }
#endif

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card != base.Card) return;
            if (!cardPlay.IsLastInSeries) return;
            if (base.Card.CombatState == null) return;

            var teammates = base.Card.CombatState.GetTeammatesOf(base.Card.Owner.Creature)
                .Where(c => c.IsPlayer && c.Player != base.Card.Owner).ToList();
            if (teammates.Count == 0) return;

            var rng = base.Card.Owner.RunState.Rng;
            if (rng?.CombatTargets == null) return;
            var target = rng.CombatTargets.NextItem(teammates);
            if (target!.Player == null) return;

            var clone = base.Card.CreateClone();
            clone.EnergyCost.AddThisCombat(1);
            clone.BaseReplayCount += 2;

            var ownerField = typeof(CardModel).GetField("_owner",
                BindingFlags.NonPublic | BindingFlags.Instance);
            ownerField?.SetValue(clone, target.Player);

            CardCmd.PreviewCardPileAdd(
                await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Draw, target.Player, CardPilePosition.Random));
        }
    }
}
