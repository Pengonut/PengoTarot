#nullable enable
using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using PengoTarot.Cards;

namespace PengoTarot.Patch.Card;

/// <summary>
/// 为特定塔罗牌在 Choose-A-Card 三选一界面提供原版稀有度级别的发光效果。
///
/// 在 NCard.ActivateRewardScreenGlow 之后运行：
/// - 金色闪烁 (Rare 级): 恋人正逆、负片系列、命运之轮正逆、高塔正逆
/// - 蓝色光晕 (Uncommon 级): 愚者逆、魔术师逆、女祭司逆、隐者正、
///   恶魔正逆、星星正逆、月亮正逆、太阳正逆、世界正逆
/// </summary>
[HarmonyPatch(typeof(NCard), nameof(NCard.ActivateRewardScreenGlow))]
internal static class NCard_RewardGlow_Patch
{
    // ================================================================
    // 金色闪烁 (Rare 级别) 的 TarCard 子类
    // ================================================================
    private static readonly HashSet<Type> GoldenGlowTypes = new()
    {
        // 恋人正逆
        typeof(TarLoversUpright),
        typeof(TarLoversReversed),
        // 负片系列 (Sub)
        typeof(TarDevilUprightSub),
        typeof(TarDevilReversedSub),
        typeof(TarStarUprightSub),
        typeof(TarStarReversedSub),
        typeof(TarMoonUprightSub),
        typeof(TarMoonReversedSub),
        typeof(TarSunUprightSub),
        typeof(TarSunReversedSub),
        typeof(TarWorldUprightSub),
        typeof(TarWorldReversedSub),
        // 命运之轮正逆
        typeof(TarWheelOfFortuneUpright),
        typeof(TarWheelOfFortuneReversed),
        // 高塔正逆
        typeof(TarTowerUpright),
        typeof(TarTowerReversed),
    };

    // ================================================================
    // 蓝色光晕 (Uncommon 级别) 的 TarCard 子类
    // ================================================================
    private static readonly HashSet<Type> BlueGlowTypes = new()
    {
        // 愚者逆
        typeof(TarFoolReversed),
        // 魔术师逆
        typeof(TarMagicianReversed),
        // 女祭司逆
        typeof(TarHighPriestessReversed),
        // 隐者正
        typeof(TarHermitUpright),
        // 恶魔正逆
        typeof(TarDevilUpright),
        typeof(TarDevilReversed),
        // 星星正逆
        typeof(TarStarUpright),
        typeof(TarStarReversed),
        // 月亮正逆
        typeof(TarMoonUpright),
        typeof(TarMoonReversed),
        // 太阳正逆
        typeof(TarSunUpright),
        typeof(TarSunReversed),
        // 世界正逆
        typeof(TarWorldUpright),
        typeof(TarWorldReversed),
    };

    // 缓存反射访问器，避免每次调用都反射查找
    private static readonly AccessTools.FieldRef<NCard, GpuParticles2D> SparklesRef =
        AccessTools.FieldRefAccess<NCard, GpuParticles2D>("_sparkles");

    private static readonly AccessTools.FieldRef<NCard, NCardRareGlow?> RareGlowRef =
        AccessTools.FieldRefAccess<NCard, NCardRareGlow?>("_rareGlow");

    private static readonly AccessTools.FieldRef<NCard, NCardUncommonGlow?> UncommonGlowRef =
        AccessTools.FieldRefAccess<NCard, NCardUncommonGlow?>("_uncommonGlow");

    /// <summary>
    /// Postfix: 在原版 ActivateRewardScreenGlow 之后运行。
    /// 只有当原版方法没有设置任何 glow 时（即 Rarity 既不是 Rare 也不是 Uncommon），
    /// 才检查 TarCard 类型并应用自定义发光效果。
    /// </summary>
    private static void Postfix(NCard __instance)
    {
        // 原版已经设置了 glow 则不处理（非 TarCard 的普通 Rare/Uncommon 卡）
        if (RareGlowRef(__instance) != null || UncommonGlowRef(__instance) != null)
            return;

        var model = __instance.Model;
        if (model == null)
            return;

        var modelType = model.GetType();

        if (GoldenGlowTypes.Contains(modelType))
        {
            ApplyRareGlow(__instance);
        }
        else if (BlueGlowTypes.Contains(modelType))
        {
            ApplyUncommonGlow(__instance);
        }
    }

    /// <summary>应用 Rare 级别的金色闪烁效果</summary>
    private static void ApplyRareGlow(NCard instance)
    {
        // 显示星星粒子
        SparklesRef(instance).Visible = true;

        // 创建并添加 Rare 光晕，写入 _rareGlow 字段让原版 Kill/QueueFree 逻辑可以清理
        var rareGlow = NCardRareGlow.Create();
        if (rareGlow != null)
        {
            rareGlow.Scale = new Vector2(1.1f, 1.2f);
            instance.Body.AddChildSafely(rareGlow);
            instance.Body.MoveChildSafely(rareGlow, 1);
        }
        RareGlowRef(instance) = rareGlow;

        // 设为金色高亮
        instance.CardHighlight.Modulate = NCardHighlight.gold;
    }

    /// <summary>应用 Uncommon 级别的蓝色光晕效果</summary>
    private static void ApplyUncommonGlow(NCard instance)
    {
        // 创建并添加 Uncommon 光晕，写入 _uncommonGlow 字段让原版清理逻辑可以处理
        var uncommonGlow = NCardUncommonGlow.Create();
        if (uncommonGlow != null)
        {
            uncommonGlow.Scale = new Vector2(1.1f, 1.2f);
            instance.Body.AddChildSafely(uncommonGlow);
            instance.Body.MoveChildSafely(uncommonGlow, 1);
        }
        UncommonGlowRef(instance) = uncommonGlow;

        // 设为蓝色高亮
        instance.CardHighlight.Modulate = NCardHighlight.playableColor;
    }
}
