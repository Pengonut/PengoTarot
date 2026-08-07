#nullable enable

using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Merchant;

namespace PengoTarot.Data
{
    public static class TarotEntryHolder
    {
        private static readonly Dictionary<MerchantInventory, MerchantTarotEntry> _map = new();

        public static void SetEntry(MerchantInventory inventory, MerchantTarotEntry entry)
        {
            _map[inventory] = entry;
        }

        public static MerchantTarotEntry? GetEntry(MerchantInventory inventory)
        {
            return _map.TryGetValue(inventory, out var entry) ? entry : null;
        }

        public static void ClearEntry(MerchantInventory inventory)
        {
            _map.Remove(inventory);
        }
    }
}