// 独立附魔特效配置：与卡牌配置(BalatroEffectSettings.json)完全分离，互不影响版本。
// 键 = 附魔 id（如 "Sharp"），值为与卡牌相同的 CardEffectEntry（mode/intensity/parts/editmode）。

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace PengoTarot.BalatroEffect
{
    /// <summary>
    /// 附魔特效配置：独立 JSON（BalatroEffectEnchantments.json），键 = 附魔 id。
    /// 用户确认：不升级现有卡牌配置文件，附魔配置单独存放。
    /// </summary>
    public static class EnchantmentConfig
    {
        private const int CurrentConfigVersion = 2;
        private static readonly string FolderPath = Path.Combine(OS.GetUserDataDir(), "mod_configs", "PengoTarot");
        private static readonly string FilePath = Path.Combine(FolderPath, "BalatroEffectEnchantments.json");
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        private class ConfigData
        {
            public int Version { get; set; } = CurrentConfigVersion;
            public Dictionary<string, Config.CardEffectEntry> Enchantments { get; set; } = [];
        }

        private static ConfigData _data = new();
        private static Dictionary<string, Config.CardEffectEntry> _enchantments => _data.Enchantments;

        /// <summary>附魔配置结构变化（部件/整卡/编辑模式）时触发，用于全局重应用特效。</summary>
        public static event Action? Changed;

        // ── 加载与保存 ─────────────────────────────────────────────
        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    _data = new ConfigData();
                    return;
                }
                string json = File.ReadAllText(FilePath);
                var data = JsonSerializer.Deserialize<ConfigData>(json);
                if (data == null)
                {
                    _data = new ConfigData();
                    return;
                }
                if (data.Version == 1)
                {
                    // v1 → v2：效果 mode 重排（负片2→5、闪箔偏5→2、aniso 6→7）
                    if (data.Enchantments != null)
                        foreach (var e in data.Enchantments.Values)
                            if (e != null) RemapEntry(e);
                    data.Version = CurrentConfigVersion;
                }
                else if (data.Version != CurrentConfigVersion)
                {
                    // 版本不符视为损坏 → 重置（不影响卡牌配置）
                    _data = new ConfigData();
                    return;
                }
                _data = data;
                PruneInvalidEntries();
            }
            catch
            {
                _data = new ConfigData();
            }
        }

        private static void RemapEntry(Config.CardEffectEntry e)
        {
            e.Mode = Config.RemapModeV7ToV8(e.Mode);
            e.FullCardEffect = Config.RemapModeV7ToV8(e.FullCardEffect);
            if (e.Parts != null)
            {
                var newParts = new Dictionary<string, int>();
                foreach (var kv in e.Parts)
                    newParts[kv.Key] = Config.RemapModeV7ToV8(kv.Value);
                e.Parts = newParts;
            }
        }

        public static void Save()
        {
            try
            {
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_data, Options));
            }
            catch (Exception e)
            {
                GD.PrintErr($"[BalatroEffect] EnchantmentConfig Save failed: {e.Message}");
            }
        }

        // ── 查询（编辑界面用，不受性能安全阀影响） ─────────────────
        public static Config.CardEffectEntry? GetEntry(string enchantmentId)
        {
            if (string.IsNullOrEmpty(enchantmentId)) return null;
            return _enchantments.TryGetValue(enchantmentId, out var entry) ? entry : null;
        }

        public static bool TryGetEntry(string enchantmentId, out Config.CardEffectEntry? entry)
        {
            entry = GetEntry(enchantmentId);
            return entry != null;
        }

        /// <summary>附魔是否具有实际效果（FullCard > 0 或任一部件 > 0）。</summary>
        public static bool HasEffect(string enchantmentId)
        {
            var entry = GetEntry(enchantmentId);
            return entry != null && EntryHasEffect(entry);
        }

        public static bool HasAnyEffect()
        {
            foreach (var entry in _enchantments.Values)
                if (EntryHasEffect(entry)) return true;
            return false;
        }

        public static IEnumerable<string> AllEnchantmentIds => _enchantments.Keys;

        public static int GetEffect(string enchantmentId, string partName)
        {
            var entry = GetEntry(enchantmentId);
            if (entry == null) return 0;
            if (partName == "FullCard") return entry.FullCardEffect;
            if (entry.Parts.TryGetValue(partName, out int val)) return val;
            return 0;
        }

        public static void SetEffect(string enchantmentId, string partName, int effectIndex)
        {
            if (string.IsNullOrEmpty(enchantmentId)) return;
            if (!_enchantments.TryGetValue(enchantmentId, out var entry))
                _enchantments[enchantmentId] = entry = new Config.CardEffectEntry();
            if (partName == "FullCard")
                entry.FullCardEffect = effectIndex;
            else
            {
                if (effectIndex == 0) entry.Parts.Remove(partName);
                else entry.Parts[partName] = effectIndex;
            }
            Config.ResetPerformanceThrottle();
            Changed?.Invoke();
            Save();
        }

        public static void ClearEffect(string enchantmentId, string? partName = null)
        {
            if (partName == null)
            {
                _enchantments.Remove(enchantmentId);
            }
            else if (_enchantments.TryGetValue(enchantmentId, out var entry))
            {
                if (partName == "FullCard") entry.FullCardEffect = 0;
                else entry.Parts.Remove(partName);
            }
            Config.ResetPerformanceThrottle();
            Changed?.Invoke();
            Save();
        }

        public static double GetIntensity(string enchantmentId, double defaultValue = 1.0)
        {
            var entry = GetEntry(enchantmentId);
            return entry != null ? entry.Intensity : defaultValue;
        }

        public static void SetIntensity(string enchantmentId, double value)
        {
            if (string.IsNullOrEmpty(enchantmentId)) return;
            if (!_enchantments.TryGetValue(enchantmentId, out var entry)) return;
            entry.Intensity = Math.Clamp(value, 0.0, 1.0);
            Config.ResetPerformanceThrottle();
            Save();
        }

        public static string GetEditMode(string enchantmentId)
        {
            var entry = GetEntry(enchantmentId);
            return entry != null ? entry.EditMode : Config.ModeNormal;
        }

        public static void SetEditMode(string enchantmentId, string newMode)
        {
            if (string.IsNullOrEmpty(enchantmentId)) return;
            if (newMode is not (Config.ModeNormal or Config.ModeSeparately or Config.ModeFully)) return;
            if (!_enchantments.TryGetValue(enchantmentId, out var entry))
                _enchantments[enchantmentId] = entry = new Config.CardEffectEntry();
            entry.EditMode = newMode;
            Config.ResetPerformanceThrottle();
            Changed?.Invoke();
            Save();
        }

        public static int GetCardEffectMode(string enchantmentId)
        {
            var entry = GetEntry(enchantmentId);
            return entry != null ? entry.Mode : 0;
        }

        public static void SetCardEffectMode(string enchantmentId, int mode)
        {
            if (string.IsNullOrEmpty(enchantmentId)) return;
            if (!_enchantments.TryGetValue(enchantmentId, out var entry))
                _enchantments[enchantmentId] = entry = new Config.CardEffectEntry();
            entry.Mode = mode;
            Config.ResetPerformanceThrottle();
            Changed?.Invoke();
            Save();
        }

        /// <summary>推导附魔实际生效的效果 mode：FullCard &gt; Parts 首个非0 &gt; Mode。</summary>
        public static int GetCardEffectIndex(string enchantmentId)
        {
            var entry = GetEntry(enchantmentId);
            return entry != null ? EffectiveMode(entry) : 0;
        }

        private static int EffectiveMode(Config.CardEffectEntry entry)
        {
            if (entry.FullCardEffect > 0) return entry.FullCardEffect;
            if (entry.Parts != null)
                foreach (var v in entry.Parts.Values)
                    if (v > 0) return v;
            return entry.Mode;
        }

        private static bool EntryHasEffect(Config.CardEffectEntry entry)
        {
            if (entry.FullCardEffect > 0) return true;
            if (entry.Parts != null)
                foreach (var v in entry.Parts.Values)
                    if (v > 0) return true;
            return false;
        }

        /// <summary>移除无实际效果的附魔条目（Parts 空且 FullCardEffect 为 0）。</summary>
        private static void PruneInvalidEntries()
        {
            var invalid = new List<string>();
            foreach (var (enchId, entry) in _enchantments)
                if (entry.FullCardEffect == 0 && (entry.Parts == null || entry.Parts.Count == 0))
                    invalid.Add(enchId);
            foreach (var id in invalid) _enchantments.Remove(id);
        }

        // ── 复制 / 粘贴附魔效果（复用 CardEffectEntry JSON 结构） ──
        public static string ExportCardPreset(string enchantmentId)
        {
            var entry = GetEntry(enchantmentId);
            var dto = new
            {
                EnchantmentId = enchantmentId,
                EditMode = entry?.EditMode ?? Config.ModeNormal,
                Mode = entry?.Mode ?? 0,
                Intensity = entry?.Intensity ?? 1.0,
                Parts = entry?.Parts,
                FullCardEffect = entry?.FullCardEffect ?? 0
            };
            return JsonSerializer.Serialize(dto, Options);
        }

        public static bool ImportCardPreset(string enchantmentId, string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                string editMode = root.TryGetProperty("EditMode", out var em)
                    ? em.GetString() ?? Config.ModeNormal : Config.ModeNormal;
                int mode = root.TryGetProperty("Mode", out var m) ? m.GetInt32() : 0;
                var parts = new Dictionary<string, int>();
                if (root.TryGetProperty("Parts", out var partsProp))
                    foreach (var item in partsProp.EnumerateObject())
                        parts[item.Name] = item.Value.GetInt32();
                int full = root.TryGetProperty("FullCardEffect", out var f) ? f.GetInt32() : 0;
                var entry = new Config.CardEffectEntry { EditMode = editMode, Mode = mode, Parts = parts, FullCardEffect = full };
                if (root.TryGetProperty("Intensity", out var inten))
                    entry.Intensity = Math.Clamp(inten.GetDouble(), 0.0, 1.0);
                _enchantments[enchantmentId] = entry;
                if (entry.FullCardEffect == 0 && (entry.Parts == null || entry.Parts.Count == 0))
                    _enchantments.Remove(enchantmentId);
                Config.ResetPerformanceThrottle();
                Changed?.Invoke();
                Save();
                return true;
            }
            catch { return false; }
        }

        // ── 作者预设（全局预设，按 id 合并） ───────────────────────
        /// <summary>
        /// 应用附魔作者预设：按 id 合并到当前配置（预设中的 id 覆盖对应条目，其余保留）。
        /// 触发 Changed 以全局重应用特效，并保存。
        /// </summary>
        public static bool ApplyAuthorPreset(string json)
        {
            try
            {
                var data = JsonSerializer.Deserialize<ConfigData>(json);
                if (data == null || data.Enchantments == null) return false;
                foreach (var kv in data.Enchantments)
                    if (kv.Value != null) _enchantments[kv.Key] = kv.Value;
                PruneInvalidEntries();
                Config.ResetPerformanceThrottle();
                Changed?.Invoke();
                Save();
                return true;
            }
            catch { return false; }
        }

        /// <summary>从文件加载附魔作者预设（不存在则跳过）。</summary>
        public static bool LoadAuthorPreset(string path)
        {
            if (!Godot.FileAccess.FileExists(path)) return false;
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            return ApplyAuthorPreset(file.GetAsText());
        }
    }
}
