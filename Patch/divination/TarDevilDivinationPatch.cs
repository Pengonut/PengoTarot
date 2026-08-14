#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch.Card;

/// <summary>
/// 恶魔（Devil, 索引15）难度开关效果：
/// 1. 你的生命值可以超过最大生命值（治疗/回血不再被上限截断）。
/// 2. 你不会因为失去最大生命值而失去生命（失去最大生命时不掉当前生命、不受伤害）。
/// 3. 每当你在休息处选择「休息(Heal)」时，失去 6 点最大生命值（参考 NightTerrors：仅在
///    非被模拟的休息触发，isMimicked=true 的模拟休息如 DenseVegetation 事件不触发）。
///
/// 实现原理：
/// - <see cref="Creature.SetCurrentHpInternal"/>：默认会把 CurrentHp clamp 到 MaxHp（这是
///   「生命值不能超过上限」的唯一闸口）。恶魔开启时改为只保护非负、不 clamp 上限。
/// - <see cref="Creature.SetMaxHpInternal"/>：默认会把 CurrentHp 压到新 MaxHp（失去最大生命
///   时当前生命被截断）。恶魔开启时用 Prefix 记录原 CurrentHp、Postfix 恢复，保证不截断。
/// - <see cref="CreatureCmd.LoseMaxHp"/>：默认当新 MaxHp 低于 CurrentHp 时会额外对玩家造成
///   伤害。恶魔开启时重写该方法跳过 Damage 分支（其余逻辑照旧：记录 MaxHpLost、SetMaxHp）。
/// - <see cref="Hook.AfterRestSiteHeal"/>：休息(Heal) 选项结算后失去 6 点最大生命值。
/// - <see cref="NHealthBar"/> 血条：前景被 %HpForegroundContainer/%Mask 裁剪 → 永远满格。
///   视觉特效仅在「恶魔开启 && 玩家 CurrentHp > MaxHp」时启动（否则完全原版）：把
///   %HpForegroundContainer 宽度扩展为 ratio×原宽 + 同步私有字段 _expectedMaxFgWidth，
///   并让 <see cref="NHealthBar.GetFgWidth"/> 在超额时改用 CurrentHp 当分母（否则毒/Doom
///   的 OffsetLeft 会按 MaxHp 分母×放大基准被推到容器右侧外 → 毒区不可见/错位）→ 游戏自动
///   重算前景（占满、%Mask 蒙版圆角）+ 毒/Doom 特效（比例基于 CurrentHp 容量，位置正确）；
///   Postfix 把底框 %HpBackground 按比例绝对缩小（k = 1/ratio）；文字 %HpLabel 不参与、位置
///   不变；超额时血条满格无缺失部分，不会露白。
/// - <see cref="Hook.ModifyExtraRestSiteHealText"/>：休息(Heal)选项详细描述后追加
///   「失去6点最大生命值。」（参考 NightTerrors）。
/// - <see cref="NRestSiteButton._Ready"/>：休息(Heal)选项图标正上方挂恶魔逆小图标。
///
/// 仅当「配置开启（GetTarFlag(15)）且当前在一局游戏中」时生效（主菜单/图鉴等不在跑局场景不生效）。
/// 只影响玩家生物（怪物/其他玩家不受影响）。
/// </summary>
public static class TarDevilDivinationPatch
{
    /// <summary>Devil 在 FlagNames 中的索引。</summary>
    private const int DevilFlagIndex = 15;

    /// <summary>每次休息(Heal)失去的最大生命值。</summary>
    private const decimal RestMaxHpLoss = 6m;

    /// <summary>恶魔超额生命的暖橙色（十六进制，BBCode 与 Color 通用）。</summary>
    private const string DevilOverflowColor = "#F59C2E";

    /// <summary>恶魔超额生命数字的深色描边（与暖橙搭配）。</summary>
    private static readonly Color DevilOverflowOutlineColor = new Color("7A3B00");

    /// <summary>是否应生效：配置开启 且 当前在一局游戏中（主菜单/图鉴不生效）。</summary>
    private static bool ShouldApply()
        => ConfigFloatingWindowRunData.GetTarFlag(DevilFlagIndex)
           && RunManager.Instance.IsInProgress;

