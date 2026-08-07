
#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.Context;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
namespace PengoTarot.Enchantments;

public sealed class TarHangedManUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return card.Keywords.Contains(CardKeyword.Exhaust);
    }
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };
    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel exhaustedCard, bool causedByEthereal)
    {
        if (exhaustedCard == base.Card) return;
        if (base.Card == null || base.Card.Pile?.Type != PileType.Draw) return;

        
        await CardPileCmd.Add(exhaustedCard, PileType.Discard);

        
        await CardCmd.AutoPlay(choiceContext, base.Card, null);
    }
}