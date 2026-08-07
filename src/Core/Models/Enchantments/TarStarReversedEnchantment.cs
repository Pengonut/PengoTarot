
#nullable enable
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarStarReversedEnchantment : EnchantmentModel
{
    private enum SwapMode
    {
        None,
        ToStarX,    
        ToEnergyX   
    }

    private SwapMode _swapMode = SwapMode.None;

    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card) => base.CanEnchant(card);

    public bool IsSwappedToStarX => _swapMode == SwapMode.ToStarX && Status == EnchantmentStatus.Normal;
    public bool IsSwappedToEnergyX => _swapMode == SwapMode.ToEnergyX && Status == EnchantmentStatus.Normal;

    protected override void OnEnchant()
    {
        ApplyCostSwap();
    }

    private void ApplyCostSwap()
    {
        var card = Card;
        if (card == null) return;

        var cost = card.EnergyCost;
        bool isEnergyX = cost.CostsX;
        bool isStarX = card.HasStarCostX;

        int energyBase = cost.GetWithModifiers(CostModifiers.None);
        int starBase = card.BaseStarCost;

        if (isEnergyX && !isStarX)
        {
            
            RuntimeEnergyXCostHelper.SetCostsX(cost, false);
            cost.SetCustomBaseCost(starBase >= 0 ? starBase : 0);
            StarXReflectionHelper.SetStarCost(card, -1);
            _swapMode = SwapMode.ToStarX;
        }
        else if (!isEnergyX && isStarX)
        {
            
            RuntimeEnergyXCostHelper.SetCostsX(cost, true);
            StarXReflectionHelper.SetStarCost(card, -1);
            _swapMode = SwapMode.ToEnergyX;
        }
        else if (!isEnergyX && !isStarX)
        {
            
            int newEnergy = starBase >= 0 ? starBase : 0;
            int newStar = energyBase;
            cost.SetCustomBaseCost(newEnergy);
            StarXReflectionHelper.SetStarCost(card, newStar);
            _swapMode = SwapMode.None;
        }
    }
}