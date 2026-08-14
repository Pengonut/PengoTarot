
#nullable enable
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace PengoTarot
{
    public static class TarJapaneseLocHelper
    {
        public static void InjectAll()
        {
            var loc = LocManager.Instance;

            var cardLibraryTable = loc.GetTable("card_library");
            cardLibraryTable.MergeWith(new Dictionary<string, string>
            {
                { "POOL_TAROT_TIP", "タロットカード。" },
                { "POOL_PLANET_TIP", "プラネットカード。" }
            });

            
            var cardsTable = loc.GetTable("cards");
            cardsTable.MergeWith(new Dictionary<string, string>
            {
                
                { "TAR_FOOL_UPRIGHT.title", "0-愚者-正" },
                { "TAR_FOOL_UPRIGHT.description", "カードを1枚選び、[gold]エンチャント[/gold]する\n[purple]愚者-正[/purple]。" },
                { "TAR_FOOL_REVERSED.title", "0-愚者-逆" },
                { "TAR_FOOL_REVERSED.description", "\nコストが[blue]X[/blue]でないカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]愚者-逆[/purple]。" },

                
                { "TAR_MAGICIAN_UPRIGHT.title", "I-魔術師-正" },
                { "TAR_MAGICIAN_UPRIGHT.description", "[gold]廃棄[/gold]を持たないスキルかアタックを5枚選び、[gold]エンチャント[/gold]する\n[purple]魔術師-正[/purple]。" },
                { "TAR_MAGICIAN_REVERSED.title", "I-魔術師-逆" },
                { "TAR_MAGICIAN_REVERSED.description", "\n[gold]廃棄[/gold]を持たないスキルかアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]魔術師-逆[/purple]。" },

                
                { "TAR_HIGH_PRIESTESS_UPRIGHT.title", "II-女教皇-正" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT.description", "[gold]エセリアル[/gold]を持たないカードを3枚選び、[gold]エンチャント[/gold]する\n[purple]女教皇-正[/purple]。" },
                { "TAR_HIGH_PRIESTESS_REVERSED.title", "II-女教皇-逆" },
                { "TAR_HIGH_PRIESTESS_REVERSED.description", "\n[gold]エセリアル[/gold]を持たないカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]女教皇-逆[/purple]。" },

                
                { "TAR_EMPRESS_UPRIGHT.title", "III-女帝-正" },
                { "TAR_EMPRESS_UPRIGHT.description", "カードを1枚選び、[gold]エンチャント[/gold]する\n[purple]女帝-正[/purple]。" },
                { "TAR_EMPRESS_REVERSED.title", "III-女帝-逆" },
                { "TAR_EMPRESS_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]女帝-逆[/purple]。" },

                
                { "TAR_EMPEROR_UPRIGHT.title", "IV-皇帝-正" },
                { "TAR_EMPEROR_UPRIGHT.description", "カードを1枚選び、[gold]エンチャント[/gold]する\n[purple]皇帝-正[/purple]。" },
                { "TAR_EMPEROR_REVERSED.title", "IV-皇帝-逆" },
                { "TAR_EMPEROR_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]皇帝-逆[/purple]。" },

                
                { "TAR_HIEROPHANT_UPRIGHT.title", "V-教皇-正" },
                { "TAR_HIEROPHANT_UPRIGHT.description", "カードを1枚選び、[gold]エンチャント[/gold]する\n[purple]教皇-正[/purple]。" },
                { "TAR_HIEROPHANT_REVERSED.title", "V-教皇-逆" },
                { "TAR_HIEROPHANT_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]教皇-逆[/purple]。" },

                
                { "TAR_LOVERS_UPRIGHT.title", "VI-恋人-正" },
                { "TAR_LOVERS_UPRIGHT.description", "カードを2枚選び、[gold]エンチャント[/gold]する\n[purple]恋人-正[/purple]。" },
                { "TAR_LOVERS_REVERSED.title", "VI-恋人-逆" },
                { "TAR_LOVERS_REVERSED.description", "\nコストが[blue]X[/blue]でないカードを2枚選び、[gold]エンチャント[/gold]する\n[purple]恋人-逆[/purple]。" },

                
                { "TAR_CHARIOT_UPRIGHT.title", "VII-戦車-正" },
                { "TAR_CHARIOT_UPRIGHT.description", "非多段のアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]戦車-正[/purple]。" },
                { "TAR_CHARIOT_REVERSED.title", "VII-戦車-逆" },
                { "TAR_CHARIOT_REVERSED.description", "\n多段のアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]戦車-逆[/purple]。" },

                
                { "TAR_STRENGTH_UPRIGHT.title", "VIII-力-正" },
                { "TAR_STRENGTH_UPRIGHT.description", "非多段のアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]力-正[/purple]。" },
                { "TAR_STRENGTH_REVERSED.title", "VIII-力-逆" },
                { "TAR_STRENGTH_REVERSED.description", "\n多段のアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]力-逆[/purple]。" },

                
                { "TAR_HERMIT_UPRIGHT.title", "IX-隠者-正" },
                { "TAR_HERMIT_UPRIGHT.description", "スキルかアタックを2枚選び、[gold]エンチャント[/gold]する\n[purple]隠者-正[/purple]。" },
                { "TAR_HERMIT_REVERSED.title", "IX-隠者-逆" },
                { "TAR_HERMIT_REVERSED.description", "\nスキルかアタックを2枚選び、[gold]エンチャント[/gold]する\n[purple]隠者-逆[/purple]。" },

                
                { "TAR_WHEEL_OF_FORTUNE_UPRIGHT.title", "X-運命の輪-正" },
                { "TAR_WHEEL_OF_FORTUNE_UPRIGHT.description", "[red]HPを11失う[/red]。ランダムな非エンシェントレリックを[gold]コピー[/gold]する。" },
                { "TAR_WHEEL_OF_FORTUNE_REVERSED.title", "X-運命の輪-逆" },
                { "TAR_WHEEL_OF_FORTUNE_REVERSED.description", "\nランダムな非エンシェントレリックを2つ[red]破壊し[/red]、その後ランダムな非エンシェントレリックを3回[gold]コピー[/gold]する。" },

                
                { "TAR_JUSTICE_UPRIGHT.title", "XI-正義-正" },
                { "TAR_JUSTICE_UPRIGHT.description", "[gold]廃棄[/gold]を持たない全体攻撃ではないアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]正義-正[/purple]。" },
                { "TAR_JUSTICE_REVERSED.title", "XI-正義-逆" },
                { "TAR_JUSTICE_REVERSED.description", "\n[gold]廃棄[/gold]を持たない全体攻撃ではないアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]正義-逆[/purple]。" },

                
                { "TAR_HANGED_MAN_UPRIGHT.title", "XII-吊された男-正" },
                { "TAR_HANGED_MAN_UPRIGHT.description", "[gold]廃棄[/gold]を持つカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]吊された男-正[/purple]。" },
                { "TAR_HANGED_MAN_REVERSED.title", "XII-吊された男-逆" },
                { "TAR_HANGED_MAN_REVERSED.description", "\n[gold]廃棄[/gold]を持つカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]吊された男-逆[/purple]。" },

                
                { "TAR_DEATH_UPRIGHT.title", "XIII-死神-正" },
                { "TAR_DEATH_UPRIGHT.description", "カードを1枚選び、[gold]エンチャント[/gold]する\n[purple]死神-正[/purple]。" },
                { "TAR_DEATH_REVERSED.title", "XIII-死神-逆" },
                { "TAR_DEATH_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]死神-逆[/purple]。" },

                
                { "TAR_TEMPERANCE_UPRIGHT.title", "XIV-節制-正" },
                { "TAR_TEMPERANCE_UPRIGHT.description", "カードを1枚選び、[gold]エンチャント[/gold]する\n[purple]節制-正[/purple]。" },
                { "TAR_TEMPERANCE_REVERSED.title", "XIV-節制-逆" },
                { "TAR_TEMPERANCE_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]節制-逆[/purple]。" },

                
                { "TAR_DEVIL_UPRIGHT.title", "XV-悪魔-正" },
                { "TAR_DEVIL_UPRIGHT.description", "カードを1枚選び、[gold]エンチャント[/gold]する\n[purple]悪魔-正[/purple]。" },
                { "TAR_DEVIL_REVERSED.title", "XV-悪魔-逆" },
                { "TAR_DEVIL_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]悪魔-逆[/purple]。" },

                
                { "TAR_TOWER_UPRIGHT.title", "XVI-塔-正" },
                { "TAR_TOWER_UPRIGHT.description", "デッキからすべてのベーシック、コモン、アンコモンのカードを[red]削除する[/red]。削除したカード1枚につき45[gold]ゴールド[/gold]を獲得する。" },
                { "TAR_TOWER_REVERSED.title", "XVI-塔-逆" },
                { "TAR_TOWER_REVERSED.description", "\nデッキからすべてのベーシック、コモンのカードを[red]削除する[/red]。削除したカード1枚につき15[gold]ゴールド[/gold]を獲得する。" },

                
                { "TAR_STAR_UPRIGHT.title", "XVII-星-正" },
                { "TAR_STAR_UPRIGHT.description", "カードを3枚選び、[gold]エンチャント[/gold]する\n[purple]星-正[/purple]。" },
                { "TAR_STAR_REVERSED.title", "XVII-星-逆" },
                { "TAR_STAR_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]星-逆[/purple]。" },

                
                { "TAR_MOON_UPRIGHT.title", "XVIII-月-正" },
                { "TAR_MOON_UPRIGHT.description", "スキルかアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]月-正[/purple]。" },
                { "TAR_MOON_REVERSED.title", "XVIII-月-逆" },
                { "TAR_MOON_REVERSED.description", "\nスキルかアタックを1枚選び、[gold]エンチャント[/gold]する\n[purple]月-逆[/purple]。" },

                
                { "TAR_SUN_UPRIGHT.title", "XIX-太陽-正" },
                { "TAR_SUN_UPRIGHT.description", "カードを3枚選び、[gold]エンチャント[/gold]する\n[purple]太陽-正[/purple]。" },
                { "TAR_SUN_REVERSED.title", "XIX-太陽-逆" },
                { "TAR_SUN_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]太陽-逆[/purple]。" },

                
                { "TAR_JUDGEMENT_UPRIGHT.title", "XX-審判-正" },
                { "TAR_JUDGEMENT_UPRIGHT.description", "カードを2枚選び、[gold]エンチャント[/gold]する\n[purple]審判-正[/purple]。" },
                { "TAR_JUDGEMENT_REVERSED.title", "XX-審判-逆" },
                { "TAR_JUDGEMENT_REVERSED.description", "\n最大21枚のカードを選び、[gold]エンチャント[/gold]する\n[purple]審判-逆[/purple]。" },

                
                { "TAR_WORLD_UPRIGHT.title", "XXI-世界-正" },
                { "TAR_WORLD_UPRIGHT.description", "カードを1枚選び、[gold]エンチャント[/gold]する\n[purple]世界-正[/purple]。" },
                { "TAR_WORLD_REVERSED.title", "XXI-世界-逆" },
                { "TAR_WORLD_REVERSED.description", "\nカードを1枚選び、[gold]エンチャント[/gold]する\n[purple]世界-逆[/purple]。" },

                
                
                { "TAR_DEVIL_UPRIGHT_SUB.title", "ネガティブ-悪魔-正" },
                { "TAR_DEVIL_UPRIGHT_SUB.description", "3枚のカードを選び、同じレアリティのアイアンクラッドのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-悪魔-正[/purple]。" },
                { "TAR_DEVIL_REVERSED_SUB.title", "ネガティブ-悪魔-逆" },
                { "TAR_DEVIL_REVERSED_SUB.description", "\n3枚のカードを選び、同じレアリティのアイアンクラッドのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-悪魔-逆[/purple]。" },
                
                { "TAR_MOON_UPRIGHT_SUB.title", "ネガティブ-月-正" },
                { "TAR_MOON_UPRIGHT_SUB.description", "3枚のカードを選び、同じレアリティのサイレントのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-月-正[/purple]。" },
                { "TAR_MOON_REVERSED_SUB.title", "ネガティブ-月-逆" },
                { "TAR_MOON_REVERSED_SUB.description", "\n3枚のカードを選び、同じレアリティのサイレントのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-月-逆[/purple]。" },
                
                { "TAR_STAR_UPRIGHT_SUB.title", "ネガティブ-星-正" },
                { "TAR_STAR_UPRIGHT_SUB.description", "3枚のカードを選び、同じレアリティのリージェントのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-星-正[/purple]。" },
                { "TAR_STAR_REVERSED_SUB.title", "ネガティブ-星-逆" },
                { "TAR_STAR_REVERSED_SUB.description", "\n3枚のカードを選び、同じレアリティのリージェントのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-星-逆[/purple]。" },
                
                { "TAR_SUN_UPRIGHT_SUB.title", "ネガティブ-太陽-正" },
                { "TAR_SUN_UPRIGHT_SUB.description", "3枚のカードを選び、同じレアリティのネクロバインダーのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-太陽-正[/purple]。" },
                { "TAR_SUN_REVERSED_SUB.title", "ネガティブ-太陽-逆" },
                { "TAR_SUN_REVERSED_SUB.description", "\n3枚のカードを選び、同じレアリティのネクロバインダーのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-太陽-逆[/purple]。" },
                
                { "TAR_WORLD_UPRIGHT_SUB.title", "ネガティブ-世界-正" },
                { "TAR_WORLD_UPRIGHT_SUB.description", "3枚のカードを選び、同じレアリティのディフェクトのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-世界-正[/purple]。" },
                { "TAR_WORLD_REVERSED_SUB.title", "ネガティブ-世界-逆" },
                { "TAR_WORLD_REVERSED_SUB.description", "\n3枚のカードを選び、同じレアリティのディフェクトのカードに[gold]変化[/gold]させ、[gold]エンチャント[/gold]する\n[purple]ネガティブ-世界-逆[/purple]。" },


                { "PLANET_MERCURY.title", "水星" },
                { "PLANET_MERCURY.description", "パワーカードを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]水星[/purple]。" },
                { "PLANET_VENUS.title", "金星" },
                { "PLANET_VENUS.description", "パワーカードを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]金星[/purple]。" },
                { "PLANET_EARTH.title", "地球" },
                { "PLANET_EARTH.description", "パワーカードを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]地球[/purple]。" },
                { "PLANET_MARS.title", "火星" },
                { "PLANET_MARS.description", "パワーカードを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]火星[/purple]。" },

                { "PLANET_JUPITER.title", "木星" },
                { "PLANET_JUPITER.description", "アタックを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]木星[/purple]。" },
                { "PLANET_SATURN.title", "土星" },
                { "PLANET_SATURN.description", "アタックを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]土星[/purple]。" },
                { "PLANET_URANUS.title", "天王星" },
                { "PLANET_URANUS.description", "アタックを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]天王星[/purple]。" },
                { "PLANET_NEPTUNE.title", "海王星" },
                { "PLANET_NEPTUNE.description", "アタックを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]海王星[/purple]。" },

                { "PLANET_PLUTO.title", "冥王星" },
                { "PLANET_PLUTO.description", "スキルを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]冥王星[/purple]。" },
                { "PLANET_X.title", "惑星X" },
                { "PLANET_X.description", "スキルを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]惑星X[/purple]。" },
                { "PLANET_CERES.title", "ケレス" },
                { "PLANET_CERES.description", "スキルを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]ケレス[/purple]。" },
                { "PLANET_ERIS.title", "エリス" },
                { "PLANET_ERIS.description", "スキルを1枚選び、\n[gold]エンチャント[/gold]する\n[purple]エリス[/purple]。" },
            });

            
            var enchantmentsTable = loc.GetTable("enchantments");
            enchantmentsTable.MergeWith(new Dictionary<string, string>
            {
                
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.description", "戦闘中初めてプレイした時、これを[gold]手札[/gold]に戻す。" },
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.title", "愚者-正" },
                { "TAR_FOOL_UPRIGHT_ENCHANTMENT.extraCardText", "初めてプレイした時、これを[gold]手札[/gold]に戻す。" },
                
                { "TAR_FOOL_REVERSED_ENCHANTMENT.description", "戦闘中初めてプレイした時、コストが{energyPrefix:energyIcons(1)}高いコピーを1枚[gold]手札[/gold]に加える。" },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.title", "愚者-逆" },
                { "TAR_FOOL_REVERSED_ENCHANTMENT.extraCardText", "初めてプレイした時、コストが{energyPrefix:energyIcons(1)}高いコピーを手札に加える。" },

                
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.description", "このカードは[gold]廃棄[/gold]を得る。" },
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.title", "魔術師-正" },
                { "TAR_MAGICIAN_UPRIGHT_ENCHANTMENT.extraCardText", "魔術師-正。" },
                
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.description", "プレイ後、このカードは[gold]山札[/gold]のランダムな位置に置かれる。" },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.title", "魔術師-逆" },
                { "TAR_MAGICIAN_REVERSED_ENCHANTMENT.extraCardText", "[gold]山札[/gold]のランダムな位置に置かれる。" },

                
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.description", "このカードは[gold]エセリアル[/gold]を得る。" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.title", "女教皇-正" },
                { "TAR_HIGH_PRIESTESS_UPRIGHT_ENCHANTMENT.extraCardText", "女教皇-正。" },
                
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.description", "このカードは[gold]保留[/gold]を得る。ターン終了時、これが手札にある場合、手札の左側のすべてのカードに[gold]エセリアル[/gold]を付与する。" },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.title", "女教皇-逆" },
                { "TAR_HIGH_PRIESTESS_REVERSED_ENCHANTMENT.extraCardText", "ターン終了時、手札の左側のカードに[gold]エセリアル[/gold]を付与する。" },

                
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.description", "この戦闘中、3回目以降のプレイで[gold]リプレイ[/gold][blue]1[/blue]を得る。" },
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.title", "女帝-正" },
                { "TAR_EMPRESS_UPRIGHT_ENCHANTMENT.extraCardText", "プレイ回数: {PlayCount}" },
                
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.description", "コストが{energyPrefix:energyIcons(1)}減少。このカードは初めて[gold]捨て札[/gold]に入るまでプレイできない。" },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.title", "女帝-逆" },
                { "TAR_EMPRESS_REVERSED_ENCHANTMENT.extraCardText", "初めて捨て札に入るまでプレイできない。" },

                
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.description", "このカードは[gold]保留[/gold]を得る。" },
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.title", "皇帝-正" },
                { "TAR_EMPEROR_UPRIGHT_ENCHANTMENT.extraCardText", "皇帝-正。" },
                
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.description", "ターン終了時、これが手札にある場合、手札のランダムな2枚に[gold]保留[/gold]を付与する。" },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.title", "皇帝-逆" },
                { "TAR_EMPEROR_REVERSED_ENCHANTMENT.extraCardText", "ターン終了時、ランダムな手札2枚に[gold]保留[/gold]を付与する。" },

                
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.description", "プレイ後、[gold]手札[/gold]のカード1枚を[gold]アップグレード[/gold]する。" },
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.title", "教皇-正" },
                { "TAR_HIEROPHANT_UPRIGHT_ENCHANTMENT.extraCardText", "[gold]手札[/gold]のカード1枚を[gold]アップグレード[/gold]する。" },
                
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.description", "プレイ後、[gold]手札[/gold]のカードを任意の枚数[red]ダウングレード[/red]する。1枚[red]ダウングレード[/red]するごとに、山札か捨て札にあるランダムなカード3枚を[gold]アップグレード[/gold]する。" },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.title", "教皇-逆" },
                { "TAR_HIEROPHANT_REVERSED_ENCHANTMENT.extraCardText", "このカードは代償を厭わない。" },

                
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.description", "これが手札に入った時、ドロー、捨て札、廃棄の山にある別の[purple]恋人[/purple]エンチャントされたカード1枚を手札に加える。" },
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.title", "恋人-正" },
                { "TAR_LOVERS_UPRIGHT_ENCHANTMENT.extraCardText", "このカードは恋人を探す。" },
                
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.description", "これをプレイした後、別の[purple]恋人[/purple]エンチャントされたカード1枚を手札に加える。それはこのターン、コストが{energyPrefix:energyIcons(1)}増加する。" },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.title", "恋人-逆" },
                { "TAR_LOVERS_REVERSED_ENCHANTMENT.extraCardText", "このカードは恋人を探す……？" },

                
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.description", "追加で[blue]1[/blue][gold]弱体[/gold]を付与する。" },
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.title", "戦車-正" },
                { "TAR_CHARIOT_UPRIGHT_ENCHANTMENT.extraCardText", "追加で1[gold]弱体[/gold]を付与する。" },
                
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.description", "このカードが敵にダメージを与えるたび、追加で[blue]1[/blue][gold]弱体[/gold]を付与する。\n自身に[blue]1[/blue][gold]弱体[/gold]を付与する。" },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.title", "戦車-逆" },
                { "TAR_CHARIOT_REVERSED_ENCHANTMENT.extraCardText", "ヒットごとに1[gold]弱体[/gold]を付与する。\n自身に1[gold]弱体[/gold]を付与する。" },

                
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.description", "追加で[blue]1[/blue][gold]脱力[/gold]を付与する。" },
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.title", "力-正" },
                { "TAR_STRENGTH_UPRIGHT_ENCHANTMENT.extraCardText", "追加で1[gold]脱力[/gold]を付与する。" },
                
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.description", "このカードが敵にダメージを与えるたび、追加で[blue]1[/blue][gold]脱力[/gold]を付与する。\n自身に[blue]1[/blue][gold]脱力[/gold]を付与する。" },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.title", "力-逆" },
                { "TAR_STRENGTH_REVERSED_ENCHANTMENT.extraCardText", "ヒットごとに1[gold]脱力[/gold]を付与する。\n自身に1[gold]脱力[/gold]を付与する。" },

                
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.description", "戦闘開始時、これを[gold]廃棄[/gold]する。2ターン目の開始時に手札に加える。" },
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.title", "隠者-正" },
                { "TAR_HERMIT_UPRIGHT_ENCHANTMENT.extraCardText", "隠者-正。" },
                
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.description", "戦闘開始時、これを[gold]廃棄[/gold]する。7ターン目の開始時に手札に加える。そのターン、コストは0になる。" },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.title", "隠者-逆" },
                { "TAR_HERMIT_REVERSED_ENCHANTMENT.extraCardText", "隠者-逆。" },

                
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.description", "[gold]廃棄[/gold]を得る。このカードは2倍のダメージを与える。" },
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.title", "正義-正" },
                { "TAR_JUSTICE_UPRIGHT_ENCHANTMENT.extraCardText", "正義-正。" },
                
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.description", "[gold]廃棄[/gold]を得る。このカードでダメージを与えた時、その値に等しい[gold]ブロック[/gold]を得る。" },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.title", "正義-逆" },
                { "TAR_JUSTICE_REVERSED_ENCHANTMENT.extraCardText", "与えたダメージに等しい[gold]ブロック[/gold]を得る。" },

                
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.description", "別のカードが[gold]廃棄[/gold]されようとした時、これが山札にある場合、これをプレイし、もう一方のカードが廃棄されるのを防ぐ。" },
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.title", "吊された男-正" },
                { "TAR_HANGED_MAN_UPRIGHT_ENCHANTMENT.extraCardText", "このカードは犠牲を切望する。" },
                
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.description", "プレイ時、手札のランダムなカード1枚を[gold]廃棄[/gold]し、自身を捨て札に置く。" },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.title", "吊された男-逆" },
                { "TAR_HANGED_MAN_REVERSED_ENCHANTMENT.extraCardText", "このカードは犠牲を切望する……？" },

                
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.description", "このカードは0コストでプレイできる。プレイ後、ターンを終了する。" },
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.title", "死神-正" },
                { "TAR_DEATH_UPRIGHT_ENCHANTMENT.extraCardText", "プレイ後、ターンを終了する。" },
                
                { "TAR_DEATH_REVERSED_ENCHANTMENT.description", "このカードは0コストでプレイできる。手札にある間、カードを引くことができない。\nこのカードはプレイ時にカードを引くことができる。" },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.title", "死神-逆" },
                { "TAR_DEATH_REVERSED_ENCHANTMENT.extraCardText", "死神が見つめている。" },

                
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.description", "戦闘中初めてプレイした時、[blue]10[/blue][gold]ゴールド[/gold]を獲得する。" },
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.title", "節制-正" },
                { "TAR_TEMPERANCE_UPRIGHT_ENCHANTMENT.extraCardText", "戦闘中初めてプレイした時、10[gold]ゴールド[/gold]を獲得する。" },
                
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.description", "プレイ後、このターンに失ったHP1につき、戦闘終了時に[blue]5[/blue][gold]ゴールド[/gold]を得る。" },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.title", "節制-逆" },
                { "TAR_TEMPERANCE_REVERSED_ENCHANTMENT.extraCardText", "このターンに失ったHP1につき、戦闘終了時に5[gold]ゴールド[/gold]を得る。" },

                
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.description", "コストが{energyPrefix:energyIcons(1)}減少。手札にある間、他のカードより先にプレイしなければならない。" },
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.title", "悪魔-正" },
                { "TAR_DEVIL_UPRIGHT_ENCHANTMENT.extraCardText", "公正な交換。" },
                
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.description", "戦闘中、HPを[blue]3[/blue]失うごとにコストが{energyPrefix:energyIcons(1)}減少する。プレイされるとリセットされる。" },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.title", "悪魔-逆" },
                { "TAR_DEVIL_REVERSED_ENCHANTMENT.extraCardText", "血で養う。" },

                
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.description", "プレイ時、現在の[img]res://images/packed/sprite_fonts/star_icon.png[/img]に等しい[gold]ブロック[/gold]を得る。" },
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.title", "星-正" },
                { "TAR_STAR_UPRIGHT_ENCHANTMENT.extraCardText", "星々は見つめ、宇宙が衣となる。" },
                
                { "TAR_STAR_REVERSED_ENCHANTMENT.description", "このカードの{energyPrefix:energyIcons(1)}コストと[img]res://images/packed/sprite_fonts/star_icon.png[/img]コストを入れ替える。" },
                { "TAR_STAR_REVERSED_ENCHANTMENT.title", "星-逆" },
                { "TAR_STAR_REVERSED_ENCHANTMENT.extraCardText", "星は巡り、我は逆に歩む。" },

                
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.description", "最初のターン終了時、自動的にプレイし、その後[gold]山札[/gold]に戻す。" },
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.title", "月-正" },
                { "TAR_MOON_UPRIGHT_ENCHANTMENT.extraCardText", "……" },
                
                { "TAR_MOON_REVERSED_ENCHANTMENT.description", "これが捨てられた時、常に手札に戻す。" },
                { "TAR_MOON_REVERSED_ENCHANTMENT.title", "月-逆" },
                { "TAR_MOON_REVERSED_ENCHANTMENT.extraCardText", "……" },

                
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.description", "プレイ時、自身のランダムな[gold]デバフ[/gold]1つのスタックを半減させる。" },
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.title", "太陽-正" },
                { "TAR_SUN_UPRIGHT_ENCHANTMENT.extraCardText", "腐敗が届きませんように。" },
                
                { "TAR_SUN_REVERSED_ENCHANTMENT.description", "このカードは0コストでプレイできる。元のエネルギーコストの代わりに、[blue]6[/blue]倍の[gold]破滅[/gold]を自身に付与する。" },
                { "TAR_SUN_REVERSED_ENCHANTMENT.title", "太陽-逆" },
                { "TAR_SUN_REVERSED_ENCHANTMENT.extraCardText", "ようやく安らげますように。" },

                
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.description", "戦闘開始時、このカードはランダムに[gold]変化[/gold]する。" },
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.title", "審判-正" },
                { "TAR_JUDGEMENT_UPRIGHT_ENCHANTMENT.extraCardText", "戦闘開始時、ランダムなカードに変化する。" },
                
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.description", "戦闘開始時、このエンチャントを持つすべてのカードは山札の一番下に置かれる。" },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.title", "審判-逆" },
                { "TAR_JUDGEMENT_REVERSED_ENCHANTMENT.extraCardText", "審判-逆。" },

                
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.description", "オーブを[gold]解放[/gold]するたび、このカードを他の領域から手札に加え、このターンコストが{energyPrefix:energyIcons(1)}増加する。" },
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.title", "世界-正" },
                { "TAR_WORLD_UPRIGHT_ENCHANTMENT.extraCardText", "ハロー、ワールド。" },
                
                { "TAR_WORLD_REVERSED_ENCHANTMENT.description", "戦闘中初めてプレイした時、[blue]1[/blue][gold]アーティファクト[/gold]を得る。" },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.title", "世界-逆" },
                { "TAR_WORLD_REVERSED_ENCHANTMENT.extraCardText", "故障。" },

                
                
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.title", "ネガティブ-悪魔-正" },
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、カードを2枚引く。" },
                { "TAR_DEVIL_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "手札に入った時、カードを2枚引く。" },
                
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.title", "ネガティブ-悪魔-逆" },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、{energyPrefix:energyIcons(1)}を得る。" },
                { "TAR_DEVIL_REVERSED_SUB_ENCHANTMENT.extraCardText", "手札に入った時、{energyPrefix:energyIcons(1)}を得る。" },
                
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.title", "ネガティブ-月-正" },
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、カードを2枚引く。" },
                { "TAR_MOON_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "手札に入った時、カードを2枚引く。" },
                
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.title", "ネガティブ-月-逆" },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、{energyPrefix:energyIcons(1)}を得る。" },
                { "TAR_MOON_REVERSED_SUB_ENCHANTMENT.extraCardText", "手札に入った時、{energyPrefix:energyIcons(1)}を得る。" },
                
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.title", "ネガティブ-星-正" },
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、カードを2枚引く。" },
                { "TAR_STAR_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "手札に入った時、カードを2枚引く。" },
                
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.title", "ネガティブ-星-逆" },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、{energyPrefix:energyIcons(1)}を得る。" },
                { "TAR_STAR_REVERSED_SUB_ENCHANTMENT.extraCardText", "手札に入った時、{energyPrefix:energyIcons(1)}を得る。" },
                
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.title", "ネガティブ-太陽-正" },
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、カードを2枚引く。" },
                { "TAR_SUN_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "手札に入った時、カードを2枚引く。" },
                
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.title", "ネガティブ-太陽-逆" },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、{energyPrefix:energyIcons(1)}を得る。" },
                { "TAR_SUN_REVERSED_SUB_ENCHANTMENT.extraCardText", "手札に入った時、{energyPrefix:energyIcons(1)}を得る。" },
                
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.title", "ネガティブ-世界-正" },
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、カードを2枚引く。" },
                { "TAR_WORLD_UPRIGHT_SUB_ENCHANTMENT.extraCardText", "手札に入った時、カードを2枚引く。" },
                
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.title", "ネガティブ-世界-逆" },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.description", "このカードが[gold]手札[/gold]に入るたび、{energyPrefix:energyIcons(1)}を得る。" },
                { "TAR_WORLD_REVERSED_SUB_ENCHANTMENT.extraCardText", "手札に入った時、{energyPrefix:energyIcons(1)}を得る。" },


                { "PLANET_MERCURY_ENCHANTMENT.title", "水星" },
                { "PLANET_MERCURY_ENCHANTMENT.description", "味方を1人指定する。その戦闘中、毎ターン終了時に相手の[gold]山札[/gold]の上から5枚を見て、任意の枚数を捨てる。" },
                { "PLANET_MERCURY_ENCHANTMENT.extraCardText", "あなたに霧を晴らす。" },

                { "PLANET_VENUS_ENCHANTMENT.title", "金星" },
                { "PLANET_VENUS_ENCHANTMENT.description", "味方を1人指定する。その戦闘中、毎ターン終了時に相手の[gold]捨て札[/gold]の上から5枚を見て、任意の枚数を相手の[gold]手札[/gold]に加える。" },
                { "PLANET_VENUS_ENCHANTMENT.extraCardText", "あなたに失くしたものを。" },

                { "PLANET_EARTH_ENCHANTMENT.title", "地球" },
                { "PLANET_EARTH_ENCHANTMENT.description", "味方を1人指定する。その戦闘中、二人の[gold]エネルギー[/gold]を同期する。" },
                { "PLANET_EARTH_ENCHANTMENT.extraCardText", "あなたと地に立つ。" },

                { "PLANET_MARS_ENCHANTMENT.title", "火星" },
                { "PLANET_MARS_ENCHANTMENT.description", "味方を1人指定する。その戦闘中、二人の生成物を同期する。" },
                { "PLANET_MARS_ENCHANTMENT.extraCardText", "あなたと新たを創る。" },

                { "PLANET_JUPITER_ENCHANTMENT.title", "木星" },
                { "PLANET_JUPITER_ENCHANTMENT.description", "このカードが命中した敵は、このターン受ける攻撃ダメージと同じ量の[gold]ゴールド[/gold]を全プレイヤーに与える。\nプレイから10秒後、[red]ターンを終了する[/red]。" },
                { "PLANET_JUPITER_ENCHANTMENT.extraCardText", "私が富をもたらす。" },

                { "PLANET_SATURN_ENCHANTMENT.title", "土星" },
                { "PLANET_SATURN_ENCHANTMENT.description", "このカードが命中した敵は、このターン受ける攻撃ダメージがこのカードのダメージを下回らない。\nプレイから10秒後、[red]ターンを終了する[/red]。" },
                { "PLANET_SATURN_ENCHANTMENT.extraCardText", "私が下限を引く。" },

                { "PLANET_URANUS_ENCHANTMENT.title", "天王星" },
                { "PLANET_URANUS_ENCHANTMENT.description", "プレイ後、コストが{energyPrefix:energyIcons(1)}増加し、[blue]2[/blue][gold]リプレイ[/gold]を得て、ランダムな味方の[gold]山札[/gold]に入る。" },
                { "PLANET_URANUS_ENCHANTMENT.extraCardText", "私は再び戦う。" },

                { "PLANET_NEPTUNE_ENCHANTMENT.title", "海王星" },
                { "PLANET_NEPTUNE_ENCHANTMENT.description", "プレイ後、このカードのコピーを全員の[gold]捨て札[/gold]に加える。" },
                { "PLANET_NEPTUNE_ENCHANTMENT.extraCardText", "私は恵みを分かつ。" },

                { "PLANET_PLUTO_ENCHANTMENT.title", "冥王星" },
                { "PLANET_PLUTO_ENCHANTMENT.description", "戦闘中初めてプレイした時、味方の[purple]冥王星[/purple]エンチャントカードをこのターン0コストにし、各自の[gold]手札[/gold]に加える。" },
                { "PLANET_PLUTO_ENCHANTMENT.extraCardText", "互いに理解し合う。" },

                { "PLANET_X_ENCHANTMENT.title", "惑星X" },
                { "PLANET_X_ENCHANTMENT.description", "戦闘中初めてプレイした時、味方の[purple]惑星X[/purple]エンチャントカードに[blue]4[/blue][gold]リプレイ[/gold]を付与する。" },
                { "PLANET_X_ENCHANTMENT.extraCardText", "互いに支え合う。" },

                { "PLANET_CERES_ENCHANTMENT.title", "ケレス" },
                { "PLANET_CERES_ENCHANTMENT.description", "戦闘中初めてプレイした時、味方の[purple]ケレス[/purple]エンチャントカードを[gold]コピー[/gold]し、それぞれの山に加える。" },
                { "PLANET_CERES_ENCHANTMENT.extraCardText", "だから独りじゃない。" },

                { "PLANET_ERIS_ENCHANTMENT.title", "エリス" },
                { "PLANET_ERIS_ENCHANTMENT.description", "戦闘中初めてプレイした時、味方の[purple]エリス[/purple]エンチャントカードを[gold]コピー[/gold]し、全プレイヤーの[gold]手札[/gold]に加える。コピーにはエンチャントがない。" },
                { "PLANET_ERIS_ENCHANTMENT.extraCardText", "だから皆で一つ。" },
            });

            var powersTable = loc.GetTable("powers");
            powersTable.MergeWith(new Dictionary<string, string>
            {
                { "TAR_TEMPERANCE_REVERSED_POWER.title", "節制-逆" },
                { "TAR_TEMPERANCE_REVERSED_POWER.description", "このターンに失ったHP[blue]1[/blue]につき、戦闘終了時に等量の[gold]ゴールド[/gold]を得る。" },
                { "TAR_TEMPERANCE_REVERSED_POWER.smartDescription", "このターンに失ったHP[blue]1[/blue]につき、戦闘終了時に[blue]{Amount}[/blue][gold]ゴールド[/gold]を得る。" },

                { "TAR_CHARIOT_REVERSED_POWER.title", "戦車-逆" },
                { "TAR_CHARIOT_REVERSED_POWER.description", "この敵が初めてあなたにブロックされていないダメージを与えた後、あなたは[gold]脆弱[/gold]を[blue]1[/blue]得る。" },
                { "TAR_CHARIOT_REVERSED_POWER.smartDescription", "この敵が初めてあなたにブロックされていないダメージを与えた後、あなたは[gold]脆弱[/gold]を[blue]{Amount}[/blue]得る。" },

                { "TAR_STRENGTH_REVERSED_POWER.title", "力-逆" },
                { "TAR_STRENGTH_REVERSED_POWER.description", "この敵が初めてあなたにブロックされていないダメージを与えた後、あなたは[gold]脱力[/gold]を[blue]1[/blue]得る。" },
                { "TAR_STRENGTH_REVERSED_POWER.smartDescription", "この敵が初めてあなたにブロックされていないダメージを与えた後、あなたは[gold]脱力[/gold]を[blue]{Amount}[/blue]得る。" },

                { "TAR_HERMIT_REVERSED_POWER.title", "隠者-逆" },
                { "TAR_HERMIT_REVERSED_POWER.description", "ターン終了時、この層数に等しい[gold]ブロック[/gold]を得る。ブロックされていないダメージを受けるたびに、[blue]1[/blue]減少する。" },
                { "TAR_HERMIT_REVERSED_POWER.smartDescription", "ターン終了時、[blue]{Amount}[/blue]の[gold]ブロック[/gold]を得る。ブロックされていないダメージを受けるたびに、[blue]1[/blue]減少する。" },

                { "TAR_JUSTICE_REVERSED_POWER.title", "正義-逆" },
                { "TAR_JUSTICE_REVERSED_POWER.description", "毎ターン最初にプレイしたアタックが[gold]廃棄[/gold]される。" },
                { "TAR_JUSTICE_REVERSED_POWER.smartDescription", "毎ターン最初にプレイしたアタックが[gold]廃棄[/gold]される。" },

                { "TAR_HANGED_MAN_REVERSED_POWER.title", "吊るされた男-逆" },
                { "TAR_HANGED_MAN_REVERSED_POWER.description", "毎ターン最初にプレイしたスキルが[gold]廃棄[/gold]される。" },
                { "TAR_HANGED_MAN_REVERSED_POWER.smartDescription", "毎ターン最初にプレイしたスキルが[gold]廃棄[/gold]される。" },

                { "TAR_DEATH_REVERSED_POWER.title", "死神-逆" },
                { "TAR_DEATH_REVERSED_POWER.description", "この戦闘中、[gold]パワー[/gold]をプレイするたびに、即座に[red]ターンを終了[/red]する。" },
                { "TAR_DEATH_REVERSED_POWER.smartDescription", "この戦闘中、[gold]パワー[/gold]をプレイするたびに、即座に[red]ターンを終了[/red]する。" },

                { "PLANET_MERCURY_POWER.title", "水星" },
                { "PLANET_MERCURY_POWER.description", "味方の捨て札フェーズ終了後、その山札の上から5枚を見て、任意の枚数を捨てる。" },
                { "PLANET_MERCURY_POWER.smartDescription", "{PairedName}の捨て札フェーズ終了後、その山札の上から5枚を見て、任意の枚数を捨てる。" },

                { "PLANET_VENUS_POWER.title", "金星" },
                { "PLANET_VENUS_POWER.description", "味方の捨て札フェーズ終了後、その[gold]捨て札[/gold]の上から5枚を見て、任意の枚数をその[gold]手札[/gold]に加える。" },
                { "PLANET_VENUS_POWER.smartDescription", "{PairedName}の捨て札フェーズ終了後、その[gold]捨て札[/gold]の上から5枚を見て、任意の枚数をその[gold]手札[/gold]に加える。" },

                { "PLANET_EARTH_POWER.title", "地球" },
                { "PLANET_EARTH_POWER.description", "味方とエネルギー{energyPrefix:energyIcons(1)}を共有する。" },
                { "PLANET_EARTH_POWER.smartDescription", "{PairedName} とエネルギー{energyPrefix:energyIcons(1)}を共有する。" },

                { "PLANET_MARS_POWER.title", "火星" },
                { "PLANET_MARS_POWER.description", "味方と生成物を共有する。" },
                { "PLANET_MARS_POWER.smartDescription", "{PairedName} と生成物を共有する。" },

                { "PLANET_JUPITER_POWER.title", "木星" },
                { "PLANET_JUPITER_POWER.description", "このターン中、この敵が受ける攻撃ダメージ1につき、戦闘終了後に全プレイヤーが同量の[gold]ゴールド[/gold]を得る。" },
                { "PLANET_JUPITER_POWER.smartDescription", "このターン中、この敵が受ける攻撃ダメージ1につき、戦闘終了後に全プレイヤーが[blue]{Amount}[/blue][gold]ゴールド[/gold]を得る。" },

                { "PLANET_SATURN_POWER.title", "土星" },
                { "PLANET_SATURN_POWER.description", "このターン中、この敵への攻撃ダメージは[blue]{Amount}[/blue]を下回らない。" },
                { "PLANET_SATURN_POWER.smartDescription", "このターン中、この敵への攻撃ダメージは[blue]{Amount}[/blue]を下回らない。" },

                { "PLANET_GOLD_POWER.title", "蓄積" },
                { "PLANET_GOLD_POWER.description", "戦闘終了時、[blue]{Amount}[/blue][gold]ゴールド[/gold]を得る。" },
                { "PLANET_GOLD_POWER.smartDescription", "戦闘終了時、[blue]{Amount}[/blue][gold]ゴールド[/gold]を得る。" },

                { "TICK_TACK_POWER.title", "カウントダウン" },
                { "TICK_TACK_POWER.description", "ゼロになると、ターンが強制終了する。" },
                { "TICK_TACK_POWER.smartDescription", "[blue]{Amount}[/blue]秒後、[red]ターンが強制終了する[/red]。" },
            });

            var afflictionsTable = loc.GetTable("afflictions");
            afflictionsTable.MergeWith(new Dictionary<string, string>
            {
                { "TAR_JUSTICE_REVERSED_AFFLICTION.title", "正義-逆" },
                { "TAR_JUSTICE_REVERSED_AFFLICTION.description", "毎ターン最初にプレイしたアタックが[gold]廃棄[/gold]される。" },

                { "TAR_HANGED_MAN_REVERSED_AFFLICTION.title", "吊るされた男-逆" },
                { "TAR_HANGED_MAN_REVERSED_AFFLICTION.description", "毎ターン最初にプレイしたスキルが[gold]廃棄[/gold]される。" },

                { "TAR_DEATH_REVERSED_AFFLICTION.title", "死神-逆" },
                { "TAR_DEATH_REVERSED_AFFLICTION.description", "プレイ後に即座にターンを終了する。" },
                { "TAR_DEATH_REVERSED_AFFLICTION.extraCardText", "ターンを終了する。" },
            });

            var gameplayUiTable = loc.GetTable("gameplay_ui");
            gameplayUiTable.MergeWith(new Dictionary<string, string>
            {
                { "CHOOSE_CARD_DOWNGRADE_HEADER", "ダウングレードするカードを選ぶ（何枚でも）" },
                { "PLANET_MERCURY_SELECTION_PROMPT", "味方の山札の上から、捨てるカードを任意の枚数選んでください" },
                { "PLANET_VENUS_SELECTION_PROMPT", "味方の捨て札の上から、手札に戻すカードを任意の枚数選んでください" },
                { "VANILLA_STYLE_TAROT", "タロット：クラシック" },
                { "VANILLA_STYLE_PLANET", "プラネット：クラシック" },
            });

            
            var roomTable = loc.GetTable("merchant_room");
            roomTable.MergeWith(new Dictionary<string, string>
            {
                { "TAROT_PILE_ENTRY.title", "タロットパック" },
                { "TAROT_PILE_ENTRY.description", "[blue]3[/blue]枚のタロットカードを引き、[blue]1[/blue]枚選んでデッキのカードに[gold]エンチャント[/gold]を付与する。\n時折、奇妙な効果が発生することも…" }
            });


            var relicsTable = loc.GetTable("relics");
            relicsTable.MergeWith(new Dictionary<string, string>
            {
                { "STARGAZER_KIT.title", "天体観測キット" },
                { "STARGAZER_KIT.description", "[gold]休憩所[/gold]で[gold]天体観測[/gold]を行う。\n[gold]エンシェント[/gold]ノードに入ると[blue]2[/blue]回分の追加使用権を得る。" },
                { "STARGAZER_KIT.flavor", "その良き夜に、おとなしく立ち去ってはいけない。" }
            });

            var restSiteTable = loc.GetTable("rest_site_ui");
            restSiteTable.MergeWith(new Dictionary<string, string>
            {
                { "OPTION_STARGAZE.description", "ランダムな惑星カード3枚から1枚を選び、[gold]デッキ[/gold]の非マルチプレイカード1枚に[gold]エンチャント[/gold]する。" },
                { "OPTION_STARGAZE.name", "天体観測" },
            });

            var mainMenuUiTable = loc.GetTable("main_menu_ui");
            mainMenuUiTable.MergeWith(new Dictionary<string, string>
            {
                { "HEXTECH_WARNING_TITLE", "PengoTarot × Hextech 互換性のお知らせ" },
                { "HEXTECH_WARNING_PAGE1", "PengoTarot と Hextech モッドが同時にインストールされています。PengoTarot 作者からのお知らせ：\n\nHextech は旧式で強引な多重エンチャント実装を使用しており、原版のエンチャント判定をハードコードで修正しているため、ほとんどのエンチャント追加モッドと互換性がありません。" },
                { "HEXTECH_WARNING_PAGE2", "より安定した多重エンチャント体験をご希望の場合は、MultiEnchantment の使用をお勧めします。\n\nご不明な点があれば、まず Hextech モッド作者に連絡して旧式の実装を修正してもらうことをお勧めします。本モッドでは有効な互換性を提供できません。" },
                { "HEXTECH_WARNING_NEXT", "次へ" },
                { "HEXTECH_WARNING_ACK", "わかりました" },
            });
        }
    }
}