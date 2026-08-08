#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Models.Afflictions;

namespace PengoTarot.Powers
{
    /// <summary>
    /// 占卜-倒吊人（逆位）效果 power：挂在玩家身上，让玩家的技能牌获得「倒吊人-逆」侵蚀（打出后消耗）。
    /// 战斗开始（AfterApplied）给当前所有技能牌上侵蚀；后续新进入战斗的技能牌（AfterCardEnteredCombat）也上。
    /// 对玩家是负面效果（Type=Debuff）、不可堆叠（Single）。
    /// 图标/名称由 PowerIconPath_Patch 与 powers 本地化表提供（逆塔罗）。
    /// </summary>
    public sealed class TarHangedManReversedPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Single;

        /// <summary>额外显示「消耗」关键词 hovertip（这些牌打出后会消耗）。</summary>
        protected override IEnumerable<IHoverTip> ExtraHoverTips
            => new IHoverTip[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };

        /// <summary>本回合是否已触发过（每回合第一张技能牌消耗；触发后本回合其余牌不再消耗且侵蚀标记消失）。</summary>
        private bool _triggeredThisTurn;

        public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            _triggeredThisTurn = false;
            await AfflictCards(Owner);
        }

        public override async Task AfterCardEnteredCombat(CardModel card)
        {
            // 本回合已触发过（第一张技能牌已消耗、其余技能牌侵蚀已清除）：
            // 新进入战斗的牌不再上侵蚀，防止同一回合抽到/生成的牌让已消失的图标重新出现。
            if (_triggeredThisTurn) return;
            // 只给新进入战斗的这张技能牌上侵蚀（参照原版 TangledPower/RingingPower 模式：
            // 只处理 card 本身，而不是重给所有牌上侵蚀，否则会覆盖触发后清除的效果）。
            if (card.Owner == Owner.Player && card.Type == CardType.Skill && card.Affliction == null)
                await CardCmd.Afflict<TarHangedManReversedAffliction>(card, 1m);
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            // 该玩家回合开始：重置「已触发」，重新给所有技能牌上侵蚀（右上角图标恢复）
            if (player != Owner.Player) return;
            _triggeredThisTurn = false;
            await AfflictCards(Owner);
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner.Creature != Owner) return;
            if (cardPlay.Card.Type != CardType.Skill) return;
            if (_triggeredThisTurn) return;   // 本回合已触发过，其余牌不再消耗

            _triggeredThisTurn = true;
            // 打出后消耗（每回合第一张技能牌，回退为 AfterCardPlayed + CardCmd.Exhaust）
            await CardCmd.Exhaust(choiceContext, cardPlay.Card);
            // 本回合其余技能牌的侵蚀标记消失（右上角图标消失，直到下回合）
            ClearOtherAfflictions(Owner, cardPlay.Card);
        }

        /// <summary>给玩家所有尚未侵蚀的技能牌上倒吊人-逆侵蚀标记（战斗开始/每回合开始）。</summary>
        private static async Task AfflictCards(Creature owner)
        {
            var allCards = owner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
            foreach (var card in allCards)
            {
                if (card.Type == CardType.Skill && card.Affliction == null)
                    await CardCmd.Afflict<TarHangedManReversedAffliction>(card, 1m);
            }
        }

        /// <summary>清除本玩家其他技能牌的倒吊人-逆侵蚀标记（触发一次后本回合图标消失）。</summary>
        private static void ClearOtherAfflictions(Creature owner, CardModel played)
        {
            var allCards = owner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
            foreach (var card in allCards)
            {
                if (card != played && card.Affliction is TarHangedManReversedAffliction)
                    CardCmd.ClearAffliction(card);
            }
        }
    }
}
