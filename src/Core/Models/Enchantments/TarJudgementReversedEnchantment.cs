
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace PengoTarot.Enchantments;

public sealed class TarJudgementReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    private static readonly HashSet<Player> _processedPlayers = new();
    private static readonly object _lock = new();

    public override Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (base.Card?.Owner != player)
            return Task.CompletedTask;

        if (player.PlayerCombatState?.TurnNumber != 1)
            return Task.CompletedTask;

        lock (_lock)
        {
            if (!_processedPlayers.Add(player))
                return Task.CompletedTask;
        }

        var cards = player.PlayerCombatState?.AllCards
            .Where(c => c.Enchantment is TarJudgementReversedEnchantment && c.Pile?.Type == PileType.Draw)
            .OrderByDescending(c => c.FloorAddedToDeck ?? int.MaxValue)
            .ToList() ?? new();

        foreach (var card in cards)
        {
            _ = CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);
        }
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        lock (_lock)
        {
            _processedPlayers.Clear();
        }
        return Task.CompletedTask;
    }
}