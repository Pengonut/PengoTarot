// PengoTarot/Powers/PlanetEarthPower.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Powers
{
    public sealed class PlanetEarthPower : PowerModel
    {
        public List<Player> PairedPlayers { get; set; } = new();
        internal static bool DisableEarthModifier;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            new[] { new StringVar("PairedName") };

        public void RefreshPairedName()
        {
            if (PairedPlayers.Count == 0) return;
            var names = PairedPlayers
                .Select(p => PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, p.NetId));
            ((StringVar)DynamicVars["PairedName"]).StringValue = string.Join(", ", names);
        }

        public override Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            RefreshPairedName();
            Flash();
            return Task.CompletedTask;
        }

        public override decimal ModifyMaxEnergy(Player player, decimal amount)
        {
            if (DisableEarthModifier)
                return amount;

            if (player != base.Owner?.Player || PairedPlayers.Count == 0)
                return amount;

            var field = typeof(Player).GetField("<MaxEnergy>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return amount;

            var seen = new HashSet<Player>();
            decimal additional = 0m;
            foreach (var paired in PairedPlayers)
            {
                if (paired != null && seen.Add(paired) && paired != player)
                    additional += (int)(field.GetValue(paired) ?? 0);
            }

            return amount + additional;
        }
    }
}