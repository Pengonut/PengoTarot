#nullable enable

using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// configfloatingwindow 的 UI 文案本地化注入（gameplay_ui 表，BAL_CFW_* 键）。
    /// </summary>
    public static class ConfigFloatingWindowLoc
    {
        private static bool _injected;

        public static void Inject()
        {
            if (_injected) return;
            var loc = LocManager.Instance;
            if (loc == null) return;

            string lang = loc.Language;
            var table = loc.GetTable("gameplay_ui");

            Dictionary<string, string> texts = lang switch
            {
                "zhs" => ChineseTexts,
                "jpn" => JapaneseTexts,
                "kor" => KoreanTexts,
                _ => EnglishTexts
            };

            table.MergeWith(texts);
            _injected = true;
        }

        // ── 配置界面左下角 TIPS 随机池（当前仅中文完整提供；其余语言返回空数组=不显示，可后续补充） ──
        /// <summary>查看配置界面达该次数后，「感谢」内容解锁并入随机池（每次启动清零）。</summary>
        public const int ThanksUnlockOpens = 3;

        /// <summary>当前语言的普通 TIPS（无则空数组）。</summary>
        public static IReadOnlyList<string> GetMainTips(string language) => language switch
        {
            "zhs" => ChineseMainTips,
            _ => EmptyTips,
        };

        /// <summary>当前语言的「感谢/其他」TIPS（无则空数组）。</summary>
        public static IReadOnlyList<string> GetThanksTips(string language) => language switch
        {
            "zhs" => ChineseThanksTips,
            _ => EmptyTips,
        };

        private static readonly string[] EmptyTips = System.Array.Empty<string>();

        private static readonly string[] ChineseMainTips =
        {
            "Mod的配置入口总是会显示在游戏自己的设置界面中。",
            "你可以开启面板下方的按钮来在一局游戏中隐藏配置入口。",
            "塔罗牌卡包跳过时，不会扣除金币，你可以随时返回商店购买心仪的卡牌。",
            "火堆的观星选项跳过时，并不会消耗行动和观星套组的可用次数。",
            "如果塔罗牌和星球牌的总开关都被关闭，Mod会在下次你启动游戏时将自己标记为“不影响游戏玩法”，不过再次开启总开关也需要重启。（这个功能暂时还没有实现！）",
            "如果你想试试在一张牌上多次附魔，不妨试试创意工坊的MultiEnchantment。",
            "如果你想以优雅的方式浏览所有原版和Mod附魔，不妨试试创意工坊的Enchantment Compendium。",
            "你可以在图鉴左下角开启卡牌3D效果，这很小丑牌！",
            "你可以在图鉴点开一张牌的详情，然后点击右下角入口，开始设置卡牌专用的特效。",
            "如果你发现自己的卡牌出现部件错位甚至消失，请尝试关闭本Mod的所有特效内容！包括3D！",
            "推荐完全勾选塔罗牌前五个配置开关游玩本Mod！",
            "如果你觉得塔罗牌的效果太强了，不妨关掉女祭司的配置开关。",
            "希望你享受星球牌带来的混乱体验！",
            "如果喜欢这个Mod，在Steam创意工坊上点个赞吧！",
        };

        private static readonly string[] ChineseThanksTips =
        {
            "感谢SeungjunKim_8224的韩语本地化支持。",
            "我的邮箱是3411737922@qq.com，你也可以直接添加我的QQ。",
            "感谢Balatri作者精美的游戏美术！",
            "感谢OLC开源的RItsulib项目！",
            "本Mod的多版本支持学习于Ritsulib。",
            "本Mod的多人联机配置同步学习于Ritsulib。",
            "感谢Ind_E开源的Balatoeffect项目！",
            "感谢我的朋友们！但是你们的名字太长了！",
            "死神-逆在最开始的设计是被附魔的牌本身也不能抽牌，后来我意识到这有多蠢。",
            "教皇-逆可以降级永世沙漏的凋亡卡牌，不过不会反过来升级它们。",
            "愚者-正附魔能力牌的实际实现是克隆一张牌，这可能存在某些小技巧……",
            "星星-逆和天际钻头的互动有非常多的额外代码！",
            "星星逆可以给原本没有星星消耗的牌附魔。",
        };

        // ── 标记占卜动态文本（战车/力量/隐者 精英类；正义/倒吊人/死神 普通类） ──────
        /// <summary>该 flagIndex 是否标记类占卜（有地图战斗效果 + 塔罗奖励）。</summary>
        public static bool IsMarkedDivination(int flagIndex)
            => flagIndex is 7 or 8 or 9 or 11 or 12 or 13;

        /// <summary>该 flagIndex 是否精英类标记占卜（完成 <see cref="TarotMarkerSystem.RewardInterval"/> 个后失效）。</summary>
        public static bool IsEliteDivination(int flagIndex)
            => flagIndex is 7 or 8 or 9;

        /// <summary>标记占卜的本地化 NAME（大写，对应 BAL_CFW_FLAG_&lt;NAME&gt;_DESC / BAL_CFW_MAP_&lt;NAME&gt;_*）。</summary>
        private static string MarkedDivinationName(int flagIndex)
            => flagIndex switch
            {
                7 => "CHARIOT", 8 => "STRENGTH", 9 => "HERMIT",
                11 => "JUSTICE", 12 => "HANGEDMAN", 13 => "DEATH",
                _ => string.Empty,
            };

        /// <summary>取当前语言下 gameplay_ui 表文本（先确保注入）。</summary>
        private static string Loc(string key)
        {
            Inject();
            return new LocString("gameplay_ui", key).GetFormattedText() ?? key;
        }

        /// <summary>
        /// 设置界面（配置面板底部提示）的 flag 描述：标记占卜追加动态行「当前已完成{Count}。」/「已失效。」；
        /// 非游戏状态（主菜单/选人界面）不显示动态行；非标记类返回静态基础描述。
        /// </summary>
        public static string BuildSettingsDescription(int flagIndex)
        {
            string name = MarkedDivinationName(flagIndex);
            if (string.IsNullOrEmpty(name)) return string.Empty;   // 仅标记类占卜；调用方应先判 IsMarkedDivination
            string baseText = Loc("BAL_CFW_FLAG_" + name + "_DESC");
            if (!RunManager.Instance.IsInProgress)
                return baseText;
            int completed = TarotMarkerSystem.GetCompletedCount(flagIndex);
            string line;
            if (IsEliteDivination(flagIndex) && completed >= TarotMarkerSystem.RewardInterval)
            {
                line = Loc("BAL_CFW_EXPIRED_LINE");
            }
            else
            {
                var ls = new LocString("gameplay_ui", "BAL_CFW_PROGRESS_LINE");
                ls.Add("Count", completed);
                line = ls.GetFormattedText() ?? "BAL_CFW_PROGRESS_LINE";
            }
            return baseText + "\n" + line;
        }

        /// <summary>
        /// 地图 hovertip 的动态描述键：精英类三态（0/1/已失效，X=本局累计完成数）、普通类两态（0/1，Y=重置计数）；
        /// 非标记类返回 null（调用方继续用静态 BAL_CFW_FLAG_&lt;NAME&gt;_DESC）。
        /// </summary>
        public static string? BuildMapDescriptionKey(int flagIndex)
        {
            string name = MarkedDivinationName(flagIndex);
            if (string.IsNullOrEmpty(name)) return null;
            if (IsEliteDivination(flagIndex))
            {
                int completed = TarotMarkerSystem.GetCompletedCount(flagIndex);
                if (completed >= TarotMarkerSystem.RewardInterval) return "BAL_CFW_MAP_" + name + "_EXP";
                return completed >= 1 ? "BAL_CFW_MAP_" + name + "_1" : "BAL_CFW_MAP_" + name + "_0";
            }
            int y = TarotMarkerSystem.GetProgressForDisplay(flagIndex);
            return y >= 1 ? "BAL_CFW_MAP_" + name + "_1" : "BAL_CFW_MAP_" + name + "_0";
        }

        private static readonly Dictionary<string, string> ChineseTexts = new()
        {
            { "BAL_CFW_TITLE", "内容和难度配置" },
            { "BAL_CFW_TAROT", "塔罗牌" },
            { "BAL_CFW_PLANET", "星球牌" },
            { "BAL_CFW_DEFAULT_HINT", "配置PengoTarot难度和更多内容" },
            { "BAL_CFW_TAROT_DESC", "塔罗牌" },
            { "BAL_CFW_PLANET_DESC", "星球牌（多人专属）" },
            { "BAL_CFW_SETTINGS_TOGGLE", "仅在游戏设置界面显示入口悬浮窗（不会立即生效）" },
            { "BAL_CFW_FLAG_MAGICIAN_DESC", "[gold]塔罗包[/gold]抽取的卡牌数量额外+[blue]2[/blue]（初始为[blue]1[/blue]）" },
            { "BAL_CFW_FLAG_HIGHPRIESTESS_DESC", "[gold]塔罗包[/gold]每次购买后价格额外-[blue]50[/blue]（初始为+[blue]50[/blue]，开启后抵消）" },
            { "BAL_CFW_FLAG_EMPRESS_DESC", "[gold]塔罗包[/gold]中可以遇到逆位塔罗" },
            { "BAL_CFW_FLAG_EMPEROR_DESC", "[gold]塔罗包[/gold]中可以遇到特殊塔罗" },
            { "BAL_CFW_FLAG_HIEROPHANT_DESC", "[gold]塔罗包[/gold]的基础价格额外-[blue]100[/blue]（初始为[blue]175~200[/blue]）" },
            { "BAL_CFW_FLAG_LOVERS_DESC", "所有[gold]精英敌人房间[/gold]共享[gold]塔罗标记[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_CHARIOT_DESC", "在每一幕标记[blue]1[/blue]个[gold]精英敌人房间[/gold]，房间内的敌人首次对你造成[gold]未被格挡[/gold]的伤害后，额外给予[blue]1[/blue]层[gold]易伤[/gold]。\n完成[blue]2[/blue]个标记后失效，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_STRENGTH_DESC", "在每一幕标记[blue]1[/blue]个[gold]精英敌人房间[/gold]，房间内的敌人首次对你造成[gold]未被格挡[/gold]的伤害后，额外给予[blue]1[/blue]层[gold]虚弱[/gold]。\n完成[blue]2[/blue]个标记后失效，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_HERMIT_DESC", "在每一幕标记[blue]1[/blue]个[gold]精英敌人房间[/gold]，房间内的敌人在战斗开始时，获得[blue]10%[/blue]生命值上限的[gold]覆甲[/gold]和[gold]格挡[/gold]。\n完成[blue]2[/blue]个标记后失效，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_WHEELOFFORTUNE_DESC", "占卜-命运之轮。「开发中」" },
            { "BAL_CFW_FLAG_JUSTICE_DESC", "在每一幕标记[blue]3[/blue]个[gold]普通敌人房间[/gold]，在这场战斗中，每回合打出的第一张[gold]攻击牌[/gold]被[gold]消耗[/gold]。\n每完成[blue]2[/blue]个标记，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_HANGEDMAN_DESC", "在每一幕标记[blue]3[/blue]个[gold]普通敌人房间[/gold]，在这场战斗中，每回合打出的第一张[gold]技能牌[/gold]被[gold]消耗[/gold]。\n每完成[blue]2[/blue]个标记，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_DEATH_DESC", "在每一幕标记[blue]3[/blue]个[gold]普通敌人房间[/gold]，在这场战斗中，每当你打出[gold]能力牌[/gold]时，立刻[red]结束你的回合[/red]。\n每完成[blue]2[/blue]个标记，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_TEMPERANCE_DESC", "每次打出一张牌，获得[blue]1[/blue]层节制-逆（本回合内每受到[blue]1[/blue]点[gold]未被格挡[/gold]的伤害，获得[blue]1[/blue][gold]金币[/gold]，可叠加）。\n战斗胜利时不再获得[gold]金币[/gold]奖励。「开发中」" },
            { "BAL_CFW_TAROT_REWARD_DESC", "选择一次塔罗牌奖励。" },
            { "BAL_CFW_FLAG_JUDGEMENT_DESC", "占卜-审判。「开发中」" },
            { "BAL_CFW_FLAG_DEVIL_DESC", "占卜-恶魔。「开发中」" },
            { "BAL_CFW_FLAG_STAR_DESC", "占卜-星星。「开发中」" },
            { "BAL_CFW_FLAG_SUN_DESC", "占卜-太阳。「开发中」" },
            { "BAL_CFW_FLAG_MOON_DESC", "占卜-月亮。「开发中」" },
            { "BAL_CFW_FLAG_WORLD_DESC", "占卜-世界。「开发中」" },
            { "BAL_CFW_FLAG_FOOL_DESC", "在商店中可以遇到[gold]塔罗包[/gold]" },
            { "BAL_CFW_FLAG_TOWER_DESC", "进阶之灾现在可以打出。\n进阶之灾被[gold]消耗[/gold]时，[gold]消耗[/gold]你的所有[gold]手牌[/gold]。「开发中」" },
            { "BAL_CFW_TOWER_CARD_DESC", "这张牌被[gold]消耗[/gold]时，额外[gold]消耗[/gold]你的所有手牌。「开发中」" },

            // ── 标记占卜动态文本（设置界面动态行 + 地图 hovertip 分状态文本） ──
            { "BAL_CFW_PROGRESS_LINE", "当前已完成[blue]{Count}[/blue]。" },
            { "BAL_CFW_EXPIRED_LINE", "已失效。" },
            { "BAL_CFW_MAP_CHARIOT_0", "这个房间的敌人首次对你造成[gold]未被格挡[/gold]的伤害时，额外给予[blue]1[/blue]层[gold]易伤[/gold]。" },
            { "BAL_CFW_MAP_CHARIOT_1", "这个房间的敌人首次对你造成[gold]未被格挡[/gold]的伤害时，额外给予[blue]1[/blue]层[gold]易伤[/gold]。\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。" },
            { "BAL_CFW_MAP_CHARIOT_EXP", "这个标记已失效。" },
            { "BAL_CFW_MAP_STRENGTH_0", "这个房间的敌人首次对你造成[gold]未被格挡[/gold]的伤害时，额外给予[blue]1[/blue]层[gold]虚弱[/gold]。" },
            { "BAL_CFW_MAP_STRENGTH_1", "这个房间的敌人首次对你造成[gold]未被格挡[/gold]的伤害时，额外给予[blue]1[/blue]层[gold]虚弱[/gold]。\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。" },
            { "BAL_CFW_MAP_STRENGTH_EXP", "这个标记已失效。" },
            { "BAL_CFW_MAP_HERMIT_0", "这个房间的敌人在战斗开始时，获得[blue]10%[/blue]生命值上限的[gold]覆甲[/gold]和[gold]格挡[/gold]。" },
            { "BAL_CFW_MAP_HERMIT_1", "这个房间的敌人在战斗开始时，获得[blue]10%[/blue]生命值上限的[gold]覆甲[/gold]和[gold]格挡[/gold]。\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。" },
            { "BAL_CFW_MAP_HERMIT_EXP", "这个标记已失效。" },
            { "BAL_CFW_MAP_JUSTICE_0", "在这场战斗中，你每回合打出的第一张[gold]攻击牌[/gold]被[gold]消耗[/gold]。" },
            { "BAL_CFW_MAP_JUSTICE_1", "在这场战斗中，你每回合打出的第一张[gold]攻击牌[/gold]被[gold]消耗[/gold]。\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。" },
            { "BAL_CFW_MAP_HANGEDMAN_0", "在这场战斗中，你每回合打出的第一张[gold]技能牌[/gold]被[gold]消耗[/gold]。" },
            { "BAL_CFW_MAP_HANGEDMAN_1", "在这场战斗中，你每回合打出的第一张[gold]技能牌[/gold]被[gold]消耗[/gold]。\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。" },
            { "BAL_CFW_MAP_DEATH_0", "在这场战斗中，每当你打出[gold]能力牌[/gold]时，立刻[red]结束你的回合[/red]。" },
            { "BAL_CFW_MAP_DEATH_1", "在这场战斗中，每当你打出[gold]能力牌[/gold]时，立刻[red]结束你的回合[/red]。\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。" },
        };

        private static readonly Dictionary<string, string> EnglishTexts = new()
        {
            { "BAL_CFW_TITLE", "Difficulty Config" },
            { "BAL_CFW_TAROT", "Tarot" },
            { "BAL_CFW_PLANET", "Planet" },
            { "BAL_CFW_DEFAULT_HINT", "Configure PengoTarot difficulty and more" },
            { "BAL_CFW_TAROT_DESC", "Tarot" },
            { "BAL_CFW_PLANET_DESC", "Planet (Multiplayer)" },
            { "BAL_CFW_SETTINGS_TOGGLE", "Show entry floater only in Settings screen (takes effect on next open)" },
            { "BAL_CFW_FLAG_MAGICIAN_DESC", "[gold]Tarot packs[/gold] draw +[blue]2[/blue] cards (default [blue]1[/blue])" },
            { "BAL_CFW_FLAG_HIGHPRIESTESS_DESC", "[gold]Tarot pack[/gold] price -[blue]50[/blue] after each purchase (default +[blue]50[/blue]; offsets it when enabled)" },
            { "BAL_CFW_FLAG_EMPRESS_DESC", "Reversed tarot can appear in [gold]tarot packs[/gold]" },
            { "BAL_CFW_FLAG_EMPEROR_DESC", "Special character tarot can appear in [gold]tarot packs[/gold]" },
            { "BAL_CFW_FLAG_HIEROPHANT_DESC", "[gold]Tarot pack[/gold] base price -[blue]100[/blue] (default [blue]175~200[/blue])" },
            { "BAL_CFW_FLAG_LOVERS_DESC", "All [gold]elite enemy rooms[/gold] share [gold]tarot markers[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_CHARIOT_DESC", "Marks [blue]1[/blue] [gold]elite enemy room[/gold] per act.\nEnemies there inflict [blue]1[/blue] [gold]Vulnerable[/gold] the first time they deal [gold]unblocked[/gold] damage to you.\nAfter [blue]2[/blue] marks, this divination expires and you gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_STRENGTH_DESC", "Marks [blue]1[/blue] [gold]elite enemy room[/gold] per act.\nEnemies there inflict [blue]1[/blue] [gold]Weak[/gold] the first time they deal [gold]unblocked[/gold] damage to you.\nAfter [blue]2[/blue] marks, this divination expires and you gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_HERMIT_DESC", "Marks [blue]1[/blue] [gold]elite enemy room[/gold] per act.\nEnemies there start combat with [gold]Plated Armor[/gold] and [gold]Block[/gold] equal to [blue]10%[/blue] of their Max HP.\nAfter [blue]2[/blue] marks, this divination expires and you gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_WHEELOFFORTUNE_DESC", "Divination - Wheel of Fortune. (In Development)" },
            { "BAL_CFW_FLAG_JUSTICE_DESC", "Marks [blue]3[/blue] [gold]normal enemy rooms[/gold] per act.\nIn that combat, the first [gold]Attack[/gold] you play each turn is [gold]Exhausted[/gold].\nFor every [blue]2[/blue] marks completed, gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_HANGEDMAN_DESC", "Marks [blue]3[/blue] [gold]normal enemy rooms[/gold] per act.\nIn that combat, the first [gold]Skill[/gold] you play each turn is [gold]Exhausted[/gold].\nFor every [blue]2[/blue] marks completed, gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_DEATH_DESC", "Marks [blue]3[/blue] [gold]normal enemy rooms[/gold] per act.\nIn that combat, whenever you play a [gold]Power[/gold] card, [red]end your turn[/red] immediately.\nFor every [blue]2[/blue] marks completed, gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_TEMPERANCE_DESC", "Each time you play a card, gain [blue]1[/blue] Temperance-Reversed (this turn, gain [blue]1[/blue] [gold]Gold[/gold] for each point of [gold]unblocked[/gold] damage you take; stacks).\nYou no longer gain [gold]Gold[/gold] from combat victories. (In Development)" },
            { "BAL_CFW_TAROT_REWARD_DESC", "Choose a tarot card reward." },
            { "BAL_CFW_FLAG_JUDGEMENT_DESC", "Divination - Judgement. (In Development)" },
            { "BAL_CFW_FLAG_DEVIL_DESC", "Divination - Devil. (In Development)" },
            { "BAL_CFW_FLAG_STAR_DESC", "Divination - Star. (In Development)" },
            { "BAL_CFW_FLAG_SUN_DESC", "Divination - Sun. (In Development)" },
            { "BAL_CFW_FLAG_MOON_DESC", "Divination - Moon. (In Development)" },
            { "BAL_CFW_FLAG_WORLD_DESC", "Divination - World. (In Development)" },
            { "BAL_CFW_FLAG_FOOL_DESC", "[gold]Tarot packs[/gold] can appear in shops" },
            { "BAL_CFW_FLAG_TOWER_DESC", "Ascender's Bane can now be played.\nWhen Ascender's Bane is [gold]Exhausted[/gold], [gold]Exhaust[/gold] all cards in your [gold]Hand[/gold]. (In Development)" },
            { "BAL_CFW_TOWER_CARD_DESC", "When this card is [gold]Exhausted[/gold], also [gold]Exhaust[/gold] all cards in your hand. (In Development)" },

            // ── 标记占卜动态文本（设置界面动态行 + 地图 hovertip 分状态文本） ──
            { "BAL_CFW_PROGRESS_LINE", "Currently completed: [blue]{Count}[/blue]." },
            { "BAL_CFW_EXPIRED_LINE", "Expired." },
            { "BAL_CFW_MAP_CHARIOT_0", "The first time enemies in this room deal [gold]unblocked[/gold] damage to you, gain [blue]1[/blue] [gold]Vulnerable[/gold]." },
            { "BAL_CFW_MAP_CHARIOT_1", "The first time enemies in this room deal [gold]unblocked[/gold] damage to you, gain [blue]1[/blue] [gold]Vulnerable[/gold].\nAfter completing the next combat, gain a special [gold]tarot reward[/gold]." },
            { "BAL_CFW_MAP_CHARIOT_EXP", "This marker has expired." },
            { "BAL_CFW_MAP_STRENGTH_0", "The first time enemies in this room deal [gold]unblocked[/gold] damage to you, gain [blue]1[/blue] [gold]Weak[/gold]." },
            { "BAL_CFW_MAP_STRENGTH_1", "The first time enemies in this room deal [gold]unblocked[/gold] damage to you, gain [blue]1[/blue] [gold]Weak[/gold].\nAfter completing the next combat, gain a special [gold]tarot reward[/gold]." },
            { "BAL_CFW_MAP_STRENGTH_EXP", "This marker has expired." },
            { "BAL_CFW_MAP_HERMIT_0", "Enemies in this room start combat with [gold]Plated Armor[/gold] and [gold]Block[/gold] equal to [blue]10%[/blue] of their Max HP." },
            { "BAL_CFW_MAP_HERMIT_1", "Enemies in this room start combat with [gold]Plated Armor[/gold] and [gold]Block[/gold] equal to [blue]10%[/blue] of their Max HP.\nAfter completing the next combat, gain a special [gold]tarot reward[/gold]." },
            { "BAL_CFW_MAP_HERMIT_EXP", "This marker has expired." },
            { "BAL_CFW_MAP_JUSTICE_0", "In this combat, the first [gold]Attack[/gold] you play each turn is [gold]Exhausted[/gold]." },
            { "BAL_CFW_MAP_JUSTICE_1", "In this combat, the first [gold]Attack[/gold] you play each turn is [gold]Exhausted[/gold].\nAfter completing the next combat, gain a special [gold]tarot reward[/gold]." },
            { "BAL_CFW_MAP_HANGEDMAN_0", "In this combat, the first [gold]Skill[/gold] you play each turn is [gold]Exhausted[/gold]." },
            { "BAL_CFW_MAP_HANGEDMAN_1", "In this combat, the first [gold]Skill[/gold] you play each turn is [gold]Exhausted[/gold].\nAfter completing the next combat, gain a special [gold]tarot reward[/gold]." },
            { "BAL_CFW_MAP_DEATH_0", "In this combat, whenever you play a [gold]Power[/gold] card, [red]end your turn[/red]." },
            { "BAL_CFW_MAP_DEATH_1", "In this combat, whenever you play a [gold]Power[/gold] card, [red]end your turn[/red].\nAfter completing the next combat, gain a special [gold]tarot reward[/gold]." },
        };

        private static readonly Dictionary<string, string> JapaneseTexts = new()
        {
            { "BAL_CFW_TITLE", "難易度設定" },
            { "BAL_CFW_TAROT", "タロット" },
            { "BAL_CFW_PLANET", "プラネット" },
            { "BAL_CFW_DEFAULT_HINT", "PengoTarot 難易度などを設定" },
            { "BAL_CFW_TAROT_DESC", "タロット" },
            { "BAL_CFW_PLANET_DESC", "プラネット（マルチ専用）" },
            { "BAL_CFW_SETTINGS_TOGGLE", "設定画面にのみ入口フローティングウィンドウを表示（すぐには反映されません）" },
            { "BAL_CFW_FLAG_MAGICIAN_DESC", "[gold]タロットパック[/gold]の引く枚数+[blue]2[/blue]（初期[blue]1[/blue]）" },
            { "BAL_CFW_FLAG_HIGHPRIESTESS_DESC", "[gold]タロットパック[/gold]購入ごとに価格-[blue]50[/blue]（初期+[blue]50[/blue]、有効時は相殺）" },
            { "BAL_CFW_FLAG_EMPRESS_DESC", "[gold]タロットパック[/gold]に逆位置タロットが出現" },
            { "BAL_CFW_FLAG_EMPEROR_DESC", "[gold]タロットパック[/gold]に専用タロットが出現" },
            { "BAL_CFW_FLAG_HIEROPHANT_DESC", "[gold]タロットパック[/gold]の基本価格-[blue]100[/blue]（初期[blue]175~200[/blue]）" },
            { "BAL_CFW_FLAG_LOVERS_DESC", "すべての[gold]エリートの部屋[/gold]が[gold]タロットマーカー[/gold]を共有する。「開発中」" },
            { "BAL_CFW_FLAG_CHARIOT_DESC", "各層で[gold]エリートの部屋[/gold]を[blue]1[/blue]つマークする。\nその部屋の敵が初めて[gold]ブロックされていない[/gold]ダメージを与えた後、さらに[gold]弱体[/gold][blue]1[/blue]を付与する。\nマークを[blue]2[/blue]つ完了すると失効し、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_STRENGTH_DESC", "各層で[gold]エリートの部屋[/gold]を[blue]1[/blue]つマークする。\nその部屋の敵が初めて[gold]ブロックされていない[/gold]ダメージを与えた後、さらに[gold]脱力[/gold][blue]1[/blue]を付与する。\nマークを[blue]2[/blue]つ完了すると失効し、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_HERMIT_DESC", "各層で[gold]エリートの部屋[/gold]を[blue]1[/blue]つマークする。\nその部屋の敵は戦闘開始時、最大HPの[blue]10%[/blue]分の[gold]プレート[/gold]と[gold]ブロック[/gold]を得る。\nマークを[blue]2[/blue]つ完了すると失効し、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_WHEELOFFORTUNE_DESC", "占い-運命の輪。「開発中」" },
            { "BAL_CFW_FLAG_JUSTICE_DESC", "各層で[gold]通常の敵の部屋[/gold]を[blue]3[/blue]つマークする。\nこの戦闘中、毎ターン最初にプレイした[gold]アタック[/gold]が[gold]廃棄[/gold]される。\nマークを[blue]2[/blue]つ完了するごとに、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_HANGEDMAN_DESC", "各層で[gold]通常の敵の部屋[/gold]を[blue]3[/blue]つマークする。\nこの戦闘中、毎ターン最初にプレイした[gold]スキル[/gold]が[gold]廃棄[/gold]される。\nマークを[blue]2[/blue]つ完了するごとに、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_DEATH_DESC", "各層で[gold]通常の敵の部屋[/gold]を[blue]3[/blue]つマークする。\nこの戦闘中、[gold]パワー[/gold]をプレイするたびに、即座に[red]ターンを終了[/red]する。\nマークを[blue]2[/blue]つ完了するごとに、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_TEMPERANCE_DESC", "カードを[blue]1[/blue]枚プレイするたびに、節制-逆を[blue]1[/blue]獲得する（このターン、[gold]ブロックされていない[/gold]ダメージ[blue]1[/blue]につき[gold]ゴールド[/gold][blue]1[/blue]を得る。重複する）。\n戦闘勝利時の[gold]ゴールド[/gold]報酬を獲得しなくなる。「開発中」" },
            { "BAL_CFW_TAROT_REWARD_DESC", "タロット報酬を一回選択する。" },
            { "BAL_CFW_FLAG_JUDGEMENT_DESC", "占い-審判。「開発中」" },
            { "BAL_CFW_FLAG_DEVIL_DESC", "占い-悪魔。「開発中」" },
            { "BAL_CFW_FLAG_STAR_DESC", "占い-星。「開発中」" },
            { "BAL_CFW_FLAG_SUN_DESC", "占い-太陽。「開発中」" },
            { "BAL_CFW_FLAG_MOON_DESC", "占い-月。「開発中」" },
            { "BAL_CFW_FLAG_WORLD_DESC", "占い-世界。「開発中」" },
            { "BAL_CFW_FLAG_FOOL_DESC", "ショップで[gold]タロットパック[/gold]に遭遇できる" },
            { "BAL_CFW_FLAG_TOWER_DESC", "アセンダーの災厄がプレイ可能になる。\nアセンダーの災厄が[gold]廃棄[/gold]された時、[gold]手札[/gold]をすべて[gold]廃棄[/gold]する。「開発中」" },
            { "BAL_CFW_TOWER_CARD_DESC", "このカードが[gold]廃棄[/gold]された時、手札をすべて[gold]廃棄[/gold]する。「開発中」" },

            // ── 标记占卜动态文本（设置界面动态行 + 地图 hovertip 分状态文本） ──
            { "BAL_CFW_PROGRESS_LINE", "現在の完了数：[blue]{Count}[/blue]。" },
            { "BAL_CFW_EXPIRED_LINE", "失効済み。" },
            { "BAL_CFW_MAP_CHARIOT_0", "この部屋の敵が初めて[gold]ブロックされていない[/gold]ダメージを与えた時、さらに[gold]弱体[/gold][blue]1[/blue]を付与する。" },
            { "BAL_CFW_MAP_CHARIOT_1", "この部屋の敵が初めて[gold]ブロックされていない[/gold]ダメージを与えた時、さらに[gold]弱体[/gold][blue]1[/blue]を付与する。\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。" },
            { "BAL_CFW_MAP_CHARIOT_EXP", "このマーカーは失効しました。" },
            { "BAL_CFW_MAP_STRENGTH_0", "この部屋の敵が初めて[gold]ブロックされていない[/gold]ダメージを与えた時、さらに[gold]脱力[/gold][blue]1[/blue]を付与する。" },
            { "BAL_CFW_MAP_STRENGTH_1", "この部屋の敵が初めて[gold]ブロックされていない[/gold]ダメージを与えた時、さらに[gold]脱力[/gold][blue]1[/blue]を付与する。\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。" },
            { "BAL_CFW_MAP_STRENGTH_EXP", "このマーカーは失効しました。" },
            { "BAL_CFW_MAP_HERMIT_0", "この部屋の敵は戦闘開始時、最大HPの[blue]10%[/blue]分の[gold]プレート[/gold]と[gold]ブロック[/gold]を得る。" },
            { "BAL_CFW_MAP_HERMIT_1", "この部屋の敵は戦闘開始時、最大HPの[blue]10%[/blue]分の[gold]プレート[/gold]と[gold]ブロック[/gold]を得る。\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。" },
            { "BAL_CFW_MAP_HERMIT_EXP", "このマーカーは失効しました。" },
            { "BAL_CFW_MAP_JUSTICE_0", "この戦闘中、毎ターン最初にプレイした[gold]アタック[/gold]が[gold]廃棄[/gold]される。" },
            { "BAL_CFW_MAP_JUSTICE_1", "この戦闘中、毎ターン最初にプレイした[gold]アタック[/gold]が[gold]廃棄[/gold]される。\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。" },
            { "BAL_CFW_MAP_HANGEDMAN_0", "この戦闘中、毎ターン最初にプレイした[gold]スキル[/gold]が[gold]廃棄[/gold]される。" },
            { "BAL_CFW_MAP_HANGEDMAN_1", "この戦闘中、毎ターン最初にプレイした[gold]スキル[/gold]が[gold]廃棄[/gold]される。\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。" },
            { "BAL_CFW_MAP_DEATH_0", "この戦闘中、[gold]パワー[/gold]をプレイするたびに、即座に[red]ターンを終了[/red]する。" },
            { "BAL_CFW_MAP_DEATH_1", "この戦闘中、[gold]パワー[/gold]をプレイするたびに、即座に[red]ターンを終了[/red]する。\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。" },
        };

        private static readonly Dictionary<string, string> KoreanTexts = new()
        {
            { "BAL_CFW_TITLE", "난이도 설정" },
            { "BAL_CFW_TAROT", "타로" },
            { "BAL_CFW_PLANET", "플래닛" },
            { "BAL_CFW_DEFAULT_HINT", "PengoTarot 난이도 등을 설정" },
            { "BAL_CFW_TAROT_DESC", "타로" },
            { "BAL_CFW_PLANET_DESC", "플래닛 (멀티 전용)" },
            { "BAL_CFW_SETTINGS_TOGGLE", "설정 화면에만 입구 플로팅 창 표시 (즉시 적용되지 않음)" },
            { "BAL_CFW_FLAG_MAGICIAN_DESC", "[gold]타로 팩[/gold] 카드 뽑기 +[blue]2[/blue] (기본 [blue]1[/blue])" },
            { "BAL_CFW_FLAG_HIGHPRIESTESS_DESC", "[gold]타로 팩[/gold] 구매 후 가격 -[blue]50[/blue] (기본 +[blue]50[/blue], 활성 시 상쇄)" },
            { "BAL_CFW_FLAG_EMPRESS_DESC", "[gold]타로 팩[/gold]에 역위 타로 등장" },
            { "BAL_CFW_FLAG_EMPEROR_DESC", "[gold]타로 팩[/gold]에 특수 타로 등장" },
            { "BAL_CFW_FLAG_HIEROPHANT_DESC", "[gold]타로 팩[/gold] 기본 가격 -[blue]100[/blue] (기본 [blue]175~200[/blue])" },
            { "BAL_CFW_FLAG_LOVERS_DESC", "모든 [gold]정예 적 방[/gold]이 [gold]타로 마커[/gold]를 공유합니다.「개발 중」" },
            { "BAL_CFW_FLAG_CHARIOT_DESC", "각 막에서 [gold]엘리트 방[/gold] [blue]1[/blue]개를 표시합니다.\n해당 방의 적이 처음으로 [gold]방어도로 막지 못한[/gold] 피해를 준 후, 추가로 [gold]취약[/gold] [blue]1[/blue]을 부여합니다.\n표시 [blue]2[/blue]개 완료 시 비활성화되며 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_STRENGTH_DESC", "각 막에서 [gold]엘리트 방[/gold] [blue]1[/blue]개를 표시합니다.\n해당 방의 적이 처음으로 [gold]방어도로 막지 못한[/gold] 피해를 준 후, 추가로 [gold]약화[/gold] [blue]1[/blue]을 부여합니다.\n표시 [blue]2[/blue]개 완료 시 비활성화되며 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_HERMIT_DESC", "각 막에서 [gold]엘리트 방[/gold] [blue]1[/blue]개를 표시합니다.\n해당 방의 적이 전투 시작 시 최대 체력의 [blue]10%[/blue]만큼 [gold]판금[/gold]과 [gold]방어도[/gold]를 얻습니다.\n표시 [blue]2[/blue]개 완료 시 비활성화되며 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_WHEELOFFORTUNE_DESC", "점괘-운명의 수레바퀴。「개발 중」" },
            { "BAL_CFW_FLAG_JUSTICE_DESC", "각 막에서 [gold]일반 적 방[/gold] [blue]3[/blue]개를 표시합니다.\n해당 전투에서 매 턴 처음 사용하는 [gold]공격 카드[/gold]가 [gold]소멸[/gold]됩니다.\n표시 [blue]2[/blue]개 완료 시마다 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_HANGEDMAN_DESC", "각 막에서 [gold]일반 적 방[/gold] [blue]3[/blue]개를 표시합니다.\n해당 전투에서 매 턴 처음 사용하는 [gold]스킬 카드[/gold]가 [gold]소멸[/gold]됩니다.\n표시 [blue]2[/blue]개 완료 시마다 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_DEATH_DESC", "각 막에서 [gold]일반 적 방[/gold] [blue]3[/blue]개를 표시합니다.\n해당 전투에서 [gold]파워 카드[/gold]를 사용할 때마다 즉시 [red]턴을 종료[/red]합니다.\n표시 [blue]2[/blue]개 완료 시마다 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_TEMPERANCE_DESC", "카드를 [blue]1[/blue]장 사용할 때마다 절제-역방향 [blue]1[/blue]을 얻습니다 (이번 턴에 [gold]방어도로 막지 못한[/gold] 피해 [blue]1[/blue]마다 [gold]골드[/gold] [blue]1[/blue]을 얻으며, 중첩됩니다).\n전투 승리 시 [gold]골드[/gold] 보상을 받지 못합니다.「개발 중」" },
            { "BAL_CFW_TAROT_REWARD_DESC", "타로 보상을 한 번 선택합니다." },
            { "BAL_CFW_FLAG_JUDGEMENT_DESC", "점괘-심판。「개발 중」" },
            { "BAL_CFW_FLAG_DEVIL_DESC", "점괘-악마。「개발 중」" },
            { "BAL_CFW_FLAG_STAR_DESC", "점괘-별。「개발 중」" },
            { "BAL_CFW_FLAG_SUN_DESC", "점괘-태양。「개발 중」" },
            { "BAL_CFW_FLAG_MOON_DESC", "점괘-달。「개발 중」" },
            { "BAL_CFW_FLAG_WORLD_DESC", "점괘-세계。「개발 중」" },
            { "BAL_CFW_FLAG_FOOL_DESC", "상점에서 [gold]타로 팩[/gold]을 만날 수 있음" },
            { "BAL_CFW_FLAG_TOWER_DESC", "등반자의 골칫거리를 사용할 수 있게 됩니다.\n등반자의 골칫거리가 [gold]소멸[/gold]될 때, 손에 있는 모든 카드를 [gold]소멸[/gold]시킵니다.「개발 중」" },
            { "BAL_CFW_TOWER_CARD_DESC", "이 카드가 [gold]소멸[/gold]될 때, 손에 있는 모든 카드를 추가로 [gold]소멸[/gold]시킵니다.「개발 중」" },

            // ── 标记占卜动态文本（设置界面动态行 + 地图 hovertip 分状态文本） ──
            { "BAL_CFW_PROGRESS_LINE", "현재 완료 수: [blue]{Count}[/blue]." },
            { "BAL_CFW_EXPIRED_LINE", "비활성화됨." },
            { "BAL_CFW_MAP_CHARIOT_0", "이 방의 적이 처음으로 [gold]방어도로 막지 못한[/gold] 피해를 준 경우, 추가로 [gold]취약[/gold] [blue]1[/blue]을 부여합니다." },
            { "BAL_CFW_MAP_CHARIOT_1", "이 방의 적이 처음으로 [gold]방어도로 막지 못한[/gold] 피해를 준 경우, 추가로 [gold]취약[/gold] [blue]1[/blue]을 부여합니다.\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다." },
            { "BAL_CFW_MAP_CHARIOT_EXP", "이 마커는 비활성화되었습니다." },
            { "BAL_CFW_MAP_STRENGTH_0", "이 방의 적이 처음으로 [gold]방어도로 막지 못한[/gold] 피해를 준 경우, 추가로 [gold]약화[/gold] [blue]1[/blue]을 부여합니다." },
            { "BAL_CFW_MAP_STRENGTH_1", "이 방의 적이 처음으로 [gold]방어도로 막지 못한[/gold] 피해를 준 경우, 추가로 [gold]약화[/gold] [blue]1[/blue]을 부여합니다.\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다." },
            { "BAL_CFW_MAP_STRENGTH_EXP", "이 마커는 비활성화되었습니다." },
            { "BAL_CFW_MAP_HERMIT_0", "이 방의 적은 전투 시작 시 최대 체력의 [blue]10%[/blue]만큼 [gold]판금[/gold]과 [gold]방어도[/gold]를 얻습니다." },
            { "BAL_CFW_MAP_HERMIT_1", "이 방의 적은 전투 시작 시 최대 체력의 [blue]10%[/blue]만큼 [gold]판금[/gold]과 [gold]방어도[/gold]를 얻습니다.\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다." },
            { "BAL_CFW_MAP_HERMIT_EXP", "이 마커는 비활성화되었습니다." },
            { "BAL_CFW_MAP_JUSTICE_0", "이 전투에서 매 턴 처음 사용하는 [gold]공격 카드[/gold]가 [gold]소멸[/gold]됩니다." },
            { "BAL_CFW_MAP_JUSTICE_1", "이 전투에서 매 턴 처음 사용하는 [gold]공격 카드[/gold]가 [gold]소멸[/gold]됩니다.\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다." },
            { "BAL_CFW_MAP_HANGEDMAN_0", "이 전투에서 매 턴 처음 사용하는 [gold]스킬 카드[/gold]가 [gold]소멸[/gold]됩니다." },
            { "BAL_CFW_MAP_HANGEDMAN_1", "이 전투에서 매 턴 처음 사용하는 [gold]스킬 카드[/gold]가 [gold]소멸[/gold]됩니다.\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다." },
            { "BAL_CFW_MAP_DEATH_0", "이 전투에서 [gold]파워 카드[/gold]를 사용할 때마다 즉시 [red]턴을 종료[/red]합니다." },
            { "BAL_CFW_MAP_DEATH_1", "이 전투에서 [gold]파워 카드[/gold]를 사용할 때마다 즉시 [red]턴을 종료[/red]합니다.\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다." },
        };
    }
}
