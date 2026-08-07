
#nullable enable

using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    public static class NCard_UpdateVisuals_SubNegativeShader_Patch
    {
        private static ShaderMaterial? _negativeMaterial;

        private static ShaderMaterial GetOrCreateMaterial()
        {
            if (_negativeMaterial != null)
                return _negativeMaterial;

            var shader = new Shader();
shader.Code = @"
shader_type canvas_item;
render_mode blend_mix;
uniform float intensity : hint_range(0, 1) = 1.0;
uniform float hue_shift : hint_range(-0.5, 0.5) = -0.15;
uniform float brightness_boost : hint_range(-1, 1) = 0.2;
// 前置声明 hue2rgb
float hue2rgb(float p, float q, float t) {
    if (t < 0.0) t += 1.0;
    if (t > 1.0) t -= 1.0;
    if (t < 1.0/6.0) return p + (q - p) * 6.0 * t;
    if (t < 1.0/2.0) return q;
    if (t < 2.0/3.0) return p + (q - p) * (2.0/3.0 - t) * 6.0;
    return p;
}
vec3 rgb_to_hsl(vec3 c) {
    float low = min(min(c.r, c.g), c.b);
    float high = max(max(c.r, c.g), c.b);
    float delta = high - low;
    float sum = high + low;
    float l = sum * 0.5;
    float h = 0.0;
    float s = 0.0;
    if (delta > 0.001) {
        s = l < 0.5 ? delta / sum : delta / (2.0 - sum);
        if (high == c.r)
            h = (c.g - c.b) / delta + (c.g < c.b ? 6.0 : 0.0);
        else if (high == c.g)
            h = (c.b - c.r) / delta + 2.0;
        else
            h = (c.r - c.g) / delta + 4.0;
        h /= 6.0;
    }
    return vec3(h, s, l);
}
vec3 hsl_to_rgb(vec3 hsl) {
    float h = hsl.x;
    float s = hsl.y;
    float l = hsl.z;
    if (s < 0.001) return vec3(l);
    float q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
    float p = 2.0 * l - q;
    return vec3(
        hue2rgb(p, q, h + 1.0/3.0),
        hue2rgb(p, q, h),
        hue2rgb(p, q, h - 1.0/3.0)
    );
}
void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    if (tex.a < 0.01) discard;
    vec3 hsl = rgb_to_hsl(tex.rgb);
    hsl.b = 1.0 - hsl.b;
    hsl.r = mod(hsl.r + hue_shift, 1.0);
    hsl.b = clamp(hsl.b + brightness_boost, 0.0, 1.0);
    vec3 neg_color = hsl_to_rgb(hsl);
    float highlight = smoothstep(0.6, 1.0, hsl.b) * 0.15;
    neg_color += vec3(0.2, 0.4, 0.9) * highlight;
    COLOR = vec4(mix(tex.rgb, neg_color, intensity), tex.a);
}
";
            _negativeMaterial = new ShaderMaterial();
            _negativeMaterial.Shader = shader;
            return _negativeMaterial;
        }

        private static bool ShouldApplyNegativeShader(CardModel card)
        {
            if (card == null)
                return false;

            
            if (card.Pool is TarotPool && card.Id.Entry.ToLowerInvariant().Contains("sub"))
                return true;

            
            if (card.Enchantment != null)
            {
                var encType = card.Enchantment.GetType();
                if (encType.Namespace?.StartsWith("PengoTarot") == true &&
                    card.Enchantment.Id.Entry.ToLowerInvariant().Contains("sub"))
                    return true;
            }

            return false;
        }

        
        static void Postfix(NCard __instance, PileType pileType, CardPreviewMode previewMode)
        {
            if (__instance.Model == null)
                return;

            bool shouldApply = ShouldApplyNegativeShader(__instance.Model);
            var material = GetOrCreateMaterial();

            
            Node2D? targetNode = __instance.GetNodeOrNull<Node2D>("Portrait")
                                ?? __instance.GetNodeOrNull<Node2D>("CardImage");

            
            if (targetNode == null)
            {
                
                
                return; 
            }

            if (shouldApply)
            {
                if (targetNode.Material != material)
                    targetNode.Material = material;
            }
            else
            {
                if (targetNode.Material == material)
                    targetNode.Material = null;
            }
        }
    }
}