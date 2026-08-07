#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models.Cards;

namespace PengoTarot.Enchantments;

public sealed class TarFoolReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;
    private bool _triggered;

    public int Generation { get; internal set; }

    /// <summary>
    /// 如果为 true，则本卡及其克隆品将不受代数加费影响（由癫狂之触等外部效果设置）。
    /// </summary>
    internal bool IgnoreGenerationCost { get; set; }

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card))
            return false;
        if (card.EnergyCost.CostsX)
            return false;
        if (card.HasStarCostX)
            return false;
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != base.Card || _triggered)
            return;

        var clone = base.Card.CreateClone();
        if (clone != null)
        {
            CardCmd.ClearEnchantment(clone);
            CardCmd.Enchant<TarFoolReversedEnchantment>(clone, 1m);

            if (clone.Enchantment is TarFoolReversedEnchantment newEnchant)
            {
                newEnchant.Generation = this.Generation + 1;
                newEnchant.IgnoreGenerationCost = this.IgnoreGenerationCost;
            }
            
            if (cardPlay.Card is Stomp)
            {
                clone.EnergyCost.AddThisTurn(1);
            }

            await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, base.Card.Owner);
        }
        _triggered = true;
        Status = EnchantmentStatus.Disabled;
    }

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != base.Card || Generation <= 0 || IgnoreGenerationCost)
            return false;

        modifiedCost = originalCost + Generation;

        
        return true;
    }
}