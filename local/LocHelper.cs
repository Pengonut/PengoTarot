
#nullable enable
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace PengoTarot
{
    public static class TarLocHelper
    {
        public static void InjectAll()
        {
            var loc = LocManager.Instance;

            var cardLibraryTable = loc.GetTable("card_library");
            cardLibraryTable.MergeWith(new Dictionary<string, string>
            {
                { "POOL_TAROT_TIP", "塔罗牌。" },
                { "POOL_PLANET_TIP", "星球牌。" },
            });

            
            var cardsTable = loc.GetTable("cards");
            cardsTable.MergeWith(new Dictionary<string, string>
            {
                
                { "TAR_FOOL_UPRIGHT.title", "0-愚者-正" },
                { "TAR_FOOL_UPRIGHT.description", "选择一张牌，为其[gold]附魔[/gold]\n[purple]愚者-正[/purple]。" },
                { "TAR_FOOL_REVERSED.title", "0-愚者-逆" },
                { "TAR_FOOL_REVERSED.description", "\n选择一张耗能不为[blue]X[/blue]的牌，为其[gold]附魔[/gold]\n[purple]愚者-逆[/purple]。" },
                
                { "TAR_MAGICIAN_UPRIGHT.title", "I-魔术师-正" },
                { "TAR_MAGICIAN_UPRIGHT.description", "选择五张没有[gold]消耗[/gold]的技能或攻击牌，为其[gold]附魔[/gold]\n[purple]魔术师-正[/purple]。" },
                { "TAR_MAGICIAN_REVERSED.title", "I-魔术师-逆" },
                { "TAR_MAGICIAN_REVERSED.description", "\n选择一张没有[gold]消耗[/gold]的技能或攻击牌，为其[gold]附魔[/gold]\n[purple]魔术师-逆[/purple]。" },
                
                { "TAR_HIGH_PRIESTESS_UPRIGHT.title", "II-女祭司-正" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT.description", "选择三张没有[gold]虚无[/gold]的牌，为其[gold]附魔[/gold]\n[purple]女祭司-正[/purple]。" },
                { "TAR_HIGH_PRIESTESS_REVERSED.title", "II-女祭司-逆" },
                { "TAR_HIGH_PRIESTESS_REVERSED.description", "\n选择一张没有[gold]虚无[/gold]的牌，为其[gold]附魔[/gold]\n[purple]女祭司-逆[/purple]。" },
                
                { "TAR_EMPRESS_UPRIGHT.title", "III-皇后-正" },
                { "TAR_EMPRESS_UPRIGHT.description", "选择一张牌，为其[gold]附魔[/gold]\n[purple]皇后-正[/purple]。" },
                { "TAR_EMPRESS_REVERSED.title", "III-皇后-逆" },
                { "TAR_EMPRESS_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]皇后-逆[/purple]。" },
                
                { "TAR_EMPEROR_UPRIGHT.title", "IV-皇帝-正" },
                { "TAR_EMPEROR_UPRIGHT.description", "选择一张牌，为其[gold]附魔[/gold]\n[purple]皇帝-正[/purple]。" },
                { "TAR_EMPEROR_REVERSED.title", "IV-皇帝-逆" },
                { "TAR_EMPEROR_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]皇帝-逆[/purple]。" },
                
                { "TAR_HIEROPHANT_UPRIGHT.title", "V-教皇-正" },
                { "TAR_HIEROPHANT_UPRIGHT.description", "选择一张牌，为其[gold]附魔[/gold]\n[purple]教皇-正[/purple]。" },
                { "TAR_HIEROPHANT_REVERSED.title", "V-教皇-逆" },
                { "TAR_HIEROPHANT_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]教皇-逆[/purple]。" },
                
                { "TAR_LOVERS_UPRIGHT.title", "VI-恋人-正" },
                { "TAR_LOVERS_UPRIGHT.description", "选择两张牌，为其[gold]附魔[/gold]\n[purple]恋人-正[/purple]。" },
                { "TAR_LOVERS_REVERSED.title", "VI-恋人-逆" },
                { "TAR_LOVERS_REVERSED.description", "\n选择两张耗能不为[blue]X[/blue]的牌，为其[gold]附魔[/gold]\n[purple]恋人-逆[/purple]。" },
                
                { "TAR_CHARIOT_UPRIGHT.title", "VII-战车-正" },
                { "TAR_CHARIOT_UPRIGHT.description", "选择一张非多段攻击牌，为其[gold]附魔[/gold]\n[purple]战车-正[/purple]。" },
                { "TAR_CHARIOT_REVERSED.title", "VII-战车-逆" },
                { "TAR_CHARIOT_REVERSED.description", "\n选择一张多段攻击牌，为其[gold]附魔[/gold]\n[purple]战车-逆[/purple]。" },
                
                { "TAR_STRENGTH_UPRIGHT.title", "VIII-力量-正" },
                { "TAR_STRENGTH_UPRIGHT.description", "选择一张非多段攻击牌，为其[gold]附魔[/gold]\n[purple]力量-正[/purple]。" },
                { "TAR_STRENGTH_REVERSED.title", "VIII-力量-逆" },
                { "TAR_STRENGTH_REVERSED.description", "\n选择一张多段攻击牌，为其[gold]附魔[/gold]\n[purple]力量-逆[/purple]。" },
                
                { "TAR_HERMIT_UPRIGHT.title", "IX-隐者-正" },
                { "TAR_HERMIT_UPRIGHT.description", "选择两张技能或攻击牌，为其[gold]附魔[/gold]\n[purple]隐者-正[/purple]。" },
                { "TAR_HERMIT_REVERSED.title", "IX-隐者-逆" },
                { "TAR_HERMIT_REVERSED.description", "\n选择两张技能或攻击牌，为其[gold]附魔[/gold]\n[purple]隐者-逆[/purple]。" },
                
                { "TAR_WHEEL_OF_FORTUNE_UPRIGHT.title", "X-命运之轮-正" },
                { "TAR_WHEEL_OF_FORTUNE_UPRIGHT.description", "[red]失去[/red]11点生命。\n[gold]复制[/gold]一个随机非先古遗物。" },
                { "TAR_WHEEL_OF_FORTUNE_REVERSED.title", "X-命运之轮-逆" },
                { "TAR_WHEEL_OF_FORTUNE_REVERSED.description", "\n随机[red]摧毁[/red]两个非先古遗物，然后将一个随机非先古遗物[gold]复制[/gold]三次。" },
                
                { "TAR_JUSTICE_UPRIGHT.title", "XI-正义-正" },
                { "TAR_JUSTICE_UPRIGHT.description", "选择一张没有[gold]消耗[/gold]的攻击牌，为其[gold]附魔[/gold]\n[purple]正义-正[/purple]。" },
                { "TAR_JUSTICE_REVERSED.title", "XI-正义-逆" },
                { "TAR_JUSTICE_REVERSED.description", "\n选择一张没有[gold]消耗[/gold]的攻击牌，为其[gold]附魔[/gold]\n[purple]正义-逆[/purple]。" },
                
                { "TAR_HANGED_MAN_UPRIGHT.title", "XII-倒吊人-正" },
                { "TAR_HANGED_MAN_UPRIGHT.description", "选择一张带[gold]消耗[/gold]的牌，为其[gold]附魔[/gold]\n[purple]倒吊人-正[/purple]。" },
                { "TAR_HANGED_MAN_REVERSED.title", "XII-倒吊人-逆" },
                { "TAR_HANGED_MAN_REVERSED.description", "\n选择一张带[gold]消耗[/gold]的牌，为其[gold]附魔[/gold]\n[purple]倒吊人-逆[/purple]。" },
                
                { "TAR_DEATH_UPRIGHT.title", "XIII-死神-正" },
                { "TAR_DEATH_UPRIGHT.description", "选择一张牌，为其[gold]附魔[/gold]\n[purple]死神-正[/purple]。" },
                { "TAR_DEATH_REVERSED.title", "XIII-死神-逆" },
                { "TAR_DEATH_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]死神-逆[/purple]。" },
                
                { "TAR_TEMPERANCE_UPRIGHT.title", "XIV-节制-正" },
                { "TAR_TEMPERANCE_UPRIGHT.description", "选择一张牌，为其[gold]附魔[/gold]\n[purple]节制-正[/purple]。" },
                { "TAR_TEMPERANCE_REVERSED.title", "XIV-节制-逆" },
                { "TAR_TEMPERANCE_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]节制-逆[/purple]。" },
                
                { "TAR_DEVIL_UPRIGHT.title", "XV-恶魔-正" },
                { "TAR_DEVIL_UPRIGHT.description", "选择一张牌，为其[gold]附魔[/gold]\n[purple]恶魔-正[/purple]。" },
                { "TAR_DEVIL_REVERSED.title", "XV-恶魔-逆" },
                { "TAR_DEVIL_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]恶魔-逆[/purple]。" },
                
                { "TAR_TOWER_UPRIGHT.title", "XVI-高塔-正" },
                { "TAR_TOWER_UPRIGHT.description", "[red]移除[/red]你卡组中所有初始、普通和罕见牌。\n每移除一张，获得45[gold]金币[/gold]。" },
                { "TAR_TOWER_REVERSED.title", "XVI-高塔-逆" },
                { "TAR_TOWER_REVERSED.description", "\n[red]移除[/red]你卡组中所有初始和普通牌。\n每移除一张，获得15[gold]金币[/gold]。" },
                
                { "TAR_STAR_UPRIGHT.title", "XVII-星星-正" },
                { "TAR_STAR_UPRIGHT.description", "选择三张牌，为其[gold]附魔[/gold]\n[purple]星星-正[/purple]。" },
                { "TAR_STAR_REVERSED.title", "XVII-星星-逆" },
                { "TAR_STAR_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]星星-逆[/purple]。" },
                
                { "TAR_MOON_UPRIGHT.title", "XVIII-月亮-正" },
                { "TAR_MOON_UPRIGHT.description", "选择一张技能或攻击牌，为其[gold]附魔[/gold]\n[purple]月亮-正[/purple]。" },
                { "TAR_MOON_REVERSED.title", "XVIII-月亮-逆" },
                { "TAR_MOON_REVERSED.description", "\n选择一张技能或攻击牌，为其[gold]附魔[/gold]\n[purple]月亮-逆[/purple]。" },
                
                { "TAR_SUN_UPRIGHT.title", "XIX-太阳-正" },
                { "TAR_SUN_UPRIGHT.description", "选择三张牌，为其[gold]附魔[/gold]\n[purple]太阳-正[/purple]。" },
                { "TAR_SUN_REVERSED.title", "XIX-太阳-逆" },
                { "TAR_SUN_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]太阳-逆[/purple]。" },
                
                { "TAR_JUDGEMENT_UPRIGHT.title", "XX-审判-正" },
                { "TAR_JUDGEMENT_UPRIGHT.description", "选择两张牌，为其[gold]附魔[/gold]\n[purple]审判-正[/purple]。" },
                { "TAR_JUDGEMENT_REVERSED.title", "XX-审判-逆" },
                { "TAR_JUDGEMENT_REVERSED.description", "\n选择至多二十一张牌，为其[gold]附魔[/gold]\n[purple]审判-逆[/purple]。" },
                
                { "TAR_WORLD_UPRIGHT.title", "XXI-世界-正" },
                { "TAR_WORLD_UPRIGHT.description", "选择一张牌，为其[gold]附魔[/gold]\n[purple]世界-正[/purple]。" },
                { "TAR_WORLD_REVERSED.title", "XXI-世界-逆" },
                { "TAR_WORLD_REVERSED.description", "\n选择一张牌，为其[gold]附魔[/gold]\n[purple]世界-逆[/purple]。" },

                
                
                { "TAR_DEVIL_UPRIGHT_SUB.title", "负片-恶魔-正" },
                { "TAR_DEVIL_UPRIGHT_SUB.description", "选择三张牌，将其[gold]变化[/gold]为铁甲战士的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-恶魔-正[/purple]。" },
                { "TAR_DEVIL_REVERSED_SUB.title", "负片-恶魔-逆" },
                { "TAR_DEVIL_REVERSED_SUB.description", "\n选择三张牌，将其[gold]变化[/gold]为铁甲战士的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-恶魔-逆[/purple]。" },
                
                { "TAR_MOON_UPRIGHT_SUB.title", "负片-月亮-正" },
                { "TAR_MOON_UPRIGHT_SUB.description", "选择三张牌，将其[gold]变化[/gold]为静默猎手的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-月亮-正[/purple]。" },
                { "TAR_MOON_REVERSED_SUB.title", "负片-月亮-逆" },
                { "TAR_MOON_REVERSED_SUB.description", "\n选择三张牌，将其[gold]变化[/gold]为静默猎手的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-月亮-逆[/purple]。" },
                
                { "TAR_STAR_UPRIGHT_SUB.title", "负片-星星-正" },
                { "TAR_STAR_UPRIGHT_SUB.description", "选择三张牌，将其[gold]变化[/gold]为储君的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-星星-正[/purple]。" },
                { "TAR_STAR_REVERSED_SUB.title", "负片-星星-逆" },
                { "TAR_STAR_REVERSED_SUB.description", "\n选择三张牌，将其[gold]变化[/gold]为储君的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-星星-逆[/purple]。" },
                
                { "TAR_SUN_UPRIGHT_SUB.title", "负片-太阳-正" },
                { "TAR_SUN_UPRIGHT_SUB.description", "选择三张牌，将其[gold]变化[/gold]为亡灵契约师的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-太阳-正[/purple]。" },
                { "TAR_SUN_REVERSED_SUB.title", "负片-太阳-逆" },
                { "TAR_SUN_REVERSED_SUB.description", "\n选择三张牌，将其[gold]变化[/gold]为亡灵契约师的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-太阳-逆[/purple]。" },
                
                { "TAR_WORLD_UPRIGHT_SUB.title", "负片-世界-正" },
                { "TAR_WORLD_UPRIGHT_SUB.description", "选择三张牌，将其[gold]变化[/gold]为故障机器人的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-世界-正[/purple]。" },
                { "TAR_WORLD_REVERSED_SUB.title", "负片-世界-逆" },
                { "TAR_WORLD_REVERSED_SUB.description", "\n选择三张牌，将其[gold]变化[/gold]为故障机器人的对应卡牌，为其[gold]附魔[/gold]\n[purple]负片-世界-逆[/purple]。" },
            
            

                { "PLANET_MERCURY.title", "水星" },
                { "PLANET_MERCURY.description", "选择一张能力牌，\n为其[gold]附魔[/gold]\n[purple]水星[/purple]。" },
                { "PLANET_VENUS.title", "金星" },
                { "PLANET_VENUS.description", "选择一张能力牌，\n为其[gold]附魔[/gold]\n[purple]金星[/purple]。" },
                { "PLANET_EARTH.title", "地球" },
                { "PLANET_EARTH.description", "选择一张能力牌，\n为其[gold]附魔[/gold]\n[purple]地球[/purple]。" },
                { "PLANET_MARS.title", "火星" },
                { "PLANET_MARS.description", "选择一张能力牌，\n为其[gold]附魔[/gold]\n[purple]火星[/purple]。" },

                { "PLANET_JUPITER.title", "木星" },
                { "PLANET_JUPITER.description", "选择一张攻击牌，\n为其[gold]附魔[/gold]\n[purple]木星[/purple]。" },
                { "PLANET_SATURN.title", "土星" },
                { "PLANET_SATURN.description", "选择一张攻击牌，\n为其[gold]附魔[/gold]\n[purple]土星[/purple]。" },
                { "PLANET_URANUS.title", "天王星" },
                { "PLANET_URANUS.description", "选择一张攻击牌，\n为其[gold]附魔[/gold]\n[purple]天王星[/purple]。" },
                { "PLANET_NEPTUNE.title", "海王星" },
                { "PLANET_NEPTUNE.description", "选择一张攻击牌，\n为其[gold]附魔[/gold]\n[purple]海王星[/purple]。" },

                { "PLANET_PLUTO.title", "冥王星" },
                { "PLANET_PLUTO.description", "选择一张技能牌，\n为其[gold]附魔[/gold]\n[purple]冥王星[/purple]。" },
                { "PLANET_X.title", "X 行星" },
                { "PLANET_X.description", "选择一张技能牌，\n为其[gold]附魔[/gold]\n[purple]X 行星[/purple]。" },
                { "PLANET_CERES.title", "谷神星" },
                { "PLANET_CERES.description", "选择一张技能牌，\n为其[gold]附魔[/gold]\n[purple]谷神星[/purple]。" },
                { "PLANET_ERIS.title", "阋神星" },
                { "PLANET_ERIS.description", "选择一张技能牌，\n为其[gold]附魔[/gold]\n[purple]阋神星[/purple]。" },
            });

            
            var enchantmentsTable = loc.GetTable("enchantments");
            enchantmentsTable.MergeWith(new Dictionary<string, string>
            {
                
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.description", "每场战斗第一次打出时，将这张牌放回你的[gold]手牌[/gold]。" },
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.title", "愚者-正" },
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.extraCardText", "第一次打出时，将这张牌放回你的[gold]手牌[/gold]。" },
                
                { "TAR_FOOL_REVERSED_ENCHANTMENT.description", "每场战斗第一次打出时，将一张费用增加{energyPrefix:energyIcons(1)}的复制品放入你的[gold]手牌[/gold]。" },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.title", "愚者-逆" },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.extraCardText", "第一次打出时，复制一张费用增加{energyPrefix:energyIcons(1)}的复制品。" },
                
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.description", "这张牌获得[gold]消耗[/gold]。" },
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.title", "魔术师-正" },
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.extraCardText", "魔术师‑正。" },
                
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.description", "这张牌打出后，将被放入你的[gold]抽牌堆[/gold]随机位置。" },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.title", "魔术师-逆" },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.extraCardText", "放置于[gold]抽牌堆[/gold]随机位置。" },
                
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.description", "这张牌获得[gold]虚无[/gold]。" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.title", "女祭司-正" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.extraCardText", "女祭司‑正。" },
                
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.description", "这张牌获得[gold]保留[/gold]。每个回合结束时，如果这张牌在手上，为这张牌左侧的所有[gold]手牌[/gold]添加[gold]虚无[/gold]。" },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.title", "女祭司-逆" },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.extraCardText", "回合结束时，为左侧的手牌添加[gold]虚无[/gold]。" },
                
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.description", "第三次及以后打出时，这张牌获得[gold]重放[/gold][blue]1[/blue]。" },
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.title", "皇后-正" },
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.extraCardText", "已打出{PlayCount}次。" },
                
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.description", "费用减少{energyPrefix:energyIcons(1)}，这张牌第一次进入弃牌堆前无法打出。" },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.title", "皇后-逆" },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.extraCardText", "第一次进入弃牌堆前无法打出。" },
                
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.description", "这张牌获得[gold]保留[/gold]。" },
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.title", "皇帝-正" },
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.extraCardText", "皇帝‑正。" },
                
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.description", "回合结束时，如果这张牌在手上，随机两张[gold]手牌[/gold]获得[gold]保留[/gold]。" },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.title", "皇帝-逆" },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.extraCardText", "回合结束时，随机2张手牌获得[gold]保留[/gold]。" },
                
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.description", "这张牌打出后，[gold]升级[/gold]你的一张[gold]手牌[/gold]。" },
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.title", "教皇-正" },
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.extraCardText", "[gold]升级[/gold]你的一张[gold]手牌[/gold]。" },
                
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.description", "这张牌打出后，[red]降级[/red]你的任意张[gold]手牌[/gold]，每[red]降级[/red]一张，随机[gold]升级[/gold][gold]抽牌堆[/gold]或[gold]弃牌堆[/gold]的三张牌。" },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.title", "教皇-逆" },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.extraCardText", "这张牌不计代价。" },
                
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.description", "这张牌进入[gold]手牌[/gold]时，将另一张附魔[purple]恋人[/purple]的牌从[gold]抽牌堆[/gold]、[gold]弃牌堆[/gold]或[gold]消耗牌堆[/gold]中放入[gold]手牌[/gold]。" },
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.title", "恋人-正" },
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.extraCardText", "这张牌寻求恋人。" },
                
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.description", "这张牌每次打出后，将另一张附魔[purple]恋人[/purple]的牌从[gold]抽牌堆[/gold]、[gold]弃牌堆[/gold]或[gold]消耗牌堆[/gold]中放入[gold]手牌[/gold]，使其本回合费用增加{energyPrefix:energyIcons(1)}。" },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.title", "恋人-逆" },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.extraCardText", "这张牌寻求恋人……？" },
                
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.description", "额外给予[blue]1[/blue]层[gold]易伤[/gold]。" },
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.title", "战车-正" },
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.extraCardText", "额外给予1层[gold]易伤[/gold]。" },
                
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.description", "这张牌每对敌人造成一次伤害，额外给予[blue]1[/blue]层[gold]易伤[/gold]。\n给予自身[blue]1[/blue]层[gold]易伤[/gold]。" },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.title", "战车-逆" },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.extraCardText", "每对敌人造成一次伤害，给予1层[gold]易伤[/gold]。\n给予自身1层[gold]易伤[/gold]。" },
                
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.description", "额外给予[blue]1[/blue]层[gold]虚弱[/gold]。" },
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.title", "力量-正" },
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.extraCardText", "额外给予1层[gold]虚弱[/gold]。" },
                
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.description", "这张牌每对敌人造成一次伤害，额外给予[blue]1[/blue]层[gold]虚弱[/gold]。\n给予自身[blue]1[/blue]层[gold]虚弱[/gold]。" },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.title", "力量-逆" },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.extraCardText", "每对敌人造成一次伤害，给予1层[gold]虚弱[/gold]。\n给予自身1层[gold]虚弱[/gold]。" },
                
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.description", "战斗开始时[gold]消耗[/gold]这张牌，第二回合开始时，将这张牌放入[gold]手牌[/gold]。" },
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.title", "隐者-正" },
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.extraCardText", "隐者‑正。" },
                
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.description", "战斗开始时[gold]消耗[/gold]这张牌，第七回合开始时，将这张牌放入[gold]手牌[/gold]，并可以免费打出。" },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.title", "隐者-逆" },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.extraCardText", "隐者‑逆。" },
                
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.description", "这张牌获得[gold]消耗[/gold]，造成[blue]2[/blue]倍伤害。" },
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.title", "正义-正" },
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.extraCardText", "正义‑正。" },
                
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.description", "这张牌获得[gold]消耗[/gold]，额外使你获得本次造成的攻击伤害点[gold]格挡[/gold]。" },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.title", "正义-逆" },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.extraCardText", "获得造成伤害点[gold]格挡[/gold]。" },
                
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.description", "当另一张牌即将被[gold]消耗[/gold]时，若这张牌在[gold]抽牌堆[/gold]中，打出这张牌，阻止另一张牌被[gold]消耗[/gold]。" },
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.title", "倒吊人-正" },
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.extraCardText", "这张牌渴望牺牲。" },
                
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.description", "这张牌打出时，随机[gold]消耗[/gold]一张[gold]手牌[/gold]，将自身置入[gold]弃牌堆[/gold]。" },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.title", "倒吊人-逆" },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.extraCardText", "这张牌渴望牺牲……？" },
                
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.description", "这张牌可以免费打出。\n这张牌打出后，[red]结束你的回合[/red]。" },
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.title", "死神-正" },
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.extraCardText", "结束你的回合。" },
                
                { "TAR_DEATH_REVERSED_ENCHANTMENT.description", "这张牌可以免费打出。\n这张牌在[gold]手牌[/gold]中时，你[red]无法抽牌[/red]。\n这张牌打出时，本身可以抽牌。" },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.title", "死神-逆" },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.extraCardText", "死神注视着你。" },
                
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.description", "每场战斗第一次打出时，获得[blue]10[/blue][gold]金币[/gold]。" },
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.title", "节制-正" },
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.extraCardText", "第一次打出时，获得10[gold]金币[/gold]。" },
                
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.description", "这张牌打出后，本回合你失去的每点生命，使你在战斗结束后获得[blue]5[/blue][gold]金币[/gold]。" },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.title", "节制-逆" },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.extraCardText", "你在本回合失去的每点生命，使你在战斗结束后获得5[gold]金币[/gold]。" },
                
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.description", "这张牌费用减少{energyPrefix:energyIcons(1)}。这张牌在[gold]手牌[/gold]中时，你必须优先打出这张牌。" },
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.title", "恶魔-正" },
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.extraCardText", "划算的交易。" },
                
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.description", "战斗中，你每失去[blue]3[/blue]点生命，这张牌费用减少{energyPrefix:energyIcons(1)}，直到下一次打出。" },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.title", "恶魔-逆" },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.extraCardText", "以血奉养。" },
                
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.description", "这张牌打出时，使你获得等同于当前[img]res://images/packed/sprite_fonts/star_icon.png[/img]的[gold]格挡[/gold]。" },
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.title", "星星-正" },
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.extraCardText", "群星守望，浩瀚成衣。" },
                
                { "TAR_STAR_REVERSED_ENCHANTMENT.description", "交换这张牌{energyPrefix:energyIcons(1)}和[img]res://images/packed/sprite_fonts/star_icon.png[/img]的消耗。" },
                { "TAR_STAR_REVERSED_ENCHANTMENT.title", "星星-逆" },
                { "TAR_STAR_REVERSED_ENCHANTMENT.extraCardText", "斗转星移，我为逆旅。" },
                
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.description", "第一回合结束时，自动打出这张牌，然后放回你的[gold]抽牌堆[/gold]。" },
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.title", "月亮-正" },
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.extraCardText", "……" },
                
                { "TAR_MOON_REVERSED_ENCHANTMENT.description", "这张牌被弃置时，总是会回到你的[gold]手牌[/gold]。" },
                { "TAR_MOON_REVERSED_ENCHANTMENT.title", "月亮-逆" },
                { "TAR_MOON_REVERSED_ENCHANTMENT.extraCardText", "……" },
                
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.description", "这张牌打出时，减半你随机一个[gold]负面状态[/gold]的层数。" },
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.title", "太阳-正" },
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.extraCardText", "愿腐败不及。" },
                
                { "TAR_SUN_REVERSED_ENCHANTMENT.description", "这张牌可以免费打出，原来的费用消耗会给予你[blue]6[/blue]倍的[gold]灾厄[/gold]层数。" },
                { "TAR_SUN_REVERSED_ENCHANTMENT.title", "太阳-逆" },
                { "TAR_SUN_REVERSED_ENCHANTMENT.extraCardText", "愿终得安息。" },
                
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.description", "这张牌每场战斗开始时随机[gold]变化[/gold]。" },
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.title", "审判-正" },
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.extraCardText", "每场战斗开始时，随机变成另一张牌。" },
                
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.description", "战斗开始时，所有拥有此附魔的牌会按照加入牌组的次序置入[gold]抽牌堆[/gold]底。" },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.title", "审判-逆" },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.extraCardText", "审判‑逆。" },
                
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.description", "每次[gold]激发[/gold]充能球时，将这张牌从[gold]抽牌堆[/gold]、[gold]弃牌堆[/gold]或[gold]消耗牌堆[/gold]中放入你的[gold]手牌[/gold]，本回合费用增加{energyPrefix:energyIcons(1)}。" },
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.title", "世界-正" },
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.extraCardText", "你好，世界。" },
                
                { "TAR_WORLD_REVERSED_ENCHANTMENT.description", "每场战斗第一次打出时，你获得[blue]1[/blue]层[gold]人工制品[/gold]。" },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.title", "世界-逆" },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.extraCardText", "故障。" },

                
                
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.title", "负片-恶魔-正" },
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，抽2张牌。" },
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "进入手牌时，抽2张牌。" },
                
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.title", "负片-恶魔-逆" },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，获得{energyPrefix:energyIcons(1)}。" },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.extraCardText", "进入手牌时，获得{energyPrefix:energyIcons(1)}。" },
                
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.title", "负片-月亮-正" },
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，抽2张牌。" },
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "进入手牌时，抽2张牌。" },
                
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.title", "负片-月亮-逆" },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，获得{energyPrefix:energyIcons(1)}。" },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.extraCardText", "进入手牌时，获得{energyPrefix:energyIcons(1)}。" },
                
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.title", "负片-星星-正" },
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，抽2张牌。" },
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "进入手牌时，抽2张牌。" },
                
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.title", "负片-星星-逆" },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，获得{energyPrefix:energyIcons(1)}。" },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.extraCardText", "进入手牌时，获得{energyPrefix:energyIcons(1)}。" },
                
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.title", "负片-太阳-正" },
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，抽2张牌。" },
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "进入手牌时，抽2张牌。" },
                
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.title", "负片-太阳-逆" },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，获得{energyPrefix:energyIcons(1)}。" },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.extraCardText", "进入手牌时，获得{energyPrefix:energyIcons(1)}。" },
                
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.title", "负片-世界-正" },
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，抽2张牌。" },
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "进入手牌时，抽2张牌。" },
                
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.title", "负片-世界-逆" },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.description", "这张牌每次进入[gold]手牌[/gold]时，获得{energyPrefix:energyIcons(1)}。" },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.extraCardText", "进入手牌时，获得{energyPrefix:energyIcons(1)}。" },
            
            
            
                { "PLANET_MERCURY_ENCHANTMENT.title", "水星" },
                { "PLANET_MERCURY_ENCHANTMENT.description", "指定一名盟友，在本场战斗中，弃牌阶段结束后，查看对方[gold]抽牌堆[/gold]顶的5张牌，选择丢弃任意张。" },
                { "PLANET_MERCURY_ENCHANTMENT.extraCardText", "为你预见迷障。" },

                { "PLANET_VENUS_ENCHANTMENT.title", "金星" },
                { "PLANET_VENUS_ENCHANTMENT.description", "指定一名盟友，在本场战斗中，弃牌阶段结束后，查看对方的[gold]弃牌堆[/gold]顶的5张牌，选择任意张放入对方的[gold]手牌[/gold]。" },
                { "PLANET_VENUS_ENCHANTMENT.extraCardText", "为你守护富足。" },

                { "PLANET_EARTH_ENCHANTMENT.title", "地球" },
                { "PLANET_EARTH_ENCHANTMENT.description", "指定一名盟友，在本场战斗中，同步两人的[gold]能量[/gold]。" },
                { "PLANET_EARTH_ENCHANTMENT.extraCardText", "与你共立根基。" },

                { "PLANET_MARS_ENCHANTMENT.title", "火星" },
                { "PLANET_MARS_ENCHANTMENT.description", "指定一名盟友，在本场战斗中，同步两人的[gold]生成[/gold]。" },
                { "PLANET_MARS_ENCHANTMENT.extraCardText", "与你共创辉芒。" },

                { "PLANET_JUPITER_ENCHANTMENT.title", "木星" },
                { "PLANET_JUPITER_ENCHANTMENT.description", "这张牌命中的敌人，在本回合受到的攻击伤害会使所有玩家获得等量的[gold]金币[/gold]。\n这张牌打出后，[red]在10s内结束你的回合[/red]。" },
                { "PLANET_JUPITER_ENCHANTMENT.extraCardText", "我来呼唤富足。" },

                { "PLANET_SATURN_ENCHANTMENT.title", "土星" },
                { "PLANET_SATURN_ENCHANTMENT.description", "这张牌命中的敌人，在本回合受到的攻击伤害不会低于这张牌的伤害。\n这张牌打出后，[red]在10s内结束你的回合[/red]。" },
                { "PLANET_SATURN_ENCHANTMENT.extraCardText", "我来开创良机。" },

                { "PLANET_URANUS_ENCHANTMENT.title", "天王星" },
                { "PLANET_URANUS_ENCHANTMENT.description", "这张牌打出后，费用增加{energyPrefix:energyIcons(1)}，获得[blue]2[/blue]层[gold]重放[/gold]，放入一名随机盟友的[gold]抽牌堆[/gold]。" },
                { "PLANET_URANUS_ENCHANTMENT.extraCardText", "我愿愈战愈勇。" },

                { "PLANET_NEPTUNE_ENCHANTMENT.title", "海王星" },
                { "PLANET_NEPTUNE_ENCHANTMENT.description", "这张牌打出后，将一张本牌的复制品放入所有玩家的[gold]弃牌堆[/gold]。" },
                { "PLANET_NEPTUNE_ENCHANTMENT.extraCardText", "我愿屡战屡胜。" },

                { "PLANET_PLUTO_ENCHANTMENT.title", "冥王星" },
                { "PLANET_PLUTO_ENCHANTMENT.description", "每场战斗第一次打出时，使盟友的[purple]冥王星[/purple]附魔卡牌在本回合可以免费打出，并放入各自的[gold]手牌[/gold]。" },
                { "PLANET_PLUTO_ENCHANTMENT.extraCardText", "因为彼此理解。" },

                { "PLANET_X_ENCHANTMENT.title", "X 行星" },
                { "PLANET_X_ENCHANTMENT.description", "每场战斗第一次打出时，使盟友的[purple]X行星[/purple]附魔卡牌获得[blue]4[/blue]层[gold]重放[/gold]。" },
                { "PLANET_X_ENCHANTMENT.extraCardText", "因为彼此支撑。" },

                { "PLANET_CERES_ENCHANTMENT.title", "谷神星" },
                { "PLANET_CERES_ENCHANTMENT.description", "每场战斗第一次打出时，将盟友的[purple]谷神星[/purple]附魔卡牌[gold]复制[/gold]，放入其所在牌堆。" },
                { "PLANET_CERES_ENCHANTMENT.extraCardText", "所以永不孤单。" },

                { "PLANET_ERIS_ENCHANTMENT.title", "阋神星" },
                { "PLANET_ERIS_ENCHANTMENT.description", "每场战斗第一次打出时，将盟友的[purple]阋神星[/purple]附魔卡牌[gold]复制[/gold]，放入所有玩家的[gold]手牌[/gold]，复制品不再具有附魔。" },
                { "PLANET_ERIS_ENCHANTMENT.extraCardText", "所以万众一心。" },
            });

            var powersTable = loc.GetTable("powers");
            powersTable.MergeWith(new Dictionary<string, string>
            {
                { "TAR_TEMPERANCE_REVERSED_POWER.title", "节制-逆" },
                { "TAR_TEMPERANCE_REVERSED_POWER.description", "本回合内，你失去的每点生命，使你在战斗结束后获得等量的[gold]金币[/gold]。" },
                { "TAR_TEMPERANCE_REVERSED_POWER.smartDescription", "本回合内，你失去的每点生命，使你在战斗结束后获得[blue]{Amount}[/blue][gold]金币[/gold]。" },

                { "TAR_CHARIOT_REVERSED_POWER.title", "战车-逆" },
                { "TAR_CHARIOT_REVERSED_POWER.description", "该敌人首次对你造成未被格挡的伤害后，你获得[blue]1[/blue]层[gold]易伤[/gold]。" },
                { "TAR_CHARIOT_REVERSED_POWER.smartDescription", "该敌人首次对你造成未被格挡的伤害后，你获得[blue]{Amount}[/blue]层[gold]易伤[/gold]。" },

                { "TAR_STRENGTH_REVERSED_POWER.title", "力量-逆" },
                { "TAR_STRENGTH_REVERSED_POWER.description", "该敌人首次对你造成未被格挡的伤害后，你获得[blue]1[/blue]层[gold]虚弱[/gold]。" },
                { "TAR_STRENGTH_REVERSED_POWER.smartDescription", "该敌人首次对你造成未被格挡的伤害后，你获得[blue]{Amount}[/blue]层[gold]虚弱[/gold]。" },

                { "TAR_HERMIT_REVERSED_POWER.title", "隐者-逆" },
                { "TAR_HERMIT_REVERSED_POWER.description", "回合结束时，获得等于层数的[gold]格挡[/gold]。每次受到未被格挡的伤害时，减少[blue]1[/blue]层。" },
                { "TAR_HERMIT_REVERSED_POWER.smartDescription", "回合结束时，获得[blue]{Amount}[/blue]点[gold]格挡[/gold]。每次受到未被格挡的伤害时，减少[blue]1[/blue]层。" },

                { "TAR_JUSTICE_REVERSED_POWER.title", "正义-逆" },
                { "TAR_JUSTICE_REVERSED_POWER.description", "每回合打出的第一张[gold]攻击牌[/gold]在打出时被[gold]消耗[/gold]。" },
                { "TAR_JUSTICE_REVERSED_POWER.smartDescription", "每回合打出的第一张[gold]攻击牌[/gold]在打出时被[gold]消耗[/gold]。" },

                { "TAR_HANGED_MAN_REVERSED_POWER.title", "倒吊人-逆" },
                { "TAR_HANGED_MAN_REVERSED_POWER.description", "每回合打出的第一张[gold]技能牌[/gold]在打出时被[gold]消耗[/gold]。" },
                { "TAR_HANGED_MAN_REVERSED_POWER.smartDescription", "每回合打出的第一张[gold]技能牌[/gold]在打出时被[gold]消耗[/gold]。" },

                { "TAR_DEATH_REVERSED_POWER.title", "死神-逆" },
                { "TAR_DEATH_REVERSED_POWER.description", "在这场战斗中，每当你打出[gold]能力牌[/gold]，立即[red]结束你的回合[/red]。" },
                { "TAR_DEATH_REVERSED_POWER.smartDescription", "在这场战斗中，每当你打出[gold]能力牌[/gold]，立即[red]结束你的回合[/red]。" },

                { "PLANET_MERCURY_POWER.title", "水星" },
                { "PLANET_MERCURY_POWER.description", "在盟友的弃牌阶段结束后，从其抽牌堆顶5张牌，选择丢弃任意张。" },
                { "PLANET_MERCURY_POWER.smartDescription", "在{PairedName}的弃牌阶段结束后，从其抽牌堆顶5张牌，选择丢弃任意张。" },

                { "PLANET_VENUS_POWER.title", "金星" },
                { "PLANET_VENUS_POWER.description", "在盟友的弃牌阶段结束后，从其[gold]弃牌堆[/gold]顶的5张牌中，选择任意张放入其[gold]手牌[/gold]。" },
                { "PLANET_VENUS_POWER.smartDescription", "在{PairedName}的弃牌阶段结束后，从其[gold]弃牌堆[/gold]顶的5张牌中，选择任意张放入其[gold]手牌[/gold]。" },

                { "PLANET_EARTH_POWER.title", "地球" },
                { "PLANET_EARTH_POWER.description", "与盟友共享能量{energyPrefix:energyIcons(1)}。" },
                { "PLANET_EARTH_POWER.smartDescription", "与{PairedName}共享能量{energyPrefix:energyIcons(1)}。" },

                { "PLANET_MARS_POWER.title", "火星" },
                { "PLANET_MARS_POWER.description", "与盟友共享生成物。" },
                { "PLANET_MARS_POWER.smartDescription", "与{PairedName}共享生成物。" },

                { "PLANET_JUPITER_POWER.title", "木星" },
                { "PLANET_JUPITER_POWER.description", "本回合中，这名敌人每受到1点攻击伤害，就使所有玩家在战斗结束后获得等量的[gold]金币[/gold]。" },
                { "PLANET_JUPITER_POWER.smartDescription", "本回合中，这名敌人每受到1点攻击伤害，就使所有玩家在战斗结束后获得[blue]{Amount}[/blue][gold]金币[/gold]。" },

                { "PLANET_SATURN_POWER.title", "土星" },
                { "PLANET_SATURN_POWER.description", "本回合中，受到的攻击伤害不会低于[blue]{Amount}[/blue]点。" },
                { "PLANET_SATURN_POWER.smartDescription", "本回合中，受到的攻击伤害不会低于[blue]{Amount}[/blue]点。" },

                { "PLANET_GOLD_POWER.title", "积攒" },
                { "PLANET_GOLD_POWER.description", "在战斗结束时，你获得[blue]{Amount}[/blue][gold]金币[/gold]。" },
                { "PLANET_GOLD_POWER.smartDescription", "在战斗结束时，你获得[blue]{Amount}[/blue][gold]金币[/gold]。" },

                { "TICK_TACK_POWER.title", "倒计时" },
                { "TICK_TACK_POWER.description", "归零时，强制结束你的回合。" },
                { "TICK_TACK_POWER.smartDescription", "[blue]{Amount}[/blue]秒后，[red]强制结束你的回合[/red]。" },
            });

            var afflictionsTable = loc.GetTable("afflictions");
            afflictionsTable.MergeWith(new Dictionary<string, string>
            {
                { "TAR_JUSTICE_REVERSED_AFFLICTION.title", "正义-逆" },
                { "TAR_JUSTICE_REVERSED_AFFLICTION.description", "每回合打出的第一张攻击牌被[gold]消耗[/gold]。" },

                { "TAR_HANGED_MAN_REVERSED_AFFLICTION.title", "倒吊人-逆" },
                { "TAR_HANGED_MAN_REVERSED_AFFLICTION.description", "每回合打出的第一张技能牌被[gold]消耗[/gold]。" },

                { "TAR_DEATH_REVERSED_AFFLICTION.title", "死神-逆" },
                { "TAR_DEATH_REVERSED_AFFLICTION.description", "打出后立即结束你的回合。" },
                { "TAR_DEATH_REVERSED_AFFLICTION.extraCardText", "结束你的回合。" },
            });

            var gameplayUiTable = loc.GetTable("gameplay_ui");
            gameplayUiTable.MergeWith(new Dictionary<string, string>
            {
                { "CHOOSE_CARD_DOWNGRADE_HEADER", "选择任意张手牌降级" },
                { "PLANET_MERCURY_SELECTION_PROMPT", "为你的盟友选择任意张来自其抽牌堆顶的牌，被选中的牌会被丢弃" },
                { "PLANET_VENUS_SELECTION_PROMPT", "为你的盟友选择任意张来自其弃牌堆顶的牌，被选中的牌会被放回其手牌" },
                { "VANILLA_STYLE_TAROT", "塔罗牌经典样式" },
                { "VANILLA_STYLE_PLANET", "星球牌经典样式" },
            });

            
            var roomTable = loc.GetTable("merchant_room");
            roomTable.MergeWith(new Dictionary<string, string>
            {
                { "TAROT_PILE_ENTRY.title", "塔罗牌补充包" },
                { "TAROT_PILE_ENTRY.description", "抽取[blue]3[/blue]张并选择[blue]1[/blue]张塔罗牌，为牌组中的牌[gold]附魔[/gold]。\n有时会出现奇怪的效果……" }
            });


            var relicsTable = loc.GetTable("relics");
            relicsTable.MergeWith(new Dictionary<string, string>
            {
                { "STARGAZER_KIT.title", "望远镜套装" },
                { "STARGAZER_KIT.description", "你可以在[gold]休息处[/gold]进行[gold]观星[/gold]。\n进入[gold]先古之民[/gold]房间时，获得[blue]2[/blue]次额外使用次数。" },
                { "STARGAZER_KIT.flavor", "不要温和地走进那个良夜。" }
            });

            var restSiteTable = loc.GetTable("rest_site_ui");
            restSiteTable.MergeWith(new Dictionary<string, string>
            {
                { "OPTION_STARGAZE.description", "从3张随机星球牌中选择1张，为[gold]牌组[/gold]中的一张非多人联机牌[gold]附魔[/gold]。" },
                { "OPTION_STARGAZE.name", "观星" },
            });
        }
    }
}