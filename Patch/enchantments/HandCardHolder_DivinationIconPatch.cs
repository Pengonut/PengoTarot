// PengoTarot/Patch/enchantments/HandCardHolder_DivinationIconPatch.cs
#nullable enable
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using PengoTarot.Data.Divination;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

/// <summary>
/// 在手牌卡牌的右上角显示普通房占卜的逆位小图标。
///
/// 模仿 HandCardHolder_EnchantmentIconPatch（星球附魔图标）的实现：
/// 挂在每个 NHandCardHolder 上的 TextureRect，靠 AddChild 末尾顺序在上层绘制，不设 ZIndex。
/// 更新时机：
///  - _Ready：创建图标节点；
///  - SetIndexLabel：卡牌绑定/索引变化；
///  - UpdateCard：从玩家 Power 与确定性战斗历史实时推导，不占用卡牌唯一的 Affliction 槽。
/// </summary>
[HarmonyPatch]
public static class HandCardHolder_DivinationIconPatch
{
    private const string IconNodeName = "DivinationAfflictionIcon";
    private static Texture2D? _justiceTexture;
    private static Texture2D? _hangedManTexture;
    private static Texture2D? _deathTexture;

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
        UpdateIcon(__instance);
    }

    private static void UpdateIcon(NHandCardHolder holder)
    {
        // CardModel 的事件可能与节点退出树/释放发生在同一帧。不要再访问已经释放的
        // Godot 实例，否则 FindChild 会抛 ObjectDisposedException 并中断战斗回合循环。
        if (!GodotObject.IsInstanceValid(holder) || !holder.IsInsideTree())
            return;

        var icon = holder.FindChild(IconNodeName, recursive: false, owned: false) as TextureRect;
        if (icon == null)
            return;

        string? path = null;
        var card = holder.CardModel;
        var creature = card?.Owner.Creature;
        if (card?.Type == CardType.Attack
            && creature?.GetPower<TarJusticeReversedPower>() != null
            && !NormalDivinationTurnState.HasPlayedCardThisTurn(creature, CardType.Attack))
            path = "res://images/enchantments/tar_justice_reversed_enchantment.png";
        else if (card?.Type == CardType.Skill
            && creature?.GetPower<TarHangedManReversedPower>() != null
            && !NormalDivinationTurnState.HasPlayedCardThisTurn(creature, CardType.Skill))
            path = "res://images/enchantments/tar_hanged_man_reversed_enchantment.png";
        else if (card?.Type == CardType.Power
            && creature?.GetPower<TarDeathReversedPower>() != null)
            path = "res://images/enchantments/tar_death_reversed_enchantment.png";

        if (path != null)
        {
            icon.Texture = path switch
            {
                "res://images/enchantments/tar_justice_reversed_enchantment.png"
                    => _justiceTexture ??= GD.Load<Texture2D>(path),
                "res://images/enchantments/tar_hanged_man_reversed_enchantment.png"
                    => _hangedManTexture ??= GD.Load<Texture2D>(path),
                _ => _deathTexture ??= GD.Load<Texture2D>(path),
            };
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

    [HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard)), HarmonyPostfix]
    private static void UpdateCard_Postfix(NHandCardHolder __instance)
        => UpdateIcon(__instance);

    // 保留原版 Flash 路径作为额外刷新时机。
    [HarmonyPatch(typeof(NHandCardHolder), "Flash"), HarmonyPostfix]
    private static void Flash_Postfix(NHandCardHolder __instance)
        => UpdateIcon(__instance);

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
        if (card?.Type == CardType.Power
            && card.Owner.Creature.GetPower<TarDeathReversedPower>() != null
            && card.CanPlay())
            __result = true;
    }
}
