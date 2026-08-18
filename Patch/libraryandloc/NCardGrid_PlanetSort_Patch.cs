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
    /// 当 PlanetPool 过滤器激活时，对星球牌进行自定义排序：
    /// 按太阳系顺序：Mercury → Venus → Earth → Mars → Jupiter → Saturn
    /// → Uranus → Neptune → Pluto → X → Ceres → Eris。
    ///
    /// 使用 Harmony __args 避免对内部枚举 SortingOrders 的编译时依赖。
    /// </summary>
    [HarmonyPatch(typeof(NCardGrid), nameof(NCardGrid.SetCards))]
    public static class NCardGrid_PlanetSort_Patch
    {
        /// <summary>
        /// 星球牌的名称（不含"Planet"前缀）与序号映射
        /// </summary>
        private static readonly Dictionary<string, int> PlanetIndex = new()
        {
            { "Mercury", 0 },
            { "Venus", 1 },
            { "Earth", 2 },
            { "Mars", 3 },
            { "Jupiter", 4 },
            { "Saturn", 5 },
            { "Uranus", 6 },
            { "Neptune", 7 },
            { "Pluto", 8 },
            { "X", 9 },
            { "Ceres", 10 },
            { "Eris", 11 },
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

            // 检测是否所有显示的卡牌都属于 PlanetPool
            bool allPlanets = true;
            foreach (var card in cardsToDisplay)
            {
                if (card.Pool is not PlanetPool)
                {
                    allPlanets = false;
                    break;
                }
            }
            if (!allPlanets)
                return;

            // 复制一份再排序，避免就地修改调用方持有的列表（如 NDeckViewScreen._cards）
            var sortedList = new List<CardModel>(cardsToDisplay);
            sortedList.Sort(ComparePlanetCards);
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
        /// 比较两张星球牌，按自定义顺序排列
        /// </summary>
        private static int ComparePlanetCards(CardModel? x, CardModel? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int keyX = GetSortKey(x);
            int keyY = GetSortKey(y);
            return keyX.CompareTo(keyY);
        }

        /// <summary>
        /// 获取星球牌的排序键值，按 PlanetIndex 映射的顺序返回 0-11
        /// </summary>
        private static int GetSortKey(CardModel card)
        {
            string name = card.GetType().Name;
            if (!name.StartsWith("Planet"))
                return int.MaxValue;

            // 去掉 "Planet" 前缀
            string planetName = name[6..];
            return PlanetIndex.GetValueOrDefault(planetName, int.MaxValue);
        }
    }
}
