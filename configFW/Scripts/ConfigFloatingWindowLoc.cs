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

        // ── 配置界面左下角 TIPS 随机池
        /// <summary>查看配置界面达该次数后，「感谢」内容解锁并入随机池（每次启动清零）。</summary>
        public const int ThanksUnlockOpens = 3;

        /// <summary>当前语言的普通 TIPS（无则空数组）。</summary>
        public static IReadOnlyList<string> GetMainTips(string language) => language switch
        {
            "zhs" => ChineseMainTips,
            "eng" => EnglishMainTips,
            "jpn" => JapaneseMainTips,
            "kor" => KoreanMainTips,
            _ => EmptyTips,
        };

        /// <summary>当前语言的「感谢/其他」TIPS（无则空数组）。</summary>
        public static IReadOnlyList<string> GetThanksTips(string language) => language switch
        {
            "zhs" => ChineseThanksTips,
            "eng" => EnglishThanksTips,
            "jpn" => JapaneseThanksTips,
            "kor" => KoreanThanksTips,
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

        private static readonly string[] EnglishMainTips =
        {
            "The Mod's config entry always appears in the game's own Settings screen.",
            "You can hide the config entry for a run by enabling the button below the panel.",
            "Skipping a Tarot pack doesn't deduct gold; you can always return to the shop and buy the cards you want.",
            "Skipping the campfire Stargazer option doesn't consume an action or the Stargazer deck's uses.",
            "If both the Tarot and Planet master toggles are off, the Mod marks itself as 'does not affect gameplay' on next launch; re-enabling them also requires a restart. (Not implemented yet!)",
            "Want to enchant a card multiple times? Try MultiEnchantment on the Workshop.",
            "Want to browse all vanilla and Mod enchantments in style? Try Enchantment Compendium on the Workshop.",
            "You can enable 3D card effects in the bottom-left of the Card Library - very Balatro!",
            "In the Card Library, open a card's details, then click the entry in the bottom-right to set up per-card effects.",
            "If your cards glitch or vanish, try turning off all of this Mod's effects, including 3D!",
            "We recommend fully enabling the first five Tarot config toggles to play this Mod!",
            "If you find the Tarot effects too strong, try disabling the High Priestess toggle.",
            "Enjoy the chaos that Planet cards bring!",
            "If you like this Mod, give it a like on the Steam Workshop!",
        };

        private static readonly string[] EnglishThanksTips =
        {
            "Thanks to SeungjunKim_8224 for the Korean localization support.",
            "My email is 3411737922@qq.com; you can also add my QQ directly.",
            "Thanks to the Balatri author for the beautiful game art!",
            "Thanks to OLC for the open-source RItsulib project!",
            "This Mod's multi-version support was learned from Ritsulib.",
            "This Mod's multiplayer config sync was learned from Ritsulib.",
            "Thanks to Ind_E for the open-source Balatoeffect project!",
            "Thanks to my friends! But your names are too long!",
            "Death-Reversed was originally designed so the enchanted card itself couldn't draw; later I realized how dumb that was.",
            "Hierophant-Reversed can downgrade the Decaying cards of the Everburning Hourglass, but won't upgrade them back.",
            "The Fool (upright) enchantment on Power cards is actually implemented by cloning a card; there may be some tricks to it...",
            "The Star-Reversed interaction with the Skybound Drill has a ton of extra code!",
            "Star-Reversed can enchant cards that don't naturally have a Star cost.",
        };

        private static readonly string[] JapaneseMainTips =
        {
            "Modの設定入口は常にゲーム本体の設定画面に表示されます。",
            "パネル下のボタンをオンにすると、そのラン中は設定入口を非表示にできます。",
            "タロットパックをスキップしてもゴールドは減りません。いつでもショップに戻って好きなカードを購入できます。",
            "焚き火の占星オプションをスキップしても、アクションや占星デッキの使用回数は消費されません。",
            "タロットとプラネットの両方のマスタースイッチをオフにすると、次回起動時にModが「ゲームプレイに影響なし」とマークします。再度オンにするには再起動が必要です。（まだ未実装！）",
            "1枚のカードに複数回エンチャントしてみたいなら、WorkshopのMultiEnchantmentをどうぞ。",
            "すべてのバニラおよびModエンチャントを優雅に閲覧したいなら、WorkshopのEnchantment Compendiumをどうぞ。",
            "図鑑の左下でカード3D効果を有効にできます。まるでBalatro！",
            "図鑑でカードの詳細を開き、右下の入口をクリックしてカード専用のエフェクトを設定できます。",
            "カードがずれたり消えたりしたら、3Dを含む本Modのすべてのエフェクトを無効にしてみてください！",
            "本Modをプレイするなら、タロットの最初の5つの設定スイッチをすべてオンにすることをおすすめします！",
            "タロットの効果が強すぎると感じたら、女教皇の設定スイッチをオフにしてみてください。",
            "プラネットカードがもたらすカオスをお楽しみください！",
            "このModが気に入ったら、Steam Workshopでいいねをお願いします！",
        };

        private static readonly string[] JapaneseThanksTips =
        {
            "SeungjunKim_8224さんの韓国語ローカライズ支援に感謝します。",
            "メールアドレスは3411737922@qq.comです。QQに直接追加してもらっても構いません。",
            "Balatri作者の美しいゲームアートに感謝します！",
            "OLC氏のオープンソースRItsulibプロジェクトに感謝します！",
            "本Modのマルチバージョン対応はRitsulibから学びました。",
            "本Modのマルチプレイ設定同期はRitsulibから学びました。",
            "Ind_E氏のオープンソースBalatoeffectプロジェクトに感謝します！",
            "友達に感謝！でも名前が長すぎる！",
            "死神-逆は当初、エンチャントされたカード自体もドローできなくなる設計でしたが、それがどれだけ馬鹿げているかに後で気づきました。",
            "教皇-逆は永遠の砂時計の凋零カードを下位にできますが、逆に上位には戻しません。",
            "愚者-正のパワーカードへのエンチャントは、実際にはカードの複製として実装されており、小技があるかもしれません……",
            "星-逆とスカイボーンドリルの相互作用には非常に多くの追加コードがあります！",
            "星-逆はもともと星コストを持たないカードにもエンチャントできます。",
        };

        private static readonly string[] KoreanMainTips =
        {
            "Mod 설정 입구는 항상 게임 자체 설정 화면에 표시됩니다.",
            "패널 아래 버튼을 켜면 이번 런 동안 설정 입구를 숨길 수 있습니다.",
            "타로 팩을 건너뛰어도 골드가 차감되지 않습니다. 언제든 상점으로 돌아와 원하는 카드를 구매할 수 있습니다.",
            "야영지의 점성술 옵션을 건너뛰어도 행동과 점성술 덱의 사용 횟수가 소모되지 않습니다.",
            "타로와 플래닛 마스터 스위치가 모두 꺼져 있으면 다음 시작 시 Mod가 '게임플레이에 영향 없음'으로 표시됩니다. 다시 켜려면 재시작이 필요합니다. (아직 미구현!)",
            "한 장의 카드에 여러 번 인챈트해 보고 싶다면 워크숍의 MultiEnchantment를 사용해 보세요.",
            "모든 기본 및 Mod 인챈트를 우아하게 둘러보고 싶다면 워크숍의 Enchantment Compendium을 사용해 보세요.",
            "도감 왼쪽 아래에서 카드 3D 효과를 켤 수 있습니다. 정말 Balatro스럽죠!",
            "도감에서 카드 상세를 열고 오른쪽 아래 입구를 클릭해 카드 전용 효과를 설정할 수 있습니다.",
            "카드가 어긋나거나 사라지면 3D를 포함한 이 Mod의 모든 효과를 꺼 보세요!",
            "이 Mod를 플레이할 때는 타로 설정 스위치 앞의 5개를 모두 켜는 것을 추천합니다!",
            "타로 효과가 너무 강하다고 느끼면 여사제 설정 스위치를 꺼 보세요.",
            "플래닛 카드가 가져오는 혼돈을 즐기세요!",
            "이 Mod가 마음에 든다면 Steam 워크숍에서 좋아요를 눌러 주세요!",
        };

        private static readonly string[] KoreanThanksTips =
        {
            "한국어 로컬라이제이션을 지원해 주신 SeungjunKim_8224님께 감사드립니다.",
            "제 이메일은 3411737922@qq.com입니다. QQ를 직접 추가하셔도 됩니다.",
            "Balatri 작가님의 아름다운 게임 아트에 감사드립니다!",
            "OLC님의 오픈소스 RItsulib 프로젝트에 감사드립니다!",
            "이 Mod의 다중 버전 지원은 Ritsulib에서 배웠습니다.",
            "이 Mod의 멀티플레이 설정 동기화는 Ritsulib에서 배웠습니다.",
            "Ind_E님의 오픈소스 Balatoeffect 프로젝트에 감사드립니다!",
            "제 친구들에게 감사합니다! 하지만 여러분의 이름은 너무 깁니다!",
            "사신-역방향은 원래 인챈트된 카드 자체도 드로우할 수 없도록 설계되었지만, 나중에 그게 얼마나 어리석은지 깨달았습니다.",
            "교황-역방향은 영원한 모래시계의 쇠퇴 카드를 하위로 낮출 수 있지만, 반대로 상위로 올리지는 않습니다.",
            "광대-정방향의 파워 카드 인챈트는 실제로 카드를 복제하는 방식으로 구현되어 있으며, 약간의 꼼수가 있을 수 있습니다……",
            "별-역방향과 스카이본드 드릴의 상호작용에는 매우 많은 추가 코드가 있습니다!",
            "별-역방향은 원래 별 비용이 없는 카드에도 인챈트할 수 있습니다.",
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
            var ls = new LocString("gameplay_ui", "BAL_CFW_PROGRESS_LINE");
            ls.Add("Expired", IsEliteDivination(flagIndex) && completed >= TarotMarkerSystem.RewardInterval);
            ls.Add("Count", completed);
            string line = ls.GetFormattedText() ?? "BAL_CFW_PROGRESS_LINE";
            return baseText + "\n" + line;
        }

        /// <summary>
        /// 地图 hovertip 的动态描述 LocString（SmartFormat 条件，同塔罗包格式）：精英类三态（{Expired}失效 + {Completed}完成奖励）、
        /// 普通类两态（{Completed}）；非标记类返回 null（调用方继续用静态 BAL_CFW_FLAG_&lt;NAME&gt;_DESC）。
        /// </summary>
        public static LocString? BuildMapDescription(int flagIndex)
        {
            string name = MarkedDivinationName(flagIndex);
            if (string.IsNullOrEmpty(name)) return null;
            var ls = new LocString("gameplay_ui", "BAL_CFW_MAP_" + name);
            if (IsEliteDivination(flagIndex))
            {
                int completed = TarotMarkerSystem.GetCompletedCount(flagIndex);
                ls.Add("Expired", completed >= TarotMarkerSystem.RewardInterval);
                ls.Add("Completed", completed >= 1);
            }
            else
            {
                ls.Add("Completed", TarotMarkerSystem.GetProgressForDisplay(flagIndex) >= 1);
            }
            return ls;
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
            { "BAL_CFW_TAROT_PACK_TITLE", "塔罗包" },
            { "BAL_CFW_TAROT_PACK_DESC", "通过调整配置开关，你可以在商店中或战斗结束后遇到[gold]塔罗包[/gold]。{FoolOn:|（商店中未启用）}\n当前基础抽取数量：{MagicianOn:[blue]3[/blue]|[blue]1[/blue]}\n当前基础卡包价格：{HierophantOn:[blue]75~100[/blue]|[blue]175~200[/blue]}[gold]金币[/gold]\n当前基础价格涨幅：{PriestessOn:每次[blue]0[/blue][gold]金币[/gold]|每次[blue]+50[/blue][gold]金币[/gold]}" },
            { "BAL_CFW_FLAG_MAGICIAN_DESC", "[gold]塔罗包[/gold]抽取的卡牌数量额外[blue]+2[/blue]" },
            { "BAL_CFW_FLAG_HIGHPRIESTESS_DESC", "[gold]塔罗包[/gold]每次购买后价格额外[blue]-50[/blue]" },
            { "BAL_CFW_FLAG_EMPRESS_DESC", "[gold]塔罗包[/gold]中可以遇到逆位塔罗" },
            { "BAL_CFW_FLAG_EMPEROR_DESC", "[gold]塔罗包[/gold]中可以遇到特殊塔罗" },
            { "BAL_CFW_FLAG_HIEROPHANT_DESC", "[gold]塔罗包[/gold]的基础价格额外[blue]-100[/blue]" },
            { "BAL_CFW_FLAG_LOVERS_DESC", "所有[gold]精英敌人房间[/gold]共享[gold]塔罗标记[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_CHARIOT_DESC", "在每一幕标记[blue]1[/blue]个[gold]精英敌人房间[/gold]，房间内的敌人首次对你造成未被格挡的伤害后，额外给予[blue]1[/blue]层[gold]易伤[/gold]。\n完成[blue]2[/blue]个标记后失效，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_STRENGTH_DESC", "在每一幕标记[blue]1[/blue]个[gold]精英敌人房间[/gold]，房间内的敌人首次对你造成未被格挡的伤害后，额外给予[blue]1[/blue]层[gold]虚弱[/gold]。\n完成[blue]2[/blue]个标记后失效，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_HERMIT_DESC", "在每一幕标记[blue]1[/blue]个[gold]精英敌人房间[/gold]，房间内的敌人在战斗开始时，获得[blue]10%[/blue]生命值上限的[gold]隐者-逆[/gold]。\n完成[blue]2[/blue]个标记后失效，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_WHEELOFFORTUNE_DESC", "占卜-命运之轮。「开发中」" },
            { "BAL_CFW_FLAG_JUSTICE_DESC", "在每一幕标记[blue]3[/blue]个[gold]普通敌人房间[/gold]，在这场战斗中，每回合打出的第一张[gold]攻击牌[/gold]被[gold]消耗[/gold]。\n每完成[blue]2[/blue]个标记，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_HANGEDMAN_DESC", "在每一幕标记[blue]3[/blue]个[gold]普通敌人房间[/gold]，在这场战斗中，每回合打出的第一张[gold]技能牌[/gold]被[gold]消耗[/gold]。\n每完成[blue]2[/blue]个标记，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_DEATH_DESC", "在每一幕标记[blue]3[/blue]个[gold]普通敌人房间[/gold]，在这场战斗中，每当你打出[gold]能力牌[/gold]时，立刻[red]结束你的回合[/red]。\n每完成[blue]2[/blue]个标记，获得一次[gold]塔罗牌奖励[/gold]。「开发中」" },
            { "BAL_CFW_FLAG_TEMPERANCE_DESC", "每当你打出一张牌，获得[blue]1[/blue]层[gold]节制-逆[/gold]。\n战斗胜利时不再获得[gold]金币[/gold]奖励。「开发中」" },
            { "BAL_CFW_TAROT_REWARD_DESC", "选择一次塔罗牌奖励。" },
            { "BAL_CFW_FLAG_JUDGEMENT_DESC", "占卜-审判。「开发中」" },
            { "BAL_CFW_FLAG_DEVIL_DESC", "你的[gold]生命[/gold]可以超过[gold]最大生命值[/gold]，你不会因为失去[gold]最大生命值[/gold]而失去[gold]生命[/gold]。\n每当你在[gold]休息处[/gold]休息时，[red]失去[/red][blue]6[/blue]点[gold]最大生命值[/gold]。「开发中」" },
            { "BAL_CFW_DEVIL_REST_HEAL_DESC", "[red]失去[/red][blue]6[/blue]点最大生命值。" },
            { "BAL_CFW_FLAG_STAR_DESC", "占卜-星星。「开发中」" },
            { "BAL_CFW_FLAG_SUN_DESC", "占卜-太阳。「开发中」" },
            { "BAL_CFW_FLAG_MOON_DESC", "每次你的[gold]抽牌堆[/gold]打乱洗牌前，丢弃所有[gold]手牌[/gold]，洗牌后，额外抽等量的牌。「开发中」" },
            { "BAL_CFW_FLAG_WORLD_DESC", "占卜-世界。「开发中」" },
            { "BAL_CFW_FLAG_FOOL_DESC", "在商店中可以遇到[gold]塔罗包[/gold]" },
            { "BAL_CFW_FLAG_TOWER_DESC", "进阶之灾现在可以打出。\n进阶之灾被[gold]消耗[/gold]时，[gold]消耗[/gold]你的所有[gold]手牌[/gold]。「开发中」" },
            { "BAL_CFW_TOWER_CARD_DESC", "这张牌被[gold]消耗[/gold]时，额外[gold]消耗[/gold]你的所有手牌。「开发中」" },

            // ── 标记占卜动态文本（设置界面动态行 + 地图 hovertip 分状态文本） ──
            { "BAL_CFW_PROGRESS_LINE", "{Expired:已失效。|当前已完成[blue]{Count}[/blue]。}" },
            { "BAL_CFW_MAP_CHARIOT", "{Expired:这个标记已失效。|这个房间的敌人首次对你造成未被格挡的伤害时，额外给予[blue]1[/blue]层[gold]易伤[/gold]。{Completed:\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。|}}" },
            { "BAL_CFW_MAP_STRENGTH", "{Expired:这个标记已失效。|这个房间的敌人首次对你造成未被格挡的伤害时，额外给予[blue]1[/blue]层[gold]虚弱[/gold]。{Completed:\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。|}}" },
            { "BAL_CFW_MAP_HERMIT", "{Expired:这个标记已失效。|这个房间的敌人在战斗开始时，获得[blue]10%[/blue]生命值上限的[gold]隐者-逆[/gold]。{Completed:\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。|}}" },
            { "BAL_CFW_MAP_JUSTICE", "在这场战斗中，你每回合打出的第一张[gold]攻击牌[/gold]被[gold]消耗[/gold]。{Completed:\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。|}" },
            { "BAL_CFW_MAP_HANGEDMAN", "在这场战斗中，你每回合打出的第一张[gold]技能牌[/gold]被[gold]消耗[/gold]。{Completed:\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。|}" },
            { "BAL_CFW_MAP_DEATH", "在这场战斗中，每当你打出[gold]能力牌[/gold]时，立刻[red]结束你的回合[/red]。{Completed:\n完成下一场战斗后，获得[blue]1[/blue]次特殊的[gold]塔罗牌奖励[/gold]。|}" },
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
            { "BAL_CFW_TAROT_PACK_TITLE", "Tarot Pack" },
            { "BAL_CFW_TAROT_PACK_DESC", "Adjust these toggles to encounter [gold]Tarot packs[/gold] in shops or after combat.{FoolOn:| Not enabled in shops}\nBase cards drawn: {MagicianOn:[blue]3[/blue]|[blue]1[/blue]}\nBase pack price: {HierophantOn:[blue]75~100[/blue]|[blue]175~200[/blue]} [gold]Gold[/gold]\nBase price increase: {PriestessOn:[blue]0[/blue] [gold]Gold[/gold] each|[blue]+50[/blue] [gold]Gold[/gold] each}" },
            { "BAL_CFW_FLAG_MAGICIAN_DESC", "[gold]Tarot packs[/gold] draw [blue]+2[/blue] cards" },
            { "BAL_CFW_FLAG_HIGHPRIESTESS_DESC", "[gold]Tarot pack[/gold] price [blue]-50[/blue] after each purchase" },
            { "BAL_CFW_FLAG_EMPRESS_DESC", "Reversed tarot can appear in [gold]tarot packs[/gold]" },
            { "BAL_CFW_FLAG_EMPEROR_DESC", "Special character tarot can appear in [gold]tarot packs[/gold]" },
            { "BAL_CFW_FLAG_HIEROPHANT_DESC", "[gold]Tarot pack[/gold] base price [blue]-100[/blue]" },
            { "BAL_CFW_FLAG_LOVERS_DESC", "All [gold]elite enemy rooms[/gold] share [gold]tarot markers[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_CHARIOT_DESC", "Marks [blue]1[/blue] [gold]elite enemy room[/gold] per act.\nEnemies there inflict [blue]1[/blue] [gold]Vulnerable[/gold] the first time they deal unblocked damage to you.\nAfter [blue]2[/blue] marks, this divination expires and you gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_STRENGTH_DESC", "Marks [blue]1[/blue] [gold]elite enemy room[/gold] per act.\nEnemies there inflict [blue]1[/blue] [gold]Weak[/gold] the first time they deal unblocked damage to you.\nAfter [blue]2[/blue] marks, this divination expires and you gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_HERMIT_DESC", "Marks [blue]1[/blue] [gold]elite enemy room[/gold] per act.\nEnemies there start combat with [gold]Hermit - Reversed[/gold] equal to [blue]10%[/blue] of their Max HP.\nAfter [blue]2[/blue] marks, this divination expires and you gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_WHEELOFFORTUNE_DESC", "Divination - Wheel of Fortune. (In Development)" },
            { "BAL_CFW_FLAG_JUSTICE_DESC", "Marks [blue]3[/blue] [gold]normal enemy rooms[/gold] per act.\nIn that combat, the first [gold]Attack[/gold] you play each turn is [gold]Exhausted[/gold].\nFor every [blue]2[/blue] marks completed, gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_HANGEDMAN_DESC", "Marks [blue]3[/blue] [gold]normal enemy rooms[/gold] per act.\nIn that combat, the first [gold]Skill[/gold] you play each turn is [gold]Exhausted[/gold].\nFor every [blue]2[/blue] marks completed, gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_DEATH_DESC", "Marks [blue]3[/blue] [gold]normal enemy rooms[/gold] per act.\nIn that combat, whenever you play a [gold]Power[/gold] card, [red]end your turn[/red] immediately.\nFor every [blue]2[/blue] marks completed, gain a [gold]tarot reward[/gold]. (In Development)" },
            { "BAL_CFW_FLAG_TEMPERANCE_DESC", "Whenever you play a card, gain [blue]1[/blue] [gold]Temperance - Reversed[/gold].\nYou no longer gain [gold]Gold[/gold] from combat victories. (In Development)" },
            { "BAL_CFW_TAROT_REWARD_DESC", "Choose a tarot card reward." },
            { "BAL_CFW_FLAG_JUDGEMENT_DESC", "Divination - Judgement. (In Development)" },
            { "BAL_CFW_FLAG_DEVIL_DESC", "Your [gold]HP[/gold] can exceed your [gold]Max HP[/gold], and you don't lose [gold]HP[/gold] when you lose [gold]Max HP[/gold].\nWhenever you [gold]Rest[/gold] at a [gold]Rest Site[/gold], [red]lose[/red] [blue]6[/blue] [gold]Max HP[/gold]. (In Development)" },
            { "BAL_CFW_DEVIL_REST_HEAL_DESC", "[red]Lose[/red] [blue]6[/blue] Max HP." },
            { "BAL_CFW_FLAG_STAR_DESC", "Divination - Star. (In Development)" },
            { "BAL_CFW_FLAG_SUN_DESC", "Divination - Sun. (In Development)" },
            { "BAL_CFW_FLAG_MOON_DESC", "Before your [gold]Draw Pile[/gold] is shuffled, discard all cards in your [gold]Hand[/gold]. After the shuffle, draw an equal number of cards. (In Development)" },
            { "BAL_CFW_FLAG_WORLD_DESC", "Divination - World. (In Development)" },
            { "BAL_CFW_FLAG_FOOL_DESC", "[gold]Tarot packs[/gold] can appear in shops" },
            { "BAL_CFW_FLAG_TOWER_DESC", "Ascender's Bane can now be played.\nWhen Ascender's Bane is [gold]Exhausted[/gold], [gold]Exhaust[/gold] all cards in your [gold]Hand[/gold]. (In Development)" },
            { "BAL_CFW_TOWER_CARD_DESC", "When this card is [gold]Exhausted[/gold], also [gold]Exhaust[/gold] all cards in your hand. (In Development)" },

            // ── 标记占卜动态文本（设置界面动态行 + 地图 hovertip 分状态文本） ──
            { "BAL_CFW_PROGRESS_LINE", "{Expired:Expired.|Completed [blue]{Count}[/blue] so far.}" },
            { "BAL_CFW_MAP_CHARIOT", "{Expired:This marker has expired.|The first time enemies in this room deal unblocked damage to you, gain [blue]1[/blue] [gold]Vulnerable[/gold].{Completed:\nAfter completing the next combat, gain a special [gold]tarot reward[/gold].|}}" },
            { "BAL_CFW_MAP_STRENGTH", "{Expired:This marker has expired.|The first time enemies in this room deal unblocked damage to you, gain [blue]1[/blue] [gold]Weak[/gold].{Completed:\nAfter completing the next combat, gain a special [gold]tarot reward[/gold].|}}" },
            { "BAL_CFW_MAP_HERMIT", "{Expired:This marker has expired.|Enemies in this room start combat with [gold]Hermit - Reversed[/gold] equal to [blue]10%[/blue] of their Max HP.{Completed:\nAfter completing the next combat, gain a special [gold]tarot reward[/gold].|}}" },
            { "BAL_CFW_MAP_JUSTICE", "In this combat, the first [gold]Attack[/gold] you play each turn is [gold]Exhausted[/gold].{Completed:\nAfter completing the next combat, gain a special [gold]tarot reward[/gold].|}" },
            { "BAL_CFW_MAP_HANGEDMAN", "In this combat, the first [gold]Skill[/gold] you play each turn is [gold]Exhausted[/gold].{Completed:\nAfter completing the next combat, gain a special [gold]tarot reward[/gold].|}" },
            { "BAL_CFW_MAP_DEATH", "In this combat, whenever you play a [gold]Power[/gold] card, [red]end your turn[/red].{Completed:\nAfter completing the next combat, gain a special [gold]tarot reward[/gold].|}" },
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
            { "BAL_CFW_TAROT_PACK_TITLE", "タロットパック" },
            { "BAL_CFW_TAROT_PACK_DESC", "設定スイッチの調整で、ショップまたは戦闘終了後に[gold]タロットパック[/gold]に出会えます。{FoolOn:|（ショップでは無効）}\n基本の引く枚数：{MagicianOn:[blue]3[/blue]|[blue]1[/blue]}\n基本パック価格：{HierophantOn:[blue]75~100[/blue]|[blue]175~200[/blue]}[gold]ゴールド[/gold]\n基本価格上昇：{PriestessOn:毎回[blue]0[/blue][gold]ゴールド[/gold]|毎回[blue]+50[/blue][gold]ゴールド[/gold]}" },
            { "BAL_CFW_FLAG_MAGICIAN_DESC", "[gold]タロットパック[/gold]の引く枚数[blue]+2[/blue]" },
            { "BAL_CFW_FLAG_HIGHPRIESTESS_DESC", "[gold]タロットパック[/gold]購入ごとに価格[blue]-50[/blue]" },
            { "BAL_CFW_FLAG_EMPRESS_DESC", "[gold]タロットパック[/gold]に逆位置タロットが出現" },
            { "BAL_CFW_FLAG_EMPEROR_DESC", "[gold]タロットパック[/gold]に専用タロットが出現" },
            { "BAL_CFW_FLAG_HIEROPHANT_DESC", "[gold]タロットパック[/gold]の基本価格[blue]-100[/blue]" },
            { "BAL_CFW_FLAG_LOVERS_DESC", "すべての[gold]エリートの部屋[/gold]が[gold]タロットマーカー[/gold]を共有する。「開発中」" },
            { "BAL_CFW_FLAG_CHARIOT_DESC", "各層で[gold]エリートの部屋[/gold]を[blue]1[/blue]つマークする。\nその部屋の敵が初めてブロックされていないダメージを与えた後、さらに[gold]弱体[/gold][blue]1[/blue]を付与する。\nマークを[blue]2[/blue]つ完了すると失効し、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_STRENGTH_DESC", "各層で[gold]エリートの部屋[/gold]を[blue]1[/blue]つマークする。\nその部屋の敵が初めてブロックされていないダメージを与えた後、さらに[gold]脱力[/gold][blue]1[/blue]を付与する。\nマークを[blue]2[/blue]つ完了すると失効し、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_HERMIT_DESC", "各層で[gold]エリートの部屋[/gold]を[blue]1[/blue]つマークする。\nその部屋の敵は戦闘開始時、最大HPの[blue]10%[/blue]分の[gold]隠者-逆[/gold]を得る。\nマークを[blue]2[/blue]つ完了すると失効し、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_WHEELOFFORTUNE_DESC", "占い-運命の輪。「開発中」" },
            { "BAL_CFW_FLAG_JUSTICE_DESC", "各層で[gold]通常の敵の部屋[/gold]を[blue]3[/blue]つマークする。\nこの戦闘中、毎ターン最初にプレイした[gold]アタック[/gold]が[gold]廃棄[/gold]される。\nマークを[blue]2[/blue]つ完了するごとに、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_HANGEDMAN_DESC", "各層で[gold]通常の敵の部屋[/gold]を[blue]3[/blue]つマークする。\nこの戦闘中、毎ターン最初にプレイした[gold]スキル[/gold]が[gold]廃棄[/gold]される。\nマークを[blue]2[/blue]つ完了するごとに、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_DEATH_DESC", "各層で[gold]通常の敵の部屋[/gold]を[blue]3[/blue]つマークする。\nこの戦闘中、[gold]パワー[/gold]をプレイするたびに、即座に[red]ターンを終了[/red]する。\nマークを[blue]2[/blue]つ完了するごとに、[gold]タロット報酬[/gold]を一回獲得する。「開発中」" },
            { "BAL_CFW_FLAG_TEMPERANCE_DESC", "カードを[blue]1[/blue]枚プレイするたびに、[gold]節制-逆[/gold]を[blue]1[/blue]獲得する。\n戦闘勝利時の[gold]ゴールド[/gold]報酬を獲得しなくなる。「開発中」" },
            { "BAL_CFW_TAROT_REWARD_DESC", "タロット報酬を一回選択する。" },
            { "BAL_CFW_FLAG_JUDGEMENT_DESC", "占い-審判。「開発中」" },
            { "BAL_CFW_FLAG_DEVIL_DESC", "あなたの[gold]HP[/gold]は[gold]最大HP[/gold]を超えて保持でき、[gold]最大HP[/gold]を失っても[gold]HP[/gold]は減らない。\n[gold]休憩所[/gold]で休憩するたびに、[gold]最大HP[/gold]を[blue]6[/blue][red]失う[/red]。「開発中」" },
            { "BAL_CFW_DEVIL_REST_HEAL_DESC", "最大HPを[blue]6[/blue][red]失う[/red]。" },
            { "BAL_CFW_FLAG_STAR_DESC", "占い-星。「開発中」" },
            { "BAL_CFW_FLAG_SUN_DESC", "占い-太陽。「開発中」" },
            { "BAL_CFW_FLAG_MOON_DESC", "あなたの[gold]山札[/gold]がシャッフルされる前に、すべての[gold]手札[/gold]を捨てる。シャッフル後、同じ枚数を追加で引く。「開発中」" },
            { "BAL_CFW_FLAG_WORLD_DESC", "占い-世界。「開発中」" },
            { "BAL_CFW_FLAG_FOOL_DESC", "ショップで[gold]タロットパック[/gold]に遭遇できる" },
            { "BAL_CFW_FLAG_TOWER_DESC", "アセンダーの災厄がプレイ可能になる。\nアセンダーの災厄が[gold]廃棄[/gold]された時、[gold]手札[/gold]をすべて[gold]廃棄[/gold]する。「開発中」" },
            { "BAL_CFW_TOWER_CARD_DESC", "このカードが[gold]廃棄[/gold]された時、手札をすべて[gold]廃棄[/gold]する。「開発中」" },

            // ── 标记占卜动态文本（设置界面动态行 + 地图 hovertip 分状态文本） ──
            { "BAL_CFW_PROGRESS_LINE", "{Expired:失効済み。|現在の完了数：[blue]{Count}[/blue]。}" },
            { "BAL_CFW_MAP_CHARIOT", "{Expired:このマーカーは失効しました。|この部屋の敵が初めてブロックされていないダメージを与えた時、さらに[gold]弱体[/gold][blue]1[/blue]を付与する。{Completed:\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。|}}" },
            { "BAL_CFW_MAP_STRENGTH", "{Expired:このマーカーは失効しました。|この部屋の敵が初めてブロックされていないダメージを与えた時、さらに[gold]脱力[/gold][blue]1[/blue]を付与する。{Completed:\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。|}}" },
            { "BAL_CFW_MAP_HERMIT", "{Expired:このマーカーは失効しました。|この部屋の敵は戦闘開始時、最大HPの[blue]10%[/blue]分の[gold]隠者-逆[/gold]を得る。{Completed:\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。|}}" },
            { "BAL_CFW_MAP_JUSTICE", "この戦闘中、毎ターン最初にプレイした[gold]アタック[/gold]が[gold]廃棄[/gold]される。{Completed:\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。|}" },
            { "BAL_CFW_MAP_HANGEDMAN", "この戦闘中、毎ターン最初にプレイした[gold]スキル[/gold]が[gold]廃棄[/gold]される。{Completed:\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。|}" },
            { "BAL_CFW_MAP_DEATH", "この戦闘中、[gold]パワー[/gold]をプレイするたびに、即座に[red]ターンを終了[/red]する。{Completed:\n次の戦闘を完了すると、特別な[gold]タロット報酬[/gold]を一回獲得する。|}" },
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
            { "BAL_CFW_TAROT_PACK_TITLE", "타로 팩" },
            { "BAL_CFW_TAROT_PACK_DESC", "설정 스위치를 조정하면 상점 또는 전투 종료 후 [gold]타로 팩[/gold]을 만날 수 있습니다.{FoolOn:| 상점에서 비활성화}\n기본 뽑는 카드 수: {MagicianOn:[blue]3[/blue]|[blue]1[/blue]}\n기본 팩 가격: {HierophantOn:[blue]75~100[/blue]|[blue]175~200[/blue]} [gold]골드[/gold]\n기본 가격 상승: {PriestessOn:매번 [blue]0[/blue] [gold]골드[/gold]|매번 [blue]+50[/blue] [gold]골드[/gold]}" },
            { "BAL_CFW_FLAG_MAGICIAN_DESC", "[gold]타로 팩[/gold] 카드 뽑기 [blue]+2[/blue]" },
            { "BAL_CFW_FLAG_HIGHPRIESTESS_DESC", "[gold]타로 팩[/gold] 구매 후 가격 [blue]-50[/blue]" },
            { "BAL_CFW_FLAG_EMPRESS_DESC", "[gold]타로 팩[/gold]에 역위 타로 등장" },
            { "BAL_CFW_FLAG_EMPEROR_DESC", "[gold]타로 팩[/gold]에 특수 타로 등장" },
            { "BAL_CFW_FLAG_HIEROPHANT_DESC", "[gold]타로 팩[/gold] 기본 가격 [blue]-100[/blue]" },
            { "BAL_CFW_FLAG_LOVERS_DESC", "모든 [gold]정예 적 방[/gold]이 [gold]타로 마커[/gold]를 공유합니다.「개발 중」" },
            { "BAL_CFW_FLAG_CHARIOT_DESC", "각 막에서 [gold]엘리트 방[/gold] [blue]1[/blue]개를 표시합니다.\n해당 방의 적이 처음으로 방어도로 막지 못한 피해를 준 후, 추가로 [gold]취약[/gold] [blue]1[/blue]을 부여합니다.\n표시 [blue]2[/blue]개 완료 시 비활성화되며 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_STRENGTH_DESC", "각 막에서 [gold]엘리트 방[/gold] [blue]1[/blue]개를 표시합니다.\n해당 방의 적이 처음으로 방어도로 막지 못한 피해를 준 후, 추가로 [gold]약화[/gold] [blue]1[/blue]을 부여합니다.\n표시 [blue]2[/blue]개 완료 시 비활성화되며 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_HERMIT_DESC", "각 막에서 [gold]엘리트 방[/gold] [blue]1[/blue]개를 표시합니다.\n해당 방의 적이 전투 시작 시 최대 체력의 [blue]10%[/blue]만큼 [gold]은둔자-역방향[/gold]을 얻습니다.\n표시 [blue]2[/blue]개 완료 시 비활성화되며 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_WHEELOFFORTUNE_DESC", "점괘-운명의 수레바퀴。「개발 중」" },
            { "BAL_CFW_FLAG_JUSTICE_DESC", "각 막에서 [gold]일반 적 방[/gold] [blue]3[/blue]개를 표시합니다.\n해당 전투에서 매 턴 처음 사용하는 [gold]공격 카드[/gold]가 [gold]소멸[/gold]됩니다.\n표시 [blue]2[/blue]개 완료 시마다 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_HANGEDMAN_DESC", "각 막에서 [gold]일반 적 방[/gold] [blue]3[/blue]개를 표시합니다.\n해당 전투에서 매 턴 처음 사용하는 [gold]스킬 카드[/gold]가 [gold]소멸[/gold]됩니다.\n표시 [blue]2[/blue]개 완료 시마다 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_DEATH_DESC", "각 막에서 [gold]일반 적 방[/gold] [blue]3[/blue]개를 표시합니다.\n해당 전투에서 [gold]파워 카드[/gold]를 사용할 때마다 즉시 [red]턴을 종료[/red]합니다.\n표시 [blue]2[/blue]개 완료 시마다 [gold]타로 보상[/gold]을 한 번 획득합니다.「개발 중」" },
            { "BAL_CFW_FLAG_TEMPERANCE_DESC", "카드를 [blue]1[/blue]장 사용할 때마다 [gold]절제-역방향[/gold] [blue]1[/blue]을 얻습니다.\n전투 승리 시 [gold]골드[/gold] 보상을 받지 못합니다.「개발 중」" },
            { "BAL_CFW_TAROT_REWARD_DESC", "타로 보상을 한 번 선택합니다." },
            { "BAL_CFW_FLAG_JUDGEMENT_DESC", "점괘-심판。「개발 중」" },
            { "BAL_CFW_FLAG_DEVIL_DESC", "체력이 [gold]최대 체력[/gold]을 초과하여 유지될 수 있으며, [gold]최대 체력[/gold]을 잃어도 [gold]체력[/gold]은 줄어들지 않습니다.\n[gold]휴식 장소[/gold]에서 휴식할 때마다 [gold]최대 체력[/gold]을 [blue]6[/blue] [red]잃습니다[/red].「개발 중」" },
            { "BAL_CFW_DEVIL_REST_HEAL_DESC", "최대 체력을 [blue]6[/blue] [red]잃습니다[/red]." },
            { "BAL_CFW_FLAG_STAR_DESC", "점괘-별。「개발 중」" },
            { "BAL_CFW_FLAG_SUN_DESC", "점괘-태양。「개발 중」" },
            { "BAL_CFW_FLAG_MOON_DESC", "당신의 [gold]뽑을 카드 더미[/gold]를 섞기 전에 모든 [gold]손[/gold]의 카드를 버리고, 섞은 후 같은 수만큼 카드를 추가로 뽑습니다.「개발 중」" },
            { "BAL_CFW_FLAG_WORLD_DESC", "점괘-세계。「개발 중」" },
            { "BAL_CFW_FLAG_FOOL_DESC", "상점에서 [gold]타로 팩[/gold]을 만날 수 있음" },
            { "BAL_CFW_FLAG_TOWER_DESC", "등반자의 골칫거리를 사용할 수 있게 됩니다.\n등반자의 골칫거리가 [gold]소멸[/gold]될 때, 손에 있는 모든 카드를 [gold]소멸[/gold]시킵니다.「개발 중」" },
            { "BAL_CFW_TOWER_CARD_DESC", "이 카드가 [gold]소멸[/gold]될 때, 손에 있는 모든 카드를 추가로 [gold]소멸[/gold]시킵니다.「개발 중」" },

            // ── 标记占卜动态文本（设置界面动态行 + 地图 hovertip 分状态文本） ──
            { "BAL_CFW_PROGRESS_LINE", "{Expired:비활성화됨.|지금까지 [blue]{Count}[/blue]개 완료.}" },
            { "BAL_CFW_MAP_CHARIOT", "{Expired:이 마커는 비활성화되었습니다.|이 방의 적이 처음으로 방어도로 막지 못한 피해를 준 경우, 추가로 [gold]취약[/gold] [blue]1[/blue]을 부여합니다.{Completed:\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다.|}}" },
            { "BAL_CFW_MAP_STRENGTH", "{Expired:이 마커는 비활성화되었습니다.|이 방의 적이 처음으로 방어도로 막지 못한 피해를 준 경우, 추가로 [gold]약화[/gold] [blue]1[/blue]을 부여합니다.{Completed:\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다.|}}" },
            { "BAL_CFW_MAP_HERMIT", "{Expired:이 마커는 비활성화되었습니다.|이 방의 적은 전투 시작 시 최대 체력의 [blue]10%[/blue]만큼 [gold]은둔자-역방향[/gold]을 얻습니다.{Completed:\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다.|}}" },
            { "BAL_CFW_MAP_JUSTICE", "이 전투에서 매 턴 처음 사용하는 [gold]공격 카드[/gold]가 [gold]소멸[/gold]됩니다.{Completed:\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다.|}" },
            { "BAL_CFW_MAP_HANGEDMAN", "이 전투에서 매 턴 처음 사용하는 [gold]스킬 카드[/gold]가 [gold]소멸[/gold]됩니다.{Completed:\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다.|}" },
            { "BAL_CFW_MAP_DEATH", "이 전투에서 [gold]파워 카드[/gold]를 사용할 때마다 즉시 [red]턴을 종료[/red]합니다.{Completed:\n다음 전투를 완료하면 특별한 [gold]타로 보상[/gold]을 한 번 획득합니다.|}" },
        };
    }
}
