#nullable enable

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Models.Afflictions
{
    /// <summary>
    /// 占卜-死神（逆位）标记：只标记能力牌。
    /// 仅用于视觉标记：卡牌右上角死神逆图标（HandCardHolder_DivinationIconPatch）与打出提示变红
    /// （NHandCardHolder.get_ShouldGlowRed patch）；「打出能力牌结束回合」逻辑在
    /// <see cref="PengoTarot.Powers.TarDeathReversedPower"/>（所有能力牌都被标记，效果一致）。
    /// 不提供 overlay 场景（HasOverlay=false）→ 卡牌 UI 走默认 overlay，无缺特效报错。
    /// </summary>
    public sealed class TarDeathReversedAffliction : AfflictionModel
    {
        public override bool HasExtraCardText => true;

        public override bool CanAfflictCardType(CardType cardType) => cardType == CardType.Power;
    }
}
