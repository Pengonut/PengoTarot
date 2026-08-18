#nullable enable

using System;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch.Card;

/// <summary>
/// 月亮（Moon, 索引18）结束回合按钮的「预测」视觉：当「结束本回合后，下一次起始抽牌会触发月亮
/// 洗牌效果」时，在结束回合按钮上方显示一个不透明度循环闪烁的月亮逆小图标，提示玩家。
///
/// 预测 = 精确模拟下一次起始抽牌（<see cref="CombatManager.SetupPlayerTurn"/> +
/// <see cref="CardPileCmd.Draw"/> 的判定逻辑，只依赖张数、与牌序无关，故可确定性计算）：
///   ① 结束回合后的起始手牌数 = 保留牌数（非保留手牌在结束回合时冲入弃牌堆）；
///   ② 下回合起始抽牌数 = <see cref="Hook.ModifyHandDraw"/>（真实钩子，覆盖 MindRot/加抽等）；
///      且受 <see cref="CardPile.MaxCardsInHand"/> 容量封顶；
///   ③ 洗牌时可洗量 = 弃牌堆 + 结束回合冲入的手牌；
///   ④ 触发判定（与 Draw 内 ShuffleIfNecessary 一致）：抽牌堆张数 &lt; 抽牌数 且 可洗量 &gt; 0。
///
/// 更新时机：挂在 <see cref="NEndTurnButton.OnCombatStateChanged"/>（按钮本就订阅了
/// CombatStateTracker.CombatStateChanged），它在「回合开始」和「牌堆内容变化（卡牌移动）」时都会
/// 触发 → 正好覆盖需求，无每帧运算、无额外事件订阅。
///
/// 仅当「月亮开启 且 一局进行中 且 玩家回合 且 本地玩家存活」时显示。
/// </summary>
public static class NEndTurnButton_MoonPredictIconPatch
{
    /// <summary>Moon 在 FlagNames 中的索引。</summary>
    private const int MoonFlagIndex = 18;

    /// <summary>月亮逆小图标路径（月亮不在标记系统 IconPaths 中，用逆附魔小图标）。</summary>
    private const string MoonReversedIconPath = "res://images/enchantments/tar_moon_reversed_enchantment.png";

    /// <summary>图标节点名（FindChild 按名查找，避免重复创建）。</summary>
    private const string IconNodeName = "MoonPredictIcon";

    /// <summary>图标边长（px，KeepAspectCentered 等比缩放）。</summary>
    private const float IconSize = 56f;

    /// <summary>图标中心相对按钮中心的垂直上移量（px，图标位于按钮上方）。</summary>
    private const float IconCenterYAbove = -60f;

    /// <summary>闪烁透明度下限（不透明度在 [MinAlpha, 1] 间循环）。</summary>
    private const float MinAlpha = 0.2f;

    /// <summary>闪烁半周期（s，一次淡入或淡出时长）。</summary>
    private const float BlinkHalfPeriod = 0.65f;

    /// <summary>是否应显示：配置开启 且 一局进行中 且 战斗进行中（主菜单/图鉴不生效）。</summary>
    private static bool ShouldApply()
        => ConfigFloatingWindowRunData.GetTarFlag(MoonFlagIndex)
           && RunManager.Instance.IsInProgress
           && CombatManager.Instance != null
           && CombatManager.Instance.IsInProgress
           && !CombatManager.Instance.IsOverOrEnding;

