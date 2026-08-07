
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Random;
using PengoTarot.Cards;
using PengoTarot.Enchantments;

namespace PengoTarot.Data
{
    public static class TarotDeck
    {
        private static int CountEnchantableCards(Player player, EnchantmentModel enchantment)
        {
            var deck = player.Piles.First(p => p.Type == PileType.Deck);
            return deck.Cards.Count(card => enchantment.CanEnchant(card));
        }

        private static bool HasAnyCardOfRarity(Player player, params CardRarity[] rarities)
        {
            var deck = player.Piles.First(p => p.Type == PileType.Deck);
            return deck.Cards.Any(card => rarities.Contains(card.Rarity));
        }

        public static readonly List<TarotDef> All = new()
        {
            new TarotDef("FOOL_UPRIGHT", 75, typeof(TarFoolUpright), ModelDb.Enchantment<TarFoolUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarFoolUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("FOOL_REVERSED", 75, typeof(TarFoolReversed), ModelDb.Enchantment<TarFoolReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarFoolReversedEnchantment>()) >= 1,
                weight: 0.8),
            
            new TarotDef("MAGICIAN_UPRIGHT", 75, typeof(TarMagicianUpright), ModelDb.Enchantment<TarMagicianUprightEnchantment>(), cardsToEnchant: 5,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarMagicianUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("MAGICIAN_REVERSED", 75, typeof(TarMagicianReversed), ModelDb.Enchantment<TarMagicianReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarMagicianReversedEnchantment>()) >= 1,
                weight: 0.8),
            
            new TarotDef("HIGH_PRIESTESS_UPRIGHT", 75, typeof(TarHighPriestessUpright), ModelDb.Enchantment<TarHighPriestessUprightEnchantment>(), cardsToEnchant: 3,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarHighPriestessUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("HIGH_PRIESTESS_REVERSED", 75, typeof(TarHighPriestessReversed), ModelDb.Enchantment<TarHighPriestessReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarHighPriestessReversedEnchantment>()) >= 1,
                weight: 0.8),
            
            new TarotDef("EMPRESS_UPRIGHT", 75, typeof(TarEmpressUpright), ModelDb.Enchantment<TarEmpressUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarEmpressUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("EMPRESS_REVERSED", 75, typeof(TarEmpressReversed), ModelDb.Enchantment<TarEmpressReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarEmpressReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("EMPEROR_UPRIGHT", 75, typeof(TarEmperorUpright), ModelDb.Enchantment<TarEmperorUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarEmperorUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("EMPEROR_REVERSED", 75, typeof(TarEmperorReversed), ModelDb.Enchantment<TarEmperorReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarEmperorReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("HIEROPHANT_UPRIGHT", 75, typeof(TarHierophantUpright), ModelDb.Enchantment<TarHierophantUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarHierophantUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("HIEROPHANT_REVERSED", 75, typeof(TarHierophantReversed), ModelDb.Enchantment<TarHierophantReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarHierophantReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("LOVERS_UPRIGHT", 75, typeof(TarLoversUpright), ModelDb.Enchantment<TarLoversUprightEnchantment>(), cardsToEnchant: 2,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarLoversUprightEnchantment>()) >= 2,
                weight: 0.6),
            new TarotDef("LOVERS_REVERSED", 75, typeof(TarLoversReversed), ModelDb.Enchantment<TarLoversReversedEnchantment>(), cardsToEnchant: 2,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarLoversReversedEnchantment>()) >= 2,
                weight: 0.6),
            
            new TarotDef("CHARIOT_UPRIGHT", 75, typeof(TarChariotUpright), ModelDb.Enchantment<TarChariotUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarChariotUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("CHARIOT_REVERSED", 75, typeof(TarChariotReversed), ModelDb.Enchantment<TarChariotReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarChariotReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("STRENGTH_UPRIGHT", 75, typeof(TarStrengthUpright), ModelDb.Enchantment<TarStrengthUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarStrengthUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("STRENGTH_REVERSED", 75, typeof(TarStrengthReversed), ModelDb.Enchantment<TarStrengthReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarStrengthReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("HERMIT_UPRIGHT", 75, typeof(TarHermitUpright), ModelDb.Enchantment<TarHermitUprightEnchantment>(), cardsToEnchant: 2,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarHermitUprightEnchantment>()) >= 1,
                weight: 0.8),
            new TarotDef("HERMIT_REVERSED", 75, typeof(TarHermitReversed), ModelDb.Enchantment<TarHermitReversedEnchantment>(), cardsToEnchant: 2,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarHermitReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("JUSTICE_UPRIGHT", 75, typeof(TarJusticeUpright), ModelDb.Enchantment<TarJusticeUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarJusticeUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("JUSTICE_REVERSED", 75, typeof(TarJusticeReversed), ModelDb.Enchantment<TarJusticeReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarJusticeReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("HANGED_MAN_UPRIGHT", 75, typeof(TarHangedManUpright), ModelDb.Enchantment<TarHangedManUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarHangedManUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("HANGED_MAN_REVERSED", 75, typeof(TarHangedManReversed), ModelDb.Enchantment<TarHangedManReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarHangedManReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("DEATH_UPRIGHT", 75, typeof(TarDeathUpright), ModelDb.Enchantment<TarDeathUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarDeathUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("DEATH_REVERSED", 75, typeof(TarDeathReversed), ModelDb.Enchantment<TarDeathReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarDeathReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("TEMPERANCE_UPRIGHT", 75, typeof(TarTemperanceUpright), ModelDb.Enchantment<TarTemperanceUprightEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarTemperanceUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("TEMPERANCE_REVERSED", 75, typeof(TarTemperanceReversed), ModelDb.Enchantment<TarTemperanceReversedEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarTemperanceReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("DEVIL_UPRIGHT", 75, typeof(TarDevilUpright), ModelDb.Enchantment<TarDevilUprightEnchantment>(),
                requiredCharacter: p => p.Character is Ironclad,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarDevilUprightEnchantment>()) >= 1,
                weight: 0.8),
            new TarotDef("DEVIL_REVERSED", 75, typeof(TarDevilReversed), ModelDb.Enchantment<TarDevilReversedEnchantment>(),
                requiredCharacter: p => p.Character is Ironclad,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarDevilReversedEnchantment>()) >= 1,
                weight: 0.8),
            
            new TarotDef("STAR_UPRIGHT", 75, typeof(TarStarUpright), ModelDb.Enchantment<TarStarUprightEnchantment>(), cardsToEnchant: 3,
                requiredCharacter: p => p.Character is Regent,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarStarUprightEnchantment>()) >= 1,
                weight: 0.8),
            new TarotDef("STAR_REVERSED", 75, typeof(TarStarReversed), ModelDb.Enchantment<TarStarReversedEnchantment>(),
                requiredCharacter: p => p.Character is Regent,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarStarReversedEnchantment>()) >= 1,
                weight: 0.8),
            
            new TarotDef("MOON_UPRIGHT", 75, typeof(TarMoonUpright), ModelDb.Enchantment<TarMoonUprightEnchantment>(),
                requiredCharacter: p => p.Character is Silent,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarMoonUprightEnchantment>()) >= 1,
                weight: 0.8),
            new TarotDef("MOON_REVERSED", 75, typeof(TarMoonReversed), ModelDb.Enchantment<TarMoonReversedEnchantment>(),
                requiredCharacter: p => p.Character is Silent,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarMoonReversedEnchantment>()) >= 1,
                weight: 0.8),
            
            new TarotDef("SUN_UPRIGHT", 75, typeof(TarSunUpright), ModelDb.Enchantment<TarSunUprightEnchantment>(), cardsToEnchant: 3,
                requiredCharacter: p => p.Character is Necrobinder,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarSunUprightEnchantment>()) >= 1,
                weight: 0.8),
            new TarotDef("SUN_REVERSED", 75, typeof(TarSunReversed), ModelDb.Enchantment<TarSunReversedEnchantment>(),
                requiredCharacter: p => p.Character is Necrobinder,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarSunReversedEnchantment>()) >= 1,
                weight: 0.8),
            
            new TarotDef("JUDGEMENT_UPRIGHT", 75, typeof(TarJudgementUpright), ModelDb.Enchantment<TarJudgementUprightEnchantment>(), cardsToEnchant: 2,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarJudgementUprightEnchantment>()) >= 1,
                weight: 1.0),
            new TarotDef("JUDGEMENT_REVERSED", 75, typeof(TarJudgementReversed), ModelDb.Enchantment<TarJudgementReversedEnchantment>(), cardsToEnchant: 21,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarJudgementReversedEnchantment>()) >= 1,
                weight: 1.0),
            
            new TarotDef("WORLD_UPRIGHT", 75, typeof(TarWorldUpright), ModelDb.Enchantment<TarWorldUprightEnchantment>(),
                requiredCharacter: p => p.Character is Defect,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarWorldUprightEnchantment>()) >= 1,
                weight: 0.8),
            new TarotDef("WORLD_REVERSED", 75, typeof(TarWorldReversed), ModelDb.Enchantment<TarWorldReversedEnchantment>(),
                requiredCharacter: p => p.Character is Defect,
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarWorldReversedEnchantment>()) >= 1,
                weight: 0.8),
            
            
            new TarotDef("WHEEL_OF_FORTUNE_UPRIGHT", 75, typeof(TarWheelOfFortuneUpright),
                (p, rng) => Task.FromResult(true), 
                availabilityCheck: p => p.Creature.CurrentHp >= 11,
                minAct: 2,
                weight: 1.0),
            
            new TarotDef("WHEEL_OF_FORTUNE_REVERSED", 75, typeof(TarWheelOfFortuneReversed),
                (p, rng) => Task.FromResult(true),
                availabilityCheck: p => p.Relics.Count(r => r.Rarity != RelicRarity.Ancient) >= 3,
                minAct: 3,
                weight: 1.0),
            
            new TarotDef("TOWER_UPRIGHT", 75, typeof(TarTowerUpright),
                (p, rng) => Task.FromResult(true),
                availabilityCheck: p => HasAnyCardOfRarity(p, CardRarity.Basic, CardRarity.Common, CardRarity.Uncommon),
                minAct: 2,
                weight: 0.8),
            
            new TarotDef("TOWER_REVERSED", 75, typeof(TarTowerReversed),
                (p, rng) => Task.FromResult(true),
                availabilityCheck: p => HasAnyCardOfRarity(p, CardRarity.Basic, CardRarity.Common),
                minAct: 3,
                weight: 0.8),


            
            new TarotDef("DEVIL_UPRIGHT_SUB", 75, typeof(TarDevilUprightSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarDevilUprightSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Ironclad),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarDevilUprightSubEnchantment>()) >= 1,
                weight: 0.0325),
            new TarotDef("DEVIL_REVERSED_SUB", 75, typeof(TarDevilReversedSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarDevilReversedSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Ironclad),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarDevilReversedSubEnchantment>()) >= 1,
                weight: 0.0325),

            
            new TarotDef("STAR_UPRIGHT_SUB", 75, typeof(TarStarUprightSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarStarUprightSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Regent),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarStarUprightSubEnchantment>()) >= 1,
                weight: 0.0325),
            new TarotDef("STAR_REVERSED_SUB", 75, typeof(TarStarReversedSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarStarReversedSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Regent),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarStarReversedSubEnchantment>()) >= 1,
                weight: 0.0325),

            
            new TarotDef("MOON_UPRIGHT_SUB", 75, typeof(TarMoonUprightSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarMoonUprightSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Silent),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarMoonUprightSubEnchantment>()) >= 1,
                weight: 0.0325),
            new TarotDef("MOON_REVERSED_SUB", 75, typeof(TarMoonReversedSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarMoonReversedSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Silent),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarMoonReversedSubEnchantment>()) >= 1,
                weight: 0.0325),

            
            new TarotDef("SUN_UPRIGHT_SUB", 75, typeof(TarSunUprightSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarSunUprightSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Necrobinder),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarSunUprightSubEnchantment>()) >= 1,
                weight: 0.0325),
            new TarotDef("SUN_REVERSED_SUB", 75, typeof(TarSunReversedSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarSunReversedSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Necrobinder),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarSunReversedSubEnchantment>()) >= 1,
                weight: 0.0325),

            
            new TarotDef("WORLD_UPRIGHT_SUB", 75, typeof(TarWorldUprightSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarWorldUprightSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Defect),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarWorldUprightSubEnchantment>()) >= 1,
                weight: 0.0325),
            new TarotDef("WORLD_REVERSED_SUB", 75, typeof(TarWorldReversedSub),
                immediateEffect: null,
                enchantment: ModelDb.Enchantment<TarWorldReversedSubEnchantment>(),
                cardsToEnchant: 3,
                requiredCharacter: p => !(p.Character is Defect),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<TarWorldReversedSubEnchantment>()) >= 1,
                weight: 0.0325),
        };

        public static List<TarotDef> DrawThreeUnique(Player player, Rng rng)
            => DrawUnique(player, rng, 3, includeReversed: false, includeCharacterSpecific: false);

        /// <summary>
        /// 加权抽取塔罗牌（可指定数量、是否含逆位、是否含角色专属）。
        /// 默认（DrawThreeUnique）只含正位+通用塔罗；占卜-皇后启用后含逆位，占卜-皇帝启用后含角色专属。
        /// </summary>
        public static List<TarotDef> DrawUnique(Player player, Rng rng, int count,
            bool includeReversed, bool includeCharacterSpecific)
        {
            var currentAct = player.RunState.CurrentActIndex + 1;
            var available = All.Where(def =>
                (includeReversed || !def.Id.EndsWith("_REVERSED")) &&
                (includeCharacterSpecific || def.RequiredCharacter == null) &&
                (def.RequiredCharacter == null || def.RequiredCharacter(player)) &&
                (def.AvailabilityCheck == null || def.AvailabilityCheck(player)) &&
                def.MinAct <= currentAct
            ).ToList();
            var drawn = new List<TarotDef>();
            var weightedPool = new List<TarotDef>(available);
            for (int i = 0; i < count && weightedPool.Count > 0; i++)
            {
                double totalWeight = weightedPool.Sum(def => def.Weight);
                if (totalWeight <= 0)
                    break;
                double roll = rng.NextFloat() * totalWeight;
                double cumulative = 0;
                TarotDef? selected = null;
                for (int j = 0; j < weightedPool.Count; j++)
                {
                    cumulative += weightedPool[j].Weight;
                    if (roll <= cumulative)
                    {
                        selected = weightedPool[j];
                        weightedPool.RemoveAt(j);
                        break;
                    }
                }
                if (selected != null)
                    drawn.Add(selected);
            }
            return drawn;
        }
    }
}