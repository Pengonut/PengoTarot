#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Data;
using PengoTarot.Relics;

namespace PengoTarot.RestSite
{
    public class StargazeRestSiteOption : RestSiteOption
    {
        private List<PlanetDef>? _cachedDefs;

        public override string OptionId => "STARGAZE";

        public StargazeRestSiteOption(Player owner) : base(owner)
        {
        }

        public override async Task<bool> OnSelect()
        {
            var player = base.Owner;
            var rng = player.PlayerRng.Shops;

            if (_cachedDefs == null)
                _cachedDefs = PlanetDeck.DrawThreeUnique(player, rng);

            if (_cachedDefs.Count == 0)
                return false;

            var pendingCards = new List<CardModel>();
            var pendingMap = new Dictionary<CardModel, PlanetDef>();

            foreach (var def in _cachedDefs)
            {
                var card = (CardModel)typeof(ModelDb)
                    .GetMethod("Card", Type.EmptyTypes)!
                    .MakeGenericMethod(def.CardType)
                    .Invoke(null, null)!;
                pendingCards.Add(card);
                pendingMap[card] = def;
            }

            var context = new SilentPlayerChoiceContext();
            var selected = await CardSelectCmd.FromChooseACardScreen(
                context, pendingCards, player, canSkip: true);

            if (selected == null)
                return false;

            // Consume one charge from StargazerKit when a planet card is confirmed
            player.GetRelic<StargazerKit>()?.UseCharge();

            var selectedDef = pendingMap[selected];
            var enchantment = selectedDef.Enchantment!;
            int targetCount = selectedDef.CardsToEnchant; 
            int minSelect = 1;

            var prefs = new CardSelectorPrefs(
                CardSelectorPrefs.EnchantSelectionPrompt,
                minSelect,
                targetCount)
            {
                Cancelable = false,
                RequireManualConfirmation = true
            };

            var chosenCards = await CardSelectCmd.FromDeckForEnchantment(
                player, enchantment, targetCount, prefs);

            if (chosenCards == null || !chosenCards.Any())
                return false;

            foreach (var card in chosenCards)
            {
                CardCmd.Enchant(enchantment.ToMutable(), card, 1m);
                if (LocalContext.IsMe(player))
                    CardCmd.Preview(card);
            }

            _cachedDefs = null;
            return true;
        }

        public override Task DoLocalPostSelectVfx(System.Threading.CancellationToken ct = default) =>
            Task.CompletedTask;

        public override Task DoRemotePostSelectVfx() =>
            Task.CompletedTask;
    }
}