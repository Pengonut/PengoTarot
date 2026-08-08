// PengoTarot/Patch/enchantments/HandCardHolder_DivinationIconPatch.cs
#nullable enable
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using PengoTarot.Models.Afflictions;

namespace PengoTarot.Patches;

/// <summary>
/// 在手牌卡牌的右上角，为「占卜侵蚀」的卡牌显示对应的逆附魔小图标（正义-逆 / 倒吊人-逆）。
///
/// 模仿 HandCardHolder_EnchantmentIconPatch（星球附魔图标）的实现：
/// 挂在每个 NHandCardHolder 上的 TextureRect，靠 AddChild 末尾顺序在上层绘制，不设 ZIndex。
/// 更新时机：
///  - _Ready：创建图标节点；
///  - SetIndexLabel：卡牌绑定/索引变化；
///  - Flash：卡牌 Affliction/关键词等变化（NHandCardHolder 订阅了 AfflictionChanged += Flash），
///    覆盖战斗开始时给卡牌上侵蚀后图标出现。
///
/// 侵蚀的 Affliction 不提供 overlay 场景（HasOverlay=false），卡牌 UI 走默认 overlay，无缺特效报错；
/// 本 patch 提供视觉提示。
/// </summary>
[HarmonyPatch]
public static class HandCardHolder_DivinationIconPatch
{
    private const string IconNodeName = "DivinationAfflictionIcon";

    /// <summary>
    /// 右上角区域（锚点中心，相对 NHandCardHolder）：
    /// 与 HandIndex（顶部中央 X[-32,32] Y[-288,-224]）同高、镜像到右侧，64×64。
    /// 卡牌区域 X[-150,150] Y[-211,211]，图标略超出右缘以贴合右上角。
    /// </summary>
    private const float IconLeft = 88f;
    private const float IconTop = -288f;
    private const float IconRight = 152f;
    private const float IconBottom = -224f;

    [HarmonyPatch(typeof(NHandCardHolder), "_Ready"), HarmonyPostfix]
    private static void OnReady(NHandCardHolder __instance)
    {
        // 补订阅：原版 SubscribeToEvents 在 SetCard（节点尚未进树）时因 IsInsideTree() 为 false 而订阅失败，
        // 导致清除侵蚀时 AfflictionChanged → Flash 链路断裂、右上角图标不消失。
        // 此处进树后（_Ready）补订阅 Flash，确保侵蚀变化（上牌/清除）时图标即时刷新。
        var card = __instance.CardModel;
        if (card != null)
            card.AfflictionChanged += __instance.Flash;

        // 避免重复添加（pool 复用场景）
        if (__instance.FindChild(IconNodeName, recursive: false, owned: false) != null)
            return;

        var icon = new TextureRect
        {
            Name = IconNodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            // 居中锚点 + 右上角偏移
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = IconLeft,
            OffsetTop = IconTop,
            OffsetRight = IconRight,
            OffsetBottom = IconBottom,
            GrowHorizontal = Control.GrowDirection.Both,
            GrowVertical = Control.GrowDirection.Both,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            // 不设 ZIndex，靠 AddChild 末尾顺序自然在卡牌上层
            Visible = false,
        };

        __instance.AddChild(icon);
    }

    private static void UpdateIcon(NHandCardHolder holder)
    {
        var icon = holder.FindChild(IconNodeName, recursive: false, owned: false) as TextureRect;
        if (icon == null)
            return;

        string? path = null;
        var affliction = holder.CardModel?.Affliction;
        if (affliction is TarJusticeReversedAffliction)
            path = "res://images/enchantments/tar_justice_reversed_enchantment.png";
        else if (affliction is TarHangedManReversedAffliction)
            path = "res://images/enchantments/tar_hanged_man_reversed_enchantment.png";
        else if (affliction is TarDeathReversedAffliction)
            path = "res://images/enchantments/tar_death_reversed_enchantment.png";

        if (path != null)
        {
            icon.Texture = GD.Load<Texture2D>(path);
            icon.Visible = true;
        }
        else
        {
            icon.Visible = false;
        }
    }

    [HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.SetIndexLabel)), HarmonyPostfix]
    private static void SetIndexLabel_Postfix(NHandCardHolder __instance)
        => UpdateIcon(__instance);

    // Flash 是 NHandCardHolder 私有方法，NHandCardHolder 订阅了 card.AfflictionChanged += Flash，
    // 故侵蚀上牌/清除时 Flash 会被调用，借此刷新右上角图标。
    [HarmonyPatch(typeof(NHandCardHolder), "Flash"), HarmonyPostfix]
    private static void Flash_Postfix(NHandCardHolder __instance)
        => UpdateIcon(__instance);

    // _ExitTree：取消 OnReady 补订阅，避免卡牌 Affliction 变化仍引用已出树的 holder。
    [HarmonyPatch(typeof(NHandCardHolder), "_ExitTree"), HarmonyPostfix]
    private static void OnExitTree(NHandCardHolder __instance)
    {
        var card = __instance.CardModel;
        if (card != null)
            card.AfflictionChanged -= __instance.Flash;
    }

    // Clear（pool 复用）：与 _ExitTree 相同，取消补订阅。
    [HarmonyPatch(typeof(NHandCardHolder), "Clear"), HarmonyPostfix]
    private static void Clear_Postfix(NHandCardHolder __instance)
    {
        var card = __instance.CardModel;
        if (card != null)
            card.AfflictionChanged -= __instance.Flash;
    }
}

/// <summary>
/// 死神标记的能力牌：可打出时高亮提示变红（替换默认可打出的蓝色高亮）。
/// NHandCardHolder 高亮更新里 ShouldGlowRed 优先于默认可打出颜色（NCardHighlight.red）。
/// </summary>
[HarmonyPatch(typeof(NHandCardHolder), "get_ShouldGlowRed")]
public static class NHandCardHolder_ShouldGlowRed_DeathPatch
{
    static void Postfix(NHandCardHolder __instance, ref bool __result)
    {
        if (__result) return;
        var card = __instance.CardNode?.Model;
        if (card?.Affliction is TarDeathReversedAffliction && card.CanPlay())
            __result = true;
    }
}
