#nullable enable

using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Powers
{
    /// <summary>
    /// 占卜-死神（逆位）效果 power：挂在玩家身上。
    /// 每当你打出一张能力牌，立即结束你的回合（参照 VoidForm 的 PlayerCmd.EndTurn(canBackOut:false)）。
    /// 隐藏的内部状态（Type=None）、不可堆叠（Single）。
    /// 图标/名称由 PowerIconPath_Patch 与 powers 本地化表提供（逆塔罗）。
    /// </summary>
    public sealed class TarDeathReversedPower : PowerModel
    {
        public override PowerType Type => PowerType.None;
        public override PowerStackType StackType => PowerStackType.Single;
        protected override bool IsVisibleInternal => false;

        public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner.Creature != Owner) return Task.CompletedTask;
            if (cardPlay.Card.Type != CardType.Power) return Task.CompletedTask;
            // 防御：power 应挂在玩家身上，Owner.Player 缺失时忽略
            if (Owner.Player == null) return Task.CompletedTask;

            Flash();
            PlayerCmd.EndTurn(Owner.Player, canBackOut: false);
            return Task.CompletedTask;
        }
    }
}
