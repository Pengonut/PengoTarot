#nullable enable

using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace PengoTarot.Patch.Card;

/// <summary>
/// 与 Minty-Spire-2 的「休息后生命值预览」（RestHPRender）兼容（2026-08-16）：
/// Minty 在休息按钮上显示「HP: 当前 → 休息后」，但计算时用了
///   healedHp = Math.Min(maxHp, currentHp + floor(healAmount))
/// 把休息后生命值 clamp 到最大生命值。恶魔开启时生命值可超过上限
/// （TarDevilDivinationPatch 的 Creature.SetCurrentHpInternal 不再 clamp），
/// 该 clamp 会把预览截断成错误值（休息后其实可以 >MaxHp）。
///
/// 实现：Minty 是第三方 DLL（编译期不可引用），运行时用反射
/// TypeByName + harmony.Patch 动态给其私有 UpdateExtraLabel 打 Postfix；
/// 仅当 Minty 已安装时生效，未装则静默跳过，零副作用。
/// Postfix 在恶魔开启（TarDevilDivinationPatch.ShouldApply）时重算 healedHp
/// （去掉 clamp），基础治疗量 + 遗物加成沿用 Minty 自身的
/// ApplyRelicHealModifiers（反射调用，缓存句柄）；恶魔关闭时不动（Minty 原逻辑）。
/// 调度时机：ModInitializer 调 ScheduleAfterModsLoaded，延迟到
/// ModManagerState.Initialized（所有 Mod 加载完，Minty 程序集必然已加载）。
/// </summary>
public static class MintyRestHPRenderRewriter
{
    /// <summary>Minty 的 RestHPRender 全名（其程序集运行时才加载，编译期不可引用）。</summary>
    private const string MintyRestHPRenderTypeName = "MintySpire2.MintySpire2Code.RestHPRender";

    /// <summary>Minty 预览 label 的节点名（RestHPRender.HealLabelNodeName = "ModHealPreviewLabel"）。</summary>
    private const string HealLabelNodeName = "ModHealPreviewLabel";

    /// <summary>Minty 私有方法 ApplyRelicHealModifiers(Player, decimal) 的反射句柄（缓存）。</summary>
    private static MethodInfo? _applyRelicHealModifiers;

    private static Harmony? _harmony;
    private static bool _hasPatched;
    private static int _deferAttempts;

    /// <summary>
    /// 延迟到所有 Mod 加载完成后（ModManagerState.Initialized）再动态 patch Minty，
    /// 确保 Minty 程序集已加载、类型可解析（仿照 GetNodeCallRewriter 的调度模式）。
    /// </summary>
    public static void ScheduleAfterModsLoaded()
    {
        try
        {
            Callable.From((Action)RunDeferred).CallDeferred();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[PengoTarot] Could not schedule Minty rest HP compat patch: {ex.Message}");
        }
    }

    private static void RunDeferred()
    {
        if (_hasPatched) return;
        if (ModManager.State != ModManagerState.Initialized)
        {
            if (++_deferAttempts < 300)
            {
                Callable.From((Action)RunDeferred).CallDeferred();
            }
            else
            {
                GD.PrintErr("[PengoTarot] ModManager never Initialized; Minty rest HP compat patch skipped.");
            }
            return;
        }

        _hasPatched = true;
        try
        {
            TryPatch();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[PengoTarot] Minty rest HP compat patch failed: {ex}");
        }
    }

    private static void TryPatch()
    {
        Type? mintyType = AccessTools.TypeByName(MintyRestHPRenderTypeName);
        if (mintyType == null)
        {
            return;   // 未安装 Minty → 静默跳过，不打印日志
        }

        MethodInfo? updateMethod = AccessTools.Method(mintyType, "UpdateExtraLabel");
        if (updateMethod == null)
        {
            GD.PrintErr("[PengoTarot] Minty rewriter FAILED: RestHPRender.UpdateExtraLabel not found.");
            return;
        }

        _applyRelicHealModifiers = AccessTools.Method(mintyType, "ApplyRelicHealModifiers");
        if (_applyRelicHealModifiers == null)
        {
            GD.PrintErr("[PengoTarot] Minty rewriter FAILED: RestHPRender.ApplyRelicHealModifiers not found.");
            return;
        }

        MethodInfo postfix = typeof(MintyRestHPRenderRewriter)
            .GetMethod(nameof(Postfix), BindingFlags.Static | BindingFlags.Public)!;

        _harmony = new Harmony("com.pengotarot.mintyresthp");
        _harmony.Patch(updateMethod, postfix: new HarmonyMethod(postfix));
        GD.Print("[PengoTarot] Minty rewriter OK: patched RestHPRender.UpdateExtraLabel for Devil overflow HP.");
    }

    /// <summary>
    /// Minty UpdateExtraLabel 的 Postfix：恶魔开启时去掉 healedHp 的 maxHp clamp，
    /// 让休息后生命值预览能正确显示超额（&gt;MaxHp）。恶魔关闭时直接返回（Minty 原逻辑）。
    /// 计算沿用 Minty：GetBaseHealAmount（基础量）+ ApplyRelicHealModifiers（遗物加成）。
    /// </summary>
    public static void Postfix(NRestSiteButton button)
    {
        if (!TarDevilDivinationPatch.ShouldApply())
            return;   // 恶魔未开启 / 不在跑局 → 保持 Minty 原预览

        if (button.Option is not HealRestSiteOption)
            return;

        // RestSiteOption.Owner 是 protected（Minty 用 publicizer 才可直接访问）；
        // 我们项目不用 Publicizer，按惯例用 Traverse 访问。
        Player? player = Traverse.Create(button.Option).Property("Owner").GetValue<Player>();
        if (player == null || !LocalContext.IsMe(player))
            return;

        if (button.FindChild(HealLabelNodeName, true, false) is not Label extra)
            return;

        var creature = player.Creature;
        int currentHp = creature.CurrentHp;
        decimal healAmount = HealRestSiteOption.GetBaseHealAmount(creature);
        if (_applyRelicHealModifiers != null)
        {
            healAmount = (decimal)_applyRelicHealModifiers.Invoke(
                null, new object?[] { player, healAmount })!;
        }

        // 恶魔下治疗不封顶：只保护非负、不 clamp 到 maxHp
        int healedHp = currentHp + Math.Max(0, (int)Math.Floor(healAmount));
        extra.Text = $"HP: {currentHp} → {healedHp}";
    }
}
