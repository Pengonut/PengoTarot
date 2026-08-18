#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch.Card;

/// <summary>
/// 高塔（Tower, 索引16）难度开关效果：
/// 1. 进阶之灾可以打出（费用为 -1 原始值）
/// 2. 进阶之灾描述添加「消耗时消耗所有手牌」
/// 3. 进阶之灾被消耗时，同时消耗本名玩家的所有手牌
/// 4. 游戏中进阶之灾可以绕过诅咒牌限制被附魔（战斗/非战斗均生效）
///
/// 仅当「配置开启（GetTarFlag(16)）且当前在一局游戏中」时生效
/// （主菜单/图鉴等不在跑局场景不生效）。
/// 词条移除遵循原版 API：CardModel.RemoveKeyword。
/// </summary>
public static class AscendersBaneTowerPatch
{
    /// <summary>Tower 在 FlagNames 中的索引。</summary>
    private const int TowerFlagIndex = 16;

    /// <summary>
    /// 是否应生效：配置开启 且 当前在一局游戏中（主菜单/图鉴不生效）。
    /// internal 供 AscendersBaneEndTurnWarningPatch（结束回合预警）复用。
    /// </summary>
    internal static bool ShouldApply()
        => ConfigFloatingWindowRunData.GetTarFlag(TowerFlagIndex)
           && RunManager.Instance.IsInProgress;

    // ═══════════════════════════════════════════════════════════════
    // Patch 1: 使进阶之灾可打出（CanPlay）
    // CanPlay 有两个重载，必须显式指定空参数列表，否则 AmbiguousMatchException。
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(CardModel), "CanPlay", new Type[0])]
    public static class CardModel_CanPlay_TowerPatch
    {
        [HarmonyPostfix]
        static void Postfix(CardModel __instance, ref bool __result)
        {
            if (__instance is not AscendersBane || !ShouldApply())
                return;

            // 防御性：即使词条已移除，费用 -1 也可能被 HasEnoughResourcesFor 判为不足
            __result = true;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 2: 修改卡牌描述文本 —— 移除 Unplayable + 追加「消耗时消耗所有手牌」
    // 目标：public string GetDescriptionForPile(PileType, Creature?)
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(CardModel), "GetDescriptionForPile",
        typeof(PileType), typeof(Creature))]
    public static class CardModel_GetDescriptionForPile_TowerPatch
    {
        /// <summary>
        /// 生成描述前，对可变实例用原版 API 移除 Unplayable 词条（幂等）。
        /// 这样描述（及 HoverTip）自动不含「不可打出」。
        /// canonical 实例不可变会抛异常，跳过（仅处理战斗中/牌组查看的 mutable 实例）。
        /// </summary>
        [HarmonyPrefix]
        static void Prefix(CardModel __instance)
        {
            if (__instance is not AscendersBane || !ShouldApply())
                return;
            if (!__instance.IsMutable)
                return;

            __instance.RemoveKeyword(CardKeyword.Unplayable);
        }

        [HarmonyPostfix]
        static void Postfix(CardModel __instance, ref string __result)
        {
            if (__instance is not AscendersBane || !ShouldApply())
                return;

            var extra = LocString.GetIfExists("gameplay_ui", "BAL_CFW_TOWER_CARD_DESC");
            if (extra == null)
                return;

            string extraText = extra.GetFormattedText();
            if (!string.IsNullOrEmpty(extraText))
            {
                __result += "\n" + extraText;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 3: 进阶之灾被消耗时，同时消耗本名玩家的所有手牌
    //
    // 遵循游戏原版模式（DrumOfBattle / Midnight）：override AfterCardExhausted，
    // 在 card == this 时触发。进阶之灾未 override 该方法，走 AbstractModel 基类
    // （空实现 Task.CompletedTask），故在此补丁拦截即可，不影响其他 override 的卡。
    //
    // 消耗手牌方式参照 Stoke / 回合结束 Ethereal：快照手牌 + 逐张 await
    // CardCmd.Exhaust（官方明确禁止批量版，会破坏 DarkEmbrace/JossPaper 等 hook 顺序）。
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(AbstractModel), "AfterCardExhausted")]
    public static class AbstractModel_AfterCardExhausted_TowerPatch
    {
        [HarmonyPrefix]
        static bool Prefix(
            ref Task __result,
            AbstractModel __instance,
            PlayerChoiceContext choiceContext,
            CardModel card,
            bool causedByEthereal)
        {
            if (__instance is not AscendersBane || card != __instance)
                return true;
            if (!ShouldApply())
                return true;

            __result = ExhaustAllHandAsync(choiceContext, card);
            return false;
        }

        /// <summary>逐张消耗该玩家所有手牌（参考 Stoke / 回合结束 Ethereal）。</summary>
        private static async Task ExhaustAllHandAsync(
            PlayerChoiceContext choiceContext,
            CardModel ascendersBane)
        {
            Player? owner = ascendersBane.Owner;
            var hand = owner?.PlayerCombatState?.Hand?.Cards?.ToList();
            if (hand == null || hand.Count == 0)
                return;

            foreach (CardModel handCard in hand)
            {
                await CardCmd.Exhaust(choiceContext, handCard);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Patch 4: 游戏中允许进阶之灾被附魔（绕过诅咒牌限制）
    //
    // 附魔资格的唯一裁决点是 EnchantmentModel.CanEnchant(CardModel)。
    // 进阶之灾会被两道门拦下：
    //   1. 关卡1：CardType.Curse 被 (uint)(type - 4) <= 2u 封杀
    //   2. 关卡2：牌组里带 Unplayable 关键词的卡被拒绝
    // 用 Prefix 跳过原方法直接返回 true，可同时绕过这两处。
    // 条件与 ShouldApply() 一致：配置开启 且 在一局游戏中（战斗中与非战斗的
    // 牌组附魔/事件/商店塔罗都生效）。
    // ═══════════════════════════════════════════════════════════════

    [HarmonyPatch(typeof(EnchantmentModel), nameof(EnchantmentModel.CanEnchant))]
    public static class EnchantmentModel_CanEnchant_TowerPatch
    {
        [HarmonyPrefix]
        static bool Prefix(EnchantmentModel __instance, CardModel card, ref bool __result)
        {
            if (card is not AscendersBane)
                return true; // 非进阶之灾 → 正常运行原方法
            if (!ShouldApply())
                return true; // 配置未开启或不在游戏中 → 正常运行原方法

            // 跳过原方法（含 Curse/Unplayable 检查），直接允许附魔
            __result = true;
            return false;
        }
    }
}
