#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.RestSite;

namespace PengoTarot.Relics
{
    public sealed class StargazerKit : RelicModel
    {
        private const int InitialCharges = 1;
        private const int AncientBonusCharges = 2;

        private int _timesUsed;
        private int _totalCharges = InitialCharges;

        public override RelicRarity Rarity => RelicRarity.Event;
        public override bool ShowCounter => !IsUsedUp;
        public override bool IsUsedUp => TimesUsed >= TotalCharges;
        public override int DisplayAmount => TotalCharges - TimesUsed;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[1]
        {
            new DynamicVar("Uses", TotalCharges)
        };

        [SavedProperty]
        public int TimesUsed
        {
            get => _timesUsed;
            set
            {
                AssertMutable();
                _timesUsed = value;
                UpdateDynamicVars();
                InvokeDisplayAmountChanged();
                CheckIfUsedUp();
            }
        }

        [SavedProperty]
        public int TotalCharges
        {
            get => _totalCharges;
            set
            {
                AssertMutable();
                _totalCharges = value;
                UpdateDynamicVars();
                InvokeDisplayAmountChanged();
                CheckIfUsedUp();
            }
        }

        private void UpdateDynamicVars()
        {
            base.DynamicVars["Uses"].BaseValue = TotalCharges - TimesUsed;
        }

        public override Task AfterRoomEntered(AbstractRoom room)
        {
            if (base.Owner.RunState.CurrentMapPoint?.PointType == MapPointType.Ancient
                && room.ModelId != ModelDb.AncientEvent<Neow>().Id)
            {
                TotalCharges += AncientBonusCharges;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Consume one charge of the Stargazer. Called when the player confirms a planet card selection.
        /// </summary>
        public void UseCharge()
        {
            if (!IsUsedUp)
            {
                TimesUsed++;
            }
        }

        private void CheckIfUsedUp()
        {
            base.Status = IsUsedUp ? RelicStatus.Disabled : RelicStatus.Normal;
        }

        public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
        {
            if (player != base.Owner)
                return false;
            if (IsUsedUp)
                return false;
            options.Add(new StargazeRestSiteOption(player));
            return true;
        }
    }
}