    /// <summary>是否应对该生物生效：仅玩家生物（怪物不受影响）。</summary>
    private static bool ShouldAffect(Creature creature)
        => creature.IsPlayer && ShouldApply();

    /// <summary>
    /// 通过反射调用属性的 setter（含 private setter），从而在修改 <see cref="Creature.CurrentHp"/>
    /// / <see cref="Creature.MaxHp"/> 时正常触发 Changed 事件（直接改字段会跳过事件，
    /// 导致顶部血条/数字 UI 不刷新）。
    /// </summary>
    private static void SetPrivatePropertyValue(Creature creature, string propertyName, object value)
    {
        PropertyInfo? property = typeof(Creature).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property!.SetValue(creature, value);
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 1: 生命值可以超过最大生命值
    // 目标：public void SetCurrentHpInternal(decimal amount)
    // 原方法：CurrentHp = (int)Math.Min(amount, MaxHp);
    // 恶魔：CurrentHp = (int)Math.Max(amount, 0m);（不 clamp 上限，只保护非负）
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(Creature), nameof(Creature.SetCurrentHpInternal))]
    public static class Creature_SetCurrentHpInternal_DevilPatch
    {
        [HarmonyPrefix]
        static bool Prefix(Creature __instance, decimal amount)
        {
            if (!ShouldAffect(__instance))
                return true; // 原逻辑

            SetPrivatePropertyValue(__instance, "CurrentHp", (int)Math.Max(amount, 0m));
            return false; // 跳过原 clamp
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 2: 不会因为失去最大生命值而失去生命（截断恢复）
    // 目标：public void SetMaxHpInternal(decimal amount)
    // 原方法：MaxHp = ...; CurrentHp = Math.Min(CurrentHp, MaxHp);
    // 恶魔：Prefix 记录原 CurrentHp，Postfix 若被压回则恢复 → CurrentHp 不被截断。
    // （只恢复被压低的情况；若 CurrentHp 本就 ≤ MaxHp 则无变化，零副作用）
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(Creature), nameof(Creature.SetMaxHpInternal))]
    public static class Creature_SetMaxHpInternal_DevilPatch
    {
        [HarmonyPrefix]
        static void Prefix(Creature __instance, out int __state)
        {
            __state = __instance.CurrentHp;
        }

        [HarmonyPostfix]
        static void Postfix(Creature __instance, int __state)
        {
            if (!ShouldAffect(__instance))
                return;
            if (__instance.CurrentHp < __state)
            {
                SetPrivatePropertyValue(__instance, "CurrentHp", __state);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 3: 不会因为失去最大生命值而失去生命（跳过伤害）
    // 目标：public static async Task LoseMaxHp(PlayerChoiceContext, Creature, decimal, bool)
    // 原方法：当 newMaxHp < CurrentHp 时会对玩家造成等量伤害（Unblockable），
    //         恶魔开启时跳过该 Damage 分支；其余逻辑（MaxHpLost 记录、SetMaxHp）照旧。
    //         SetMaxHp 内部 SetMaxHpInternal 已被 Patch 2 保证不压 CurrentHp。
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.LoseMaxHp))]
    public static class CreatureCmd_LoseMaxHp_DevilPatch
    {
        [HarmonyPrefix]
        static bool Prefix(Creature creature, decimal amount, ref Task __result)
        {
            if (!ShouldAffect(creature))
                return true; // 原逻辑

            __result = LoseMaxHpWithoutCurrentHpLoss(creature, amount);
            return false; // 跳过原方法（含 Damage 分支）
        }

        /// <summary>恶魔版 LoseMaxHp：失去最大生命但不掉当前生命、不造成伤害。</summary>
        private static async Task LoseMaxHpWithoutCurrentHpLoss(Creature creature, decimal amount)
        {
            if (amount < 0m)
            {
                throw new ArgumentException("amount must be non-negative. Use GainMaxHp for max HP gain.");
            }

            decimal newMaxHp = (decimal)creature.MaxHp - amount;
            MapPointHistoryEntry? entry = creature.Player?.RunState.CurrentMapPointHistoryEntry;
            if (entry != null)
            {
                // entry 非空 ⇒ creature.Player 必然非空
                entry.GetEntry(creature.Player!.NetId).MaxHpLost += (int)amount;
            }

            await CreatureCmd.SetMaxHp(creature, Math.Max(1.0m, newMaxHp));
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 4: 每当你在休息处休息时，失去 6 点最大生命值
    // 目标：public static async Task AfterRestSiteHeal(IRunState, Player, bool)
    // 仅在「休息(Heal)」选项结算后触发（Smith 锻造/其他选项不触发）；
    // 排除被模拟的休息（isMimicked，参考 NightTerrors）。
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterRestSiteHeal))]
    public static class Hook_AfterRestSiteHeal_DevilPatch
    {
        [HarmonyPostfix]
        static async void Postfix(IRunState runState, Player player, bool isMimicked)
        {
            if (isMimicked)
                return;
            if (!ShouldApply())
                return;

            // 用 async void（fire-and-forget）：CreatureCmd.LoseMaxHp 内部有依赖主循环
            // tick 的等待，在 Harmony Postfix 里同步阻塞会死锁（参考 Temperance 模式）。
            await CreatureCmd.LoseMaxHp(
                new ThrowingPlayerChoiceContext(), player.Creature, RestMaxHpLoss, isFromCard: false);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 5: 玩家血条超额显示（仅在恶魔开启且玩家血量超过上限时启动）
    // 血条前景位于 health_bar.tscn 的 %HpForegroundContainer（clip_children=1）> %Mask
    // （clip_children=1）。%Mask 是 alpha 蒙版（ClipOnly，按 Godot 文档）——前景被裁剪成
    // %Mask 的圆角形状（左/右端圆角天然正确、无方角），但超额部分也被裁掉 → 血条永远满格。
    // 方案（用生命值当 maxHp 重新计算大小，走游戏完整机制 → 特效比例正确）：
    //   Prefix（RefreshForeground 前）把 %HpForegroundContainer 宽度扩展为
    //   newFgW = baseFgW × min(ratio, 2)（超出 2 倍时整体压缩封顶，血条最长 2×）、
    //   同步私有字段 _expectedMaxFgWidth = 容器新宽 → 让游戏把「满血位置」当作容器右边缘。
    //   ⚠️ 关键：游戏 GetFgWidth(amount) = amount / MaxHp * maxFgWidth（分母固定 MaxHp）。
    //   若只放大基准，毒区 OffsetLeft = GetFgWidth(CurrentHp-毒) 会被推到容器右侧外 → 毒区
    //   不可见（毒层数须 ≥ 生命值-最大生命值 才显示）；而 Doom（左端区）反而需要放大基准，
    //   二者矛盾 → 再 patch GetFgWidth：恶魔超额时改用 CurrentHp 当分母 → 前景/毒/Doom 比例
    //   全部基于 CurrentHp 容量，位置正确（毒区=容器右端毒宽度，Doom 区=容器左端 doom 宽度）。
    //   %HpLabel 是 %HpBarContainer 子节点（不跟随 %HpForegroundContainer）→ 文字天然不动。
    //   超额生命 shader（用户追加需求，2026-08-15 由独立节点+排序改为 shader）：血条右端
    //   （从 MaxHp 位置到右端，即超出最大生命值的部分）染暖橙色 —— 给 %HpForeground 挂
    //   shader（shaders/devil_hp_overflow.gdshader，local_pos.x > max_hp_edge 时输出暖橙，
    //   保留贴图 alpha 圆角；用 VERTEX 不用 UV —— NinePatchRect 的 UV 被九宫格限制）。
    //   不新增节点、不改子条顺序（毒/Doom 保持原版层级，毒与橙色接壤处保持原版尖角）。
    //   maxHpEdge = baseFgW × min(1, 2/ratio)（MaxHp 位置，压缩时随压缩左移）。
    //   底框 %HpBackground（用户追加规格）：「血条红色区域（正常生命 0~MaxHp）有多长，
    //   底框就有多长，橙色区域始终在底框之外」——底框右端对齐 MaxHp 位置
    //   （OffsetRight = 5 + maxHpEdge - W），ratio≤2 时底框保持 1x；ratio>2 压缩时底框
    //   随 MaxHp 位置同步左移压缩。
    //   不超额时完全恢复原版（容器偏移/基准/底框/隐藏超额段/恢复树序）。
    // （顶部数字仍显示真实值，如 70/60）
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(NHealthBar), "GetFgWidth",
        new Type[] { typeof(int), typeof(float) })]
    public static class NHealthBar_GetFgWidth_DevilPatch
    {
        /// <summary>
        /// 游戏公式：GetFgWidth(amount) = amount / MaxHp * maxFgWidth（分母固定 MaxHp）。
        /// 恶魔超额时血条容量 = CurrentHp。若分母仍是 MaxHp，毒区 OffsetLeft
        /// （= GetFgWidth(CurrentHp-毒)）会被放大基准推到容器右侧外 → 毒区不可见/错位；
        /// 而 Doom（左端区）又需要放大基准。二者矛盾 → 统一改用 CurrentHp 当分母，
        /// 让前景/毒/Doom 比例全部基于 CurrentHp 容量（配合 Prefix 已扩展的容器宽）。
        /// 不超额 / 恶魔关闭 → 走原逻辑（分母 MaxHp）。
        /// </summary>
        [HarmonyPrefix]
        static bool Prefix(NHealthBar __instance, int amount, float maxFgWidth, ref float __result)
        {
            Creature? creature = Traverse.Create(__instance).Field("_creature").GetValue<Creature>();
            if (creature is not { IsPlayer: true } || !ShouldApply())
                return true;
            if (creature.MaxHp <= 0 || creature.CurrentHp <= creature.MaxHp)
                return true;   // 不超额 → 原逻辑（分母 MaxHp）

            float val = (float)amount / creature.CurrentHp * maxFgWidth;
            __result = Math.Max(val, creature.CurrentHp > 0 ? 12f : 0f);
            return false;   // 跳过原实现（分母改 CurrentHp）
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // 血条数字变色（用户追加需求，后改只变描边）：玩家持有超额生命时，血量数字
    // （%HpLabel）只变描边色（暖橙深色 7A3B00），字色保持游戏逻辑色（doom 紫 /
    // poison 绿 / 默认奶油）。复用原版 RefreshText 的 AddThemeColorOverride 机制：
    // 游戏每次无条件设置 font_color / font_outline_color，Postfix 在它之后只覆盖
    // font_outline_color（后设置者生效）→ 超额时描边橙；不超额走原版逻辑
    // （游戏每次都会重新设置颜色，自动恢复，无需额外处理）。
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(NHealthBar), "RefreshText")]
    public static class NHealthBar_RefreshText_DevilPatch
    {
        [HarmonyPostfix]
        static void Postfix(NHealthBar __instance)
        {
            Creature? creature = Traverse.Create(__instance).Field("_creature").GetValue<Creature>();
            if (creature is not { IsPlayer: true } || !ShouldApply())
                return;
            if (creature.CurrentHp <= 0 || creature.CurrentHp <= creature.MaxHp)
                return;   // 死亡 / 不超额 → 原版逻辑（doom 紫 / poison 绿 / 默认）

            if (__instance.GetNodeOrNull<Label>("%HpLabel") is Label hpLabel)
            {
                // 只变描边为暖橙深色，字色保持游戏逻辑（doom 紫 / poison 绿 / 默认奶油）
                hpLabel.AddThemeColorOverride("font_outline_color", DevilOverflowOutlineColor);
            }
        }
    }

    [HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
    public static class NHealthBar_RefreshForeground_DevilPatch
    {
        /// <summary>%HpForegroundContainer 原始右边缘偏移（health_bar.tscn 固定 -5）。</summary>
        private const float FgContainerDefaultOffsetRight = -5f;

        /// <summary>%HpBackground 原始右边缘偏移（health_bar.tscn 固定 -1）。</summary>
        private const float BgDefaultOffsetRight = -1f;

        [HarmonyPrefix]
        static void Prefix(NHealthBar __instance, out float __state)
        {
            __state = 0f;   // >0 表示超额（值为 ratio），供 Postfix 用
            Creature? creature = Traverse.Create(__instance).Field("_creature").GetValue<Creature>();
            if (creature is not { IsPlayer: true } || !ShouldApply())
                return;
            if (creature.MaxHp <= 0)
                return;

            float W = __instance.HpBarContainer.Size.X;
            float baseFgW = W - 10f;   // %HpForegroundContainer 原宽（offset 5/-5）
            Control? fgContainer = __instance.GetNodeOrNull<Control>("%HpForegroundContainer");
            Traverse traverse = Traverse.Create(__instance);

            if (creature.CurrentHp <= creature.MaxHp)
            {
                // 不超额 → 恢复原基准（避免上一帧超额残留）
                if (fgContainer != null)
                    fgContainer.OffsetRight = FgContainerDefaultOffsetRight;
                traverse.Field("_expectedMaxFgWidth").SetValue(baseFgW);
                return;
            }

            // 超额：用生命值当 maxHp 重新计算血条大小；超过 2 倍时整体压缩封顶（血条最长 2×）
            float ratio = (float)creature.CurrentHp / creature.MaxHp;
            __state = ratio;
            float newFgW = baseFgW * Math.Min(ratio, 2f);
            if (fgContainer != null)
                fgContainer.OffsetRight = newFgW - (W - 5f);
            traverse.Field("_expectedMaxFgWidth").SetValue(newFgW);
        }

        /// <summary>
        /// 超额生命 shader：给 %HpForeground 挂上，把右端（超出最大生命值的部分）染暖橙，
        /// 不新增节点/不改子条顺序（毒/Doom 保持原版层级与接壤形状）。.gdshader 资源文件
        /// （运行时 new Shader{Code} 不生效，必须用资源 + GD.Load）。
        /// </summary>
        private static readonly Shader DevilOverflowShader =
            GD.Load<Shader>("res://shaders/devil_hp_overflow.gdshader");

        /// <summary>每个血条前景对应的超额 shader material（避免重复创建）。节点销毁后自动回收。</summary>
        private static readonly ConditionalWeakTable<NHealthBar, ShaderMaterial> DevilOverflowMaterials = new();

        [HarmonyPostfix]
        static void Postfix(NHealthBar __instance, float __state)
        {
            if (__state <= 0f)
            {
                // 不超额 → 移除超额 shader + 恢复底框原始宽度（完全原版）
                RestoreOverflowShader(__instance);
                if (__instance.GetNodeOrNull<Control>("%HpBarContainer/HpBackground") is Control hpBg)
                    hpBg.OffsetRight = BgDefaultOffsetRight;
                return;
            }

            // 底框 = 红色区（正常生命 0~MaxHp）长度：右端对齐 MaxHp 位置（绝对坐标 5+maxHpEdge），
            // 橙色超额段始终在底框之外；ratio≤2 底框保持 1x；ratio>2 压缩时随 MaxHp 位置左移。
            float ratio = __state;
            float W = __instance.HpBarContainer.Size.X;
            float baseFgW = W - 10f;
            float maxHpEdge = baseFgW * Math.Min(1f, 2f / ratio);   // MaxHp 位置（容器坐标）
            if (__instance.GetNodeOrNull<Control>("%HpBarContainer/HpBackground") is Control hpBg2)
                hpBg2.OffsetRight = 5f + maxHpEdge - W;

            // 超额生命 shader：%HpForeground 右端（超出最大生命值的部分）染暖橙
            ApplyOverflowShader(__instance, maxHpEdge);
        }

        /// <summary>
        /// 给 %HpForeground 挂「超额生命」shader：右端（local_pos.x &gt; maxHpEdge）染暖橙，
        /// 左端保持原前景色（跟随游戏 SelfModulate）。不新增节点、不改子条顺序，
        /// 毒/Doom 在原版层级 → 毒与橙色接壤处保持原版尖角形状。
        /// maxHpEdge 为节点局部像素坐标（前景左端=容器左端，0 起）。
        /// </summary>
        private static void ApplyOverflowShader(NHealthBar bar, float maxHpEdge)
        {
            Control? hpFg = bar.GetNodeOrNull<Control>("%HpForeground");
            if (hpFg == null || DevilOverflowShader == null)
                return;

            if (!DevilOverflowMaterials.TryGetValue(bar, out ShaderMaterial? mat))
            {
                mat = new ShaderMaterial { Shader = DevilOverflowShader };
                DevilOverflowMaterials.Add(bar, mat);
            }

            mat.SetShaderParameter("max_hp_edge", maxHpEdge);
            hpFg.Material = mat;
        }

        /// <summary>移除 %HpForeground 的超额 shader（不超额时完全恢复原版）。</summary>
        private static void RestoreOverflowShader(NHealthBar bar)
        {
            if (bar.GetNodeOrNull<Control>("%HpForeground") is Control hpFg)
                hpFg.Material = null;
            DevilOverflowMaterials.Remove(bar);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 6: 休息(Heal)选项详细描述后追加「失去6点最大生命值。」
    // 目标：public static IReadOnlyList<LocString> ModifyExtraRestSiteHealText(
    //         IRunState, Player, IReadOnlyList<LocString>)
    // 该方法只被 HealRestSiteOption.Description 调用（生成休息选项的详细描述时），
    // 参考 NightTerrors.ModifyExtraRestSiteHealText；恶魔开启时在列表末尾追加一行。
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyExtraRestSiteHealText))]
    public static class Hook_ModifyExtraRestSiteHealText_DevilPatch
    {
        [HarmonyPostfix]
        static void Postfix(ref IReadOnlyList<LocString> __result)
        {
            if (!ShouldApply())
                return;

            var line = new LocString("gameplay_ui", "BAL_CFW_DEVIL_REST_HEAL_DESC");
            __result = __result.Append(line).ToArray();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 7: 休息(Heal)选项图标正上方显示恶魔逆小图标
    // 目标：public override void _Ready()（NRestSiteButton : NButton，休息处选项按钮）
    // 仅对休息(Heal)选项（HealRestSiteOption）生效；恶魔小图标挂在 %Visuals 下
    // （跟随按钮 hover 缩放动画），水平居中于选项图标、紧贴图标顶部上方。
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(NRestSiteButton), nameof(NRestSiteButton._Ready))]
    public static class NRestSiteButton_Ready_DevilPatch
    {
        /// <summary>恶魔逆小图标资源路径（附魔图标，与地图逆位标记同一套资源）。</summary>
        private const string DevilReversedIconPath =
            "res://images/enchantments/tar_devil_reversed_enchantment.png";

        /// <summary>恶魔逆小图标边长（不用太大）。</summary>
        private const float DevilIconSize = 32f;

        /// <summary>恶魔逆小图标底部距休息(Heal)选项图标顶部的距离。</summary>
        private const float DevilIconGap = 4f;

        /// <summary>恶魔逆小图标再往右偏移量（用户追加：显示在选项右上角）。</summary>
        private const float DevilIconRightOffset = 54f;

        [HarmonyPostfix]
        static void Postfix(NRestSiteButton __instance)
        {
            if (__instance.Option is not HealRestSiteOption)
                return;
            if (!ShouldApply())
                return;

            AddDevilIconAboveIcon(__instance);
        }

        private static void AddDevilIconAboveIcon(NRestSiteButton btn)
        {
            Control? visuals = Traverse.Create(btn).Field("_visuals").GetValue<Control>();
            TextureRect? icon = Traverse.Create(btn).Field("_icon").GetValue<TextureRect>();
            if (visuals == null || icon == null)
                return;

            var devilIcon = new TextureRect
            {
                Texture = GD.Load<Texture2D>(DevilReversedIconPath),
                Size = new Vector2(DevilIconSize, DevilIconSize),
                Position = new Vector2(
                    icon.Position.X + icon.Size.X / 2f - DevilIconSize / 2f + DevilIconRightOffset,
                    icon.Position.Y - DevilIconSize - DevilIconGap),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            visuals.AddChild(devilIcon);
        }
    }
}
