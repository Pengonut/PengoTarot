
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PengoTarot.Enchantments;

public sealed class TarWorldReversedEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromPower<ArtifactPower>() };

    public override bool HasExtraCardText => true;

    private bool _triggered;

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (base.Card?.Owner == null || _triggered) return;
        _triggered = true;
        Status = EnchantmentStatus.Disabled;

        await PowerCmd.Apply<ArtifactPower>(choiceContext, base.Card.Owner.Creature, 1m, base.Card.Owner.Creature, base.Card);
    }
}