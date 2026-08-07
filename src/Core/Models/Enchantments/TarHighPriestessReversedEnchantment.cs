
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarHighPriestessReversedEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Retain),
            HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        };

    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return !card.Keywords.Contains(CardKeyword.Ethereal);
    }

    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Retain);
    }

    public override Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Card == null)
            return Task.CompletedTask;
        var player = Card.Owner;
        if (!participants.Contains(player.Creature))
            return Task.CompletedTask;
        if (Card.Pile?.Type != PileType.Hand)
            return Task.CompletedTask;
        var handPile = PileType.Hand.GetPile(player);
        int index = handPile.Cards.IndexOf(Card);
        if (index <= 0)
            return Task.CompletedTask;
        for (int i = 0; i < index; i++)
        {
            var leftCard = handPile.Cards[i];
            if (!leftCard.Keywords.Contains(CardKeyword.Ethereal))
            {
                CardCmd.ApplyKeyword(leftCard, CardKeyword.Ethereal);
            }
        }
        return Task.CompletedTask;
    }
}