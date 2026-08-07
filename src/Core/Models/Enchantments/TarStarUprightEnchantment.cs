
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

using MegaCrit.Sts2.Core.Context;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
namespace PengoTarot.Enchantments;

public sealed class TarStarUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    new[] { HoverTipFactory.Static(StaticHoverTip.Block) };
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {

        if (base.Card?.Owner?.PlayerCombatState == null) return;
        int stars = base.Card.Owner.PlayerCombatState.Stars;
        if (stars > 0)
        {
            await CreatureCmd.GainBlock(base.Card.Owner.Creature, stars, ValueProp.Move, null);
        }
    }
}