// PengoTarot: 按当前语言返回游戏官方替换字体（对齐游戏原版 FontManager 机制）。
// zhs→Noto Sans Mono CJK SC / jpn→Noto CJK JP / kor→韩文：字形正确（非日式异体字），
// 且 glyph 图集已被游戏本体 UI 预热 → 首次显示不卡顿。

#nullable enable
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;

namespace PengoTarot.BalatroEffect;

/// <summary>按当前语言返回游戏官方替换字体；非 CJK（如英文）返回 null，调用方回退到 kreon。</summary>
public static class LocaleFontUtil
{
    public static Font? GetLocaleFont(FontType type)
    {
        if (LocManager.Instance != null && FontManager.NeedsFontSubstitution(LocManager.Instance.Language))
            return FontManager.GetSubstituteFont(LocManager.Instance.Language, type);
        return null;
    }
}
