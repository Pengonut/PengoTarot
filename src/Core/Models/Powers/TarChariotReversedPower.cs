#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PengoTarot.Data.Divination;

namespace PengoTarot.Powers
{
    /// <summary>
    /// 占卜-战车（逆位）效果 power：挂在被战车标记的精英房间敌人身上。
    /// 敌人对玩家造成未被格挡的伤害后，给予玩家 1 层易伤。
    /// <b>房间级共享计数</b>：同一房间所有敌人共享「每个玩家只触发一次」；
    /// 当所有玩家都触发过后，移除房间里所有敌人身上的该 power。
    /// 正面 power（对敌方是增益）、不可堆叠（Single，Amount 恒 1）。
    /// 图标/名称由 PowerIconPath_Patch 与 powers 本地化表提供（逆塔罗）。
    /// </summary>
    public sealed class TarChariotReversedPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        /// <summary>额外显示「易伤」hovertip（该敌人对玩家造成未格挡伤害后会给玩家易伤）。</summary>
        protected override IEnumerable<IHoverTip> ExtraHoverTips
            => new IHoverTip[] { HoverTipFactory.FromPower<VulnerablePower>() };

        public override async Task AfterDamageReceived(
            PlayerChoiceContext choiceContext, Creature target, DamageResult result,
            ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            // 只处理「本敌人」对「玩家」造成的未被格挡伤害
            if (dealer != Owner) return;
            if (!target.IsPlayer) return;
            if (result.UnblockedDamage <= 0) return;
            if (target.Player == null) return;
            if (Owner.CombatState is not { } combat) return;

            // 房间级共享计数：每个玩家只触发一次（无论哪个敌人造成伤害）
            var shared = EliteDivinationSharedState.Chariot(combat);
            if (!shared.Add(target.Player.NetId)) return;

            Flash();
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 1m, Owner, null);

            // 所有玩家都触发过 → 结束房间里所有敌人身上的战车 power（共享计数已满，不再触发）
            if (shared.Count >= combat.Players.Count)
            {
                foreach (var enemy in combat.Enemies)
                    await PowerCmd.Remove<TarChariotReversedPower>(enemy);
            }
        }
    }
}
