// PengoTarot/Enchantments/PlanetEarthEnchantment.cs
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
    public sealed class PlanetEarthEnchantment : EnchantmentModel
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

            // 1. 收集完整的连通集合：以两人为起点，BFS 遍历所有已连接的 EarthPower 持有者
            var connectedSet = new HashSet<Player>();
            void Collect(Player? player)
            {
                if (player == null || !connectedSet.Add(player)) return;
                var ep = player.Creature?.GetPower<PlanetEarthPower>();
                if (ep != null)
                    foreach (var p in ep.PairedPlayers)
                        Collect(p);
            }
            Collect(selfPlayer);
            Collect(targetPlayer);

            bool anyNewConnection = false;

            // 2. 更新集合内所有玩家的 EarthPower（新人新建，老人追加）
            foreach (var player in connectedSet)
            {
                var existing = player.Creature?.GetPower<PlanetEarthPower>();

                if (existing != null)
                {
                    bool modified = false;
                    foreach (var other in connectedSet)
                    {
                        if (other != player && !existing.PairedPlayers.Contains(other))
                        {
                            existing.PairedPlayers.Add(other);
                            modified = true;
                            anyNewConnection = true;
                        }
                    }
                    if (modified)
                        existing.RefreshPairedName();
                }
                else
                {
                    var newPower = (PlanetEarthPower)ModelDb.Power<PlanetEarthPower>().ToMutable();
                    newPower.PairedPlayers = connectedSet.Where(p => p != player).ToList();
                    await PowerCmd.Apply(choiceContext, newPower, player.Creature!, 1m, selfPlayer.Creature, base.Card);
                    anyNewConnection = true;
                }
            }

            // 3. 有新增连接时合并两个集群的能量
            //    打出者和目标各自代表其所在集群（集群内能量已均等）
            if (anyNewConnection)
            {
                int totalEnergy = (selfPlayer.PlayerCombatState?.Energy ?? 0)
                                + (targetPlayer.PlayerCombatState?.Energy ?? 0);
                foreach (var player in connectedSet)
                    await PlayerCmd.SetEnergy(totalEnergy, player);
            }
        }
    }
}