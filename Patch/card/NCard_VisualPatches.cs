#nullable enable
using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using PengoTarot.Cards;
using PengoTarot.Patch.VisualVanilla;

namespace PengoTarot.Patch.Card;

/// <summary>
/// Patch NCard 的视觉部件，根据卡牌类型定制节点可见性和变换。
///
/// 所有偏移值均使用绝对值（基于 card.tscn 原始值），确保 NCard 被池回收再赋值时不会累加。
///
/// 塔罗牌 (TarCard):
///   - 正位 (Upright): 隐藏 TypePlaque, 灰底缩短
///   - 逆位 (Reversed): 隐藏 TypePlaque, 灰底旋转 180° 移到上半,
///                       DescriptionLabel 移到上半靠下对齐
///   公共: 横幅+标题上移 9px + 左移 1px
/// 星球牌 (PlanetCard):   隐藏 TypePlaque, 灰底扩大+下移, 文本下移
///
/// Patches both Reload and UpdateVisuals: Reload handles initial setup,
/// UpdateVisuals re-applies after layout (fixes pool reuse timing issues).
/// OnFreedToPool resets offsets so normal cards don't inherit modifications.
/// </summary>
[HarmonyPatch]
public static class NCard_VisualPatches
{
    // card.tscn 原始值 (用于复位)
    private const float BannerTop = -207f;
    private const float BannerBottom = -124f;
    private const float BannerLeft = -163f;
    private const float BannerRight = 164f;
    private const float TitleTop = -204f;
    private const float TitleBottom = -150f;
    private const float TitleLeft = -105f;
    private const float TitleRight = 105f;
    private const float DescTop = 37f;
    private const float DescBottom = 173f;
    private const float TextBgTop = -22f;
    private const float TextBgBottom = 181f;
    private const float TextBgLeft = -133f;
    private const float TextBgRight = 131f;
    private const float PortraitLeft = -153f;
    private const float PortraitRight = 445f;
    private const float BorderLeft = -154f;
    private const float BorderRight = 152f;
    private const float BorderGlassLeft = -148.465f;
    private const float BorderGlassRight = 442.08f;

