
#nullable enable
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarSunUprightEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (base.Card?.Owner == null)
            return;
            
        var debuffs = base.Card.Owner.Creature.Powers
            .Where(p => p.Type == PowerType.Debuff)
            .Where(p => p.InstanceType == PowerInstanceType.None)
            .Where(p => p.Amount > 0)
            .ToList();

        if (debuffs.Count == 0)
            return;

        
        var rng = base.Card.Owner.RunState.Rng.CombatTargets;
        var power = rng.NextItem(debuffs);
        if (power == null)
            return;

        int current = power.Amount;
        int newAmount = current / 2; 
        int reduce = current - newAmount;

        if (reduce > 0)
        {
            await PowerCmd.ModifyAmount(choiceContext, power, -reduce, applier: null, cardSource: base.Card);
        }
    }
}