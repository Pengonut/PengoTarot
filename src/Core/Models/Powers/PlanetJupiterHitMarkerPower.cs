// PengoTarot/Powers/PlanetJupiterHitMarkerPower.cs
#nullable enable
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PengoTarot.Powers
{
    /// <summary>
    /// 不可见的临时标记 Power，施加于被 Jupiter 附魔卡牌命中的敌人。
    /// 多段伤害可叠加多层，卡牌打出结束后统一结算：
    /// 有标记的敌人 → 1 层 <see cref="PlanetJupiterPower"/>，然后清除所有标记。
    /// </summary>
    public sealed class PlanetJupiterHitMarkerPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        protected override bool IsVisibleInternal => false;
    }
}
