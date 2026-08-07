
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;

namespace PengoTarot.Enchantments;

public sealed class TarTemperanceUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    private bool _triggered;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != base.Card || _triggered) return;
        _triggered = true;
        Status = EnchantmentStatus.Disabled;

        if (base.Card?.Owner != null)
            await PlayerCmd.GainGold(10m, base.Card.Owner);
    }
}