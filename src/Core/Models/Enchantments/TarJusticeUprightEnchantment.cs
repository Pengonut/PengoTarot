
#nullable enable
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Enchantments;

public sealed class TarJusticeUprightEnchantment : EnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };

    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        if (card.Keywords.Contains(CardKeyword.Exhaust)) return false;
        if (card.TargetType == TargetType.AllEnemies) return false; // 群攻(全体敌人)不可附魔
        return card.Type == CardType.Attack;
    }

    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Exhaust);
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
    {
        if (props.IsPoweredAttack())
            return 2m;
        return 1m;
    }
}