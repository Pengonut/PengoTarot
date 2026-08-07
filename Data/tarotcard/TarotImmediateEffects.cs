
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Data;

public static class TarotImmediateEffects
{
    public static async Task WheelOfFortuneUpright(Player player, RelicModel? targetRelic)
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), player.Creature, 11m,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, null, null);

        if (targetRelic != null)
        {
            var clone = ModelDb.GetById<RelicModel>(targetRelic.Id).ToMutable();
            await RelicCmd.Obtain(clone, player);
        }
    }

    public static async Task WheelOfFortuneReversed(Player player, List<RelicModel> toRemove, RelicModel? toClone)
    {
        var nonAncientRelics = player.Relics.Where(r => r.Rarity != RelicRarity.Ancient).ToList();
        if (nonAncientRelics.Count < 3) return;

        foreach (var relic in toRemove)
            await RelicCmd.Remove(relic);

        if (toClone != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var clone = ModelDb.GetById<RelicModel>(toClone.Id).ToMutable();
                await RelicCmd.Obtain(clone, player);
            }
        }
    }

    public static async Task TowerUpright(Player player)
    {
        var toRemove = player.Deck.Cards
            .Where(c => c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Basic)
            .ToList();
        if (toRemove.Count == 0) return;
        await CardPileCmd.RemoveFromDeck(toRemove, showPreview: true);
        await PlayerCmd.GainGold(toRemove.Count * 45, player);
    }

    public static async Task TowerReversed(Player player)
    {
        var toRemove = player.Deck.Cards
            .Where(c => c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Basic)
            .ToList();
        if (toRemove.Count == 0) return;
        await CardPileCmd.RemoveFromDeck(toRemove, showPreview: true);
        await PlayerCmd.GainGold(toRemove.Count * 15, player);
    }

    public static CardPoolModel GetTargetPool(string defId)
    {
        return defId switch
        {
            "DEVIL_UPRIGHT_SUB" or "DEVIL_REVERSED_SUB" => ModelDb.CardPool<IroncladCardPool>(),
            "STAR_UPRIGHT_SUB" or "STAR_REVERSED_SUB" => ModelDb.CardPool<RegentCardPool>(),
            "MOON_UPRIGHT_SUB" or "MOON_REVERSED_SUB" => ModelDb.CardPool<SilentCardPool>(),
            "SUN_UPRIGHT_SUB" or "SUN_REVERSED_SUB" => ModelDb.CardPool<NecrobinderCardPool>(),
            "WORLD_UPRIGHT_SUB" or "WORLD_REVERSED_SUB" => ModelDb.CardPool<DefectCardPool>(),
            _ => throw new System.InvalidOperationException($"Unknown sub def: {defId}")
        };
    }
}