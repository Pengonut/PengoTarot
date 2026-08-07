
#nullable enable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace PengoTarot.Enchantments;

public sealed class TarJudgementUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    private bool _hasTransformed;

    public override async Task BeforeCombatStart()
    {
        if (_hasTransformed || base.Card == null || base.Card.Owner == null || !base.Card.IsInCombat)
            return;

        _hasTransformed = true;

        bool wasUpgraded = base.Card.IsUpgraded;
        var original = base.Card;

        
        var result = await CardCmd.TransformToRandom(original, original.Owner.RunState.Rng.Shuffle);
        if (result.success && result.cardAdded != null && wasUpgraded)
        {
            
            CardCmd.Upgrade(result.cardAdded, CardPreviewStyle.None);
        }
    }
}