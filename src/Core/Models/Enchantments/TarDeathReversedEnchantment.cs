
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;

namespace PengoTarot.Enchantments;

public sealed class TarDeathReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;
    public override bool ShouldGlowRed => true;

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

    protected override void OnEnchant()
    {
        // 能量基础费用永久设为 0（纯数据，安全，同原版 TezcatarasEmber 做法）；
        // 战斗中另有 BeforeCombatStart 兜底 SetToFreeThisCombat（不能放 OnEnchant，110 起非游戏状态会崩/炸档）
        base.Card.EnergyCost.SetCustomBaseCost(0);
        // 有辉星费用（如 Comet）的牌也永久归 0（反射改私有字段，安全，同塔星逆位做法）；
        // 无辉星费用的牌不改，避免多出「0 辉星」显示
        if (base.Card.BaseStarCost >= 0)
            StarXReflectionHelper.SetStarCost(base.Card, 0);
    }

    public override Task BeforeCombatStart()
    {
        base.Card?.SetToFreeThisCombat();
        return Task.CompletedTask;
    }

    public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public override bool ShouldDraw(Player player, bool fromHandDraw)
    {
        if (base.Card == null) return true;
        if (base.Card.Owner != player) return true;
        return base.Card.Pile?.Type != PileType.Hand;
    }
}