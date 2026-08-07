#nullable enable

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Models.Afflictions
{
    /// <summary>
    /// 占卜-正义（逆位）侵蚀：只侵蚀攻击牌。
    /// 消耗由「卡牌获得 Exhaust 关键词」实现（TarJusticeReversedPower 添加），游戏自动显示「消耗」关键词与描述；
    /// 本侵蚀仅作标记（手牌右上角正义逆图标），无自身逻辑、无额外卡面文本。
    /// 不提供 overlay 场景（HasOverlay=false）→ 卡牌 UI 走默认 overlay，无缺侵蚀特效报错。
    /// </summary>
    public sealed class TarJusticeReversedAffliction : AfflictionModel
    {
        public override bool CanAfflictCardType(CardType cardType) => cardType == CardType.Attack;
    }
}
