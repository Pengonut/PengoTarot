
#nullable enable
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace PengoTarot
{
    public static class TarEnglishLocHelper
    {
        public static void InjectAll()
        {
            var loc = LocManager.Instance;

            var cardLibraryTable = loc.GetTable("card_library");
            cardLibraryTable.MergeWith(new Dictionary<string, string>
            {
                { "POOL_TAROT_TIP", "Tarot card." },
                { "POOL_PLANET_TIP", "Planet card." }
            });

            
            var cardsTable = loc.GetTable("cards");
            cardsTable.MergeWith(new Dictionary<string, string>
            {
                
                { "TAR_FOOL_UPRIGHT.title", "0 - The Fool" },
                { "TAR_FOOL_UPRIGHT.description", "Choose a card to [gold]Enchant[/gold] with\n[purple]The Fool - Upright[/purple]." },
                { "TAR_FOOL_REVERSED.title", "0 - The Fool" },
                { "TAR_FOOL_REVERSED.description", "\nChoose a card that does not cost [blue]X[/blue] to [gold]Enchant[/gold] with\n[purple]The Fool - Reversed[/purple]." },

                
                { "TAR_MAGICIAN_UPRIGHT.title", "I - The Magician" },
                { "TAR_MAGICIAN_UPRIGHT.description", "Choose 5 Skills or Attacks without [gold]Exhaust[/gold] to [gold]Enchant[/gold] with\n[purple]The Magician - Upright[/purple]." },
                { "TAR_MAGICIAN_REVERSED.title", "I - The Magician" },
                { "TAR_MAGICIAN_REVERSED.description", "\nChoose a Skill or Attack without [gold]Exhaust[/gold] to [gold]Enchant[/gold] with\n[purple]The Magician - Reversed[/purple]." },

                
                { "TAR_HIGH_PRIESTESS_UPRIGHT.title", "II - The High Priestess" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT.description", "Choose 3 cards without [gold]Ethereal[/gold] to [gold]Enchant[/gold] with\n[purple]The High Priestess - Upright[/purple]." },
                { "TAR_HIGH_PRIESTESS_REVERSED.title", "II - The High Priestess" },
                { "TAR_HIGH_PRIESTESS_REVERSED.description", "\nChoose a card without [gold]Ethereal[/gold] to [gold]Enchant[/gold] with\n[purple]The High Priestess - Reversed[/purple]." },

                
                { "TAR_EMPRESS_UPRIGHT.title", "III - The Empress" },
                { "TAR_EMPRESS_UPRIGHT.description", "Choose a card to [gold]Enchant[/gold] with\n[purple]The Empress - Upright[/purple]." },
                { "TAR_EMPRESS_REVERSED.title", "III - The Empress" },
                { "TAR_EMPRESS_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]The Empress - Reversed[/purple]." },

                
                { "TAR_EMPEROR_UPRIGHT.title", "IV - The Emperor" },
                { "TAR_EMPEROR_UPRIGHT.description", "Choose a card to [gold]Enchant[/gold] with\n[purple]The Emperor - Upright[/purple]." },
                { "TAR_EMPEROR_REVERSED.title", "IV - The Emperor" },
                { "TAR_EMPEROR_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]The Emperor - Reversed[/purple]." },

                
                { "TAR_HIEROPHANT_UPRIGHT.title", "V - The Hierophant" },
                { "TAR_HIEROPHANT_UPRIGHT.description", "Choose a card to [gold]Enchant[/gold] with\n[purple]The Hierophant - Upright[/purple]." },
                { "TAR_HIEROPHANT_REVERSED.title", "V - The Hierophant" },
                { "TAR_HIEROPHANT_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]The Hierophant - Reversed[/purple]." },

                
                { "TAR_LOVERS_UPRIGHT.title", "VI - The Lovers" },
                { "TAR_LOVERS_UPRIGHT.description", "Choose 2 cards to [gold]Enchant[/gold] with\n[purple]The Lovers - Upright[/purple]." },
                { "TAR_LOVERS_REVERSED.title", "VI - The Lovers" },
                { "TAR_LOVERS_REVERSED.description", "\nChoose 2 cards that do not cost [blue]X[/blue] to [gold]Enchant[/gold] with\n[purple]The Lovers - Reversed[/purple]." },

                
                { "TAR_CHARIOT_UPRIGHT.title", "VII - The Chariot" },
                { "TAR_CHARIOT_UPRIGHT.description", "Choose a non-multi-hit Attack to [gold]Enchant[/gold] with\n[purple]The Chariot - Upright[/purple]." },
                { "TAR_CHARIOT_REVERSED.title", "VII - The Chariot" },
                { "TAR_CHARIOT_REVERSED.description", "\nChoose a multi-hit Attack to [gold]Enchant[/gold] with\n[purple]The Chariot - Reversed[/purple]." },

                
                { "TAR_STRENGTH_UPRIGHT.title", "VIII - Strength" },
                { "TAR_STRENGTH_UPRIGHT.description", "Choose a non-multi-hit Attack to [gold]Enchant[/gold] with\n[purple]Strength - Upright[/purple]." },
                { "TAR_STRENGTH_REVERSED.title", "VIII - Strength" },
                { "TAR_STRENGTH_REVERSED.description", "\nChoose a multi-hit Attack to [gold]Enchant[/gold] with\n[purple]Strength - Reversed[/purple]." },

                
                { "TAR_HERMIT_UPRIGHT.title", "IX - The Hermit" },
                { "TAR_HERMIT_UPRIGHT.description", "Choose 2 Skills or Attacks to [gold]Enchant[/gold] with\n[purple]The Hermit - Upright[/purple]." },
                { "TAR_HERMIT_REVERSED.title", "IX - The Hermit" },
                { "TAR_HERMIT_REVERSED.description", "\nChoose 2 Skills or Attacks to [gold]Enchant[/gold] with\n[purple]The Hermit - Reversed[/purple]." },

                
                { "TAR_WHEEL_OF_FORTUNE_UPRIGHT.title", "X - Wheel of Fortune" },
                { "TAR_WHEEL_OF_FORTUNE_UPRIGHT.description", "[red]Lose[/red] 11 HP. [gold]Copy[/gold] a random non-Ancient Relic." },
                { "TAR_WHEEL_OF_FORTUNE_REVERSED.title", "X - Wheel of Fortune" },
                { "TAR_WHEEL_OF_FORTUNE_REVERSED.description", "\nRandomly [red]destroy[/red] two non-Ancient Relics, then [gold]copy[/gold] a random non-Ancient Relic 3 times." },

                
                { "TAR_JUSTICE_UPRIGHT.title", "XI - Justice" },
                { "TAR_JUSTICE_UPRIGHT.description", "Choose a non-AoE Attack without [gold]Exhaust[/gold] to [gold]Enchant[/gold] with\n[purple]Justice - Upright[/purple]." },
                { "TAR_JUSTICE_REVERSED.title", "XI - Justice" },
                { "TAR_JUSTICE_REVERSED.description", "\nChoose a non-AoE Attack without [gold]Exhaust[/gold] to [gold]Enchant[/gold] with\n[purple]Justice - Reversed[/purple]." },

                
                { "TAR_HANGED_MAN_UPRIGHT.title", "XII - The Hanged Man" },
                { "TAR_HANGED_MAN_UPRIGHT.description", "Choose a card with [gold]Exhaust[/gold] to [gold]Enchant[/gold] with\n[purple]The Hanged Man - Upright[/purple]." },
                { "TAR_HANGED_MAN_REVERSED.title", "XII - The Hanged Man" },
                { "TAR_HANGED_MAN_REVERSED.description", "\nChoose a card with [gold]Exhaust[/gold] to [gold]Enchant[/gold] with\n[purple]The Hanged Man - Reversed[/purple]." },

                
                { "TAR_DEATH_UPRIGHT.title", "XIII - Death" },
                { "TAR_DEATH_UPRIGHT.description", "Choose a card to [gold]Enchant[/gold] with\n[purple]Death - Upright[/purple]." },
                { "TAR_DEATH_REVERSED.title", "XIII - Death" },
                { "TAR_DEATH_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]Death - Reversed[/purple]." },

                
                { "TAR_TEMPERANCE_UPRIGHT.title", "XIV - Temperance" },
                { "TAR_TEMPERANCE_UPRIGHT.description", "Choose a card to [gold]Enchant[/gold] with\n[purple]Temperance - Upright[/purple]." },
                { "TAR_TEMPERANCE_REVERSED.title", "XIV - Temperance" },
                { "TAR_TEMPERANCE_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]Temperance - Reversed[/purple]." },

                
                { "TAR_DEVIL_UPRIGHT.title", "XV - The Devil" },
                { "TAR_DEVIL_UPRIGHT.description", "Choose a card to [gold]Enchant[/gold] with\n[purple]The Devil - Upright[/purple]." },
                { "TAR_DEVIL_REVERSED.title", "XV - The Devil" },
                { "TAR_DEVIL_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]The Devil - Reversed[/purple]." },

                
                { "TAR_TOWER_UPRIGHT.title", "XVI - The Tower" },
                { "TAR_TOWER_UPRIGHT.description", "[red]Remove[/red] all Basic, Common, and Uncommon cards from your deck. Gain 45 [gold]Gold[/gold] for each card removed." },
                { "TAR_TOWER_REVERSED.title", "XVI - The Tower" },
                { "TAR_TOWER_REVERSED.description", "\n[red]Remove[/red] all Basic and Common cards from your deck. Gain 15 [gold]Gold[/gold] for each card removed." },

                
                { "TAR_STAR_UPRIGHT.title", "XVII - The Star" },
                { "TAR_STAR_UPRIGHT.description", "Choose 3 cards to [gold]Enchant[/gold] with\n[purple]The Star - Upright[/purple]." },
                { "TAR_STAR_REVERSED.title", "XVII - The Star" },
                { "TAR_STAR_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]The Star - Reversed[/purple]." },

                
                { "TAR_MOON_UPRIGHT.title", "XVIII - The Moon" },
                { "TAR_MOON_UPRIGHT.description", "Choose a Skill or Attack to [gold]Enchant[/gold] with\n[purple]The Moon - Upright[/purple]." },
                { "TAR_MOON_REVERSED.title", "XVIII - The Moon" },
                { "TAR_MOON_REVERSED.description", "\nChoose a Skill or Attack to [gold]Enchant[/gold] with\n[purple]The Moon - Reversed[/purple]." },

                
                { "TAR_SUN_UPRIGHT.title", "XIX - The Sun" },
                { "TAR_SUN_UPRIGHT.description", "Choose 3 cards to [gold]Enchant[/gold] with\n[purple]The Sun - Upright[/purple]." },
                { "TAR_SUN_REVERSED.title", "XIX - The Sun" },
                { "TAR_SUN_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]The Sun - Reversed[/purple]." },

                
                { "TAR_JUDGEMENT_UPRIGHT.title", "XX - Judgement" },
                { "TAR_JUDGEMENT_UPRIGHT.description", "Choose 2 cards to [gold]Enchant[/gold] with\n[purple]Judgement - Upright[/purple]." },
                { "TAR_JUDGEMENT_REVERSED.title", "XX - Judgement" },
                { "TAR_JUDGEMENT_REVERSED.description", "\nChoose up to 21 cards to [gold]Enchant[/gold] with\n[purple]Judgement - Reversed[/purple]." },

                
                { "TAR_WORLD_UPRIGHT.title", "XXI - The World" },
                { "TAR_WORLD_UPRIGHT.description", "Choose a card to [gold]Enchant[/gold] with\n[purple]The World - Upright[/purple]." },
                { "TAR_WORLD_REVERSED.title", "XXI - The World" },
                { "TAR_WORLD_REVERSED.description", "\nChoose a card to [gold]Enchant[/gold] with\n[purple]The World - Reversed[/purple]." },

                
                
                { "TAR_DEVIL_UPRIGHT_SUB.title", "Negative - The Devil" },
                { "TAR_DEVIL_UPRIGHT_SUB.description", "Choose 3 cards, [gold]Transform[/gold] them into Ironclad cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The Devil - Upright[/purple]." },
                { "TAR_DEVIL_REVERSED_SUB.title", "Negative - The Devil" },
                { "TAR_DEVIL_REVERSED_SUB.description", "\nChoose 3 cards, [gold]Transform[/gold] them into Ironclad cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The Devil - Reversed[/purple]." },
                
                { "TAR_MOON_UPRIGHT_SUB.title", "Negative - The Moon" },
                { "TAR_MOON_UPRIGHT_SUB.description", "Choose 3 cards, [gold]Transform[/gold] them into Silent cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The Moon - Upright[/purple]." },
                { "TAR_MOON_REVERSED_SUB.title", "Negative - The Moon" },
                { "TAR_MOON_REVERSED_SUB.description", "\nChoose 3 cards, [gold]Transform[/gold] them into Silent cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The Moon - Reversed[/purple]." },
                
                { "TAR_STAR_UPRIGHT_SUB.title", "Negative - The Star" },
                { "TAR_STAR_UPRIGHT_SUB.description", "Choose 3 cards, [gold]Transform[/gold] them into Regent cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The Star - Upright[/purple]." },
                { "TAR_STAR_REVERSED_SUB.title", "Negative - The Star" },
                { "TAR_STAR_REVERSED_SUB.description", "\nChoose 3 cards, [gold]Transform[/gold] them into Regent cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The Star - Reversed[/purple]." },
                
                { "TAR_SUN_UPRIGHT_SUB.title", "Negative - The Sun" },
                { "TAR_SUN_UPRIGHT_SUB.description", "Choose 3 cards, [gold]Transform[/gold] them into Necrobinder cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The Sun - Upright[/purple]." },
                { "TAR_SUN_REVERSED_SUB.title", "Negative - The Sun" },
                { "TAR_SUN_REVERSED_SUB.description", "\nChoose 3 cards, [gold]Transform[/gold] them into Necrobinder cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The Sun - Reversed[/purple]." },
                
                { "TAR_WORLD_UPRIGHT_SUB.title", "Negative - The World" },
                { "TAR_WORLD_UPRIGHT_SUB.description", "Choose 3 cards, [gold]Transform[/gold] them into Defect cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The World - Upright[/purple]." },
                { "TAR_WORLD_REVERSED_SUB.title", "Negative - The World" },
                { "TAR_WORLD_REVERSED_SUB.description", "\nChoose 3 cards, [gold]Transform[/gold] them into Defect cards of the same rarity, and [gold]Enchant[/gold] them with\n[purple]Negative - The World - Reversed[/purple]." },


                { "PLANET_MERCURY.title", "Mercury" },
                { "PLANET_MERCURY.description", "Choose a Power card to\n[gold]Enchant[/gold] with\n[purple]Mercury[/purple]." },
                { "PLANET_VENUS.title", "Venus" },
                { "PLANET_VENUS.description", "Choose a Power card to\n[gold]Enchant[/gold] with\n[purple]Venus[/purple]." },
                { "PLANET_EARTH.title", "Earth" },
                { "PLANET_EARTH.description", "Choose a Power card to\n[gold]Enchant[/gold] with\n[purple]Earth[/purple]." },
                { "PLANET_MARS.title", "Mars" },
                { "PLANET_MARS.description", "Choose a Power card to\n[gold]Enchant[/gold] with\n[purple]Mars[/purple]." },

                { "PLANET_JUPITER.title", "Jupiter" },
                { "PLANET_JUPITER.description", "Choose an Attack card to\n[gold]Enchant[/gold] with\n[purple]Jupiter[/purple]." },
                { "PLANET_SATURN.title", "Saturn" },
                { "PLANET_SATURN.description", "Choose an Attack card to\n[gold]Enchant[/gold] with\n[purple]Saturn[/purple]." },
                { "PLANET_URANUS.title", "Uranus" },
                { "PLANET_URANUS.description", "Choose an Attack card to\n[gold]Enchant[/gold] with\n[purple]Uranus[/purple]." },
                { "PLANET_NEPTUNE.title", "Neptune" },
                { "PLANET_NEPTUNE.description", "Choose an Attack card to\n[gold]Enchant[/gold] with\n[purple]Neptune[/purple]." },

                { "PLANET_PLUTO.title", "Pluto" },
                { "PLANET_PLUTO.description", "Choose a Skill card to\n[gold]Enchant[/gold] with\n[purple]Pluto[/purple]." },
                { "PLANET_X.title", "Planet X" },
                { "PLANET_X.description", "Choose a Skill card to\n[gold]Enchant[/gold] with\n[purple]Planet X[/purple]." },
                { "PLANET_CERES.title", "Ceres" },
                { "PLANET_CERES.description", "Choose a Skill card to\n[gold]Enchant[/gold] with\n[purple]Ceres[/purple]." },
                { "PLANET_ERIS.title", "Eris" },
                { "PLANET_ERIS.description", "Choose a Skill card to\n[gold]Enchant[/gold] with\n[purple]Eris[/purple]." },
            });

            
            var enchantmentsTable = loc.GetTable("enchantments");
            enchantmentsTable.MergeWith(new Dictionary<string, string>
            {
                
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.description", "The first time you play this each combat, return it to your [gold]Hand[/gold]." },
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.title", "The Fool" },
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.extraCardText", "The first time you play this, return it to your [gold]Hand[/gold]." },
                
                { "TAR_FOOL_REVERSED_ENCHANTMENT.description", "The first time you play this each combat, add a copy that costs {energyPrefix:energyIcons(1)} more into your [gold]Hand[/gold]." },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.title", "0" },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.extraCardText", "The first time you play this, add a copy that costs {energyPrefix:energyIcons(1)} more to your hand." },

                
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.description", "This card gains [gold]Exhaust[/gold]." },
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.title", "The Magician" },
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.extraCardText", "The Magician - Upright." },
                
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.description", "After this card is played, it is placed at a random position in your [gold]draw pile[/gold]." },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.title", "I" },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.extraCardText", "Placed at a random position in your [gold]draw pile[/gold]." },

                
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.description", "This card gains [gold]Ethereal[/gold]." },
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.title", "The High Priestess" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.extraCardText", "The High Priestess - Upright." },
                
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.description", "This card gains [gold]Retain[/gold]. At the end of your turn, if this is in your hand, give [gold]Ethereal[/gold] to all cards to its left in your hand." },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.title", "II" },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.extraCardText", "At the end of your turn, if this is in your hand, give [gold]Ethereal[/gold] to all cards to its left in your hand." },

                
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.description", "The third and later times you play this each combat, it gains [gold]Replay[/gold] [blue]1[/blue]." },
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.title", "The Empress" },
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.extraCardText", "Played {PlayCount} times." },
                
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.description", "Costs {energyPrefix:energyIcons(1)} less. This card cannot be played until it has entered the [gold]discard pile[/gold] at least once." },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.title", "III" },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.extraCardText", "Cannot be played until it has entered the discard pile once." },

                
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.description", "This card gains [gold]Retain[/gold]." },
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.title", "The Emperor" },
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.extraCardText", "The Emperor - Upright." },
                
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.description", "At the end of your turn, if this is in your hand, give [gold]Retain[/gold] to 2 random cards in your hand." },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.title", "IV" },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.extraCardText", "At the end of your turn, if this is in your hand, give [gold]Retain[/gold] to 2 random cards in your hand." },

                
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.description", "After this card is played, [gold]Upgrade[/gold] a card in your [gold]hand[/gold]." },
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.title", "The Hierophant" },
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.extraCardText", "[gold]Upgrade[/gold] a card in your [gold]hand[/gold]." },
                
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.description", "After this card is played, [red]Downgrade[/red] any number of cards in your [gold]hand[/gold]. For each card [red]downgraded[/red], randomly [gold]Upgrade[/gold] 3 cards in your draw or discard pile." },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.title", "V" },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.extraCardText", "This card spares no expense." },

                
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.description", "When this enters your hand, put another card enchanted with [purple]The Lovers[/purple] from your draw, discard, or exhaust pile into your hand." },
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.title", "The Lovers" },
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.extraCardText", "This card seeks The Lovers." },
                
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.description", "After you play this, put another card enchanted with [purple]The Lovers[/purple] into your hand. It costs {energyPrefix:energyIcons(1)} more this turn." },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.title", "VI" },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.extraCardText", "This card seeks The Lovers...?" },

                
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.description", "Apply an additional [blue]1[/blue] [gold]Vulnerable[/gold]." },
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.title", "The Chariot" },
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.extraCardText", "Apply an additional 1 [gold]Vulnerable[/gold]." },
                
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.description", "For each time this card deals damage to an enemy, apply an additional [blue]1[/blue] [gold]Vulnerable[/gold].\nApply [blue]1[/blue] [gold]Vulnerable[/gold] to yourself." },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.title", "VII" },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.extraCardText", "For each hit, apply 1 [gold]Vulnerable[/gold].\nApply 1 [gold]Vulnerable[/gold] to yourself." },

                
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.description", "Apply an additional [blue]1[/blue] [gold]Weak[/gold]." },
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.title", "Strength" },
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.extraCardText", "Apply an additional 1 [gold]Weak[/gold]." },
                
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.description", "For each time this card deals damage to an enemy, apply an additional [blue]1[/blue] [gold]Weak[/gold].\nApply [blue]1[/blue] [gold]Weak[/gold] to yourself." },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.title", "VIII" },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.extraCardText", "For each hit, apply 1 [gold]Weak[/gold].\nApply 1 [gold]Weak[/gold] to yourself." },

                
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.description", "At the start of combat, [gold]Exhaust[/gold] this. At the start of turn 2, put this into your hand." },
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.title", "The Hermit" },
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.extraCardText", "The Hermit - Upright." },
                
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.description", "At the start of combat, [gold]Exhaust[/gold] this. At the start of turn 7, put it into your hand. It costs 0 this turn." },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.title", "IX" },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.extraCardText", "The Hermit - Reversed." },

                
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.description", "Gains [gold]Exhaust[/gold]. This card deals double damage." },
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.title", "Justice" },
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.extraCardText", "Justice - Upright." },
                
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.description", "Gains [gold]Exhaust[/gold]. When you deal damage with this, gain [gold]Block[/gold] equal to the damage dealt." },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.title", "XI" },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.extraCardText", "Gain [gold]Block[/gold] equal to the damage dealt." },

                
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.description", "When another card would be [gold]Exhausted[/gold], if this is in your draw pile, play this, preventing the other card from being Exhausted." },
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.title", "The Hanged Man" },
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.extraCardText", "This card yearns for sacrifice." },
                
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.description", "When played, [gold]Exhaust[/gold] a random card in your hand. This card goes to your discard pile." },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.title", "XII" },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.extraCardText", "This card yearns for sacrifice...?" },

                
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.description", "This card costs 0 to play. After playing this, end your turn." },
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.title", "Death" },
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.extraCardText", "End your turn after playing this." },
                
                { "TAR_DEATH_REVERSED_ENCHANTMENT.description", "This card costs 0 to play. While in your hand, you cannot draw cards.\nThis card draws cards when played." },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.title", "XIII" },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.extraCardText", "Death watches over you." },

                
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.description", "The first time you play this each combat, gain [blue]10[/blue] [gold]Gold[/gold]." },
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.title", "Temperance" },
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.extraCardText", "The first time you play this each combat, gain 10 [gold]Gold[/gold]." },
                
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.description", "After this card is played, for each point of HP you lose this turn, gain [blue]5[/blue] [gold]Gold[/gold] at the end of combat." },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.title", "XIV" },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.extraCardText", "For each point of HP you lose this turn, gain 5 [gold]Gold[/gold] at the end of combat." },

                
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.description", "Costs {energyPrefix:energyIcons(1)} less. While in your hand, you must play this before other cards." },
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.title", "The Devil" },
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.extraCardText", "A *fair* exchange." },
                
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.description", "Costs {energyPrefix:energyIcons(1)} less for every [blue]3[/blue] HP you lose this combat. Resets when played." },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.title", "XV" },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.extraCardText", "Fueled by blood." },

                
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.description", "When played, gain [gold]Block[/gold] equal to your current [img]res://images/packed/sprite_fonts/star_icon.png[/img]." },
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.title", "The Star" },
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.extraCardText", "The stars watch, the cosmos clothes." },
                
                { "TAR_STAR_REVERSED_ENCHANTMENT.description", "Swap this card's {energyPrefix:energyIcons(1)} cost and [img]res://images/packed/sprite_fonts/star_icon.png[/img] cost." },
                { "TAR_STAR_REVERSED_ENCHANTMENT.title", "XVII" },
                { "TAR_STAR_REVERSED_ENCHANTMENT.extraCardText", "The stars turn, I walk in reverse." },

                
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.description", "At the end of your first turn, automatically play this card, then place it back into your [gold]draw pile[/gold]." },
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.title", "The Moon" },
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.extraCardText", "......" },
                
                { "TAR_MOON_REVERSED_ENCHANTMENT.description", "Whenever this is discarded, return it to your hand." },
                { "TAR_MOON_REVERSED_ENCHANTMENT.title", "XVIII" },
                { "TAR_MOON_REVERSED_ENCHANTMENT.extraCardText", "......" },

                
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.description", "When played, halve the stacks of a random [gold]debuff[/gold] on you." },
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.title", "The Sun" },
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.extraCardText", "May rot remain at bay." },
                
                { "TAR_SUN_REVERSED_ENCHANTMENT.description", "This card costs 0 to play. Instead, its original energy cost gives you [blue]6[/blue] times that amount of [gold]Doom[/gold]." },
                { "TAR_SUN_REVERSED_ENCHANTMENT.title", "XIX" },
                { "TAR_SUN_REVERSED_ENCHANTMENT.extraCardText", "May you finally rest." },

                
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.description", "At the start of combat, this card randomly [gold]Transforms[/gold]." },
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.title", "Judgement" },
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.extraCardText", "At the start of combat, transforms into a random card." },
                
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.description", "At the start of combat, all cards with this enchantment are placed on the bottom of your draw pile in deck insertion order." },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.title", "XX" },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.extraCardText", "Judgement - Reversed." },

                
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.description", "Whenever you [gold]Evoke[/gold] an Orb, put this card into your hand from anywhere else, and it costs {energyPrefix:energyIcons(1)} more this turn." },
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.title", "The World" },
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.extraCardText", "Hello, World." },
                
                { "TAR_WORLD_REVERSED_ENCHANTMENT.description", "The first time you play this each combat, gain [blue]1[/blue] [gold]Artifact[/gold]." },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.title", "XXI" },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.extraCardText", "Defect." },

                
                
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.title", "Negative - The Devil" },
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], draw 2 cards." },
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "Draw 2 cards when entering your hand." },
                
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.title", "Negative - The Devil" },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], gain {energyPrefix:energyIcons(1)}." },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.extraCardText", "Gain {energyPrefix:energyIcons(1)} when entering your hand." },
                
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.title", "Negative - The Moon" },
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], draw 2 cards." },
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "Draw 2 cards when entering your hand." },
                
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.title", "Negative - The Moon" },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], gain {energyPrefix:energyIcons(1)}." },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.extraCardText", "Gain {energyPrefix:energyIcons(1)} when entering your hand." },
                
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.title", "Negative - The Star" },
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], draw 2 cards." },
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "Draw 2 cards when entering your hand." },
                
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.title", "Negative - The Star" },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], gain {energyPrefix:energyIcons(1)}." },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.extraCardText", "Gain {energyPrefix:energyIcons(1)} when entering your hand." },
                
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.title", "Negative - The Sun" },
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], draw 2 cards." },
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "Draw 2 cards when entering your hand." },
                
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.title", "Negative - The Sun" },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], gain {energyPrefix:energyIcons(1)}." },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.extraCardText", "Gain {energyPrefix:energyIcons(1)} when entering your hand." },
                
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.title", "Negative - The World" },
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], draw 2 cards." },
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "Draw 2 cards when entering your hand." },
                
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.title", "Negative - The World" },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.description", "Whenever this card enters your [gold]hand[/gold], gain {energyPrefix:energyIcons(1)}." },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.extraCardText", "Gain {energyPrefix:energyIcons(1)} when entering your hand." },


                { "PLANET_MERCURY_ENCHANTMENT.title", "Mercury" },
                { "PLANET_MERCURY_ENCHANTMENT.description", "Choose an ally. At the end of their turn each combat, look at the top 5 cards of their [gold]draw pile[/gold] and choose any number to discard." },
                { "PLANET_MERCURY_ENCHANTMENT.extraCardText", "For you, I pierce the mist." },

                { "PLANET_VENUS_ENCHANTMENT.title", "Venus" },
                { "PLANET_VENUS_ENCHANTMENT.description", "Choose an ally. At the end of their turn each combat, look at the top 5 cards of their [gold]discard pile[/gold] and choose any number to put into their [gold]hand[/gold]." },
                { "PLANET_VENUS_ENCHANTMENT.extraCardText", "For you, I save the lost." },

                { "PLANET_EARTH_ENCHANTMENT.title", "Earth" },
                { "PLANET_EARTH_ENCHANTMENT.description", "Choose an ally. For this combat, synchronize your [gold]Energy[/gold] with theirs." },
                { "PLANET_EARTH_ENCHANTMENT.extraCardText", "With you, I share the ground." },

                { "PLANET_MARS_ENCHANTMENT.title", "Mars" },
                { "PLANET_MARS_ENCHANTMENT.description", "Choose an ally. For this combat, synchronize your all creations with theirs." },
                { "PLANET_MARS_ENCHANTMENT.extraCardText", "With you, I craft the new." },

                { "PLANET_JUPITER_ENCHANTMENT.title", "Jupiter" },
                { "PLANET_JUPITER_ENCHANTMENT.description", "Enemies hit by this card grant [gold]Gold[/gold] to ALL players equal to the attack damage they take this turn.\n10s after playing, [red]end your turn[/red]." },
                { "PLANET_JUPITER_ENCHANTMENT.extraCardText", "I come to bring the gold." },

                { "PLANET_SATURN_ENCHANTMENT.title", "Saturn" },
                { "PLANET_SATURN_ENCHANTMENT.description", "Enemies hit by this card cannot take less attack damage than this card's damage this turn.\n10s after playing, [red]end your turn[/red]." },
                { "PLANET_SATURN_ENCHANTMENT.extraCardText", "I come to hold the line." },

                { "PLANET_URANUS_ENCHANTMENT.title", "Uranus" },
                { "PLANET_URANUS_ENCHANTMENT.description", "After playing, costs {energyPrefix:energyIcons(1)} more, gains [blue]2[/blue] [gold]Replay[/gold], and is placed into a random ally's [gold]draw pile[/gold]." },
                { "PLANET_URANUS_ENCHANTMENT.extraCardText", "I vow to fight once more." },

                { "PLANET_NEPTUNE_ENCHANTMENT.title", "Neptune" },
                { "PLANET_NEPTUNE_ENCHANTMENT.description", "After playing, put a copy of this card into everyone's [gold]discard pile[/gold]." },
                { "PLANET_NEPTUNE_ENCHANTMENT.extraCardText", "I vow to share the gain." },

                { "PLANET_PLUTO_ENCHANTMENT.title", "Pluto" },
                { "PLANET_PLUTO_ENCHANTMENT.description", "The first time you play this each combat, allies' [purple]Pluto[/purple]-enchanted cards cost 0 this turn and are placed into their respective hands." },
                { "PLANET_PLUTO_ENCHANTMENT.extraCardText", "For we understand." },

                { "PLANET_X_ENCHANTMENT.title", "Planet X" },
                { "PLANET_X_ENCHANTMENT.description", "The first time you play this each combat, allies' [purple]Planet X[/purple]-enchanted cards gain [blue]4[/blue] [gold]Replay[/gold]." },
                { "PLANET_X_ENCHANTMENT.extraCardText", "For we uphold." },

                { "PLANET_CERES_ENCHANTMENT.title", "Ceres" },
                { "PLANET_CERES_ENCHANTMENT.description", "The first time you play this each combat, [gold]Copy[/gold] allies' [purple]Ceres[/purple]-enchanted cards into their respective piles." },
                { "PLANET_CERES_ENCHANTMENT.extraCardText", "So never alone." },

                { "PLANET_ERIS_ENCHANTMENT.title", "Eris" },
                { "PLANET_ERIS_ENCHANTMENT.description", "The first time you play this each combat, [gold]Copy[/gold] allies' [purple]Eris[/purple]-enchanted cards into ALL players' hands. The copies have no enchantment." },
                { "PLANET_ERIS_ENCHANTMENT.extraCardText", "So all as one." },
            });

            var powersTable = loc.GetTable("powers");
            powersTable.MergeWith(new Dictionary<string, string>
            {
                { "TAR_TEMPERANCE_REVERSED_POWER.title", "Temperance - Reversed" },
                { "TAR_TEMPERANCE_REVERSED_POWER.description", "For each point of HP you lose this turn, gain an equal amount of [gold]Gold[/gold] at the end of combat." },
                { "TAR_TEMPERANCE_REVERSED_POWER.smartDescription", "For each point of HP you lose this turn, gain [blue]{Amount}[/blue] [gold]Gold[/gold] at the end of combat." },

                { "TAR_CHARIOT_REVERSED_POWER.title", "Chariot - Reversed" },
                { "TAR_CHARIOT_REVERSED_POWER.description", "The first time this enemy deals unblocked damage to you, you gain [blue]1[/blue] [gold]Vulnerable[/gold]." },
                { "TAR_CHARIOT_REVERSED_POWER.smartDescription", "The first time this enemy deals unblocked damage to you, you gain [blue]{Amount}[/blue] [gold]Vulnerable[/gold]." },

                { "TAR_STRENGTH_REVERSED_POWER.title", "Strength - Reversed" },
                { "TAR_STRENGTH_REVERSED_POWER.description", "The first time this enemy deals unblocked damage to you, you gain [blue]1[/blue] [gold]Weak[/gold]." },
                { "TAR_STRENGTH_REVERSED_POWER.smartDescription", "The first time this enemy deals unblocked damage to you, you gain [blue]{Amount}[/blue] [gold]Weak[/gold]." },

                { "TAR_HERMIT_REVERSED_POWER.title", "Hermit - Reversed" },
                { "TAR_HERMIT_REVERSED_POWER.description", "At the end of your turn, gain [gold]Block[/gold] equal to this. Each time you receive unblocked damage, reduce this by [blue]1[/blue]." },
                { "TAR_HERMIT_REVERSED_POWER.smartDescription", "At the end of your turn, gain [blue]{Amount}[/blue] [gold]Block[/gold]. Each time you receive unblocked damage, reduce this by [blue]1[/blue]." },

                { "TAR_JUSTICE_REVERSED_POWER.title", "Justice - Reversed" },
                { "TAR_JUSTICE_REVERSED_POWER.description", "The first Attack you play each turn is [gold]Exhausted[/gold]." },
                { "TAR_JUSTICE_REVERSED_POWER.smartDescription", "The first Attack you play each turn is [gold]Exhausted[/gold]." },

                { "TAR_HANGED_MAN_REVERSED_POWER.title", "Hanged Man - Reversed" },
                { "TAR_HANGED_MAN_REVERSED_POWER.description", "The first Skill you play each turn is [gold]Exhausted[/gold]." },
                { "TAR_HANGED_MAN_REVERSED_POWER.smartDescription", "The first Skill you play each turn is [gold]Exhausted[/gold]." },

                { "TAR_DEATH_REVERSED_POWER.title", "Death - Reversed" },
                { "TAR_DEATH_REVERSED_POWER.description", "In this combat, whenever you play a [gold]Power[/gold] card, [red]end your turn[/red] immediately." },
                { "TAR_DEATH_REVERSED_POWER.smartDescription", "In this combat, whenever you play a [gold]Power[/gold] card, [red]end your turn[/red] immediately." },

                { "PLANET_MERCURY_POWER.title", "Mercury" },
                { "PLANET_MERCURY_POWER.description", "After an ally's discard phase, look at the top 5 cards of their draw pile and choose any number to discard." },
                { "PLANET_MERCURY_POWER.smartDescription", "After {PairedName}'s discard phase, look at the top 5 cards of their draw pile and choose any number to discard." },

                { "PLANET_VENUS_POWER.title", "Venus" },
                { "PLANET_VENUS_POWER.description", "After an ally's discard phase, look at the top 5 cards of their [gold]discard pile[/gold] and choose any number to put into their [gold]hand[/gold]." },
                { "PLANET_VENUS_POWER.smartDescription", "After {PairedName}'s discard phase, look at the top 5 cards of their [gold]discard pile[/gold] and choose any number to put into their [gold]hand[/gold]." },

                { "PLANET_EARTH_POWER.title", "Earth" },
                { "PLANET_EARTH_POWER.description", "Share Energy{energyPrefix:energyIcons(1)} with your ally." },
                { "PLANET_EARTH_POWER.smartDescription", "Share Energy{energyPrefix:energyIcons(1)} with {PairedName}." },

                { "PLANET_MARS_POWER.title", "Mars" },
                { "PLANET_MARS_POWER.description", "Share creations with your ally." },
                { "PLANET_MARS_POWER.smartDescription", "Share creations with {PairedName}." },

                { "PLANET_JUPITER_POWER.title", "Jupiter" },
                { "PLANET_JUPITER_POWER.description", "When this enemy takes attack damage this turn, ALL players gain [gold]Gold[/gold] equal to the damage taken after combat." },
                { "PLANET_JUPITER_POWER.smartDescription", "When this enemy takes attack damage this turn, ALL players gain [blue]{Amount}[/blue] [gold]Gold[/gold] after combat." },

                { "PLANET_SATURN_POWER.title", "Saturn" },
                { "PLANET_SATURN_POWER.description", "Attack damage against this enemy cannot be lower than [blue]{Amount}[/blue] this turn." },
                { "PLANET_SATURN_POWER.smartDescription", "Attack damage against this enemy cannot be lower than [blue]{Amount}[/blue] this turn." },

                { "PLANET_GOLD_POWER.title", "Accumulate" },
                { "PLANET_GOLD_POWER.description", "At the end of combat, gain [blue]{Amount}[/blue] [gold]Gold[/gold]." },
                { "PLANET_GOLD_POWER.smartDescription", "At the end of combat, gain [blue]{Amount}[/blue] [gold]Gold[/gold]." },

                { "TICK_TACK_POWER.title", "Countdown" },
                { "TICK_TACK_POWER.description", "When it reaches zero, your turn is forced to end." },
                { "TICK_TACK_POWER.smartDescription", "After [blue]{Amount}[/blue] seconds, [red]your turn is forced to end[/red]." },
            });

            var afflictionsTable = loc.GetTable("afflictions");
            afflictionsTable.MergeWith(new Dictionary<string, string>
            {
                { "TAR_JUSTICE_REVERSED_AFFLICTION.title", "Justice - Reversed" },
                { "TAR_JUSTICE_REVERSED_AFFLICTION.description", "The first Attack you play each turn is [gold]Exhausted[/gold]." },

                { "TAR_HANGED_MAN_REVERSED_AFFLICTION.title", "Hanged Man - Reversed" },
                { "TAR_HANGED_MAN_REVERSED_AFFLICTION.description", "The first Skill you play each turn is [gold]Exhausted[/gold]." },

                { "TAR_DEATH_REVERSED_AFFLICTION.title", "Death - Reversed" },
                { "TAR_DEATH_REVERSED_AFFLICTION.description", "Ends your turn immediately when played." },
                { "TAR_DEATH_REVERSED_AFFLICTION.extraCardText", "End your turn." },
            });

            var gameplayUiTable = loc.GetTable("gameplay_ui");
            gameplayUiTable.MergeWith(new Dictionary<string, string>
            {
                { "CHOOSE_CARD_DOWNGRADE_HEADER", "Choose any number of cards to Downgrade" },
                { "PLANET_MERCURY_SELECTION_PROMPT", "Choose any number of cards from the top of your ally's draw pile to discard" },
                { "PLANET_VENUS_SELECTION_PROMPT", "Choose any number of cards from the top of your ally's discard pile to return to their hand" },
                { "VANILLA_STYLE_TAROT", "Vanilla Style Tarots" },
                { "VANILLA_STYLE_PLANET", "Vanilla Style Planets" },
            });

            var roomTable = loc.GetTable("merchant_room");
            roomTable.MergeWith(new Dictionary<string, string>
            {
                { "TAROT_PILE_ENTRY.title", "Tarot Pack" },
                { "TAROT_PILE_ENTRY.description", "Draw [blue]3[/blue] Tarot cards and pick [blue]1[/blue] to [gold]Enchant[/gold] a card in your deck.\nSometimes weird effects happen..." }
            });


            var relicsTable = loc.GetTable("relics");
            relicsTable.MergeWith(new Dictionary<string, string>
            {
                { "STARGAZER_KIT.title", "Stargazer's Kit" },
                { "STARGAZER_KIT.description", "You may [gold]Stargaze[/gold] at [gold]Rest Sites[/gold].\nEntering [gold]Ancient[/gold] nodes grants [blue]2[/blue] additional uses." },
                { "STARGAZER_KIT.flavor", "Do not go gentle into that good night." }
            });

            var restSiteTable = loc.GetTable("rest_site_ui");
            restSiteTable.MergeWith(new Dictionary<string, string>
            {
                { "OPTION_STARGAZE.description", "Choose 1 of 3 random Planet cards to [gold]Enchant[/gold] a non-multiplayer card in your [gold]Deck[/gold]." },
                { "OPTION_STARGAZE.name", "Stargaze" },
            });

            var mainMenuUiTable = loc.GetTable("main_menu_ui");
            mainMenuUiTable.MergeWith(new Dictionary<string, string>
            {
                { "HEXTECH_WARNING_TITLE", "PengoTarot × Hextech Compatibility Notice" },
                { "HEXTECH_WARNING_PAGE1", "Both PengoTarot and the Hextech mod are installed. A note from the PengoTarot author:\n\nHextech uses an outdated and heavy-handed multi-enchantment implementation that hard-codes fixes for the base game's enchantment checks, making it incompatible with the vast majority of enchantment content mods." },
                { "HEXTECH_WARNING_PAGE2", "For a more stable multi-enchantment experience, we recommend using MultiEnchantment instead;\n\nIf you have questions, please report them to the Hextech mod author first to fix its outdated implementation, as this mod cannot provide effective compatibility." },
                { "HEXTECH_WARNING_NEXT", "Next" },
                { "HEXTECH_WARNING_ACK", "OK" },
            });
        }
    }
}