
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using PengoTarot.Utils;

namespace PengoTarot.Enchantments;

public sealed class TarStrengthUprightEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromPower<WeakPower>() };

    public override bool HasExtraCardText => true;

    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return !MultiHitDetector.IsMultiHitCard(card);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (base.Card == null) return;

        IReadOnlyList<Creature> targets;
        if (base.Card.TargetType == TargetType.AllEnemies)
        {
            targets = base.Card.CombatState!.HittableEnemies.ToList();
        }
        else if (cardPlay?.Target != null)
        {
            targets = new[] { cardPlay.Target };
        }
        else
        {
            return;
        }

        await PowerCmd.Apply<WeakPower>(choiceContext, targets, 1m, base.Card.Owner.Creature, base.Card);
    }
}