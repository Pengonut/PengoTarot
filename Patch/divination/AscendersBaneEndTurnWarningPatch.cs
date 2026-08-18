// PengoTarot/Patch/divination/AscendersBaneEndTurnWarningPatch.cs
#nullable enable

using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace PengoTarot.Patch.Card;

/// <summary>
/// 高塔（Tower）开关的「进阶之灾烧牌」预警：
/// 玩家把鼠标移到「结束回合」按钮上时，若手牌中存在带「虚无」（Ethereal）的进阶之灾，
/// 则给所有手牌刷上红色高亮（NCardHighlight.red），并让进阶之灾持续闪烁，
/// 预警「点击结束回合会烧掉进阶之灾，并连锁消耗所有手牌」。
///
/// 触发链：
///   NEndTurnButton.OnFocus（鼠标 hover / 手柄焦点）→ 订阅 SceneTree.ProcessFrame 每帧重检；
///   NEndTurnButton.OnUnfocus（离开）→ 取消订阅并恢复所有手牌高亮。
/// 每帧重检条件：进阶之灾被出牌/消耗/离手后红色特效自动消失，无需等 OnUnfocus。
/// 生效条件与 AscendersBaneTowerPatch 一致（配置开启 且 在一局游戏中）。
/// </summary>
[HarmonyPatch]
public static class AscendersBaneEndTurnWarningPatch
{
    /// <summary>闪烁周期（秒）。</summary>
    private const float FlashPeriod = 0.5f;

    /// <summary>闪烁 alpha 范围（下限&gt;0，避免完全不可见）。</summary>
    private const float FlashMinAlpha = 0.15f;
    private const float FlashMaxAlpha = 0.7f;

    private static bool _processHooked;
    private static bool _warningActive;
    private static readonly List<Control> _flashTargets = new();

    // ═══════════════════════════════════════════════════════════════
    // Patch 1: 鼠标 hover / 焦点到结束回合按钮 → 订阅每帧检测
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(NEndTurnButton), "OnFocus")]
    public static class EndTurnButton_OnFocus_WarningPatch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            EnsureProcessHook();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 2: 鼠标离开 / 失去焦点 → 停止预警并恢复所有手牌高亮
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(NEndTurnButton), "OnUnfocus")]
    public static class EndTurnButton_OnUnfocus_WarningPatch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            StopWarning();
        }
    }

    // ═══════════════════════════════════════════════════════════════

    private static void EnsureProcessHook()
    {
        if (_processHooked) return;
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree == null) return;
        tree.ProcessFrame += OnProcessFrame;
        _processHooked = true;
    }

    private static void StopWarning()
    {
        if (_processHooked)
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree != null) tree.ProcessFrame -= OnProcessFrame;
            _processHooked = false;
        }
        DeactivateVisuals();
    }

    /// <summary>每帧检测：条件满足 → 染色 + 闪烁；条件消失 → 恢复；战斗结束 → 彻底停止。</summary>
    private static void OnProcessFrame()
    {
        var hand = NCombatRoom.Instance?.Ui?.Hand;
        if (hand == null)
        {
            // 安全网：战斗结束/离开战斗时按钮随 UI 销毁，OnUnfocus 不一定触发
            DeactivateVisuals();
            StopWarning();
            return;
        }

        if (!AscendersBaneTowerPatch.ShouldApply() || !HasEtherealAscendersBane(hand))
        {
            // 条件暂不满足（进阶之灾已打出/离手）：恢复，但保持订阅等待它回来
            DeactivateVisuals();
            return;
        }

        if (!_warningActive)
            ActivateVisuals(hand);

        ApplyRedTint(hand);
        ApplyFlash();
    }

    /// <summary>手牌中是否存在带「虚无」的进阶之灾。</summary>
    private static bool HasEtherealAscendersBane(NPlayerHand hand)
    {
        foreach (var holder in hand.ActiveHolders)
        {
            var model = holder.CardModel;
            if (model is AscendersBane && model.Keywords.Contains(CardKeyword.Ethereal))
                return true;
        }
        return false;
    }

    /// <summary>首次激活：所有手牌红色高亮淡入，进阶之灾 Flash 节点进入闪烁列表。</summary>
    private static void ActivateVisuals(NPlayerHand hand)
    {
        _warningActive = true;
        _flashTargets.Clear();
        foreach (var holder in hand.ActiveHolders)
        {
            var hl = holder.CardNode?.CardHighlight;
            if (hl == null) continue;
            hl.AnimShow();
            hl.Modulate = NCardHighlight.red;

            if (holder.CardModel is AscendersBane)
            {
                var flash = holder.FindChild("Flash", recursive: false, owned: false) as Control;
                if (flash != null) _flashTargets.Add(flash);
            }
        }
    }

    /// <summary>每帧：把所有手牌高亮强制为红色（覆盖 UpdateCard 可能的重置）。</summary>
    private static void ApplyRedTint(NPlayerHand hand)
    {
        foreach (var holder in hand.ActiveHolders)
        {
            var hl = holder.CardNode?.CardHighlight;
            if (hl == null) continue;
            if (hl.Modulate != NCardHighlight.red)
                hl.AnimShow();
            hl.Modulate = NCardHighlight.red;
        }
    }

    /// <summary>每帧：让进阶之灾的 Flash 节点红色 alpha 正弦摆动，形成持续闪烁。</summary>
    private static void ApplyFlash()
    {
        if (_flashTargets.Count == 0) return;

        float t = Time.GetTicksMsec() / 1000f;
        float pulse = (Mathf.Sin(t * Mathf.Tau / FlashPeriod) + 1f) * 0.5f;
        float alpha = FlashMinAlpha + pulse * (FlashMaxAlpha - FlashMinAlpha);
        var color = new Color(NCardHighlight.red.R, NCardHighlight.red.G, NCardHighlight.red.B, alpha);

        for (int i = _flashTargets.Count - 1; i >= 0; i--)
        {
            var flash = _flashTargets[i];
            if (flash == null || !GodotObject.IsInstanceValid(flash))
            {
                _flashTargets.RemoveAt(i);
                continue;
            }
            flash.Modulate = color;
        }
    }

    /// <summary>停止：恢复所有手牌高亮（按原逻辑重新计算），复位 Flash 节点。</summary>
    private static void DeactivateVisuals()
    {
        if (!_warningActive && _flashTargets.Count == 0) return;

        _warningActive = false;
        _flashTargets.Clear();

        var hand = NCombatRoom.Instance?.Ui?.Hand;
        if (hand == null) return;

        foreach (var holder in hand.ActiveHolders)
        {
            if (holder.CardModel is AscendersBane)
            {
                var flash = holder.FindChild("Flash", recursive: false, owned: false) as Control;
                if (flash != null && GodotObject.IsInstanceValid(flash))
                {
                    var c = flash.Modulate;
                    flash.Modulate = new Color(c.R, c.G, c.B, 0f);
                }
            }
            if (holder.CardNode != null && GodotObject.IsInstanceValid(holder.CardNode))
                holder.UpdateCard();
        }
    }
}
