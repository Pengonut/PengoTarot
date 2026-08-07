
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Utilities
{
    public static class MultiEnchantmentHelper
    {
        private static readonly Lazy<Func<CardModel, IEnumerable<EnchantmentModel>>?> _getEnchantmentsFunc =
            new(() =>
            {
                try
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "MultiEnchantmentMod");
                    if (assembly == null)
                        return null;

                    var supportType = assembly.GetType("MultiEnchantmentMod.MultiEnchantmentSupport");
                    if (supportType == null)
                        return null;

                    var method = supportType.GetMethod("GetEnchantments", BindingFlags.Public | BindingFlags.Static);
                    if (method == null)
                        return null;

                    return (Func<CardModel, IEnumerable<EnchantmentModel>>)Delegate.CreateDelegate(
                        typeof(Func<CardModel, IEnumerable<EnchantmentModel>>), method);
                }
                catch
                {
                    return null;
                }
            });

        
        
        
        public static bool HasEnchantment<T>(CardModel? card) where T : EnchantmentModel
        {
            return HasEnchantment(card, typeof(T));
        }

        public static bool HasEnchantment(CardModel? card, Type enchantmentType)
        {
            if (card == null)
                return false;

            
            var getEnchantments = _getEnchantmentsFunc.Value;
            if (getEnchantments != null)
            {
                try
                {
                    return getEnchantments(card).Any(e => e.GetType() == enchantmentType);
                }
                catch
                {
                    
                }
            }

            
            return card.Enchantment?.GetType() == enchantmentType;
        }

        
        
        
        public static IEnumerable<T> GetEnchantmentsOfType<T>(CardModel? card) where T : EnchantmentModel
        {
            if (card == null)
                return Enumerable.Empty<T>();

            var getEnchantments = _getEnchantmentsFunc.Value;
            if (getEnchantments != null)
            {
                try
                {
                    return getEnchantments(card).OfType<T>();
                }
                catch { }
            }

            
            if (card.Enchantment is T enchantment)
                return new[] { enchantment };
            return Enumerable.Empty<T>();
        }
    }
}