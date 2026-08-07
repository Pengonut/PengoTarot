#nullable enable

using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Models.Afflictions;

namespace PengoTarot.Powers
{
    /// <summary>
    /// 占卜-死神（逆位）效果 power：挂在玩家身上。
    /// 1. 战斗开始给玩家的能力牌上「死神-逆」标记（TarDeathReversedAffliction），用于卡牌右上角图标与打出提示变红；
    /// 2. 每当你打出一张能力牌，立即结束你的回合（参照 VoidForm 的 PlayerCmd.EndTurn(canBackOut:false)）。
    /// 对玩家是负面效果（Type=Debuff）、不可堆叠（Single）。
    /// 图标/名称由 PowerIconPath_Patch 与 powers 本地化表提供（逆塔罗）。
    /// </summary>
    public sealed class TarDeathReversedPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            await AfflictCards(Owner);
        }

        public override async Task AfterCardEnteredCombat(CardModel card)
        {
            if (card.Owner == Owner.Player)
                await AfflictCards(Owner);
        }

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

        /// <summary>给玩家所有尚未标记的能力牌上死神-逆标记。</summary>
        private static async Task AfflictCards(Creature owner)
        {
            var allCards = owner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
            foreach (var card in allCards)
            {
                if (card.Type == CardType.Power && card.Affliction == null)
                    await CardCmd.Afflict<TarDeathReversedAffliction>(card, 1m);
            }
        }
    }
}
