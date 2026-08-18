#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;

namespace PengoTarot.Enchantments;

public sealed class TarMoonReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override bool CanEnchantCardType(CardType cardType) => cardType != CardType.Power;

    private bool _allowDiscard;
    private bool _endOfTurnDiscard;
    private bool _slyReturnPending;
    private bool _returning;
    public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        _allowDiscard = true;
        return Task.CompletedTask;
    }
    public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        _endOfTurnDiscard = true;
        return Task.CompletedTask;
    }
    public override Task AfterFlush(PlayerChoiceContext choiceContext, Player player,
        IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
    {
        _endOfTurnDiscard = false;
        return Task.CompletedTask;
    }
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPile, AbstractModel? clonedBy)
    {
        if (card != base.Card || _returning) return;

        
        if (card.Pile?.Type == PileType.Hand)
            _allowDiscard = false;

        if (oldPile != PileType.Hand || card.Pile?.Type != PileType.Discard) return;

        
        if (_endOfTurnDiscard)
        {
            _returning = true;
            await CardPileCmd.Add(card, PileType.Hand);
            _returning = false;
            return;
        }

        
        if (_allowDiscard)
        {
            _allowDiscard = false;
            return;
        }

        
        if (card.IsSlyThisTurn)
        {
            _slyReturnPending = true;
            return;
        }

        
        _returning = true;
        await CardPileCmd.Add(card, PileType.Hand);
        _returning = false;
    }
#if STS2_AT_LEAST_0_110_0
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card, bool isAutoPlay, ResourceInfo resources,
        CardLocation cardLocation)
    {
        if (card != base.Card || !_slyReturnPending)
            return cardLocation;

        _slyReturnPending = false;

        if (card.Type == CardType.Power) return cardLocation;
        cardLocation.pileType = PileType.Hand;
        cardLocation.position = CardPilePosition.Bottom;
        return cardLocation;
    }
#else
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card, bool isAutoPlay, ResourceInfo resources,
        PileType currentPileType, CardPilePosition currentPosition)
    {
        if (card != base.Card || !_slyReturnPending)
            return (currentPileType, currentPosition);

        _slyReturnPending = false;

        if (card.Type == CardType.Power) return (currentPileType, currentPosition);
        return (PileType.Hand, CardPilePosition.Bottom);
    }
#endif
}