#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Logging;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 抑制 PengoTarot 附魔图标在 Compendium 浏览时产生的 "Asset not cached" 警告。
    /// 
    /// 问题根因：
    ///   PreloadManager.GetRunAssetPaths() 包含所有附魔图标（含 mod 的），但它只在
    ///   LoadRunAssets()（开始游戏时）被调用。而在主菜单浏览图鉴时，附魔图标按需加载，
    ///   AssetCache.LoadAsset 检测到未预缓存就会打 WARN。
    /// 
    /// 解决方案：
    ///   对 res://images/enchantments/tar_* 路径的资源，在 LoadAsset 的 Prefix 中
    ///   自己加载并注入到 _cache + _missedCacheAssets，绕过原方法的警告逻辑。
    ///   加入 _missedCacheAssets 确保资源在 UnloadMissedCacheAssets 时不被清除。
    /// </summary>
    [HarmonyPatch]
    internal static class SuppressEnchantmentAssetWarningPatch
    {
        /// <summary>
        /// AssetCache 中 _cache 字段的反射缓存 (ConcurrentDictionary)
        /// </summary>
        private static readonly Lazy<FieldInfo?> CacheField = new(() =>
            typeof(AssetCache).GetField("_cache",
                BindingFlags.Instance | BindingFlags.NonPublic));

        /// <summary>
        /// AssetCache 中 _missedCacheAssets 字段的反射缓存 (HashSet)
        /// </summary>
        private static readonly Lazy<FieldInfo?> MissedCacheField = new(() =>
            typeof(AssetCache).GetField("_missedCacheAssets",
                BindingFlags.Instance | BindingFlags.NonPublic));

        /// <summary>
        /// 我们想要静默处理的路径前缀集合
        /// </summary>
        private static readonly HashSet<string> SuppressedPathPrefixes = new(StringComparer.OrdinalIgnoreCase)
        {
            "res://images/enchantments/tar_",
            "res://images/enchantments/planet_",
        };

        /// <summary>
        /// Harmony 要求：声明目标方法。LoadAsset 是 private，需要用此方式指定。
        /// </summary>
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(AssetCache), "LoadAsset");
        }

        private static bool ShouldSuppress(string path)
        {
            return SuppressedPathPrefixes.Any(prefix =>
                path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        [HarmonyPrefix]
        static bool Prefix(AssetCache __instance, string path, ref Resource __result)
        {
            if (!ShouldSuppress(path))
                return true;

            try
            {
                // 先查 _cache 是否已有
                var cache = CacheField.Value?.GetValue(__instance) as ConcurrentDictionary<string, Resource>;
                if (cache != null && cache.TryGetValue(path, out var cached) &&
                    GodotObject.IsInstanceValid(cached))
                {
                    __result = cached;
                    return false;
                }

                // 加载资源（和原方法完全一样的方式）
                var resource = ResourceLoader.Load<Resource>(path, null, ResourceLoader.CacheMode.Reuse);
                if (resource == null)
                {
                    Log.Warn($"[PengoTarot] Failed to load enchantment icon: {path}");
                    __result = null!;
                    return false;
                }

                // 注入 _cache，这样下次 GetAsset 就能命中
                if (cache != null)
                    cache[path] = resource;

                // 也加入 _missedCacheAssets，这样 UnloadMissedCacheAssets 不会清除它
                var missedSet = MissedCacheField.Value?.GetValue(__instance) as HashSet<string>;
                missedSet?.Add(path);

                __result = resource;
                return false;
            }
            catch (Exception ex)
            {
                Log.Warn($"[PengoTarot] SuppressEnchantmentWarning error for {path}: {ex.Message}");
                return true;
            }
        }
    }
}
