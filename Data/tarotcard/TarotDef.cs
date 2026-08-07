
#nullable enable
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace PengoTarot.Data
{
    public class TarotDef
    {
        public string Id { get; }
        public int Cost { get; }
        public Type CardType { get; }
        public EnchantmentModel? Enchantment { get; }
        public int CardsToEnchant { get; }
        public bool IsImmediateEffect { get; }
        public Func<Player, Rng, Task>? ImmediateEffect { get; }
        public Func<Player, bool>? RequiredCharacter { get; }
        public Func<Player, bool>? AvailabilityCheck { get; }
        public int MinAct { get; }
        public double Weight { get; }

        
        public TarotDef(string id, int cost, Type cardType, EnchantmentModel enchantment,
                        int cardsToEnchant = 1,
                        Func<Player, bool>? requiredCharacter = null,
                        Func<Player, bool>? availabilityCheck = null,
                        int minAct = 1,
                        double weight = 1.0)
        {
            Id = id;
            Cost = cost;
            CardType = cardType;
            Enchantment = enchantment;
            CardsToEnchant = cardsToEnchant;
            IsImmediateEffect = false;
            ImmediateEffect = null;
            RequiredCharacter = requiredCharacter;
            AvailabilityCheck = availabilityCheck;
            MinAct = minAct;
            Weight = weight;
        }

        
        public TarotDef(string id, int cost, Type cardType,
                        Func<Player, Rng, Task>? immediateEffect = null,
                        EnchantmentModel? enchantment = null,
                        int cardsToEnchant = 3,
                        Func<Player, bool>? requiredCharacter = null,
                        Func<Player, bool>? availabilityCheck = null,
                        int minAct = 1,
                        double weight = 1.0)
        {
            Id = id;
            Cost = cost;
            CardType = cardType;
            IsImmediateEffect = true;
            ImmediateEffect = immediateEffect;

            if (immediateEffect != null)
            {
                
                Enchantment = null;
                CardsToEnchant = 0;
            }
            else
            {
                
                Enchantment = enchantment;
                CardsToEnchant = cardsToEnchant;
            }

            RequiredCharacter = requiredCharacter;
            AvailabilityCheck = availabilityCheck;
            MinAct = minAct;
            Weight = weight;
        }
    }
}