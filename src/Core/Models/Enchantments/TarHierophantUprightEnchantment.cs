
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

namespace PengoTarot.Enchantments;

public sealed class TarHierophantUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;
    public override bool CanEnchant(CardModel card)
    {
        return base.CanEnchant(card);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        var player = Card.Owner;
        var cardModel = await CardSelectCmd.FromHandForUpgrade(choiceContext, player, this);
        if (cardModel != null)
        {
            CardCmd.Upgrade(cardModel);
            CardCmd.Preview(cardModel);
        }
    }
}