#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// configfloatingwindow 的配置模型与 JSON 持久化。
    /// 数据：左侧 2 个大按钮（塔罗/星球）+ 右侧 3 列 7/8/7 共 22 个难度开关。
    /// JSON 只作为「每次打开选人界面时的默认值」，任何改动即时写盘（改动即写）。
    /// 本局真相在 <see cref="ConfigFloatingWindowRunData"/>：新局快照、存档注入 _pengotarot_cfw、
    /// 多人由 <see cref="ConfigFloatingWindowDataMessage"/> 分发——本类是默认值来源，不直接参与局内逻辑。
    /// </summary>
    public static class ConfigFloatingWindowConfig
    {
        /// <summary>配置版本（预留：暂未校验/迁移；改字段结构前需实现迁移逻辑）。</summary>
        private const int CurrentVersion = 1;

        /// <summary>右侧难度按钮总数（3 列：7 + 8 + 7）。</summary>
        public const int DifficultyFlagCount = 22;
        /// <summary>默认开启的难度按钮数（愚者/魔术师/女祭司/皇后/皇帝/教皇）。</summary>
        public const int DefaultEnabledFlagCount = 6;
        /// <summary>右侧三列每列按钮数（7/8/7）。</summary>
        public static readonly int[] ColumnSizes = { 7, 8, 7 };

        private static readonly string FolderPath = Path.Combine(OS.GetUserDataDir(), "mod_configs", "PengoTarot");
        private static readonly string FilePath = Path.Combine(FolderPath, "ConfigFloatingWindow.json");
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        private static ConfigData _data = new();

        static ConfigFloatingWindowConfig()
        {
            Load();
        }

        // ── 数据模型 ─────────────────────────────────────────────
        private class ConfigData
        {
            public int Version { get; set; } = CurrentVersion;
            /// <summary>左侧大按钮1：启用塔罗牌（总开关，关闭时右侧难度全部不生效，默认开启）。</summary>
            public bool TarotEnabled { get; set; } = true;
            /// <summary>左侧大按钮2：启用星球牌（功能尚未实现，仅配置存储与多人同步，默认开启）。</summary>
            public bool PlanetEnabled { get; set; } = true;
            /// <summary>塔罗牌附魔包基础价格范围（基础价；教皇 GetTarFlag(5) -100；购买后默认 +50，女祭司 GetTarFlag(2) -50 抵消）。</summary>
            public int TarotBasePriceMin { get; set; } = 175;
            public int TarotBasePriceMax { get; set; } = 200;
            /// <summary>右侧难度开关（长度固定 22）。</summary>
            public List<bool> DifficultyFlags { get; set; } = NewFlags();
            /// <summary>仅在游戏设置界面显示配置入口（不在选人界面/游戏过程中出现）。默认关闭。</summary>
            public bool ShowInSettingsOnly { get; set; } = false;
        }

        private static List<bool> NewFlags()
        {
            var list = new List<bool>(DifficultyFlagCount);
            for (int i = 0; i < DifficultyFlagCount; i++)
                list.Add(i < DefaultEnabledFlagCount);  // 愚者~教皇 6 项默认开启
            return list;
        }

        // ── 读取接口 ─────────────────────────────────────────────
        public static bool TarotEnabled => _data.TarotEnabled;
        public static bool PlanetEnabled => _data.PlanetEnabled;
        public static int TarotBasePriceMin => _data.TarotBasePriceMin;
        public static int TarotBasePriceMax => _data.TarotBasePriceMax;
        /// <summary>仅在游戏设置界面显示配置入口（不在选人界面/游戏过程中出现）。</summary>
        public static bool ShowInSettingsOnly => _data.ShowInSettingsOnly;

        /// <summary>
        /// 读取全局 JSON 配置的难度开关原始值（无塔罗总开关门控）。
        /// 门控在 <see cref="ConfigFloatingWindowRunData.GetTarFlag"/>（TarotEnabled 关时全部不生效）；
        /// 本方法仅作快照/默认值来源，不参与局内判断。
        /// </summary>
        public static bool GetDifficultyFlag(int index)
        {
            if (index < 0 || index >= _data.DifficultyFlags.Count) return false;
            return _data.DifficultyFlags[index];
        }

        // ── 写入接口（改动即写盘） ───────────────────────────────
        public static void SetTarotEnabled(bool value)
        {
            if (_data.TarotEnabled == value) return;
            _data.TarotEnabled = value;
            Save();
        }

        public static void SetPlanetEnabled(bool value)
        {
            if (_data.PlanetEnabled == value) return;
            _data.PlanetEnabled = value;
            Save();
        }

        public static void SetDifficultyFlag(int index, bool value)
        {
            if (index < 0 || index >= _data.DifficultyFlags.Count) return;
            if (_data.DifficultyFlags[index] == value) return;
            _data.DifficultyFlags[index] = value;
            Save();
        }

        public static void SetShowInSettingsOnly(bool value)
        {
            if (_data.ShowInSettingsOnly == value) return;
            _data.ShowInSettingsOnly = value;
            Save();
        }

        // ── 序列化 ───────────────────────────────────────────────
        private static void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                using var file = File.OpenText(FilePath);
                string json = file.ReadToEnd();
                if (string.IsNullOrWhiteSpace(json)) return;
                if (JsonNode.Parse(json) is not JsonObject root) return;

                // 逐字段容错读取：单字段类型损坏只丢该字段，不回退全部配置
                var data = new ConfigData();
                TryRead(root, "Version", (int v) => data.Version = v);
                TryRead(root, "TarotEnabled", (bool v) => data.TarotEnabled = v);
                TryRead(root, "PlanetEnabled", (bool v) => data.PlanetEnabled = v);
                TryRead(root, "TarotBasePriceMin", (int v) => data.TarotBasePriceMin = v);
                TryRead(root, "TarotBasePriceMax", (int v) => data.TarotBasePriceMax = v);
                TryRead(root, "ShowInSettingsOnly", (bool v) => data.ShowInSettingsOnly = v);

                if (root["DifficultyFlags"] is JsonArray arr)
                {
                    data.DifficultyFlags = new List<bool>(DifficultyFlagCount);
                    for (int i = 0; i < DifficultyFlagCount; i++)
                    {
                        if (i < arr.Count && arr[i] is JsonValue jv)
                        {
                            try { data.DifficultyFlags.Add(jv.GetValue<bool>()); }
                            catch { data.DifficultyFlags.Add(i < DefaultEnabledFlagCount); }
                        }
                        else
                        {
                            data.DifficultyFlags.Add(i < DefaultEnabledFlagCount);
                        }
                    }
                }
                else
                {
                    data.DifficultyFlags = NewFlags();
                }

                // 兼容旧数据：补足/截断长度
                while (data.DifficultyFlags.Count < DifficultyFlagCount)
                    data.DifficultyFlags.Add(false);
                if (data.DifficultyFlags.Count > DifficultyFlagCount)
                    data.DifficultyFlags.RemoveRange(DifficultyFlagCount, data.DifficultyFlags.Count - DifficultyFlagCount);

                _data = data;
            }
            catch (Exception ex)
            {
                GD.PushWarning($"[ConfigFloatingWindow] 读取配置失败：{ex.Message}");
            }
        }

        /// <summary>从 JsonObject 安全读取单个值（字段损坏/缺失时保留默认值）。</summary>
        private static void TryRead<T>(JsonObject obj, string key, Action<T> setter)
        {
            try
            {
                if (obj[key] is JsonValue v)
                    setter(v.GetValue<T>());
            }
            catch { /* 单字段损坏：保留 ConfigData 构造时的默认值 */ }
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(FolderPath);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_data, Options));
            }
            catch (Exception ex)
            {
                GD.PushWarning($"[ConfigFloatingWindow] 写入配置失败：{ex.Message}");
            }
        }
    }
}