    [HarmonyPatch(typeof(NCard), "Reload"), HarmonyPostfix]
    private static void Reload_Postfix(NCard __instance)
    {
        // Defer to next frame: Reload may destroy/recreate children,
        // and ShaderController's TiltContainer QueueFree may not have completed yet.
        Callable.From(() => ApplyVisuals(__instance)).CallDeferred();
    }

    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals)), HarmonyPostfix]
    private static void UpdateVisuals_Postfix(NCard __instance) => ApplyVisuals(__instance);

    [HarmonyPatch(typeof(NCard), nameof(NCard.OnFreedToPool)), HarmonyPostfix]
    private static void OnFreedToPool_Postfix(NCard __instance) => ResetPengoTarotVisuals(__instance);

    private static void ApplyVisuals(NCard __instance)
    {
        if (!GodotObject.IsInstanceValid(__instance)) return;
        var model = __instance.Model;
        if (model == null) return;

        // Always reset ZIndex to prevent pollution when a card transitions
        // between TarCard/PlanetCard and regular cards in the library grid.
        // TODO: re-enable after fixing text layer issue in card library
        // ResetTextZIndex(__instance);

        // Only touch TarCard / PlanetCard — leave regular cards untouched
        // to avoid overwriting other mods' layout changes.
        if (model is TarCard)
        {
            ResetPengoTarotVisuals(__instance);
            ApplyTarCard(__instance, model);
        }
        else if (model is PlanetCard)
        {
            ResetPengoTarotVisuals(__instance);
            ApplyPlanetCard(__instance);
        }
    }

    /// <summary>Reset all modified offsets to card.tscn defaults.</summary>
    public static void ResetPengoTarotVisuals(NCard __instance)
    {
        if (!GodotObject.IsInstanceValid(__instance)) return;
        var typePlaque = __instance.FindChild("TypePlaque", recursive: true, owned: false) as NinePatchRect;
        if (typePlaque != null)
            typePlaque.Visible = true;

        var ancientBanner = __instance.FindChild("AncientBanner", recursive: true, owned: false) as Control;
        if (ancientBanner != null)
        {
            ancientBanner.Visible = true;
            ancientBanner.Modulate = Colors.White;
            ancientBanner.OffsetTop = BannerTop;
            ancientBanner.OffsetBottom = BannerBottom;
            ancientBanner.OffsetLeft = BannerLeft;
            ancientBanner.OffsetRight = BannerRight;
        }

        var titleLabel = __instance.FindChild("TitleLabel", recursive: true, owned: false) as Label;
        if (titleLabel != null)
        {
            titleLabel.Visible = true;
            titleLabel.Modulate = Colors.White;
            titleLabel.ZIndex = 0;
            titleLabel.OffsetTop = TitleTop;
            titleLabel.OffsetBottom = TitleBottom;
            // OffsetLeft/OffsetRight managed by SetTextAutoSize - do not reset
        }

        var ancientTextBg = __instance.FindChild("AncientTextBg", recursive: true, owned: false) as TextureRect;
        if (ancientTextBg != null)
        {
            ancientTextBg.PivotOffset = Vector2.Zero;
            ancientTextBg.Rotation = 0;
            ancientTextBg.OffsetTop = TextBgTop;
            ancientTextBg.OffsetBottom = TextBgBottom;
            ancientTextBg.OffsetLeft = TextBgLeft;
            ancientTextBg.OffsetRight = TextBgRight;
        }

        var descLabel = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as RichTextLabel;
        if (descLabel != null)
        {
            descLabel.OffsetTop = DescTop;
            descLabel.OffsetBottom = DescBottom;
            descLabel.VerticalAlignment = VerticalAlignment.Center;
            descLabel.ZIndex = 0;
        }

        var ancientPortrait = __instance.FindChild("AncientPortrait", recursive: true, owned: false) as TextureRect;
        if (ancientPortrait != null)
        {
            ancientPortrait.OffsetLeft = PortraitLeft;
            ancientPortrait.OffsetRight = PortraitRight;
        }

        var ancientBorder = __instance.FindChild("AncientBorder", recursive: true, owned: false) as TextureRect;
        if (ancientBorder != null)
        {
            ancientBorder.OffsetLeft = BorderLeft;
            ancientBorder.OffsetRight = BorderRight;
        }

        var ancientBorderGlass = __instance.FindChild("AncientBorderGlassOverlay", recursive: true, owned: false) as TextureRect;
        if (ancientBorderGlass != null)
        {
            ancientBorderGlass.OffsetLeft = BorderGlassLeft;
            ancientBorderGlass.OffsetRight = BorderGlassRight;
        }
    }

    /// <summary>Reset only ZIndex on TitleLabel and DescriptionLabel.
    /// Safe to call on any card type — won't affect layout.</summary>
    private static void ResetTextZIndex(NCard __instance)
    {
        var title = __instance.FindChild("TitleLabel", recursive: true, owned: false) as Control;
        if (title != null) title.ZIndex = 0;
        var desc = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as Control;
        if (desc != null) desc.ZIndex = 0;
    }

    private static void ApplyTarCard(NCard __instance, CardModel model)
    {
        bool isVanilla = VanillaStyleConfig.TarotVanilla;
        bool isReversed = model.GetType().Name.Contains("Reversed");

        // ---- 公共：隐藏类型徽章 ----
        var typePlaque = __instance.FindChild("TypePlaque", recursive: true, owned: false) as NinePatchRect;
        if (typePlaque != null)
            typePlaque.Visible = false;

        if (isVanilla)
        {
            // ---- 经典样式：横幅和标题不透明+上移，保留其他自定义布局 ----
            var ancientBanner = __instance.FindChild("AncientBanner", recursive: true, owned: false) as Control;
            if (ancientBanner != null)
            {
                ancientBanner.Modulate = Colors.White;
                ancientBanner.OffsetTop = BannerTop - 8;
                ancientBanner.OffsetBottom = BannerBottom - 8;
            }

            var titleLabel = __instance.FindChild("TitleLabel", recursive: true, owned: false) as Label;
            if (titleLabel != null)
            {
                titleLabel.Modulate = Colors.White;
                titleLabel.OffsetTop = TitleTop - 5;
                titleLabel.OffsetBottom = TitleBottom - 5;
            }

            if (isReversed)
            {
                // 逆位经典：恢复文本框默认位置+拉底
                var ancientTextBg = __instance.FindChild("AncientTextBg", recursive: true, owned: false) as TextureRect;
                if (ancientTextBg != null)
                {
                    ancientTextBg.PivotOffset = Vector2.Zero;
                    ancientTextBg.Rotation = 0;
                    ancientTextBg.Visible = true;
                    ancientTextBg.OffsetTop = TextBgTop;
                    ancientTextBg.OffsetBottom = 205;
                }

                var descLabel = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as RichTextLabel;
                if (descLabel != null)
                {
                    descLabel.OffsetTop = DescTop;
                    descLabel.OffsetBottom = DescBottom;
                    descLabel.VerticalAlignment = VerticalAlignment.Center;
                }
            }
            else
            {
                // 正位经典：保留灰底拉长到接近底部
                var ancientTextBg = __instance.FindChild("AncientTextBg", recursive: true, owned: false) as TextureRect;
                if (ancientTextBg != null)
                {
                    ancientTextBg.Visible = true;
                    ancientTextBg.PivotOffset = Vector2.Zero;
                    ancientTextBg.Rotation = 0;
                    ancientTextBg.OffsetTop = TextBgTop;
                    ancientTextBg.OffsetBottom = 205;
                }

                var descLabel = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as RichTextLabel;
                if (descLabel != null)
                {
                    descLabel.OffsetTop = DescTop;
                    descLabel.OffsetBottom = DescBottom;
                    descLabel.VerticalAlignment = VerticalAlignment.Center;
                }
            }

            // TODO: re-enable after fixing text layer issue
            // SetTextAboveOverlay(__instance);
            // ApplyTextShadow(__instance);
            return;
        }

        // ---- 自定义样式：横幅和标题透明（不隐藏，避免字体重渲染卡顿） ----
        var ancientBanner2 = __instance.FindChild("AncientBanner", recursive: true, owned: false) as Control;
        if (ancientBanner2 != null)
            ancientBanner2.Modulate = Colors.Transparent;

        var titleLabel2 = __instance.FindChild("TitleLabel", recursive: true, owned: false) as Label;
        if (titleLabel2 != null)
            titleLabel2.Modulate = Colors.Transparent;

        if (isReversed)
        {
            // ---- 逆位：灰底旋转 180° 移到上半 ----
            var ancientTextBg = __instance.FindChild("AncientTextBg", recursive: true, owned: false) as TextureRect;
            if (ancientTextBg != null)
            {
                // Pivot computed from offsets (not Size, which may be stale after pool reuse)
                float w = TextBgRight - TextBgLeft;  // 131 - (-133) = 264
                float h = 22f - (-181f);              // 203
                ancientTextBg.PivotOffset = new Vector2(w / 2f, h / 2f);
                ancientTextBg.Rotation = Mathf.Pi;
                ancientTextBg.Visible = true;
                ancientTextBg.OffsetTop = -211;
                ancientTextBg.OffsetBottom = -5;
            }

            // ---- 逆位：描述文本移到上半 ----
            var descLabel = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as RichTextLabel;
            if (descLabel != null)
            {
                descLabel.OffsetTop = -211;
                descLabel.OffsetBottom = -40;
            }
        }
        else
        {
            // ---- 正位：灰底缩短，上边不变，下边接近底边 ----
            var ancientTextBg = __instance.FindChild("AncientTextBg", recursive: true, owned: false) as TextureRect;
            if (ancientTextBg != null)
            {
                ancientTextBg.Visible = true;
                ancientTextBg.PivotOffset = Vector2.Zero;
                ancientTextBg.Rotation = 0;
                ancientTextBg.OffsetTop = TextBgTop;
                ancientTextBg.OffsetBottom = 205;
            }

            // ---- 正位：恢复描述文本到默认位置 ----
            var descLabel = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as RichTextLabel;
            if (descLabel != null)
            {
                descLabel.OffsetTop = DescTop;
                descLabel.OffsetBottom = DescBottom;
                descLabel.VerticalAlignment = VerticalAlignment.Center;
            }
        }

        // TODO: re-enable after fixing text layer issue
        // SetTextAboveOverlay(__instance);
        // ApplyTextShadow(__instance);
    }

    private static void ApplyPlanetCard(NCard __instance)
    {
        bool isVanilla = VanillaStyleConfig.PlanetVanilla;

        // 隐藏卡牌类型徽章
        var typePlaque = __instance.FindChild("TypePlaque", recursive: true, owned: false) as NinePatchRect;
        if (typePlaque != null)
            typePlaque.Visible = false;

        // ---- 横幅和标题：vanilla 不透明，自定义透明（不隐藏，避免字体重渲染卡顿） ----
        var ancientBanner = __instance.FindChild("AncientBanner", recursive: true, owned: false) as Control;
        if (ancientBanner != null)
            ancientBanner.Modulate = isVanilla ? Colors.White : Colors.Transparent;

        var titleLabel = __instance.FindChild("TitleLabel", recursive: true, owned: false) as Label;
        if (titleLabel != null)
            titleLabel.Modulate = isVanilla ? Colors.White : Colors.Transparent;

        // ---- 以下自定义布局始终生效（frame/portrait/textbg/desc） ----

        // 图片偏移
        var ancientPortrait = __instance.FindChild("AncientPortrait", recursive: true, owned: false) as TextureRect;
        if (ancientPortrait != null)
        {
            ancientPortrait.OffsetLeft = PortraitLeft + 0.5f;
            ancientPortrait.OffsetRight = PortraitRight + 0.5f;
        }

        // 先古卡框：维持高度(440)不变，长:宽 = 190:142
        var ancientBorder = __instance.FindChild("AncientBorder", recursive: true, owned: false) as TextureRect;
        if (ancientBorder != null)
        {
            ancientBorder.OffsetLeft = -165;
            ancientBorder.OffsetRight = 164;
        }

        var ancientBorderGlass = __instance.FindChild("AncientBorderGlassOverlay", recursive: true, owned: false) as TextureRect;
        if (ancientBorderGlass != null)
        {
            ancientBorderGlass.OffsetLeft = -165f;
            ancientBorderGlass.OffsetRight = 493f;
        }

        // 灰底宽度同步变化
        var ancientTextBg = __instance.FindChild("AncientTextBg", recursive: true, owned: false) as TextureRect;
        if (ancientTextBg != null)
        {
            ancientTextBg.Visible = true;
            ancientTextBg.PivotOffset = Vector2.Zero;
            ancientTextBg.Rotation = 0;
            ancientTextBg.Modulate = new Color(0, 0, 0, 0.50f);
            ancientTextBg.OffsetLeft = -165;
            ancientTextBg.OffsetRight = 164;
            ancientTextBg.OffsetTop = 5;
            ancientTextBg.OffsetBottom = 211;
        }

        // 文字区域往下压缩15px
        var descLabel = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as RichTextLabel;
        if (descLabel != null)
        {
            descLabel.OffsetTop = 52;
            descLabel.OffsetBottom = 188;
        }

        // TODO: re-enable after fixing text layer issue
        // SetTextAboveOverlay(__instance);
        // ApplyTextShadow(__instance);
    }

    /// <summary>Raise TitleLabel and DescriptionLabel ZIndex above OverlayContainer.</summary>
    private static void SetTextAboveOverlay(NCard __instance)
    {
        int z = 1; // default: above OverlayContainer (ZIndex=0)
        var overlay = __instance.FindChild("OverlayContainer", recursive: true, owned: false) as Control;
        if (overlay != null) z = overlay.ZIndex + 1;

        var title = __instance.FindChild("TitleLabel", recursive: true, owned: false) as Control;
        if (title != null) title.ZIndex = z;

        var desc = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as Control;
        if (desc != null) desc.ZIndex = z;
    }

    /// <summary>White outline + shadow for DescriptionLabel on TarCard/PlanetCard.</summary>
    private static void ApplyTextShadow(NCard __instance)
    {
        var descLabel = __instance.FindChild("DescriptionLabel", recursive: true, owned: false) as RichTextLabel;
        if (descLabel != null)
        {
            // Thick black shadow (zero offset, only outline thickness)
            descLabel.AddThemeColorOverride(ThemeConstants.RichTextLabel.FontShadowColor, new Color(0, 0, 0, 0.85f));
            descLabel.AddThemeConstantOverride("shadow_outline_size", 8);
        }
    }

    
    [HarmonyPatch(typeof(NInspectCardScreen), "SetCard")]
    [HarmonyPrefix]
    private static void SetCard_Prefix(NInspectCardScreen __instance, int index)
    {
        var cardField = AccessTools.Field(typeof(NInspectCardScreen), "_card");
        var card = cardField?.GetValue(__instance) as NCard;
        if (card == null || !GodotObject.IsInstanceValid(card) || card.Model == null) return;

        var model = card.Model;
        if (model is TarCard || model is PlanetCard)
        {
            ResetPengoTarotVisuals(card);
        }
    }
}
