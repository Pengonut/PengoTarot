
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

using MegaCrit.Sts2.Core.Context;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
namespace PengoTarot.Enchantments;

public sealed class TarSunReversedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;
    private int _originalEnergyCost;
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
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    new IHoverTip[] { HoverTipFactory.FromPower<DoomPower>() };
    protected override void OnEnchant()
    {
        _originalEnergyCost = base.Card.EnergyCost.GetWithModifiers(CostModifiers.None);
        // 能量基础费用永久设为 0（纯数据，安全，同原版 TezcatarasEmber 做法）；
        // 战斗中另有 BeforeCombatStart 兜底 SetToFreeThisCombat（不能放 OnEnchant，110 起非游戏状态会崩/炸档）
        base.Card.EnergyCost.SetCustomBaseCost(0);
    }

    public override Task BeforeCombatStart()
    {
        base.Card?.SetToFreeThisCombat();
        return Task.CompletedTask;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
if (base.Card?.Owner == null) return;

        int doomAmount = _originalEnergyCost * 6;
        if (doomAmount > 0)
        {
            await PowerCmd.Apply<DoomPower>(
                choiceContext,
                base.Card.Owner.Creature,
                doomAmount,
                base.Card.Owner.Creature,
                base.Card);
        }
    }
}