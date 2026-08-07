// Based on code from BalatroEffects by Indi (MIT License)

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace PengoTarot.BalatroEffect
{
	public static class Config
	{
		private const int CurrentConfigVersion = 8;
		private static readonly string FolderPath = Path.Combine(OS.GetUserDataDir(), "mod_configs", "PengoTarot");
		private static readonly string FilePath = Path.Combine(FolderPath, "BalatroEffectSettings.json");
		private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

		// ── 性能安全阀 ───────────────────────────────────────────────
		private static bool _performanceThrottled = false;

		/// <summary>
		/// 当性能超过阈值时，由 GetNode 补丁设为 true，将禁用所有特效。
		/// 任何用户配置的修改都会自动将其重置为 false。
		/// </summary>
		public static bool IsPerformanceThrottled
		{
			get => _performanceThrottled;
		}

		/// <summary>
		/// 内部设置性能限制状态。设为 true 时会触发 PerformanceThrottled 事件。
		/// </summary>
		internal static void SetPerformanceThrottled(bool value)
		{
			if (_performanceThrottled == value) return;
			_performanceThrottled = value;
			if (value)
			{
				PerformanceThrottled?.Invoke();
			}
		}

		/// <summary>
		/// 性能限制触发时的事件，由 ShaderController 监听以执行全局清理。
		/// </summary>
		public static event Action? PerformanceThrottled;

		// ── 重置安全阀的辅助方法 ─────────────────────────────────────
		internal static void ResetPerformanceThrottle()
		{
			_performanceThrottled = false;
		}

		// ── 配置数据结构 ─────────────────────────────────────────────
		private class ConfigData
		{
			public int Version { get; set; } = CurrentConfigVersion;
			public Dictionary<string, CardEffectEntry> Cards { get; set; } = [];
			public bool GlobalDynamicEffect { get; set; } = false;
			public bool EnableShaderInNonCombat { get; set; } = false;
			public bool ShownTestWarning { get; set; } = false;
		}

		/// <summary>v5 兼容结构：仅用于读取旧配置文件以执行迁移（v6 已移除 IntensitySettings）。</summary>
		private class ConfigDataV5
		{
			public int Version { get; set; } = 5;
			public Dictionary<string, CardEffectEntry> Cards { get; set; } = [];
			public Dictionary<int, double> IntensitySettings { get; set; } = [];
			public bool GlobalDynamicEffect { get; set; } = false;
			public bool EnableShaderInNonCombat { get; set; } = false;
			public bool ShownTestWarning { get; set; } = false;
		}

		// ── 编辑模式（v7 起三模式互斥） ────────────────────────────
		public const string ModeNormal = "normal";
		public const string ModeSeparately = "separately";
		public const string ModeFully = "fully";

		public class CardEffectEntry
		{
			/// <summary>当前编辑模式：normal | separately | fully。v7 起。</summary>
			public string EditMode { get; set; } = ModeNormal;
			public int Mode { get; set; }
			/// <summary>每卡强度 (0-1)，三模式共享。</summary>
			public double Intensity { get; set; } = 1.0;
			/// <summary>部件→effect：normal/separately 模式使用。</summary>
			public Dictionary<string, int> Parts { get; set; } = [];
			/// <summary>整卡 effect：fully 模式使用。</summary>
			public int FullCardEffect { get; set; }
		}

		private static ConfigData _data = new();
		private static Dictionary<string, CardEffectEntry> _cards => _data.Cards;

		// ── 全局开关属性（读取时考虑性能限制） ────────────────────────
		public static bool GlobalDynamicEffect
		{
			get => !IsPerformanceThrottled && _data.GlobalDynamicEffect;
			set
			{
				_data.GlobalDynamicEffect = value;
				ResetPerformanceThrottle();
				Save();
			}
		}

		public static bool EnableShaderInNonCombat
		{
			get => !IsPerformanceThrottled && _data.EnableShaderInNonCombat;
			set
			{
				_data.EnableShaderInNonCombat = value;
				ResetPerformanceThrottle();
				Save();
			}
		}

		public static bool ShownTestWarning
		{
			get => _data.ShownTestWarning;
			set
			{
				_data.ShownTestWarning = value;
				ResetPerformanceThrottle();
				Save();
			}
		}

		public static readonly string[] AllPartNames = new[]
		{
			"Portrait", "Frame", "TitleBanner",
			"TypePlaque", "PortraitBorder", "EnergyIcon", "StarIcon", "Enchantment"
		};

		// ── 三模式缓存（会话内，一次游戏进程） ─────────────────────
		private sealed class CardModeCache
		{
			public Dictionary<string, int> NormalParts = [];
			public Dictionary<string, int> SeparateParts = [];
			public int FullCardEffect = 0;
		}
		private static readonly Dictionary<string, CardModeCache> _modeCache = new();

		private static CardModeCache GetModeCache(string cardId)
		{
			if (!_modeCache.TryGetValue(cardId, out var c))
				_modeCache[cardId] = c = new CardModeCache();
			return c;
		}

		public static string GetEditMode(string cardId)
		{
			if (IsPerformanceThrottled) return ModeNormal;
			if (TryGetOverlay(cardId, out var enchId, out bool preview))
			{
				var ee = EnchantmentConfig.GetEntry(enchId);
				if (preview) return ee != null ? ee.EditMode : ModeNormal; // 预览：仅附魔配置
				if (ee != null) return ee.EditMode;
			}
			return _cards.TryGetValue(cardId, out var entry) ? entry.EditMode : ModeNormal;
		}

		/// <summary>
		/// 切换编辑模式：先把当前模式数据存入会话缓存，再从缓存恢复目标模式数据（不丢失），
		/// 最后重写 json（只持久化当前模式）。Intensity 为三模式共享，切换时保持不变。
		/// </summary>
		public static void SetEditMode(string cardId, string newMode)
		{
			if (string.IsNullOrEmpty(cardId)) return;
			if (newMode is not (ModeNormal or ModeSeparately or ModeFully)) return;
			if (!_cards.TryGetValue(cardId, out var entry)) _cards[cardId] = entry = new CardEffectEntry();
			if (entry.EditMode == newMode) return;

			SaveCurrentModeToCache(cardId, entry);
			RestoreModeFromCache(cardId, entry, newMode);
			entry.EditMode = newMode;
			ResetPerformanceThrottle();
			Save();
		}

		private static void SaveCurrentModeToCache(string cardId, CardEffectEntry entry)
		{
			var c = GetModeCache(cardId);
			switch (entry.EditMode)
			{
				case ModeSeparately: c.SeparateParts = new Dictionary<string, int>(entry.Parts); break;
				case ModeFully: c.FullCardEffect = entry.FullCardEffect; break;
				default: c.NormalParts = new Dictionary<string, int>(entry.Parts); break;
			}
		}

		private static void RestoreModeFromCache(string cardId, CardEffectEntry entry, string mode)
		{
			var c = GetModeCache(cardId);
			switch (mode)
			{
				case ModeSeparately:
					entry.FullCardEffect = 0;
					if (c.SeparateParts.Count > 0)
						entry.Parts = new Dictionary<string, int>(c.SeparateParts);
					else if (entry.Parts.Count > 0)
						c.SeparateParts = new Dictionary<string, int>(entry.Parts); // 首次进入：保留当前勾选并初始化缓存
					break;
				case ModeFully:
					entry.Parts = [];
					if (c.FullCardEffect > 0)
						entry.FullCardEffect = c.FullCardEffect;
					// 缓存为 0：保留 entry.FullCardEffect（继承，通常为 0）
					break;
				default:
					entry.FullCardEffect = 0;
					if (c.NormalParts.Count > 0)
						entry.Parts = new Dictionary<string, int>(c.NormalParts);
					else if (entry.Parts.Count > 0)
						c.NormalParts = new Dictionary<string, int>(entry.Parts); // 首次进入：保留当前勾选并初始化缓存
					break;
			}
		}

		// ── 加载与保存 ─────────────────────────────────────────────
		public static void Load()
		{
			try
			{
				if (!File.Exists(FilePath))
				{
					ResetAll();
					ApplyAllAuthorPresets(); // 首次初始化：应用作者预设（卡牌 + 附魔，按 id 合并）
					return;
				}
				string json = File.ReadAllText(FilePath);
				var data = JsonSerializer.Deserialize<ConfigData>(json);
				if (data == null)
				{
					ResetAll();
					return;
				}
				bool upgraded = data.Version != CurrentConfigVersion;
				if (upgraded)
				{
					// 逐步升级：v5→v6→v7→v8（每步只转换数据，不覆盖玩家已有配置）
					data = UpgradeToCurrent(json);
					if (data == null)
					{
						ResetAll();
						return;
					}
				}
				_data = data;
				PruneInvalidEntries();
				if (upgraded)
					ApplyAllAuthorPresets(); // 版本升级后：应用作者预设（按 id 合并）
				ResetPerformanceThrottle(); // 加载配置后确保不受之前性能限制影响
			}
			catch
			{
				ResetAll();
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
				GD.PrintErr($"[BalatroEffect] Save failed: {e.Message}");
			}
		}

		// ── 附魔覆盖（运行时自动包裹 + 附魔预览） ───────────────────
		// 会话内：cardId → 附魔 id（含是否仅用附魔配置的预览标志）。
		// 由 ShaderController.ApplyShader 按卡牌当前 Model 维护；各读取访问器据此解析。
		// 运行时：先应用卡牌效果，再应用附魔效果（附魔逐字段覆盖）。
		// 预览：仅用附魔配置（忽略卡自身配置，保证附魔预览卡“干净”）。
		private static readonly Dictionary<string, string> _cardEnchantOverlay = new();
		private static readonly HashSet<string> _previewEnchantCards = new();

		private static bool TryGetOverlay(string cardId, out string enchantmentId, out bool preview)
		{
			if (_cardEnchantOverlay.TryGetValue(cardId, out var enchId))
			{
				enchantmentId = enchId;
				preview = _previewEnchantCards.Contains(cardId);
				return true;
			}
			enchantmentId = "";
			preview = false;
			return false;
		}

		/// <summary>设置卡牌 → 附魔 的运行时覆盖。enchantmentId 为 null 时清除。</summary>
		internal static void SetCardEnchantmentOverlay(string cardId, string? enchantmentId, bool previewOnly)
		{
			if (string.IsNullOrEmpty(enchantmentId))
			{
				_cardEnchantOverlay.Remove(cardId);
				_previewEnchantCards.Remove(cardId);
				return;
			}
			_cardEnchantOverlay[cardId] = enchantmentId;
			if (previewOnly) _previewEnchantCards.Add(cardId);
			else _previewEnchantCards.Remove(cardId);
		}

		internal static void ClearCardEnchantmentOverlay(string cardId)
		{
			_cardEnchantOverlay.Remove(cardId);
			_previewEnchantCards.Remove(cardId);
		}

		/// <summary>清除全部附魔覆盖（离开附魔预览界面时调用）。</summary>
		internal static void ClearAllEnchantOverlays()
		{
			_cardEnchantOverlay.Clear();
			_previewEnchantCards.Clear();
		}

		// ── 卡牌效果查询（所有读取均受性能限制影响） ─────────────────
		public static int GetCardEffectMode(string cardId)
		{
			if (IsPerformanceThrottled) return 0;
			if (TryGetOverlay(cardId, out var enchId, out bool preview))
			{
				var ee = EnchantmentConfig.GetEntry(enchId);
				if (preview) return ee != null ? ee.Mode : 0; // 预览：仅附魔配置
				if (ee != null) return ee.Mode;
			}
			return _cards.TryGetValue(cardId, out var entry) ? entry.Mode : 0;
		}

		public static void SetCardEffectMode(string cardId, int mode)
		{
			if (!_cards.ContainsKey(cardId)) _cards[cardId] = new CardEffectEntry();
			_cards[cardId].Mode = mode;
			ResetPerformanceThrottle();
			Save();
		}

		public static int GetEffect(string cardId, string partName)
		{
			if (IsPerformanceThrottled) return 0;
			if (TryGetOverlay(cardId, out var enchId, out bool preview))
			{
				var ee = EnchantmentConfig.GetEntry(enchId);
				if (preview)
				{
					// 附魔预览：仅用附魔配置；无配置 → 无效果（卡自身配置不参与，保证“干净”）
					if (ee == null) return 0;
					if (partName == "FullCard") return ee.FullCardEffect;
					return ee.Parts.TryGetValue(partName, out int previewEv) ? previewEv : 0;
				}
				if (ee != null)
				{
					// 运行时：先应用卡牌效果，再应用附魔效果（附魔覆盖）
					if (partName == "FullCard")
						return ee.FullCardEffect > 0 ? ee.FullCardEffect : GetCardEffectCore(cardId, partName);
					int cardVal = GetCardEffectCore(cardId, partName);
					return ee.Parts.TryGetValue(partName, out int mergeEv) && mergeEv > 0 ? mergeEv : cardVal;
				}
			}
			return GetCardEffectCore(cardId, partName);
		}

		private static int GetCardEffectCore(string cardId, string partName)
		{
			if (_cards.TryGetValue(cardId, out var entry))
			{
				if (partName == "FullCard") return entry.FullCardEffect;
				if (entry.Parts.TryGetValue(partName, out int val)) return val;
			}
			return 0;
		}

		public static void SetEffect(string cardId, string partName, int effectIndex)
		{
			if (!_cards.ContainsKey(cardId)) _cards[cardId] = new CardEffectEntry();
			var entry = _cards[cardId];
			if (partName == "FullCard")
				entry.FullCardEffect = effectIndex;
			else
			{
				if (effectIndex == 0)
					entry.Parts.Remove(partName);
				else
					entry.Parts[partName] = effectIndex;
			}
			// 注意：运行中不删除条目（fully/separately 切到空时保留 EditMode）；
			// 无效数据由 Load/初始化时的 PruneInvalidEntries 统一清理。
			ResetPerformanceThrottle();
			Save();
		}

		public static void ClearEffect(string cardId, string? partName = null)
		{
			if (partName == null)
			{
				// 菜单「清空当前卡效果」：显式清空整卡，移除条目
				_cards.Remove(cardId);
			}
			else if (_cards.TryGetValue(cardId, out var entry))
			{
				if (partName == "FullCard")
					entry.FullCardEffect = 0;
				else
					entry.Parts.Remove(partName);
				// 运行中保留条目（EditMode 不因单部件清空而丢失）；无效数据由 Load/Prune 统一清理
			}
			ResetPerformanceThrottle();
			Save();
		}

		public static double GetIntensity(string cardId, double defaultValue = 1.0)
		{
			if (IsPerformanceThrottled) return 0.0;
			if (TryGetOverlay(cardId, out var enchId, out bool preview))
			{
				var ee = EnchantmentConfig.GetEntry(enchId);
				if (preview) return ee != null ? ee.Intensity : defaultValue; // 预览：仅附魔配置
				if (ee != null && EnchantmentConfig.HasEffect(enchId))
					return ee.Intensity;
			}
			return GetCardIntensityCore(cardId, defaultValue);
		}

		private static double GetCardIntensityCore(string cardId, double defaultValue)
		{
			return _cards.TryGetValue(cardId, out var entry) ? entry.Intensity : defaultValue;
		}

		public static void SetIntensity(string cardId, double value)
		{
			if (string.IsNullOrEmpty(cardId)) return;
			if (!_cards.TryGetValue(cardId, out var entry)) return; // 无效果条目不创建
			entry.Intensity = Math.Clamp(value, 0.0, 1.0);
			ResetPerformanceThrottle();
			Save();
		}

		/// <summary>推导一张卡当前实际生效的效果 mode：FullCard &gt; Parts 首个非0 &gt; Mode。</summary>
		public static int GetCardEffectIndex(string cardId)
		{
			if (IsPerformanceThrottled) return 0;
			if (TryGetOverlay(cardId, out var enchId, out bool preview))
			{
				var ee = EnchantmentConfig.GetEntry(enchId);
				if (preview) return ee != null ? EnchantmentConfig.GetCardEffectIndex(enchId) : 0; // 预览：仅附魔配置
				if (ee != null) return EnchantmentConfig.GetCardEffectIndex(enchId);
			}
			if (!_cards.TryGetValue(cardId, out var entry)) return 0;
			return EffectiveMode(entry);
		}

		/// <summary>
		/// 移除无实际效果的卡牌条目：Parts 为空且 FullCardEffect 为 0（即使 Mode/Intensity 非默认）。
		/// 加载/迁移/导入后调用，保证配置文件不含无效内容。
		/// </summary>
		private static void PruneInvalidEntries()
		{
			var invalid = new List<string>();
			foreach (var (cardId, entry) in _cards)
			{
				if (entry.FullCardEffect == 0 && (entry.Parts == null || entry.Parts.Count == 0))
					invalid.Add(cardId);
			}
			foreach (var id in invalid) _cards.Remove(id);
		}

		private static int EffectiveMode(CardEffectEntry entry)
		{
			if (entry.FullCardEffect > 0) return entry.FullCardEffect;
			if (entry.Parts != null)
				foreach (var v in entry.Parts.Values)
					if (v > 0) return v;
			return entry.Mode;
		}

		public static bool HasAnyEffect()
		{
			if (IsPerformanceThrottled) return false;
			if (_cards == null) return false;
			foreach (var entry in _cards.Values)
			{
				if (entry.FullCardEffect > 0) return true;
				if (entry.Parts != null && entry.Parts.Values.Any(v => v > 0)) return true;
			}
			return false;
		}

		/// <summary>单张卡是否具有实际效果（FullCard &gt; 0 或任一部件 &gt; 0）。用于判断「复制效果」是否有效。</summary>
		public static bool HasEffect(string cardId)
		{
			if (IsPerformanceThrottled) return false;
			if (TryGetOverlay(cardId, out var enchId, out bool preview))
			{
				if (preview) return EnchantmentConfig.HasEffect(enchId); // 预览：仅附魔配置
				return CardHasEffectCore(cardId) || EnchantmentConfig.HasEffect(enchId);
			}
			return CardHasEffectCore(cardId);
		}

		private static bool CardHasEffectCore(string cardId)
		{
			if (!_cards.TryGetValue(cardId, out var entry)) return false;
			return EntryHasEffect(entry);
		}

		/// <summary>返回当前卡牌配置条目（只读引用），供批量复制用。</summary>
		public static CardEffectEntry? GetEntry(string cardId)
		{
			if (IsPerformanceThrottled) return null;
			return _cards.TryGetValue(cardId, out var entry) ? entry : null;
		}

		/// <summary>用源条目完整覆盖目标卡（含 EditMode/强度），供「应用到同页所有卡」使用。</summary>
		public static void ReplaceCardEffects(string cardId, CardEffectEntry source)
		{
			if (source == null || string.IsNullOrEmpty(cardId)) return;
			var copy = new CardEffectEntry
			{
				EditMode = source.EditMode,
				Mode = source.Mode,
				Intensity = source.Intensity,
				Parts = new Dictionary<string, int>(source.Parts),
				FullCardEffect = source.FullCardEffect
			};
			if (!EntryHasEffect(copy))
				_cards.Remove(cardId);
			else
				_cards[cardId] = copy;
			ResetPerformanceThrottle();
			Save();
		}

		private static bool EntryHasEffect(CardEffectEntry entry)
		{
			if (entry.FullCardEffect > 0) return true;
			if (entry.Parts != null)
				foreach (var v in entry.Parts.Values)
					if (v > 0) return true;
			return false;
		}

		/// <summary>
		/// 只读校验剪贴板中的「卡牌预设」JSON 是否有效（可解析且含实际效果），不写入配置。
		/// 用于判断「粘贴效果」按钮是否有效。
		/// </summary>
		public static bool IsValidCardPresetJson(string json)
		{
			try
			{
				using var doc = JsonDocument.Parse(json);
				var root = doc.RootElement;
				if (root.TryGetProperty("FullCardEffect", out var f) && f.GetInt32() > 0)
					return true;
				if (root.TryGetProperty("Parts", out var partsProp))
				{
					foreach (var item in partsProp.EnumerateObject())
						if (item.Value.GetInt32() > 0) return true;
				}
				return false;
			}
			catch { return false; }
		}

		/// <summary>
		/// 只读校验剪贴板中的「全局预设」JSON 是否有效（可解析且版本可接受），不写入配置。
		/// 用于判断「导入全局」菜单项是否有效。
		/// </summary>
		public static bool IsValidPresetJson(string json)
		{
			try
			{
				var data = JsonSerializer.Deserialize<ConfigData>(json);
				if (data == null) return false;
				return data.Version == CurrentConfigVersion || data.Version is 5 or 6 or 7;
			}
			catch { return false; }
		}

		// ── 导入导出（同样重置安全阀） ─────────────────────────────
		public static string ExportPreset() =>
			JsonSerializer.Serialize(new ConfigData
			{
				Version = CurrentConfigVersion,
				Cards = _cards,
				GlobalDynamicEffect = _data.GlobalDynamicEffect,
				EnableShaderInNonCombat = _data.EnableShaderInNonCombat,
				ShownTestWarning = _data.ShownTestWarning
			}, Options);

		public static bool ImportPreset(string json)
		{
			try
			{
				var data = JsonSerializer.Deserialize<ConfigData>(json);
				if (data == null) return false;
				if (data.Version != CurrentConfigVersion)
				{
					// 剪贴板旧版全局预设：逐步升级 v5→v6→v7→v8（仅转换数据，不覆盖当前配置）
					data = UpgradeToCurrent(json);
					if (data == null) return false;
				}
				// 按 id 合并（不整文件替换）：预设中的 id 覆盖对应条目，其余保留；全局开关保持当前
				if (data.Cards != null)
					foreach (var kv in data.Cards)
						if (kv.Value != null) _cards[kv.Key] = kv.Value;
				PruneInvalidEntries();
				ResetPerformanceThrottle();
				Save();
				return true;
			}
			catch { return false; }
		}

		public static string ExportCardPreset(string cardId)
		{
			var entry = _cards.TryGetValue(cardId, out var val) ? val : null;
			var dto = new
			{
				CardId = cardId,
				EditMode = entry?.EditMode ?? ModeNormal,
				Mode = entry?.Mode ?? 0,
				Intensity = entry?.Intensity ?? 1.0,
				Parts = entry?.Parts,
				FullCardEffect = entry?.FullCardEffect ?? 0
			};
			return JsonSerializer.Serialize(dto, Options);
		}

		public static bool ImportCardPreset(string cardId, string json)
		{
			try
			{
				using var doc = JsonDocument.Parse(json);
				var root = doc.RootElement;
				string editMode = root.TryGetProperty("EditMode", out var em)
					? em.GetString() ?? ModeNormal : ModeNormal;
				int mode = root.TryGetProperty("Mode", out var m) ? m.GetInt32() : 0;
				var parts = new Dictionary<string, int>();
				if (root.TryGetProperty("Parts", out var partsProp))
				{
					foreach (var item in partsProp.EnumerateObject())
						parts[item.Name] = item.Value.GetInt32();
				}
				int full = root.TryGetProperty("FullCardEffect", out var f) ? f.GetInt32() : 0;
				var entry = new CardEffectEntry { EditMode = editMode, Mode = mode, Parts = parts, FullCardEffect = full };
				// v6：卡牌预设携带该卡强度（旧剪贴板无 Intensity 字段则保持默认 1.0）
				if (root.TryGetProperty("Intensity", out var inten))
					entry.Intensity = Math.Clamp(inten.GetDouble(), 0.0, 1.0);
				_cards[cardId] = entry;
				// 与加载时一致：无实际效果的预设（Parts 空且非 FullCard）不保留
				if (entry.FullCardEffect == 0 && (entry.Parts == null || entry.Parts.Count == 0))
					_cards.Remove(cardId);
				ResetPerformanceThrottle();
				Save();
				return true;
			}
			catch { return false; }
		}

		/// <summary>
		/// 推断一张卡在 v7 下的编辑模式：FullCardEffect &gt; 0 → fully；
		/// Parts 含 ≥2 种不同 effect → separately；否则 → normal。
		/// </summary>
		private static string InferEditMode(CardEffectEntry entry)
		{
			if (entry.FullCardEffect > 0) return ModeFully;
			int first = 0;
			if (entry.Parts != null)
			{
				foreach (var v in entry.Parts.Values)
				{
					if (v <= 0) continue;
					if (first == 0) first = v;
					else if (v != first) return ModeSeparately;
				}
			}
			return ModeNormal;
		}

		/// <summary>
		/// 把旧版本配置逐步升级到当前版本（v5→v6→v7→v8），返回升级后数据；解析失败返回 null。
		/// 每步只转换 data（不写 _data），由调用方决定应用方式（Load / ImportPreset）。
		/// </summary>
		private static ConfigData? UpgradeToCurrent(string json)
		{
			var data = JsonSerializer.Deserialize<ConfigData>(json);
			if (data == null) return null;
			if (data.Version == 5)
			{
				// v5 → v6：旧全局强度按卡导入，移除 IntensitySettings
				var v5 = JsonSerializer.Deserialize<ConfigDataV5>(json) ?? new ConfigDataV5();
				data = V5ToV6(v5);
			}
			if (data.Version == 6)
			{
				// v6 → v7：推断每卡编辑模式
				V6ToV7(data);
			}
			if (data.Version == 7)
			{
				// v7 → v8：效果 mode 重排
				V7ToV8(data);
			}
			return data.Version == CurrentConfigVersion ? data : null;
		}

		/// <summary>v5 → v6：把旧的「按效果 mode 的全局强度」导入到每一张使用了该效果的卡牌上（内嵌于条目）。</summary>
		private static ConfigData V5ToV6(ConfigDataV5 v5)
		{
			var cards = v5.Cards ?? [];
			if (v5.IntensitySettings != null)
			{
				foreach (var entry in cards.Values)
				{
					if (entry == null) continue;
					int mode = EffectiveMode(entry);
					if (mode > 0 && v5.IntensitySettings.TryGetValue(mode, out double v))
						entry.Intensity = v; // 旧全局强度按卡导入（内嵌于条目）
				}
			}
			return new ConfigData
			{
				Version = 6,
				Cards = cards,
				GlobalDynamicEffect = v5.GlobalDynamicEffect,
				EnableShaderInNonCombat = v5.EnableShaderInNonCombat,
				ShownTestWarning = v5.ShownTestWarning
			};
		}

		/// <summary>v6 → v7：v6 条目无 EditMode，按内容智能推断编辑模式（纯转换）。</summary>
		private static void V6ToV7(ConfigData data)
		{
			if (data.Cards != null)
				foreach (var entry in data.Cards.Values)
					if (entry != null) entry.EditMode = InferEditMode(entry);
			data.Version = 7;
		}

		/// <summary>v7 → v8：效果 mode 重排（1闪箔 2闪箔偏 3多彩 4镭射 5负片-A 6负片-B）。</summary>
		private static void V7ToV8(ConfigData data)
		{
			if (data.Cards != null)
				foreach (var e in data.Cards.Values)
					if (e != null) RemapEntry(e);
			data.Version = 8;
		}

		private static void RemapEntry(CardEffectEntry e)
		{
			e.Mode = RemapModeV7ToV8(e.Mode);
			e.FullCardEffect = RemapModeV7ToV8(e.FullCardEffect);
			if (e.Parts != null)
			{
				var newParts = new Dictionary<string, int>();
				foreach (var kv in e.Parts)
					newParts[kv.Key] = RemapModeV7ToV8(kv.Value);
				e.Parts = newParts;
			}
		}

		/// <summary>旧 mode 编号 → 新编号（v7→v8 重排）。供 Config 与 EnchantmentConfig 共用。</summary>
		internal static int RemapModeV7ToV8(int mode) => mode switch
		{
			2 => 5, // Negative（负片）→ 负片-A
			5 => 2, // Foil Alt（闪箔偏）→ 与闪箔相邻
			6 => 7, // aniso_fixed 顺延
			_ => mode
		};

		public static void ResetAll()
		{
			_data = new ConfigData();
			ResetPerformanceThrottle();
			Save();
		}

		public static bool LoadAuthorPreset(string path)
		{
			if (!Godot.FileAccess.FileExists(path)) return false;
			using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
			return ImportPreset(file.GetAsText());
		}

		/// <summary>
		/// 应用全部作者预设（全局预设）：卡牌配置 + 附魔配置。均按 id 合并，不覆盖已有配置。
		/// 在 JSON 首次初始化与版本升级（v5/v6 → v7）后调用；菜单「加载作者预设」也调用。
		/// </summary>
		public static void ApplyAllAuthorPresets()
		{
			LoadAuthorPreset("res://balatroeffect/Assets/author_preset.json");
			EnchantmentConfig.LoadAuthorPreset("res://balatroeffect/Assets/author_enchant_preset.json");
		}
	}
}
