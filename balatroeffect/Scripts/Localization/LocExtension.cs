// Based on code from BalatroEffects by Indi (MIT License)
// Modified for PengoTarot: extended part keys, merged ancient parts

#nullable enable

using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace PengoTarot.BalatroEffect
{
    public static class LocExtension
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


        private static readonly Dictionary<string, string> ChineseTexts = new()
        {
            { "BAL_LABEL_EFFECT", "效果" },
            { "BAL_MODE_NORMAL", "普通" },
            { "BAL_MODE_SEPARATELY", "独立" },
            { "BAL_MODE_FULLY", "整卡" },
            { "BAL_LABEL_INTENSITY", "强度" },
            { "BAL_LABEL_PARTS", "卡面部件" },
            { "BAL_HINT_CHECK_PARTS", "被勾选的部件将会应用对应的效果" },
            { "BAL_GLOBAL_DYNAMIC_EFFECT", "全局3D效果" },
            { "BAL_BTN_HIDE_PANEL", "隐藏面板" },
            { "BAL_BTN_SHOW_PANEL", "显示面板" },
            { "BAL_BTN_COPY_CARD", "复制效果" },
            { "BAL_BTN_PASTE_CARD", "粘贴效果" },
            { "BAL_BTN_ENTRY", "PengoTarot编辑器" },
            { "BAL_VIEW_ENCHANTMENTS", "查看附魔专用特效配置界面" },
            { "BAL_BTN_EXPORT", "导出预设" },
            { "BAL_BTN_IMPORT", "导入预设" },
            { "BAL_TOGGLE_TILT", "动态倾斜" },
            { "BAL_MENU_TITLE", "更多选项" },
            { "BAL_MENU_CLEAR", "清空当前卡牌效果" },
            { "BAL_MENU_CLEAR_ENCHANT", "清空当前附魔效果" },
            { "BAL_MENU_RESET", "恢复当前卡牌默认效果" },
            { "BAL_MENU_APPLY_TO_VISIBLE", "将当前卡效果应用给同页面所有卡牌" },
            { "BAL_MENU_EXPORT_GLOBAL", "导出全局效果（到剪贴簿）" },
            { "BAL_MENU_IMPORT_GLOBAL", "导入全局效果（从剪贴簿）" },
            { "BAL_MENU_LOAD_AUTHOR_GLOBAL", "使用预设全局效果" },
            { "BAL_MENU_LOAD_AUTHOR", "读取作者预设" },
            { "BAL_OPTION_NONE", "无" },
            { "BAL_OPTION_FOIL", "闪箔" },
            { "BAL_OPTION_NEGATIVE", "负片-A" },
            { "BAL_OPTION_NEGATIVE_BLUE", "负片-B" },
            { "BAL_OPTION_POLYCHROME", "多彩" },
            { "BAL_OPTION_HOLOGRAPHIC", "镭射" },
            { "BAL_OPTION_FOIL_ALT", "闪箔-偏" },

            { "BAL_OPTION_ANISO_FIXED", "放射虹" },
            { "BAL_OPTION_ANISO_STRIPE", "线状虹" },
            { "BAL_OPTION_ANISO_DUAL", "双重虹" },

            { "BAL_OPTION_VHS", "录像带" },
            { "BAL_OPTION_CRT", "CRT" },
            { "BAL_OPTION_VHS2", "故障" },
            { "BAL_OPTION_SWEEP", "扫光" },

            { "BAL_OPTION_HOVER_GLOW", "高光" },
            { "BAL_OPTION_GLITTER", "碎彩" },
            { "BAL_OPTION_AURORA", "极光" },
            { "BAL_OPTION_PIXELATE", "像素" },
            { "BAL_OPTION_OUTLINE", "描边" },
            { "BAL_OPTION_STARCLOUD", "星辰" },
            { "BAL_OPTION_RANDOMSTARS", "星芒" },

            { "BAL_PART_PORTRAIT", "插画" },
            { "BAL_PART_FRAME", "边框" },
            { "BAL_PART_TITLEBANNER", "横幅" },
            { "BAL_PART_TITLELABEL", "标题文字" },
            { "BAL_PART_DESCRIPTIONLABEL", "描述文字" },
            { "BAL_PART_TYPEPLAQUE", "类型" },
            { "BAL_PART_PORTRAITBORDER", "画框" },
            { "BAL_PART_ENERGYICON", "能量" },
            { "BAL_PART_STARICON", "星辰" },
            { "BAL_PART_SHADOW", "阴影" },
            { "BAL_PART_FULLCARD", "完整卡牌" },
            { "BAL_PART_PORTRAITCANVASGROUP", "插画" },
            { "BAL_PART_ENCHANTMENT", "附魔" },
            { "BAL_PART_CARDVFXCONTAINER", "卡牌特效" },
            { "BAL_PART_OVERLAYCONTAINER", "覆盖层" },

            { "BAL_PART_ANCIENTBORDER", "边框(先古)" },
            { "BAL_PART_ANCIENTBANNER", "标题横幅(先古)" },
            { "BAL_PART_ANCIENTTEXTBG", "文本背景(先古)" },
            { "BAL_PART_ANCIENTPORTRAIT", "插画(先古)" },
            { "BAL_PART_ANCIENTHIGHLIGHT", "高光(先古)" },

            { "BAL_BETA_WARNING_TITLE", "功能测试中" },
            { "BAL_BETA_WARNING_TEXT", "该功能仍在测试中，开启可能导致mod兼容问题。\n是否继续？" },
            { "BAL_BETA_WARNING_CONFIRM", "确定" },
        };


        private static readonly Dictionary<string, string> EnglishTexts = new()
        {
            { "BAL_LABEL_EFFECT", "Effect" },
            { "BAL_MODE_NORMAL", "Normal" },
            { "BAL_MODE_SEPARATELY", "Separately" },
            { "BAL_MODE_FULLY", "Fully" },
            { "BAL_LABEL_INTENSITY", "Intensity" },
            { "BAL_LABEL_PARTS", "Card Parts" },
            { "BAL_HINT_CHECK_PARTS", "Checked parts will have the effect applied" },
            { "BAL_GLOBAL_DYNAMIC_EFFECT", "3D Tilt Effect" },
            { "BAL_BTN_HIDE_PANEL", "Hide Panel" },
            { "BAL_BTN_SHOW_PANEL", "Show Panel" },
            { "BAL_BTN_COPY_CARD", "Copy Effect" },
            { "BAL_BTN_PASTE_CARD", "Paste Effect" },
            { "BAL_BTN_ENTRY", "PengoTarot Editor" },
            { "BAL_VIEW_ENCHANTMENTS", "View Enchant-Effect Config Screen" },
            { "BAL_BTN_EXPORT", "Export Preset" },
            { "BAL_BTN_IMPORT", "Import Preset" },
            { "BAL_TOGGLE_TILT", "Dynamic Tilt" },
            { "BAL_MENU_TITLE", "More Options" },
            { "BAL_MENU_CLEAR", "Clear Current Card Effects" },
            { "BAL_MENU_CLEAR_ENCHANT", "Clear Current Enchant Effects" },
            { "BAL_MENU_RESET", "Reset Current Card to Default" },
            { "BAL_MENU_APPLY_TO_VISIBLE", "Apply current effect to all cards on this page" },
            { "BAL_MENU_EXPORT_GLOBAL", "Export Global Effect (to clipboard)" },
            { "BAL_MENU_IMPORT_GLOBAL", "Import Global Effect (from clipboard)" },
            { "BAL_MENU_LOAD_AUTHOR_GLOBAL", "Load Author Preset Global Effect" },
            { "BAL_MENU_LOAD_AUTHOR", "Load Author Preset" },
            { "BAL_OPTION_NONE", "None" },
            { "BAL_OPTION_FOIL", "Foil" },
            { "BAL_OPTION_NEGATIVE", "Negative A" },
            { "BAL_OPTION_NEGATIVE_BLUE", "Negative B" },
            { "BAL_OPTION_POLYCHROME", "Polychrome" },
            { "BAL_OPTION_HOLOGRAPHIC", "Holographic" },
            { "BAL_OPTION_FOIL_ALT", "Foil Alt" },

            { "BAL_OPTION_ANISO_FIXED", "Aniso Fixed" },
            { "BAL_OPTION_ANISO_STRIPE", "Aniso Stripe" },
            { "BAL_OPTION_ANISO_DUAL", "Aniso Dual" },

            { "BAL_OPTION_VHS", "VHS" },
            { "BAL_OPTION_CRT", "CRT" },
            { "BAL_OPTION_VHS2", "Glitch" },
            { "BAL_OPTION_SWEEP", "Sweep" },

            { "BAL_OPTION_HOVER_GLOW", "Hover Glow" },
            { "BAL_OPTION_GLITTER", "Glitter" },
            { "BAL_OPTION_AURORA", "Aurora" },
            { "BAL_OPTION_PIXELATE", "Pixelate" },
            { "BAL_OPTION_OUTLINE", "Outline" },
            { "BAL_OPTION_STARCLOUD", "Star Cloud" },
            { "BAL_OPTION_RANDOMSTARS", "Star Spray" },

            { "BAL_PART_PORTRAIT", "Portrait" },
            { "BAL_PART_FRAME", "Frame" },
            { "BAL_PART_TITLEBANNER", "Banner" },
            { "BAL_PART_TITLELABEL", "Title Label" },
            { "BAL_PART_DESCRIPTIONLABEL", "Description" },
            { "BAL_PART_TYPEPLAQUE", "Type" },
            { "BAL_PART_PORTRAITBORDER", "Portrait Frame" },
            { "BAL_PART_ENERGYICON", "Energy" },
            { "BAL_PART_STARICON", "Rarity Star" },
            { "BAL_PART_SHADOW", "Shadow" },
            { "BAL_PART_FULLCARD", "Full Card" },
            { "BAL_PART_PORTRAITCANVASGROUP", "Portrait" },
            { "BAL_PART_ENCHANTMENT", "Enchantment" },
            { "BAL_PART_CARDVFXCONTAINER", "Card VFX" },
            { "BAL_PART_OVERLAYCONTAINER", "Overlay" },

            { "BAL_PART_ANCIENTBORDER", "Frame (Ancient)" },
            { "BAL_PART_ANCIENTBANNER", "Title Banner (Ancient)" },
            { "BAL_PART_ANCIENTTEXTBG", "Text BG (Ancient)" },
            { "BAL_PART_ANCIENTPORTRAIT", "Portrait (Ancient)" },
            { "BAL_PART_ANCIENTHIGHLIGHT", "Highlight (Ancient)" },

            { "BAL_BETA_WARNING_TITLE", "Feature in Testing" },
            { "BAL_BETA_WARNING_TEXT", "This feature is still in testing and may cause mod compatibility issues.\nContinue?" },
            { "BAL_BETA_WARNING_CONFIRM", "OK" },
        };


        private static readonly Dictionary<string, string> JapaneseTexts = new()
        {
            { "BAL_LABEL_EFFECT", "エフェクト" },
            { "BAL_MODE_NORMAL", "通常" },
            { "BAL_MODE_SEPARATELY", "個別" },
            { "BAL_MODE_FULLY", "全体" },
            { "BAL_LABEL_INTENSITY", "強度" },
            { "BAL_LABEL_PARTS", "カードパーツ" },
            { "BAL_HINT_CHECK_PARTS", "チェックしたパーツにエフェクトを適用" },
            { "BAL_GLOBAL_DYNAMIC_EFFECT", "全体3D傾斜" },
            { "BAL_BTN_HIDE_PANEL", "パネルを隠す" },
            { "BAL_BTN_SHOW_PANEL", "パネルを表示" },
            { "BAL_BTN_COPY_CARD", "エフェクトをコピー" },
            { "BAL_BTN_PASTE_CARD", "エフェクトを貼り付け" },
            { "BAL_BTN_ENTRY", "PengoTarot 編集者" },
            { "BAL_VIEW_ENCHANTMENTS", "エンチャント専用エフェクト設定画面を見る" },
            { "BAL_BTN_EXPORT", "プリセットをエクスポート" },
            { "BAL_BTN_IMPORT", "プリセットをインポート" },
            { "BAL_TOGGLE_TILT", "動的傾き" },
            { "BAL_MENU_TITLE", "その他" },
            { "BAL_MENU_CLEAR", "現在のカードのエフェクトを消去" },
            { "BAL_MENU_CLEAR_ENCHANT", "現在のエンチャントのエフェクトを消去" },
            { "BAL_MENU_RESET", "現在のカードをデフォルトに戻す" },
            { "BAL_MENU_APPLY_TO_VISIBLE", "現在の効果をページ内の全カードに適用" },
            { "BAL_MENU_EXPORT_GLOBAL", "グローバル効果をエクスポート（クリップボードに）" },
            { "BAL_MENU_IMPORT_GLOBAL", "グローバル効果をインポート（クリップボードから）" },
            { "BAL_MENU_LOAD_AUTHOR_GLOBAL", "作成者のグローバル効果を読み込む" },
            { "BAL_MENU_LOAD_AUTHOR", "作者プリセットを読み込む" },
            { "BAL_OPTION_NONE", "なし" },
            { "BAL_OPTION_FOIL", "ホイル" },
            { "BAL_OPTION_NEGATIVE", "ネガ・エー" },
            { "BAL_OPTION_NEGATIVE_BLUE", "ネガ・ビー" },
            { "BAL_OPTION_POLYCHROME", "ポリクローム" },
            { "BAL_OPTION_HOLOGRAPHIC", "ホログラフィック" },
            { "BAL_OPTION_FOIL_ALT", "ホイル オルタ" },

            { "BAL_OPTION_ANISO_FIXED", "アニソ固定" },
            { "BAL_OPTION_ANISO_STRIPE", "アニソストライプ" },
            { "BAL_OPTION_ANISO_DUAL", "アニソデュアル" },

            { "BAL_OPTION_VHS", "VHS" },
            { "BAL_OPTION_CRT", "CRT" },
            { "BAL_OPTION_VHS2", "グリッチ" },
            { "BAL_OPTION_SWEEP", "スイープ" },

            { "BAL_OPTION_HOVER_GLOW", "ホバーグロー" },
            { "BAL_OPTION_GLITTER", "グリッター" },
            { "BAL_OPTION_AURORA", "オーロラ" },
            { "BAL_OPTION_PIXELATE", "ピクセル" },
            { "BAL_OPTION_OUTLINE", "アウトライン" },
            { "BAL_OPTION_STARCLOUD", "星雲" },
            { "BAL_OPTION_RANDOMSTARS", "スプレー" },

            { "BAL_PART_PORTRAIT", "ポートレート" },
            { "BAL_PART_FRAME", "フレーム" },
            { "BAL_PART_TITLEBANNER", "バナー" },
            { "BAL_PART_TITLELABEL", "タイトル文字" },
            { "BAL_PART_DESCRIPTIONLABEL", "説明文" },
            { "BAL_PART_TYPEPLAQUE", "タイプ" },
            { "BAL_PART_PORTRAITBORDER", "絵枠" },
            { "BAL_PART_ENERGYICON", "エネルギー" },
            { "BAL_PART_STARICON", "レアリティ星" },
            { "BAL_PART_SHADOW", "影" },
            { "BAL_PART_FULLCARD", "カード全体" },
            { "BAL_PART_PORTRAITCANVASGROUP", "ポートレート" },
            { "BAL_PART_ENCHANTMENT", "エンチャント" },
            { "BAL_PART_CARDVFXCONTAINER", "カードVFX" },
            { "BAL_PART_OVERLAYCONTAINER", "オーバーレイ" },

            { "BAL_PART_ANCIENTBORDER", "フレーム(古代)" },
            { "BAL_PART_ANCIENTBANNER", "タイトルバナー(古代)" },
            { "BAL_PART_ANCIENTTEXTBG", "テキスト背景(古代)" },
            { "BAL_PART_ANCIENTPORTRAIT", "ポートレート(古代)" },
            { "BAL_PART_ANCIENTHIGHLIGHT", "ハイライト(古代)" },

            { "BAL_BETA_WARNING_TITLE", "機能テスト中" },
            { "BAL_BETA_WARNING_TEXT", "この機能はテスト中であり、MODの互換性問題を引き起こす可能性があります。\n続行しますか？" },
            { "BAL_BETA_WARNING_CONFIRM", "OK" },
        };


        private static readonly Dictionary<string, string> KoreanTexts = new()
        {
            { "BAL_LABEL_EFFECT", "효과" },
            { "BAL_MODE_NORMAL", "일반" },
            { "BAL_MODE_SEPARATELY", "개별" },
            { "BAL_MODE_FULLY", "전체" },
            { "BAL_LABEL_INTENSITY", "강도" },
            { "BAL_LABEL_PARTS", "카드 파츠" },
            { "BAL_HINT_CHECK_PARTS", "체크된 파츠에 효과가 적용됩니다" },
            { "BAL_GLOBAL_DYNAMIC_EFFECT", "전체 3D 기울기" },
            { "BAL_BTN_HIDE_PANEL", "패널 숨기기" },
            { "BAL_BTN_SHOW_PANEL", "패널 표시" },
            { "BAL_BTN_COPY_CARD", "효과 복사" },
            { "BAL_BTN_PASTE_CARD", "효과 붙여넣기" },
            { "BAL_BTN_ENTRY", "PengoTarot 편집자" },
            { "BAL_VIEW_ENCHANTMENTS", "인챈트 전용 이펙트 설정 화면 보기" },
            { "BAL_BTN_EXPORT", "프리셋 내보내기" },
            { "BAL_BTN_IMPORT", "프리셋 가져오기" },
            { "BAL_TOGGLE_TILT", "동적 기울기" },
            { "BAL_MENU_TITLE", "추가 옵션" },
            { "BAL_MENU_CLEAR", "현재 카드 효과 지우기" },
            { "BAL_MENU_CLEAR_ENCHANT", "현재 인챈트 효과 지우기" },
            { "BAL_MENU_RESET", "현재 카드를 기본값으로 초기화" },
            { "BAL_MENU_APPLY_TO_VISIBLE", "현재 효과를 이 페이지의 모든 카드에 적용" },
            { "BAL_MENU_EXPORT_GLOBAL", "전역 효과 내보내기(클립보드)" },
            { "BAL_MENU_IMPORT_GLOBAL", "전역 효과 가져오기(클립보드)" },
            { "BAL_MENU_LOAD_AUTHOR_GLOBAL", "제작자 전역 효과 프리셋 불러오기" },
            { "BAL_MENU_LOAD_AUTHOR", "제작자 프리셋 불러오기" },
            { "BAL_OPTION_NONE", "없음" },
            { "BAL_OPTION_FOIL", "포일" },
            { "BAL_OPTION_NEGATIVE", "네거티브 A" },
            { "BAL_OPTION_NEGATIVE_BLUE", "네거티브 B" },
            { "BAL_OPTION_POLYCHROME", "폴리크롬" },
            { "BAL_OPTION_HOLOGRAPHIC", "홀로그래픽" },
            { "BAL_OPTION_FOIL_ALT", "포일 변형" },

            { "BAL_OPTION_ANISO_FIXED", "애니소 고정" },
            { "BAL_OPTION_ANISO_STRIPE", "애니소 스트라이프" },
            { "BAL_OPTION_ANISO_DUAL", "애니소 듀얼" },

            { "BAL_OPTION_VHS", "VHS" },
            { "BAL_OPTION_CRT", "CRT" },
            { "BAL_OPTION_VHS2", "글리치" },
            { "BAL_OPTION_SWEEP", "스윕" },

            { "BAL_OPTION_HOVER_GLOW", "호버 발광" },
            { "BAL_OPTION_GLITTER", "글리터" },
            { "BAL_OPTION_AURORA", "오로라" },
            { "BAL_OPTION_PIXELATE", "픽셀" },
            { "BAL_OPTION_OUTLINE", "외곽선" },
            { "BAL_OPTION_STARCLOUD", "별구름" },
            { "BAL_OPTION_RANDOMSTARS", "별 산발" },

            { "BAL_PART_PORTRAIT", "일러스트" },
            { "BAL_PART_FRAME", "프레임" },
            { "BAL_PART_TITLEBANNER", "배너" },
            { "BAL_PART_TITLELABEL", "카드명" },
            { "BAL_PART_DESCRIPTIONLABEL", "설명" },
            { "BAL_PART_TYPEPLAQUE", "유형" },
            { "BAL_PART_PORTRAITBORDER", "그림틀" },
            { "BAL_PART_ENERGYICON", "에너지" },
            { "BAL_PART_STARICON", "희귀도 별" },
            { "BAL_PART_SHADOW", "그림자" },
            { "BAL_PART_FULLCARD", "카드 전체" },
            { "BAL_PART_PORTRAITCANVASGROUP", "일러스트" },
            { "BAL_PART_ENCHANTMENT", "인챈트" },
            { "BAL_PART_CARDVFXCONTAINER", "카드 VFX" },
            { "BAL_PART_OVERLAYCONTAINER", "오버레이" },

            { "BAL_PART_ANCIENTBORDER", "프레임(고대)" },
            { "BAL_PART_ANCIENTBANNER", "제목 배너(고대)" },
            { "BAL_PART_ANCIENTTEXTBG", "텍스트 배경(고대)" },
            { "BAL_PART_ANCIENTPORTRAIT", "일러스트(고대)" },
            { "BAL_PART_ANCIENTHIGHLIGHT", "하이라이트(고대)" },

            { "BAL_BETA_WARNING_TITLE", "기능 테스트 중" },
            { "BAL_BETA_WARNING_TEXT", "이 기능은 아직 테스트 중이며, 모드 호환성 문제가 발생할 수 있습니다.\n계속하시겠습니까?" },
            { "BAL_BETA_WARNING_CONFIRM", "확인" },
        };
    }
}