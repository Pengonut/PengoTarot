// balatroeffect/Shaders/balatro_effects_parts.gdshader
// Based on code from BalatroEffects by Indi (MIT License)
// Modified for PengoTarot: foil alt with separate uv offset for corner shift

shader_type canvas_item;

#include "res://balatroeffect/Shaders/pengo_perspective_warp.gdshaderinc"

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 0;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);
uniform vec2 foil_alt_uv_offset = vec2(0.0);

float hue(float s, float t, float h) {
    float hs = mod(h, 1.0) * 6.0;
    if (hs < 1.0) return (t - s) * hs + s;
    if (hs < 3.0) return t;
    if (hs < 4.0) return (t - s) * (4.0 - hs) + s;
    return s;
}
vec4 RGB(vec4 c) {
    if (c.y < 0.0001) return vec4(vec3(c.z), c.a);
    float t = (c.z < 0.5) ? c.y * c.z + c.z : -c.y * c.z + (c.y + c.z);
    float s = 2.0 * c.z - t;
    return vec4(hue(s, t, c.x + 1.0/3.0), hue(s, t, c.x), hue(s, t, c.x - 1.0/3.0), c.w);
}
vec4 HSL(vec4 c) {
    float low = min(c.r, min(c.g, c.b));
    float high = max(c.r, max(c.g, c.b));
    float delta = high - low;
    float sum = high + low;
    vec4 hsl = vec4(0.0, 0.0, 0.5 * sum, c.a);
    if (delta > 0.0) {
        hsl.y = (hsl.z < 0.5) ? delta / sum : delta / (2.0 - sum);
        if (high == c.r) hsl.x = (c.g - c.b) / delta;
        else if (high == c.g) hsl.x = (c.b - c.r) / delta + 2.0;
        else hsl.x = (c.r - c.g) / delta + 4.0;
        hsl.x = mod(hsl.x / 6.0, 1.0);
    }
    return hsl;
}

