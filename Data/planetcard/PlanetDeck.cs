#nullable enable
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using PengoTarot.Cards;
using PengoTarot.Enchantments;

namespace PengoTarot.Data
{
    public static class PlanetDeck
    {
        private static int CountEnchantableCards(Player player, EnchantmentModel enchantment)
        {
            var deck = player.Piles.First(p => p.Type == PileType.Deck);
            return deck.Cards.Count(card => enchantment.CanEnchant(card));
        }

        public static readonly List<PlanetDef> All = new()
        {
            // 能力
            new PlanetDef("MERCURY", 0, typeof(PlanetMercury), CardType.Power,
                ModelDb.Enchantment<PlanetMercuryEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetMercuryEnchantment>()) >= 1),
            new PlanetDef("VENUS", 0, typeof(PlanetVenus), CardType.Power,
                ModelDb.Enchantment<PlanetVenusEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetVenusEnchantment>()) >= 1),
            new PlanetDef("EARTH", 0, typeof(PlanetEarth), CardType.Power,
                ModelDb.Enchantment<PlanetEarthEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetEarthEnchantment>()) >= 1),
            new PlanetDef("MARS", 0, typeof(PlanetMars), CardType.Power,
                ModelDb.Enchantment<PlanetMarsEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetMarsEnchantment>()) >= 1),

            // 攻击
            new PlanetDef("JUPITER", 0, typeof(PlanetJupiter), CardType.Attack,
                ModelDb.Enchantment<PlanetJupiterEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetJupiterEnchantment>()) >= 1),
            new PlanetDef("SATURN", 0, typeof(PlanetSaturn), CardType.Attack,
                ModelDb.Enchantment<PlanetSaturnEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetSaturnEnchantment>()) >= 1),
            new PlanetDef("URANUS", 0, typeof(PlanetUranus), CardType.Attack,
                ModelDb.Enchantment<PlanetUranusEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetUranusEnchantment>()) >= 1),
            new PlanetDef("NEPTUNE", 0, typeof(PlanetNeptune), CardType.Attack,
                ModelDb.Enchantment<PlanetNeptuneEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetNeptuneEnchantment>()) >= 1),

            // 技能
            new PlanetDef("PLUTO", 0, typeof(PlanetPluto), CardType.Skill,
                ModelDb.Enchantment<PlanetPlutoEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetPlutoEnchantment>()) >= 1),
            new PlanetDef("X", 0, typeof(PlanetX), CardType.Skill,
                ModelDb.Enchantment<PlanetXEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetXEnchantment>()) >= 1),
            new PlanetDef("CERES", 0, typeof(PlanetCeres), CardType.Skill,
                ModelDb.Enchantment<PlanetCeresEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetCeresEnchantment>()) >= 1),
            new PlanetDef("ERIS", 0, typeof(PlanetEris), CardType.Skill,
                ModelDb.Enchantment<PlanetErisEnchantment>(),
                availabilityCheck: p => CountEnchantableCards(p, ModelDb.Enchantment<PlanetErisEnchantment>()) >= 1),
        };

        public static List<PlanetDef> DrawThreeUnique(Player player, Rng rng)
        {
            var currentAct = player.RunState.CurrentActIndex + 1;
            var available = All.Where(def =>
                (def.RequiredCharacter == null || def.RequiredCharacter(player)) &&
                (def.AvailabilityCheck == null || def.AvailabilityCheck(player)) &&
                def.MinAct <= currentAct
            ).ToList();

            var abilities = available.Where(d => d.PlanetCardType == CardType.Power).ToList();
            var attacks   = available.Where(d => d.PlanetCardType == CardType.Attack).ToList();
            var skills    = available.Where(d => d.PlanetCardType == CardType.Skill).ToList();

            var drawn = new List<PlanetDef>();

            PlanetDef? TryTake(List<PlanetDef> pool, List<PlanetDef> already)
            {
                var candidate = pool.Where(d => !already.Contains(d)).ToList();
                if (candidate.Count == 0) return null;
                return rng.NextItem(candidate);
            }

            var pick = TryTake(abilities, drawn);
            if (pick != null) drawn.Add(pick);
            pick = TryTake(attacks, drawn);
            if (pick != null) drawn.Add(pick);
            pick = TryTake(skills, drawn);
            if (pick != null) drawn.Add(pick);

            return drawn;
        }
    }
}