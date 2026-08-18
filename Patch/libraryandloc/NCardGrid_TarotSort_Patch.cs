#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 当 TarotPool 过滤器激活时，对塔罗牌进行自定义排序：
    /// 正位 0 (Fool) → 正位 21 (World)，然后逆位 0 → 逆位 21。
    /// 负片塔罗牌（Sub）已被过滤器排除，不在此列。
    ///
    /// 使用 Harmony __args 避免对内部枚举 SortingOrders 的编译时依赖。
    /// </summary>
    [HarmonyPatch(typeof(NCardGrid), nameof(NCardGrid.SetCards))]
    public static class NCardGrid_TarotSort_Patch
    {
        /// <summary>
        /// 塔罗牌22张大阿卡纳的名称与序号映射（0=Fool, 21=World）
        /// </summary>
        private static readonly Dictionary<string, int> TarotIndex = new()
        {
            { "Fool", 0 },
            { "Magician", 1 },
            { "HighPriestess", 2 },
            { "Empress", 3 },
            { "Emperor", 4 },
            { "Hierophant", 5 },
            { "Lovers", 6 },
            { "Chariot", 7 },
            { "Strength", 8 },
            { "Hermit", 9 },
            { "WheelOfFortune", 10 },
            { "Justice", 11 },
            { "HangedMan", 12 },
            { "Death", 13 },
            { "Temperance", 14 },
            { "Devil", 15 },
            { "Tower", 16 },
            { "Star", 17 },
            { "Moon", 18 },
            { "Sun", 19 },
            { "Judgement", 20 },
            { "World", 21 },
        };

        static void Prefix(NCardGrid __instance, object[] __args)
        {
            // __args[0] = IReadOnlyList<CardModel> cardsToDisplay
            // __args[1] = PileType pileType
            // __args[2] = List<SortingOrders> sortingPriority
            if (__args.Length < 3)
                return;

            var cardsToDisplay = __args[0] as IReadOnlyList<CardModel>;
            if (cardsToDisplay == null || cardsToDisplay.Count == 0)
                return;

            // 检测是否所有显示的卡牌都属于 TarotPool
            bool allTarot = true;
            foreach (var card in cardsToDisplay)
            {
                if (card.Pool is not TarotPool)
                {
                    allTarot = false;
                    break;
                }
            }
            if (!allTarot)
                return;

            // 复制一份再排序，避免就地修改调用方持有的列表（如 NDeckViewScreen._cards）
            var sortedList = new List<CardModel>(cardsToDisplay);
            sortedList.Sort(CompareTarotCards);
            __args[0] = sortedList;

            // 用新的 [Ascending] 列表替换 sortingPriority，避免清空调用方持有的排序状态
            // （如图鉴 NCardLibrary._sortingPriority）。Harmony 的 __args 改写会传给原方法，
            // SetCards 见 sortingPriority[0]==Ascending 即跳过内部排序，从而保留我们的自定义顺序。
            var sortingPriority = __args[2];
            if (sortingPriority != null)
            {
                var listType = sortingPriority.GetType();
                var addMethod = listType.GetMethod("Add")!;

                // SortingOrders.Ascending — 通过名称解析，避免硬编码底层整数值
                var enumType = listType.GetGenericArguments()[0];
                var ascendingValue = Enum.Parse(enumType, "Ascending");

                var newPriority = (System.Collections.IList)Activator.CreateInstance(listType)!;
                newPriority.Add(ascendingValue);
                __args[2] = newPriority;
            }
        }

        /// <summary>
        /// 比较两张塔罗牌，按 正位0-22 → 逆位0-22 的顺序排列
        /// </summary>
        private static int CompareTarotCards(CardModel? x, CardModel? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int keyX = GetSortKey(x);
            int keyY = GetSortKey(y);
            return keyX.CompareTo(keyY);
        }

        /// <summary>
        /// 获取卡牌的排序键值。
        /// 正位牌返回 0-21，逆位牌返回 100-121，
        /// 从而保证正位全部排在逆位之前。
        /// </summary>
        private static int GetSortKey(CardModel card)
        {
            string name = card.GetType().Name;
            if (!name.StartsWith("Tar"))
                return int.MaxValue;

            // 去掉 "Tar" 前缀
            ReadOnlySpan<char> span = name.AsSpan(3);

            // 判断正逆位："Upright"(7字符) 或 "Reversed"(8字符)
            bool isReversed = span.EndsWith("Reversed".AsSpan());
            int nameLength = isReversed ? span.Length - 8 : span.Length - 7;
            string tarotName = span[..nameLength].ToString();

            int index = TarotIndex.GetValueOrDefault(tarotName, int.MaxValue);
            // 正位: 0-21, 逆位: 100-121
            return isReversed ? index + 100 : index;
        }
    }
}
