
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace PengoTarot.Enchantments;

public sealed class TarHermitReversedEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };

    public override bool HasExtraCardText => true;

    public override bool CanEnchantCardType(CardType cardType) => cardType != CardType.Power;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (base.Card == null || base.Card.Owner != player) return;
        if (player.PlayerCombatState?.TurnNumber != 1) return;

        if (base.Card.Pile != null && base.Card.Pile.Type != PileType.Exhaust)
        {
            await CardCmd.Exhaust(choiceContext, base.Card);
        }
    }

    public override async Task AfterAutoPrePlayPhaseEnteredEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (base.Card == null || base.Card.Owner != player) return;
        if (player.PlayerCombatState == null) return;
        int turn = player.PlayerCombatState.TurnNumber;
        if (turn == 7)
        {
            if (base.Card.Pile?.Type == PileType.Exhaust)
            {
                await CardPileCmd.Add(base.Card, PileType.Hand);
                base.Card.SetToFreeThisCombat();
            }
        }
    }
}