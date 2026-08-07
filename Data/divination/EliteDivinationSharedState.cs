#nullable enable

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;

namespace PengoTarot.Data.Divination
{
    /// <summary>
    /// 精英标记占卜（战车/力量）的<b>房间级共享计数</b>。
    /// 同一房间的所有敌人共享「每个玩家只触发一次」的计数：
    /// 某个敌人让玩家 A 触发过后，其他敌人对玩家 A 的未格挡伤害不再触发（易伤/虚弱）。
    /// 按 <see cref="ICombatState"/> 索引存放；战斗结束 combatState 被 GC 后条目自动回收（无泄漏）。
    /// 读档后 combatState 是新反序列化实例 → 计数重建（战斗内读档重新计数，可接受）。
    /// </summary>
    internal static class EliteDivinationSharedState
    {
        private static readonly ConditionalWeakTable<ICombatState, HashSet<ulong>> ChariotTriggered = new();
        private static readonly ConditionalWeakTable<ICombatState, HashSet<ulong>> StrengthTriggered = new();

        /// <summary>战车：已触发过「易伤」的玩家 NetId 集合（房间共享）。</summary>
        public static HashSet<ulong> Chariot(ICombatState combat)
            => ChariotTriggered.GetValue(combat, static _ => new HashSet<ulong>());

        /// <summary>力量：已触发过「虚弱」的玩家 NetId 集合（房间共享）。</summary>
        public static HashSet<ulong> Strength(ICombatState combat)
            => StrengthTriggered.GetValue(combat, static _ => new HashSet<ulong>());
    }
}