    /// <summary>
    /// 创建图标节点 + 循环不透明度闪烁 Tween。重复 _Ready（pool 复用）时按名查找防重复。
    /// </summary>
    [HarmonyPatch(typeof(NEndTurnButton), "_Ready")]
    public static class NEndTurnButton_Ready_MoonPatch
    {
        [HarmonyPostfix]
        static void Postfix(NEndTurnButton __instance)
        {
            if (__instance.FindChild(IconNodeName, recursive: false, owned: false) != null)
                return; // 已创建（防重复）

            var icon = new TextureRect
            {
                Name = IconNodeName,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                // 居中锚点 + 按钮上方偏移
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -IconSize / 2f,
                OffsetTop = IconCenterYAbove - IconSize / 2f,
                OffsetRight = IconSize / 2f,
                OffsetBottom = IconCenterYAbove + IconSize / 2f,
                GrowHorizontal = Control.GrowDirection.Both,
                GrowVertical = Control.GrowDirection.Both,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Texture = GD.Load<Texture2D>(MoonReversedIconPath),
                Visible = false,
            };
            __instance.AddChild(icon);

            // 循环不透明度闪烁（淡出↔淡入）
            Tween tween = icon.CreateTween().SetLoops();
            tween.TweenProperty(icon, "modulate:a", MinAlpha, BlinkHalfPeriod)
                .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(icon, "modulate:a", 1f, BlinkHalfPeriod)
                .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        }
    }

    /// <summary>
    /// 每帧不需要：挂在按钮已有的 CombatStateChanged 处理器上（回合开始 + 卡牌移动时触发），
    /// 重新求值预测并显示/隐藏图标。
    /// </summary>
    [HarmonyPatch(typeof(NEndTurnButton), "OnCombatStateChanged")]
    public static class NEndTurnButton_OnCombatStateChanged_MoonPatch
    {
        [HarmonyPostfix]
        static void Postfix(NEndTurnButton __instance)
            => UpdateIcon(__instance);
    }

    /// <summary>按当前预测刷新图标可见性（显示时把闪烁相位重置到亮起）。</summary>
    internal static void UpdateIcon(NEndTurnButton button)
    {
        if (button.FindChild(IconNodeName, recursive: false, owned: false) is not TextureRect icon)
            return; // _Ready 未执行/按钮异常，不处理

        bool show = ShouldShowMoonPredictIcon(button);
        if (icon.Visible == show)
            return;
        icon.Visible = show;
        if (show)
            icon.Modulate = new Color(1f, 1f, 1f, 1f); // 显示时从亮起开始闪烁
    }

    /// <summary>
    /// 精确模拟「结束本回合后，下一次起始抽牌是否触发月亮洗牌效果」。
    /// </summary>
    private static bool ShouldShowMoonPredictIcon(NEndTurnButton button)
    {
        if (!ShouldApply())
            return false;

        // 战斗状态（NEndTurnButton.Initialize 时设置，整场战斗同一对象）
        CombatState? state = Traverse.Create(button).Field("_combatState").GetValue<CombatState>();
        if (state == null || state.CurrentSide != CombatSide.Player)
            return false;

        Player? player = LocalContext.GetMe(state);
        if (player == null || !player.Creature.IsAlive)
            return false;

        CardPile hand = PileType.Hand.GetPile(player);
        CardPile draw = PileType.Draw.GetPile(player);
        CardPile discard = PileType.Discard.GetPile(player);

        // ① 结束回合后的起始手牌数 = 保留牌数（非保留手牌冲入弃牌堆）
        bool flush = Hook.ShouldFlush(state, player);
        int retained = 0;
        foreach (CardModel card in hand.Cards)
        {
            if (!flush || card.ShouldRetainThisTurn)
                retained++;
        }

        // ② 下回合起始抽牌数（真实 ModifyHandDraw 钩子），且受手牌容量封顶
        decimal handDraw = Hook.ModifyHandDraw(state, player, CombatManager.baseHandDrawCount, out _);
        int drawable = (int)Math.Min(handDraw, Math.Max(0, CardPile.MaxCardsInHand - retained));
        if (drawable <= 0)
            return false;

        // ③ 洗牌时可洗量 = 弃牌堆 + 结束回合冲入的手牌
        int discardAfterFlush = discard.Cards.Count + (hand.Cards.Count - retained);

        // ④ 抽牌堆会被抽空 且 有牌可洗 → 触发洗牌 → 月亮生效
        return draw.Cards.Count < drawable && discardAfterFlush > 0;
    }
}
