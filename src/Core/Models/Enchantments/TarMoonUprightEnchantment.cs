
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarMoonUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    private bool _triggered;

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (base.Card?.Owner == null)
            return;

        if (_triggered || base.Card == null || base.Card.Owner != player)
            return;

        if (player.PlayerCombatState?.TurnNumber != 1)
            return;

        var pile = base.Card.Pile;
        if (pile == null || !pile.IsCombatPile)
            return;

        _triggered = true;
        Status = EnchantmentStatus.Disabled;

        await CardCmd.AutoPlay(choiceContext, base.Card, null);
        
        await CardPileCmd.Add(base.Card, PileType.Draw.GetPile(player), CardPilePosition.Random);
    }
}