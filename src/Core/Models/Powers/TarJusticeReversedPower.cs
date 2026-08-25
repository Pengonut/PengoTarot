#nullable enable

using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Data.Divination;

namespace PengoTarot.Powers
{
    /// <summary>
    /// 占卜-正义（逆位）效果 power：挂在玩家身上，每回合打出的第一张攻击牌被消耗。
    /// 隐藏的内部状态（Type=None）、不可堆叠（Single）。
    /// 图标/名称由 PowerIconPath_Patch 与 powers 本地化表提供（逆塔罗）。
    /// </summary>
    public sealed class TarJusticeReversedPower : PowerModel
    {
        public override PowerType Type => PowerType.None;
        public override PowerStackType StackType => PowerStackType.Single;
        protected override bool IsVisibleInternal => false;

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner.Creature != Owner) return;
            if (cardPlay.Card.Type != CardType.Attack) return;
            // AfterCardPlayed 时当前牌已进入 CardPlaysStarted；等于 1 即本回合第一张攻击牌。
            if (NormalDivinationTurnState.CountCardsPlayedThisTurn(Owner, CardType.Attack) != 1) return;

            await CardCmd.Exhaust(choiceContext, cardPlay.Card);
        }
    }
}
