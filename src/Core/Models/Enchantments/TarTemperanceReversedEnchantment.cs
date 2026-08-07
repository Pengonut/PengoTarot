
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments;

public sealed class TarTemperanceReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (base.Card == null || base.Card.Owner == null)
            return;

        
        await PowerCmd.Apply<TarTemperanceReversedPower>(
            choiceContext,
            base.Card.Owner.Creature,
            5m,
            base.Card.Owner.Creature,
            base.Card);
    }
}