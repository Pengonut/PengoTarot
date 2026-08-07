#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using static Godot.Control;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 给 PengoTarot 模组的 HoverTip 逐个添加金色 shader 特效。
    /// 
    /// 依赖 ModelHoverTipCanonicalPatch 来确保 EnchantmentModel /
    /// PowerModel / AfflictionModel 的 HoverTip 正确设置了 CanonicalModel。
    /// 
    /// 对每个文本 HoverTip 单独判断：只有当该 tip 的 CanonicalModel
    /// 命名空间以 "PengoTarot" 开头时，才对其应用金色背景和文字颜色。
    /// 其他模组或原版的 tip 不受影响。
    /// </summary>
    [HarmonyPatch(typeof(NHoverTipSet), "Init")]
    internal static class HoverTipShaderPatch
    {
        /// <summary>
        private static readonly ConditionalWeakTable<NHoverTipSet, List<bool>> GoldTextTipMasks = [];

        private static readonly Lazy<ShaderMaterial> ColorizeMaterial = new(CreateColorizeMaterial);

        private static readonly Lazy<ShaderMaterial> StarcloudMaterial = new(() =>
        {
            var shader = ResourceLoader.Load<Shader>("res://balatroeffect/Shaders/pengo_starcloud.gdshader");
            if (shader == null)
            {
                Log.Error("[PengoTarot] Failed to load starcloud shader!");
                return new ShaderMaterial();
            }
            var mat = new ShaderMaterial { Shader = shader };
            mat.SetShaderParameter("star_size", 3.6f);
            mat.SetShaderParameter("star_softness", 0.08f); // 柔化十字尖端
            mat.SetShaderParameter("fbm_mix", 0f);     // 关云彩/泛光
            mat.SetShaderParameter("fbm_strength", 0f);
            mat.SetShaderParameter("cloud_amp", 0f);
            return mat;
        });

        /// <summary>
        /// Prefix：物化 hoverTips 枚举，生成 mask 表（哪些 text tip 来自 PengoTarot）。
        /// </summary>
        internal static void Prefix(NHoverTipSet __instance, ref IEnumerable<IHoverTip> hoverTips)
        {
            // 物化并过滤 null，防御其他 Mod 传递的不完整数据
            var materializedTips = hoverTips?.Where(tip => tip != null).ToList() ?? new List<IHoverTip>();
            hoverTips = materializedTips;

            // 如果没有任何有效 tip，直接返回，避免后续空引用
            if (materializedTips.Count == 0)
                return;

            var textTips = IHoverTip.RemoveDupes(materializedTips)
                .OfType<HoverTip>()
                .ToList();

            var goldTextTipMask = textTips
                .Select(tip => IsPengoTarotTip(tip))
                .ToList();

            if (!goldTextTipMask.Any(static isGold => isGold))
                return;

            GoldTextTipMasks.AddOrUpdate(__instance, goldTextTipMask);
        }

        /// <summary>
        /// Postfix：遍历文本 tip，对标记为金色的应用 shader 和颜色覆盖。
        /// </summary>
        internal static void Postfix(NHoverTipSet __instance)
        {
            
            if (!GoldTextTipMasks.TryGetValue(__instance, out var mask))
                return;

            GoldTextTipMasks.Remove(__instance);

            // 用 Traverse 访问私有字段（不依赖 Publicizer，零外部依赖）
            var container = Traverse.Create(__instance)
                .Field("_textHoverTipContainer")
                .GetValue<VFlowContainer>();

            if (container == null || !GodotObject.IsInstanceValid(container))
                return;

            int appliedCount = 0;
            var pairs = container
                .GetChildren()
                .OfType<Control>()
                .Zip(mask);

            foreach (var (textTip, isGold) in pairs)
            {
                if (!isGold || !GodotObject.IsInstanceValid(textTip))
                    continue;

                var bg = textTip.GetNode<Control>("%Bg");
                if (bg == null) continue;

                if (StarcloudMaterial.Value.Shader != null && textTip.GetNodeOrNull("StarOverlay") == null)
                {
                    // 随机 UV 偏移 → 每次 hover 星星分布不同
                    var rng = new Random();
                    StarcloudMaterial.Value.SetShaderParameter("uv_offset", new Vector2(
                        (float)rng.NextDouble() * 100f,
                        (float)rng.NextDouble() * 100f));
                    StarcloudMaterial.Value.SetShaderParameter("density", 120f);

                    var tipSize = textTip.Size;
                    // uv_scale 补偿宽高比，使星星保持正方形
                    float maxDim = Mathf.Max(tipSize.X, tipSize.Y);
                    StarcloudMaterial.Value.SetShaderParameter("uv_scale", new Vector2(
                        tipSize.X / maxDim,
                        tipSize.Y / maxDim));

                    var vp = new SubViewport
                    {
                        Name = "StarViewport",
                        TransparentBg = true, Disable3D = true,
                        Size = (Vector2I)(tipSize * 2f),  // 2× 超采样
                    };
                    var vpc = new SubViewportContainer
                    {
                        Name = "StarOverlay",
                        Stretch = true,  // 2× → 1× 缩回，柔化边缘
                        MouseFilter = MouseFilterEnum.Ignore,
                        Material = StarcloudMaterial.Value,
                    };
                    vpc.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
                    vpc.AddChild(vp);

                    // Bg 移入 SubViewport，只做 HSL
                    textTip.RemoveChild(bg);
                    vp.AddChild(bg);
                    bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
                    bg.Material = ColorizeMaterial.Value;

                    textTip.AddChild(vpc);
                    textTip.MoveChild(vpc, textTip.GetNode("TextContainer").GetIndex());
                }
                else
                {
                    bg.Material = ColorizeMaterial.Value;
                }

                appliedCount++;
            }
        }

        /// <summary>
        /// 判断一个 HoverTip 是否来自 PengoTarot 模组。
        /// </summary>
        internal static bool IsPengoTarotTip(HoverTip tip)
        {
            return tip.CanonicalModel != null &&
                   tip.CanonicalModel.GetType().Namespace?.StartsWith("PengoTarot") == true;
        }

        /// <summary>
        /// 创建 HSL Colorize ShaderMaterial（只做金色调色）。
        /// </summary>
        internal static ShaderMaterial CreateColorizeMaterial()
        {
            var shader = new Shader
            {
                Code = @"
shader_type canvas_item;

uniform float colorize_hue : hint_range(0.0, 1.0) = 0.611;
uniform float colorize_saturation : hint_range(0.0, 2.0) = 0.4;
uniform float colorize_lightness : hint_range(-0.5, 0.5) = 0.0;

vec3 rgb_to_hsl(vec3 c) {
    float maxC = max(max(c.r, c.g), c.b);
    float minC = min(min(c.r, c.g), c.b);
    float delta = maxC - minC;
    float l = (maxC + minC) * 0.5;
    float s = 0.0;
    float h = 0.0;
    if (delta > 0.0001) {
        s = (l < 0.5) ? delta / (maxC + minC) : delta / (2.0 - maxC - minC);
        if (c.r == maxC)
            h = (c.g - c.b) / delta + (c.g < c.b ? 6.0 : 0.0);
        else if (c.g == maxC)
            h = (c.b - c.r) / delta + 2.0;
        else
            h = (c.r - c.g) / delta + 4.0;
        h /= 6.0;
    }
    return vec3(h, s, l);
}

float hue_to_rgb(float p, float q, float t) {
    if (t < 0.0) t += 1.0;
    if (t > 1.0) t -= 1.0;
    if (t < 1.0/6.0) return p + (q - p) * 6.0 * t;
    if (t < 0.5) return q;
    if (t < 2.0/3.0) return p + (q - p) * (2.0/3.0 - t) * 6.0;
    return p;
}

vec3 hsl_to_rgb(vec3 hsl) {
    if (hsl.y < 0.0001) return vec3(hsl.z);
    float q = hsl.z < 0.5 ? hsl.z * (1.0 + hsl.y) : hsl.z + hsl.y - hsl.z * hsl.y;
    float p = 2.0 * hsl.z - q;
    return vec3(
        hue_to_rgb(p, q, hsl.x + 1.0/3.0),
        hue_to_rgb(p, q, hsl.x),
        hue_to_rgb(p, q, hsl.x - 1.0/3.0)
    );
}

void fragment() {
    COLOR = texture(TEXTURE, UV);
    if (COLOR.a < 0.01) discard;
    vec3 hsl = rgb_to_hsl(COLOR.rgb);
    hsl.x = colorize_hue;
    hsl.y = colorize_saturation;
    hsl.z = clamp(hsl.z + colorize_lightness, 0.0, 1.0);
    COLOR.rgb = hsl_to_rgb(hsl);
}"
            };
            return new ShaderMaterial { Shader = shader };
        }
    }
}
