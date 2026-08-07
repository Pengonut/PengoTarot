// PengoTarot/Enchantments/PlanetMarsEnchantment.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments
{
    public sealed class PlanetMarsEnchantment : EnchantmentModel
    {
        public override bool HasExtraCardText => true;
        public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Power;

        public override bool CanEnchant(CardModel card)
        {
            if (!base.CanEnchant(card)) return false;
            if (card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly) return false;
            return true;
        }

        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
        {
            if (cardPlay?.Target?.Player == null) return;
            var targetPlayer = cardPlay.Target.Player;
            var selfPlayer = base.Card.Owner;

            // 1. 收集完整的连通集合
            var connectedSet = new HashSet<Player>();
            void Collect(Player? player)
            {
                if (player == null || !connectedSet.Add(player)) return;
                var mp = player.Creature?.GetPower<PlanetMarsPower>();
                if (mp != null)
                    foreach (var p in mp.PairedPlayers)
                        Collect(p);
            }
            Collect(selfPlayer);
            Collect(targetPlayer);

            // 2. 更新集合内所有玩家的 MarsPower
            foreach (var player in connectedSet)
            {
                var existing = player.Creature?.GetPower<PlanetMarsPower>();

                if (existing != null)
                {
                    bool modified = false;
                    foreach (var other in connectedSet)
                    {
                        if (other != player && !existing.PairedPlayers.Contains(other))
                        {
                            existing.PairedPlayers.Add(other);
                            modified = true;
                        }
                    }
                    if (modified)
                        existing.RefreshPairedName();
                }
                else
                {
                    var newPower = (PlanetMarsPower)ModelDb.Power<PlanetMarsPower>().ToMutable();
                    newPower.PairedPlayers = connectedSet.Where(p => p != player).ToList();
                    await PowerCmd.Apply(choiceContext, newPower, player.Creature!, 1m, selfPlayer.Creature, base.Card);
                }
            }
        }
    }
}