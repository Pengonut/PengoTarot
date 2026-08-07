// PengoTarot/Enchantments/PlanetXEnchantment.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetXEnchantment : EnchantmentModel
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, new DynamicVar("Times", 1)) };
        
        private bool _usedThisCombat;

        public override bool HasExtraCardText => true;

        public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Skill;

        public override bool CanEnchant(CardModel card)
        {
            if (!base.CanEnchant(card)) return false;
            if (card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly) return false;
            return true;
        }

        public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
        {
            if (_usedThisCombat) return Task.CompletedTask;
            _usedThisCombat = true;
            Status = EnchantmentStatus.Disabled;

            if (base.Card.CombatState == null) return Task.CompletedTask;

            foreach (var player in base.Card.CombatState.Players.Where(p => p != base.Card.Owner))
            foreach (var card in player.PlayerCombatState!.AllCards)
                if (card.Enchantment is PlanetXEnchantment)
                    card.BaseReplayCount += 4;

            return Task.CompletedTask;
        }
    }
}
