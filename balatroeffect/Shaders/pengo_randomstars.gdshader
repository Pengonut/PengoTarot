shader_type canvas_item;

// ── 系统必需 uniform ──
uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 21;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// ── 星星参数 ──
const int MAX_STARS = 20;
uniform int star_count : hint_range(1, 20) = 12;
uniform float star_scale : hint_range(100.0, 20000.0, 1.0) = 8000.0;   // 星芒张开尺度（UV空间放大倍数）
uniform float frequency_base : hint_range(0.0, 10.0, 0.1) = 2.0;        // 基础频率
uniform float frequency_variation : hint_range(0.0, 5.0, 0.1) = 1.5;
uniform float decay_magnitude : hint_range(0.0, 1.0, 0.01) = 0.08;      // 光晕衰减
uniform float twinkle_speed : hint_range(0.0, 5.0, 0.1) = 0.7;
uniform float twinkle_strength : hint_range(0.0, 1.0, 0.1) = 0.6;
uniform float tilt_response : hint_range(0.0, 1.0, 0.01) = 0.25;

// ── 防冲突数学函数 ──
float pengo_star_onedx(float x) {
    return x == 0.0 ? 1.0 : 1.0 / x;
}
float pengo_star_onedx2(float x) {
    return pengo_star_onedx(x) * pengo_star_onedx(x);
}
float pengo_star_getBetaW(float x, float f, float size) {
    return size * x * 3.14159265 / f;
}
float pengo_star_getBetaH(float y, float f, float size) {
    return size * y * 3.14159265 / f;
}
float pengo_star_getI(vec2 uv, float f, vec2 SIZE) {
    return pengo_star_onedx2(pengo_star_getBetaW(uv.x, f, SIZE.x)) *
           pengo_star_onedx2(pengo_star_getBetaH(uv.y, f, SIZE.y));
}
vec2 pengo_star_rotate(vec2 uv, float addtheta) {
    float theta = atan(uv.y, uv.x) + addtheta;
    float r = length(uv);
    return vec2(r * cos(theta), r * sin(theta));
}

float pengo_star_random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

void fragment() {
    vec4 original = texture(TEXTURE, UV);
    vec2 global_uv = UV * uv_scale + uv_offset;

    vec3 star_light = vec3(0.0);

    // 倾斜位移（UV空间微小移动）
    float tilt_x = x_rot * 0.02 * tilt_response;
    float tilt_y = y_rot * 0.02 * tilt_response;
    vec2 tilt_offset = vec2(tilt_x, tilt_y);

    for (int i = 0; i < MAX_STARS; i++) {
        if (i >= star_count) break;

        // 随机中心（UV空间）
        vec2 center = vec2(
            pengo_star_random(vec2(float(i), 0.0) + seed),
            pengo_star_random(vec2(float(i), 1.0) + seed)
        );

        // 极慢漂移 + 倾斜偏移
        center += vec2(
            sin(TIME * 0.05 + float(i)) * 0.005,
            cos(TIME * 0.07 + float(i)) * 0.005
        ) + tilt_offset * 0.5;   // 倾斜影响适度

        // 局部坐标（UV空间）
        vec2 local_uv = global_uv - center;

        // 随机旋转
        float rand_rot = pengo_star_random(vec2(float(i), 5.0));
        float rot_speed = 0.08 * (rand_rot - 0.5);
        float angle = TIME * rot_speed + x_rot * 0.01 + y_rot * 0.01;
        local_uv = pengo_star_rotate(local_uv, angle);

        // 缩放坐标用于星芒计算
        float scale_var = mix(0.8, 1.2, pengo_star_random(vec2(float(i), 2.0)));
        float cur_scale = star_scale * scale_var;
        vec2 st = local_uv * cur_scale;

        // 频率随机
        float freq = frequency_base + pengo_star_random(vec2(float(i), 3.0)) * frequency_variation;

        // 距离衰减（基于原始的 UV 距离，而非缩放后）
        float dist = length(local_uv) * cur_scale;  // 缩放后的距离近似
        float M = exp(-dist * dist * decay_magnitude * decay_magnitude / cur_scale);
        M = clamp(M, 0.0, 1.0);

        // 忽略极暗区域
        if (M < 0.01) continue;

        // 星芒形状：使用固定的卡片像素尺寸 (1/480) 替代 TEXTURE_PIXEL_SIZE，
        // 避免 FullCard(1/1200) 和部件(1/200) 的 SIZE 差异导致星星胖瘦不同。
        float I = pengo_star_getI(st, freq, vec2(1.0 / 480.0));
        float shape = clamp(I * M, 0.0, 1.0);

        // 随机明灭
        float twinkle_phase = pengo_star_random(vec2(float(i), 6.0)) * 6.2831;
        float twinkle = 1.0 - twinkle_strength * (sin(TIME * twinkle_speed + twinkle_phase) * 0.5 + 0.5);
        shape *= twinkle;

        // 随机颜色
        float hue = pengo_star_random(vec2(float(i), 4.0));
        vec3 star_color = vec3(
            0.5 + 0.5 * sin(6.2831 * hue),
            0.5 + 0.5 * sin(6.2831 * (hue + 0.333)),
            0.5 + 0.5 * sin(6.2831 * (hue + 0.667))
        );

        star_light += star_color * shape;
    }

    // 叠加到原图，强度控制整体亮度
    float alpha = intensity * 1.3;
    vec3 result = original.rgb + star_light * alpha;

    result = mix(result, clamp(result, 0.0, 1.0), 0.95);
    COLOR = vec4(result, original.a);
}