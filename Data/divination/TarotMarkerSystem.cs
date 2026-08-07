#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Data.Divination
{
    /// <summary>
    /// 占卜标记系统（通用基建，各占卜的具体战斗效果后续实现）。
    /// 在地图生成后为「标记类」占卜挑选目标房间并记录坐标；
    /// 节点 UI（NNormalMapPoint patch）据此显示小图标；后续战斗效果可用 <see cref="GetMarkedFlagsAt"/> 查询本场战斗受哪些占卜影响。
    /// 状态持久化：序列化并入 <see cref="ConfigFloatingWindowRunData"/> 的存档 JSON（字段 _pengotarot_cfw 下的 markers）。
    /// 标记选择使用确定性伪随机（同幕 + 同占卜 → 各端一致），保证多人/读档可复现。
    /// </summary>
    public static class TarotMarkerSystem
    {
        /// <summary>单个占卜的标记配置：FlagIndex（对应 configFW 难度开关）、目标节点类型、每幕数量、最小层数（row）、是否标记全部。</summary>
        public readonly record struct MarkerConfig(
            int FlagIndex, MapPointType TargetType, int CountPerAct, int MinRow, bool MarkAll = false);

        /// <summary>恋人占卜的难度开关索引（标记放大器：开启时精英类标记改为标记所有精英）。</summary>
        public const int LoversFlagIndex = 6;

        /// <summary>
        /// 标记完成阈值：精英类完成 N 个后发放一次奖励并失效；普通类每完成 N 个发放一次奖励并归零
        /// （本地化文本 N=2）。角标显示与发放逻辑共用，避免两处不一致。
        /// </summary>
        public const int RewardInterval = 2;

        /// <summary>当前启用的「标记类」占卜配置（来自占卜设计，具体数值可后续调整）。</summary>
        public static readonly MarkerConfig[] Configs =
        {
            new(6,  MapPointType.Elite,   0, 0, MarkAll: true),  // 恋人：标记所有精英（放大器，无战斗效果、不发奖励，仅提供地图图标）
            new(7,  MapPointType.Elite,   1, 0),                 // 战车
            new(8,  MapPointType.Elite,   1, 0),                 // 力量
            new(9,  MapPointType.Elite,   1, 0),                 // 隐者
            new(11, MapPointType.Monster, 3, 5),                 // 正义
            new(12, MapPointType.Monster, 3, 5),                 // 倒吊人
            new(13, MapPointType.Monster, 3, 0),                 // 死神
        };

        /// <summary>单个占卜的当前幕标记状态。</summary>
        public sealed class MarkState
        {
            /// <summary>状态所属幕（-1 表示未初始化）。</summary>
            public int ActIndex = -1;
            /// <summary>本幕已标记的节点坐标。</summary>
            public List<MapCoord> Coords = new();
            /// <summary>已完成的标记数（后续战斗效果在打完被标记房间后调用 <see cref="RecordCompletion"/>）。</summary>
            public int CompletedCount;
            /// <summary>已发放的塔罗奖励次数（用于「每完成2个发1次」的防重复计数）。</summary>
            public int RewardsAwarded;
            /// <summary>是否已失效（达成设计目标后由 <see cref="Expire"/> 置位，此后不再标记新幕）。</summary>
            public bool Expired;
        }

        private static readonly Dictionary<int, MarkState> _states = new();

        // ── 查询接口（节点图标 / 后续战斗效果用） ───────────────
        /// <summary>返回某坐标被哪些占卜标记（flagIndex 升序，仅统计启用且未失效的）。战斗效果等「未失效判定」用此接口。</summary>
        public static List<int> GetMarkedFlagsAt(MapCoord coord)
        {
            var result = new List<int>();
            foreach (var cfg in Configs)
            {
                if (cfg.FlagIndex < 0) continue;
                if (!IsFlagEnabled(cfg.FlagIndex)) continue;
                if (!_states.TryGetValue(cfg.FlagIndex, out var st) || st.ActIndex < 0) continue;
                if (st.Coords.Contains(coord)) result.Add(cfg.FlagIndex);
            }
            return result;
        }

        /// <summary>
        /// 返回某坐标被哪些占卜标记（**含已失效的**，仅要求开关开启）。
        /// 供地图图标显示用：失效后图标仍需保留（换逆位图），只是不再触发战斗效果/计数。
        /// </summary>
        public static List<int> GetDisplayedFlagsAt(MapCoord coord)
        {
            var result = new List<int>();
            foreach (var cfg in Configs)
            {
                if (cfg.FlagIndex < 0) continue;
                if (!ConfigFloatingWindowRunData.GetTarFlag(cfg.FlagIndex)) continue;
                if (!_states.TryGetValue(cfg.FlagIndex, out var st) || st.ActIndex < 0) continue;
                if (st.Coords.Contains(coord)) result.Add(cfg.FlagIndex);
            }
            return result;
        }

        /// <summary>某坐标是否被任一占卜标记。</summary>
        public static bool IsMarkedAt(MapCoord coord) => GetMarkedFlagsAt(coord).Count > 0;

        /// <summary>某占卜当前是否启用（难度开关开启 且 未失效）。</summary>
        public static bool IsFlagEnabled(int flagIndex)
            => ConfigFloatingWindowRunData.GetTarFlag(flagIndex) && !IsExpired(flagIndex);

        /// <summary>某占卜是否已失效。</summary>
        public static bool IsExpired(int flagIndex)
            => _states.TryGetValue(flagIndex, out var st) && st.Expired;

        /// <summary>取某占卜当前幕的标记坐标（未启用返回空列表）。</summary>
        public static IReadOnlyList<MapCoord> GetMarkedCoords(int flagIndex)
        {
            if (!IsFlagEnabled(flagIndex)) return Array.Empty<MapCoord>();
            return _states.TryGetValue(flagIndex, out var st) ? st.Coords : Array.Empty<MapCoord>();
        }

        /// <summary>某占卜的完成标记数（未初始化返回 0；角标数字用）。</summary>
        public static int GetCompletedCount(int flagIndex)
            => _states.TryGetValue(flagIndex, out var st) ? st.CompletedCount : 0;

        /// <summary>
        /// 角标显示数字：精英类（完成 <see cref="RewardInterval"/> 个失效）显示累计完成数；
        /// 普通类（每 <see cref="RewardInterval"/> 个发放一次奖励并归零）显示累计完成数 % <see cref="RewardInterval"/>。
        /// 失效后调用方应停止显示角标（此值对失效的占卜无意义）。
        /// </summary>
        public static int GetProgressForDisplay(int flagIndex)
        {
            int completed = GetCompletedCount(flagIndex);
            bool elite = false;
            foreach (var cfg in Configs)
            {
                if (cfg.FlagIndex == flagIndex)
                {
                    elite = cfg.TargetType == MapPointType.Elite;
                    break;
                }
            }
            return elite ? completed : completed % RewardInterval;
        }

        /// <summary>记录一次标记完成（后续战斗效果在打完被标记房间后调用）。</summary>
        public static void RecordCompletion(int flagIndex)
        {
            if (_states.TryGetValue(flagIndex, out var st)) st.CompletedCount++;
        }

        /// <summary>把某占卜置为失效（达成设计目标后调用，此后不再标记新幕）。</summary>
        public static void Expire(int flagIndex)
        {
            if (_states.TryGetValue(flagIndex, out var st)) st.Expired = true;
        }

        // ── 塔罗奖励发放（被标记房间战斗胜利后调用） ─────────────
        /// <summary>
        /// 被标记房间战斗胜利：记录完成并按各占卜规则发放塔罗奖励。
        /// - 精英类（战车/力量/隐者）：完成 2 个标记后失效，并获得一次塔罗奖励；
        /// - 普通类（正义/倒吊人/死神）：每完成 2 个标记，获得一次塔罗奖励（不失效）。
        /// </summary>
        public static void OnMarkedCombatVictory(MapCoord coord, CombatRoom room, IReadOnlyList<Player> players)
        {
            foreach (var cfg in Configs)
            {
                if (cfg.FlagIndex < 0) continue;
                // 恋人：标记放大器，无战斗效果、不计数、不发塔罗奖励
                if (cfg.FlagIndex == LoversFlagIndex) continue;
                if (!_states.TryGetValue(cfg.FlagIndex, out var st)) continue;
                if (st.ActIndex < 0 || !st.Coords.Contains(coord)) continue;
                if (!IsFlagEnabled(cfg.FlagIndex)) continue;

                st.CompletedCount++;
                AwardTarotReward(cfg, st, room, players);
            }
        }

        private static void AwardTarotReward(MarkerConfig cfg, MarkState st, CombatRoom room, IReadOnlyList<Player> players)
        {
            if (cfg.TargetType == MapPointType.Elite)
            {
                // 精英类：完成 2 个 → 发放 1 次并失效（此后不再标记新幕）
                if (st.CompletedCount >= 2 && st.RewardsAwarded == 0)
                {
                    st.RewardsAwarded = 1;
                    st.Expired = true;
                    AddTarotReward(room, players, cfg.FlagIndex);
                }
            }
            else
            {
                // 普通类：每完成 RewardInterval 个 → 发放 1 次（防重复，不失效）
                int due = st.CompletedCount / RewardInterval;
                if (due > st.RewardsAwarded)
                {
                    st.RewardsAwarded = due;
                    AddTarotReward(room, players, cfg.FlagIndex);
                }
            }
        }

        private static void AddTarotReward(CombatRoom room, IReadOnlyList<Player> players, int flagIndex)
        {
            foreach (var player in players)
                room.AddExtraReward(player, new TarotReward(player, flagIndex));
        }

        // ── 应用标记（每幕地图生成后调用，新建/读档都覆盖） ─────
        public static void ApplyMarkers(IRunState runState, ActMap map, int actIndex)
        {
            foreach (var cfg in Configs)
            {
                if (cfg.FlagIndex < 0) continue;
                // 只检查开关是否开启（不管失效）：已失效的占卜也要跨幕继续标记，
                // 供地图显示逆位图标；失效的标记不计数、不发奖励、不触发战斗效果（那些路径走 IsFlagEnabled/GetMarkedFlagsAt）。
                if (!ConfigFloatingWindowRunData.GetTarFlag(cfg.FlagIndex)) continue;

                if (!_states.TryGetValue(cfg.FlagIndex, out var st))
                {
                    st = new MarkState();
                    _states[cfg.FlagIndex] = st;
                }

                // 已是本幕且坐标均仍在地图中（读档恢复 / 同一幕）→ 保留，不重新随机
                if (st.ActIndex == actIndex && st.Coords.All(map.HasPoint))
                    continue;

                st.ActIndex = actIndex;
                st.Coords = PickCoords(map, actIndex, cfg);
            }
        }

        private static List<MapCoord> PickCoords(ActMap map, int actIndex, MarkerConfig cfg)
        {
            var candidates = map.GetAllMapPoints()
                .Where(p => p.PointType == cfg.TargetType && p.coord.row >= cfg.MinRow)
                .Select(p => p.coord)
                .Distinct()
                .OrderBy(c => c.col).ThenBy(c => c.row)
                .ToList();

            // 恋人（放大器）：开启时精英类标记改为标记所有精英（普通类不放大）
            bool loversAmplified = cfg.TargetType == MapPointType.Elite
                && ConfigFloatingWindowRunData.GetTarFlag(LoversFlagIndex);
            int count = cfg.MarkAll || loversAmplified
                ? candidates.Count
                : Math.Min(cfg.CountPerAct, candidates.Count);
            if (count <= 0 || candidates.Count == 0) return new List<MapCoord>();

            // 确定性伪随机：同幕 + 同占卜 → 各端一致（多人/读档可复现）
            var rng = new Random((actIndex + 1) * 1000 + cfg.FlagIndex * 17);
            var pool = new List<MapCoord>(candidates);
            var picked = new List<MapCoord>(count);
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int idx = rng.Next(pool.Count);
                picked.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
            return picked;
        }

        /// <summary>
        /// 防御性补标（房间完成后调用）：若当前幕应有标记（任一开启的标记占卜）却一个都没有
        /// （被其他破坏性读档 mod 清空/坐标失效），则重新应用标记。幂等：正常情况坐标都在时无副作用，
        /// 不影响当前玩家游玩体验；只兜底「读档后标记全部丢失」场景。
        /// </summary>
        public static void TryRemarker(IRunState runState)
        {
            if (runState?.Map == null) return;
            int actIndex = runState.CurrentActIndex;

            // 配置满足要求：至少一个开启的标记占卜（开关开启即算，失效的占卜也要补供逆图显示）
            bool anyEnabled = false;
            foreach (var cfg in Configs)
            {
                if (cfg.FlagIndex >= 0 && ConfigFloatingWindowRunData.GetTarFlag(cfg.FlagIndex))
                {
                    anyEnabled = true;
                    break;
                }
            }
            if (!anyEnabled) return;

            // 当前幕是否已有任何标记（含失效保留的）：有则不补（正常游玩零开销）
            bool anyMarked = false;
            foreach (var st in _states.Values)
            {
                if (st.ActIndex == actIndex && st.Coords.Count > 0)
                {
                    anyMarked = true;
                    break;
                }
            }
            if (anyMarked) return;

            // 全部消失（读档 mod 破坏）→ 重新应用标记（ApplyMarkers 幂等：已有保留、缺失补上）
            ApplyMarkers(runState, runState.Map, actIndex);
        }

        /// <summary>重置所有标记状态（新局起点）。</summary>
        public static void Reset()
        {
            _states.Clear();
        }

        // ── 节点小图标（enchantments 正位附魔小图标，64×64） ────
        /// <summary>flagIndex → 小图标资源路径（仅标记类占卜有图标；命运之轮暂无附魔图标，用卡面兜底）。</summary>
        private static readonly string[] IconPaths =
        {
            "", "", "", "", "", "",                                                  // 0-5 已实现
            "res://images/enchantments/tar_lovers_upright_enchantment.png",          // 6 恋人
            "res://images/enchantments/tar_chariot_upright_enchantment.png",         // 7 战车
            "res://images/enchantments/tar_strength_upright_enchantment.png",        // 8 力量
            "res://images/enchantments/tar_hermit_upright_enchantment.png",          // 9 隐者
            "",                                                                      // 10 命运之轮（暂无附魔图标，暂不显示）
            "res://images/enchantments/tar_justice_upright_enchantment.png",         // 11 正义
            "res://images/enchantments/tar_hanged_man_upright_enchantment.png",      // 12 倒吊人
            "res://images/enchantments/tar_death_upright_enchantment.png",           // 13 死神
            "", "", "", "", "", "", "", "",                                          // 14-21
        };

        /// <summary>某占卜的节点小图标路径（无图标返回空串）。</summary>
        public static string GetMarkerIconPath(int flagIndex)
        {
            if (flagIndex < 0 || flagIndex >= IconPaths.Length)
                return string.Empty;
            return IconPaths[flagIndex];
        }

        /// <summary>某占卜的逆位节点小图标路径（由正位路径推导；无图标返回空串）。失效后地图图标切换为此。</summary>
        public static string GetReversedMarkerIconPath(int flagIndex)
        {
            string p = GetMarkerIconPath(flagIndex);
            return string.IsNullOrEmpty(p) ? string.Empty : p.Replace("_upright_", "_reversed_");
        }

        // ── 序列化（并入 ConfigFloatingWindowRunData 存档） ──────
        public static JsonObject ToJson()
        {
            var obj = new JsonObject();
            foreach (var cfg in Configs)
            {
                if (!_states.TryGetValue(cfg.FlagIndex, out var st)) continue;
                var arr = new JsonArray();
                foreach (var c in st.Coords)
                    arr.Add(new JsonObject { ["c"] = c.col, ["r"] = c.row });
                obj[cfg.FlagIndex.ToString()] = new JsonObject
                {
                    ["act"] = st.ActIndex,
                    ["coords"] = arr,
                    ["done"] = st.CompletedCount,
                    ["awarded"] = st.RewardsAwarded,
                    ["exp"] = st.Expired,
                };
            }
            return obj;
        }

        public static void FromJson(JsonObject? obj)
        {
            _states.Clear();
            if (obj == null) return;
            foreach (var cfg in Configs)
            {
                if (obj[cfg.FlagIndex.ToString()] is not JsonObject node) continue;
                var st = new MarkState
                {
                    ActIndex = node["act"]?.GetValue<int>() ?? -1,
                    CompletedCount = node["done"]?.GetValue<int>() ?? 0,
                    RewardsAwarded = node["awarded"]?.GetValue<int>() ?? 0,
                    Expired = node["exp"]?.GetValue<bool>() ?? false,
                };
                if (node["coords"] is JsonArray arr)
                {
                    foreach (var item in arr)
                    {
                        if (item is JsonObject c && c["c"] is { } cc && c["r"] is { } rr)
                            st.Coords.Add(new MapCoord(cc.GetValue<int>(), rr.GetValue<int>()));
                    }
                }
                _states[cfg.FlagIndex] = st;
            }
        }
    }
}
