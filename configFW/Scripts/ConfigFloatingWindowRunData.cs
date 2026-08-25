#nullable enable

using System;
using System.Text.Json.Nodes;
using PengoTarot.Data.Divination;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// 本局游戏的配置（custom 难度的运行时快照）。
    /// 与全局 JSON（ConfigFloatingWindowConfig，仅作选人界面默认值）分离：
    /// 开始新局时从默认值快照、或从存档恢复、或接收主机分发，此后本局逻辑一律读取本类。
    /// 保存/加载：序列化为 JSON 注入 SerializableRun 存档（字段名 <see cref="SaveFieldName"/>）。
    /// </summary>
    public static class ConfigFloatingWindowRunData
    {
        /// <summary>注入到 SerializableRun JSON 的顶层字段名。</summary>
        public const string SaveFieldName = "_pengotarot_cfw";

        /// <summary>本局配置数据格式版本（预留校验；多人消息与存档 JSON 的 "v" 字段共用）。</summary>
        public const int CurrentVersion = 1;

        private static bool _tarotEnabled = true;
        private static bool _planetEnabled = true;
        private static int _tarotPriceMin = 175;
        private static int _tarotPriceMax = 200;
        /// <summary>塔罗包整体价格偏移（本局动态累计：塔罗包默认购买后 +50，女祭司购买后 -50 抵消）。</summary>
        private static int _tarotPriceOffset;
        /// <summary>命运之轮占卜累计完成的战斗数。</summary>
        private static int _wheelOfFortuneCombatCount;
        private static readonly bool[] _flags = CreateDefaultFlags();

        static ConfigFloatingWindowRunData()
        {
            // 启动时 run 从全局 JSON 初始化（用户偏好），与静态默认对齐；
            // 之后每次开局（SetUpNew/Multiplayer）按 json 重写、读档按 save 重写。
            SnapshotFromDefaults();
        }

        /// <summary>
        /// 静态默认与 Config 默认对齐（愚者~教皇 6 项开启）。
        /// 防止「未走快照」的路径（首次开局/继续游戏/快速开始等）下 GetFlag(0) 误判为关，
        /// 导致商店塔罗包消失。
        /// </summary>
        private static bool[] CreateDefaultFlags()
        {
            var flags = new bool[ConfigFloatingWindowConfig.DifficultyFlagCount];
            for (int i = 0; i < ConfigFloatingWindowConfig.DefaultEnabledFlagCount && i < flags.Length; i++)
                flags[i] = true;
            return flags;
        }

        // ── 读取接口（游戏逻辑用） ──────────────────────────────
        public static bool TarotEnabled => _tarotEnabled;
        public static bool PlanetEnabled => _planetEnabled;
        public static int TarotPriceMin => _tarotPriceMin;
        public static int TarotPriceMax => _tarotPriceMax;
        public static int TarotPriceOffset => _tarotPriceOffset;
        public static int WheelOfFortuneCombatCount => _wheelOfFortuneCombatCount;

        public static bool GetTarFlag(int index)
            // 塔罗总开关未启用时，右侧全部难度（含愚者）不生效
            => TarotEnabled && index >= 0 && index < _flags.Length && _flags[index];

        // ── 快照 / 重置 ─────────────────────────────────────────
        /// <summary>开始新局：从全局 JSON 默认值快照。</summary>
        public static void SnapshotFromDefaults()
        {
            _tarotEnabled = ConfigFloatingWindowConfig.TarotEnabled;
            _planetEnabled = ConfigFloatingWindowConfig.PlanetEnabled;
            _tarotPriceMin = Math.Max(0, ConfigFloatingWindowConfig.TarotBasePriceMin);
            _tarotPriceMax = Math.Max(0, ConfigFloatingWindowConfig.TarotBasePriceMax);
            // 新局从基础价开始，避免上一局购买偏移（默认+50 / 女祭司-50）残留污染
            _tarotPriceOffset = 0;
            _wheelOfFortuneCombatCount = 0;
            for (int i = 0; i < _flags.Length; i++)
                _flags[i] = ConfigFloatingWindowConfig.GetDifficultyFlag(i);
            // 占卜标记：新局起点清空
            TarotMarkerSystem.Reset();
        }

        /// <summary>退出/重置时恢复内置默认（与 Config 默认一致）。</summary>
        public static void Reset()
        {
            _tarotEnabled = true;
            _planetEnabled = true;  // 星球牌默认开（与 Config 默认/静态默认一致）
            _tarotPriceMin = 175;
            _tarotPriceMax = 200;
            _tarotPriceOffset = 0;
            _wheelOfFortuneCombatCount = 0;
            Array.Clear(_flags);
            // 愚者~教皇 6 项默认开启
            for (int i = 0; i < ConfigFloatingWindowConfig.DefaultEnabledFlagCount && i < _flags.Length; i++)
                _flags[i] = true;
            // 占卜标记：重置时清空
            TarotMarkerSystem.Reset();
        }

        /// <summary>直接应用一组值（多人消息分发用）。</summary>
        public static void Apply(bool tarot, bool planet, int priceMin, int priceMax, bool[]? flags)
        {
            _tarotEnabled = tarot;
            _planetEnabled = planet;
            _tarotPriceMin = Math.Max(0, priceMin);
            _tarotPriceMax = Math.Max(0, priceMax);
            // 配置分发发生在选人界面/新局阶段，购买偏移归零，避免客机跨局残留
            _tarotPriceOffset = 0;
            _wheelOfFortuneCombatCount = 0;
            if (flags != null)
            {
                for (int i = 0; i < _flags.Length; i++)
                    _flags[i] = i < flags.Length && flags[i];
            }
            // 客机按主机配置重写 run 后，同步固定本局存档配置快照（客户端不写盘，仅保持一致）
            SetSaveConfigFromRunData();
        }

        /// <summary>调整塔罗包整体价格偏移（塔罗包默认购买后 +50，女祭司购买后 -50 抵消）。</summary>
        public static void AdjustTarotPrice(int delta)
        {
            _tarotPriceOffset += delta;
        }

        /// <summary>记录命运之轮的一场战斗胜利，并返回新的累计数。</summary>
        public static int RecordWheelOfFortuneCombat()
            => ++_wheelOfFortuneCombatCount;

        /// <summary>应用主机同步的命运之轮战斗计数。</summary>
        public static void SetWheelOfFortuneCombatCount(int count)
            => _wheelOfFortuneCombatCount = Math.Max(0, count);

        // ── 选人界面编辑（同时写 JSON 默认值 + 广播） ───────────
        public static void SetTarotEnabled(bool value) => _tarotEnabled = value;
        public static void SetPlanetEnabled(bool value) => _planetEnabled = value;

        public static void SetDifficultyFlag(int index, bool value)
        {
            if (index >= 0 && index < _flags.Length)
                _flags[index] = value;
        }

        // ── 序列化（存档注入） ──────────────────────────────────
        /// <summary>
        /// 本局存档配置快照（不可变：tarot/planet/flags/pmin/pmax）。
        /// 开局（SetUpNew/Multiplayer）时固定；读档时以存档为准重设；
        /// 配置界面编辑只改 json+run，不碰本快照 → 外部编辑不会改写已有存档。
        /// </summary>
        private static JsonObject? _saveConfig;

        /// <summary>本局存档配置快照（null 表示尚无本局存档，写盘注入会跳过）。</summary>
        public static JsonObject? SaveConfig => _saveConfig;

        /// <summary>以当前本局配置（tarot/planet/flags/pmin/pmax）作为本局存档配置快照。</summary>
        public static void SetSaveConfigFromRunData() => _saveConfig = BuildConfigSnapshot();

        private static JsonObject BuildConfigSnapshot()
        {
            var flags = new JsonArray();
            foreach (var f in _flags) flags.Add(f);
            return new JsonObject
            {
                ["tarot"] = _tarotEnabled,
                ["planet"] = _planetEnabled,
                ["pmin"] = _tarotPriceMin,
                ["pmax"] = _tarotPriceMax,
                ["flags"] = flags,
            };
        }

        /// <summary>
        /// 构建写入 run 存档的 JSON（分两部分）：
        /// - cfg：本局存档配置快照（不可变，开局固定、读档重设）；
        /// - run：局内可变量（poff 价格偏移、markers 占卜标记，随局内变化持久化）。
        /// </summary>
        public static JsonObject ToJson()
        {
            JsonObject cfg = _saveConfig ?? BuildConfigSnapshot();
            return new JsonObject
            {
                ["v"] = CurrentVersion,
                ["cfg"] = cfg.DeepClone(),
                ["run"] = new JsonObject
                {
                    ["poff"] = _tarotPriceOffset,
                    ["wheelCombats"] = _wheelOfFortuneCombatCount,
                    ["markers"] = TarotMarkerSystem.ToJson(),
                },
            };
        }

        /// <summary>
        /// 从 run 存档恢复本局配置。兼容旧格式（扁平字段 tarot/planet/pmin/pmax/poff/flags/markers）。
        /// 注意：本局存档配置快照（SaveConfig）由调用方（ExtractFromSave / 开局）设置，本方法不直接改。
        /// </summary>
        public static void FromJson(JsonObject? obj)
        {
            if (obj == null) return;
            // 新格式：cfg（不可变配置）+ run（可变量）分开
            if (obj["cfg"] is JsonObject cfg)
            {
                // 逐字段容错：单字段类型损坏只丢该字段保留默认值，不中断其余字段读取
                _tarotEnabled = TryGet(cfg["tarot"], true);
                _planetEnabled = TryGet(cfg["planet"], true);
                _tarotPriceMin = Math.Max(0, TryGet(cfg["pmin"], 175));
                _tarotPriceMax = Math.Max(0, TryGet(cfg["pmax"], 200));
                if (cfg["flags"] is JsonArray arr)
                {
                    for (int i = 0; i < _flags.Length; i++)
                        _flags[i] = i < arr.Count && TryGet(arr[i], false);
                }
                if (obj["run"] is JsonObject run)
                {
                    _tarotPriceOffset = TryGet(run["poff"], 0);
                    _wheelOfFortuneCombatCount = Math.Max(0, TryGet(run["wheelCombats"], 0));
                    TarotMarkerSystem.FromJson(run["markers"] as JsonObject);
                }
                return;
            }
            // 旧格式兼容：扁平字段
            _tarotEnabled = TryGet(obj["tarot"], true);
            _planetEnabled = TryGet(obj["planet"], true);
            _tarotPriceMin = Math.Max(0, TryGet(obj["pmin"], 175));
            _tarotPriceMax = Math.Max(0, TryGet(obj["pmax"], 200));
            _tarotPriceOffset = TryGet(obj["poff"], 0);
            _wheelOfFortuneCombatCount = Math.Max(0, TryGet(obj["wheelCombats"], 0));
            if (obj["flags"] is JsonArray legacyFlags)
            {
                for (int i = 0; i < _flags.Length; i++)
                    _flags[i] = i < legacyFlags.Count && TryGet(legacyFlags[i], false);
            }
            // 占卜标记状态（随存档恢复）
            TarotMarkerSystem.FromJson(obj["markers"] as JsonObject);
        }

        /// <summary>从 JsonNode 安全读取值（字段缺失/类型不匹配时返回默认值）。</summary>
        private static T TryGet<T>(JsonNode? node, T defaultValue) where T : struct
        {
            try { return node?.GetValue<T>() ?? defaultValue; }
            catch { return defaultValue; }
        }
    }
}
