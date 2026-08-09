// PengoTarot/Powers/TarHermitReversedPower.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Powers;

/// <summary>
/// 占卜-隐者（逆位）效果 power：挂在被隐者标记的精英房间敌人身上。
/// 类似杀戮尖塔1的 Plated Armor（覆甲）：
/// - 拥有者回合结束时，获得 Amount 点格挡；
/// - 拥有者受到未被格挡的伤害时，减少 1 层（减到 0 后移除）。
/// 正面 power（对敌方是增益）、层数型（Counter，Amount = 最大生命×10%）。
/// 图标/名称由 PowerIconPath_Patch 与 powers 本地化表提供（逆塔罗）。
/// </summary>
public sealed class TarHermitReversedPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>额外显示「格挡」hovertip。</summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips
        => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Block) };

    /// <summary>
    /// 拥有者回合结束时获得 Amount 格挡（在回合结束伤害结算之前触发，参照 PlatingPower）。
    /// </summary>
    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            Flash();
            await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
        }
    }

    /// <summary>
    /// 拥有者受到未被格挡的伤害时，减少 1 层；减到 0 后移除（塔1 Plated Armor 行为）。
    /// </summary>
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || result.UnblockedDamage <= 0)
            return;

        if (await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, null) <= 0)
            await PowerCmd.Remove(this);
    }
}