void fragment() {
    vec4 tex = texture(TEXTURE, UV);

    vec2 global_uv;
    if (effect_mode == 2) {
        global_uv = UV * uv_scale + foil_alt_uv_offset;
    } else {
        global_uv = UV * uv_scale + uv_offset;
    }

    vec2 adj_uv_global = global_uv - 0.5;
    float slow_time = TIME * 0.1 + seed;
    float tilt_shift = (x_rot * 0.02) + (y_rot * 0.03);

    if (effect_mode == 1) { // Foil
        float f_r = (slow_time * 0.66) + (tilt_shift * 0.66);
        float f_g = (slow_time * 0.5) + (tilt_shift * 0.5);

        float fac1 = clamp(2.0 * sin((length(90.0 * adj_uv_global) + f_r * 2.0) + 3.0 * (1.0 + 0.8 * cos(length(113.1121 * adj_uv_global) - f_r * 3.121))) - 1.0 - max(5.0 - length(90.0 * adj_uv_global), 0.0), 0.0, 1.0);
        vec2 rotater = vec2(cos(f_r * 0.1221), sin(f_r * 0.3512));
        float radial_angle = dot(rotater, adj_uv_global) / (length(rotater) * length(adj_uv_global) + 0.0001);
        float fac2 = clamp(5.0 * cos(f_g * 0.3 + radial_angle * 3.1415 * (2.2 + 0.9 * sin(f_r * 1.65 + 0.2 * f_g))) - 4.0 - max(2.0 - length(20.0 * adj_uv_global), 0.0), 0.0, 1.0);

        float fac3 = 0.3 * clamp(2.0 * sin(f_r * 5.0 + global_uv.x * 3.0 + 3.0 * (1.0 + 0.5 * cos(f_r * 7.0))) - 1.0, -1.0, 1.0);
        float fac4 = 0.3 * clamp(2.0 * sin(f_r * 6.66 + global_uv.y * 3.8 + 3.0 * (1.0 + 0.5 * cos(f_r * 3.414))) - 1.0, -1.0, 1.0);

        float maxfac = max(max(fac1, max(fac2, max(fac3, max(fac4, 0.0)))) + 2.2 * (fac1 + fac2 + fac3 + fac4), 0.0);

        float low = min(tex.r, min(tex.g, tex.b));
        float high = max(tex.r, max(tex.g, tex.b));
        float delta = min(high, max(0.5, 1.0 - low));

        vec3 foil_color;
        foil_color.r = tex.r - delta + delta * maxfac * 0.5;
        foil_color.g = tex.g - delta + delta * maxfac * 0.5;
        foil_color.b = tex.b + delta * maxfac * 0.7;

        float foil_alpha = clamp(0.3 + 0.9 * min(0.5, maxfac * 0.1), 0.0, 1.0);
        tex.rgb = mix(tex.rgb, foil_color, foil_alpha);
    }
    else if (effect_mode == 2) { // Foil Alt
        float f_r = (slow_time * 0.66) + (tilt_shift * 0.66);
        float f_g = (slow_time * 0.5) + (tilt_shift * 0.5);

        float fac1 = clamp(2.0 * sin((length(90.0 * adj_uv_global) + f_r * 2.0) + 3.0 * (1.0 + 0.8 * cos(length(113.1121 * adj_uv_global) - f_r * 3.121))) - 1.0 - max(5.0 - length(90.0 * adj_uv_global), 0.0), 0.0, 1.0);
        vec2 rotater = vec2(cos(f_r * 0.1221), sin(f_r * 0.3512));
        float radial_angle = dot(rotater, adj_uv_global) / (length(rotater) * length(adj_uv_global) + 0.0001);
        float fac2 = clamp(5.0 * cos(f_g * 0.3 + radial_angle * 3.1415 * (2.2 + 0.9 * sin(f_r * 1.65 + 0.2 * f_g))) - 4.0 - max(2.0 - length(20.0 * adj_uv_global), 0.0), 0.0, 1.0);

        float fac3 = 0.3 * clamp(2.0 * sin(f_r * 5.0 + global_uv.x * 3.0 + 3.0 * (1.0 + 0.5 * cos(f_r * 7.0))) - 1.0, -1.0, 1.0);
        float fac4 = 0.3 * clamp(2.0 * sin(f_r * 6.66 + global_uv.y * 3.8 + 3.0 * (1.0 + 0.5 * cos(f_r * 3.414))) - 1.0, -1.0, 1.0);

        float maxfac = max(max(fac1, max(fac2, max(fac3, max(fac4, 0.0)))) + 2.2 * (fac1 + fac2 + fac3 + fac4), 0.0);

        float low = min(tex.r, min(tex.g, tex.b));
        float high = max(tex.r, max(tex.g, tex.b));
        float delta = min(high, max(0.5, 1.0 - low));

        vec3 foil_color;
        foil_color.r = tex.r - delta + delta * maxfac * 0.5;
        foil_color.g = tex.g - delta + delta * maxfac * 0.5;
        foil_color.b = tex.b + delta * maxfac * 0.7;

        float foil_alpha = clamp(0.3 + 0.9 * min(0.5, maxfac * 0.1), 0.0, 1.0);
        tex.rgb = mix(tex.rgb, foil_color, foil_alpha);
    }
    else if (effect_mode == 5) { // Negative（负片-A：原 shader 负片效果，带蓝光泽）
        vec4 hsl = HSL(tex);
        hsl.z = 1.0 - hsl.z;
        hsl.x = mod(-hsl.x + 0.2, 1.0);
        vec4 neg_base = RGB(hsl);
        neg_base.rgb += vec3(0.31, 0.38, 0.4) * 0.2;

        float shine_shift = slow_time + (tilt_shift * 0.2);
        float low = min(tex.r, min(tex.g, tex.b));
        float high = max(tex.r, max(tex.g, tex.b));
        float delta = high - low;

        float fac = 0.8 + 0.9 * sin(11.0 * global_uv.x + 4.32 * global_uv.y + shine_shift * 12.0 + cos(shine_shift * 5.3 + global_uv.y * 4.2 - global_uv.x * 4.0));
        float fac2 = 0.5 + 0.5 * sin(8.0 * global_uv.x + 2.32 * global_uv.y + shine_shift * 5.0 - cos(shine_shift * 2.3 + global_uv.x * 8.2));
        float fac3 = 0.5 + 0.5 * sin(10.0 * global_uv.x + 5.32 * global_uv.y + shine_shift * 6.111 + sin(shine_shift * 5.3 + global_uv.y * 3.2));
        float fac4 = 0.5 + 0.5 * sin(3.0 * global_uv.x + 2.32 * global_uv.y + shine_shift * 8.111 + sin(shine_shift * 1.3 + global_uv.y * 11.2));

        float maxfac = 0.7 * max(max(fac, max(fac2, fac3)) + (fac + fac2 + fac3 * fac4), 0.0);
        tex.rgb = neg_base.rgb * 1.1;
        float shine_glow = maxfac * 0.12;
        tex.rgb += vec3(0.6, 0.8, 1.0) * shine_glow * (delta + 0.5);
        if (tex.a > 0.0 && tex.a < 0.7) tex.a /= 3.0;
    }
    else if (effect_mode == 3) { // Polychrome
        float low = min(tex.r, min(tex.g, tex.b));
        float high = max(tex.r, max(tex.g, tex.b));
        float delta = high - low;
        float saturation_fac = 1.0 - max(0.0, 0.05 * (1.1 - delta));
        vec4 hsl = HSL(vec4(tex.r * saturation_fac, tex.g * saturation_fac, tex.b, tex.a));
        float t_poly = slow_time * 5.0;
        vec2 texture_res = 1.0 / TEXTURE_PIXEL_SIZE;
        vec2 floored_uv = floor(global_uv * texture_res) / texture_res;
        vec2 uv_p = (floored_uv - 0.5 + fract(seed)) * 50.0;

        vec2 p1 = uv_p + 50.0 * vec2(sin(-t_poly / 143.6340), cos(-t_poly / 99.4324));
        vec2 p2 = uv_p + 50.0 * vec2(cos( t_poly / 53.1532),  cos( t_poly / 61.4532));
        vec2 p3 = uv_p + 50.0 * vec2(sin(-t_poly / 87.53218), sin(-t_poly / 49.0000));

        float poly_field = (1.0 + (
            cos(length(p1) / 19.483) +
            sin(length(p2) / 33.155) * cos(p2.y / 15.73) +
            cos(length(p3) / 27.193) * sin(p3.x / 21.92)
        )) / 2.0;

        float res = (0.5 + 0.5 * cos(tilt_shift * 2.0 + (poly_field - 0.5) * 3.14));
        hsl.x = mod(hsl.x + res + slow_time * 0.25, 1.0);
        hsl.y = min(0.6, hsl.y + 0.5);
        tex.rgb = RGB(hsl).rgb;
        if (tex.a > 0.0 && tex.a < 0.7) tex.a /= 3.0;
    }
    else if (effect_mode == 4) { // Holographic
        vec4 hsl = HSL(0.5 * tex + 0.5 * vec4(0.0, 0.0, 1.0, tex.a));
        float t_field = slow_time * 8.0 + 2003.0;
        vec2 uv_scaled = adj_uv_global * 250.0;
        vec2 fp1 = uv_scaled + 50.0 * vec2(sin(-t_field / 143.634), cos(-t_field / 99.432));
        vec2 fp2 = uv_scaled + 50.0 * vec2(cos( t_field / 53.153),  cos( t_field / 61.453));
        vec2 fp3 = uv_scaled + 50.0 * vec2(sin(-t_field / 87.532), sin(-t_field / 49.000));
        float field = (1.0 + (cos(length(fp1) / 19.483) + sin(length(fp2) / 33.155) * cos(fp2.y / 15.73) + cos(length(fp3) / 27.193) * sin(fp3.x / 21.92))) / 2.0;
        float res = (0.5 + 0.5 * cos(tilt_shift * 6.0 + (field - 0.5) * 3.14));
        float gridsize = 0.79;
        float fac = 0.5 * max(max(0.0, 7.0 * abs(cos(global_uv.x * gridsize * 20.0)) - 6.0), max(0.0, 7.0 * cos(global_uv.y * gridsize * 45.0 + global_uv.x * gridsize * 20.0) - 6.0));
        float fac3 = 0.5 * max(0.0, 7.0 * cos(global_uv.y * gridsize * 45.0 - global_uv.x * gridsize * 20.0) - 6.0);
        fac = max(fac, fac3);
        hsl.x = mod(hsl.x + res + fac, 1.0);
        hsl.y = hsl.y * 1.3;
        hsl.z = hsl.z * 0.6 + 0.4;
        float high_h = max(tex.r, max(tex.g, tex.b));
        float low_h = min(tex.r, min(tex.g, tex.b));
        float delta_h = 0.2 + 0.3 * (high_h - low_h) + 0.1 * high_h;
        tex = (1.0 - delta_h) * tex + delta_h * RGB(hsl) * vec4(0.9, 0.8, 1.2, tex.a);
        if (tex.a > 0.0 && tex.a < 0.7) tex.a /= 3.0;
    }
    else if (effect_mode == 6) { // Negative B（负片-B：简单反色，原 NegativeShaderPatch 硬编码效果）
        tex.rgb = 1.0 - tex.rgb;
    }

    COLOR = mix(texture(TEXTURE, UV), tex, intensity);
}