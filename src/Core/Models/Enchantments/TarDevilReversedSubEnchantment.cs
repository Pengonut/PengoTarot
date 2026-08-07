#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace PengoTarot.Enchantments
{
    public sealed class TarDevilReversedSubEnchantment : EnchantmentModel
    {
        private bool _isInHand;

        public override bool HasExtraCardText => true;

        public override async Task AfterCardChangedPiles(CardModel card, PileType oldPile, AbstractModel? clonedBy)
        {
            if (card != base.Card) return;
            bool wasInHand = _isInHand;
            _isInHand = card.Pile?.Type == PileType.Hand;
            if (_isInHand && !wasInHand)
            {
                await PlayerCmd.GainEnergy(1m, card.Owner);
            }
        }
    }
}
