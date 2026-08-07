// PengoTarot/Patch/enchantments/HandCardHolder_EnchantmentIconPatch.cs
#nullable enable
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace PengoTarot.Patches;

/// <summary>
/// 在手牌卡牌的 HandIndex 标签位置添加星球附魔图标。
/// 
/// 挂在每个 NHandCardHolder 上的 TextureRect，位置与 %HandIndex 重叠。
/// 利用 AddChild 添加到末尾的自然层级（后添加的子节点在上层绘制）
/// 即可显示在数字上方，无需额外 ZIndex 以免盖住系统覆盖层。
/// 图标在 SetIndexLabel 时根据卡牌是否有星球附魔更新可见性和纹理。
/// </summary>
[HarmonyPatch]
public static class HandCardHolder_EnchantmentIconPatch
{
    private const string IconNodeName = "PlanetEnchantmentIcon";

    /// <summary>
    /// HandIndex 标签在 hand_card_holder.tscn 中的原始偏移值。
    /// 图标放大 1.5 倍，以相同中心点扩张。
    /// 原始区域：X[-32,32] Y[-288,-224] → 64×64
    /// 放大后：X[-48,48] Y[-304,-208] → 96×96
    /// </summary>
    private const float IconTop = -304f;
    private const float IconBottom = -208f;
    private const float IconLeft = -48f;
    private const float IconRight = 48f;

    [HarmonyPatch(typeof(NHandCardHolder), "_Ready"), HarmonyPostfix]
    private static void OnReady(NHandCardHolder __instance)
    {
        // 避免重复添加（pool 复用场景）
        if (__instance.FindChild(IconNodeName, recursive: false, owned: false) != null)
            return;

        var icon = new TextureRect
        {
            Name = IconNodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            // 居中锚点，放大的区域
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
            // 不设 ZIndex，靠 AddChild 的末尾顺序自然在数字上层
            Visible = false,
        };

        __instance.AddChild(icon);
    }

    [HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.SetIndexLabel)), HarmonyPostfix]
    private static void SetIndexLabel_Postfix(NHandCardHolder __instance)
    {
        var icon = __instance.FindChild(IconNodeName, recursive: false, owned: false) as TextureRect;
        if (icon == null)
            return;

        var enchantment = __instance.CardModel?.Enchantment;
        if (enchantment != null && enchantment.GetType().Name.StartsWith("Planet"))
        {
            icon.Texture = enchantment.Icon;
            icon.Visible = true;
        }
        else
        {
            icon.Visible = false;
        }
    }
}
