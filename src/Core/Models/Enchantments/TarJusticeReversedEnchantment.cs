#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Enchantments;

public sealed class TarJusticeReversedEnchantment : EnchantmentModel
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
            HoverTipFactory.Static(StaticHoverTip.Block)
        };

    public override bool HasExtraCardText => true;

    private decimal _pendingDamage;

    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new DynamicVar("DamageDealt", 0) };

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

    public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (cardSource == base.Card)
        {
            _pendingDamage += result.TotalDamage + result.OverkillDamage;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != base.Card || _pendingDamage <= 0)
            return;

        decimal damageToBlock = _pendingDamage;
        _pendingDamage = 0;

        
        DynamicVars["DamageDealt"].BaseValue = damageToBlock;
        RecalculateValues(); 

        await CreatureCmd.GainBlock(base.Card!.Owner!.Creature, damageToBlock, ValueProp.Move, cardPlay);
    }
}