
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PengoTarot.Utils;

namespace PengoTarot.Enchantments;

public sealed class TarChariotReversedEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromPower<VulnerablePower>() };

    public override bool HasExtraCardText => true;

    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return MultiHitDetector.IsMultiHitCard(card);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (base.Card == null) return;

        
        var selfVuln = await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            base.Card.Owner.Creature,
            1m,
            base.Card.Owner.Creature,
            base.Card
        );
        if (selfVuln != null)
        {
            selfVuln.SkipNextDurationTick = false;
        }
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (cardSource != base.Card) return;
        if (target == null || dealer == null) return;

        await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 1m, dealer, base.Card);
    }
}