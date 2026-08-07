shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 7;      // 7=Radial/Fixed, 8=Linear, 9=Radial+Linear（旧 6=Radial 兼容保留）
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// 时间相关常量（极慢）
const float SPEED_HUE_SHIFT = 0.015;    // 整体色相旋转速度
const float SPEED_RADIAL_ROT = 0.012;   // 径向螺旋旋转速度
const float SPEED_RADIAL_PULSE = 0.025; // 径向脉动速度
const float SPEED_LINE_WAVE = 0.018;    // 线性条纹波浪速度
const float SPEED_BREATH = 0.008;       // 全局呼吸速度

// 纯色生成（避免函数名冲突）
vec3 pengo_rainbow_hue(float h) {
    float r = abs(h * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(h * 6.0 - 2.0);
    float b = 2.0 - abs(h * 6.0 - 4.0);
    return clamp(vec3(r, g, b), 0.0, 1.0);
}

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    vec2 global_uv = UV * uv_scale + uv_offset;
    vec2 center = vec2(0.5);

    // 极慢的时间基准（不同用途使用不同速度）
    float t_global = TIME + seed;
    float slow_hue = t_global * SPEED_HUE_SHIFT;      // 整体色相漂移
    float slow_rot = t_global * SPEED_RADIAL_ROT;     // 径向旋转
    float slow_pulse = t_global * SPEED_RADIAL_PULSE; // 径向脉动
    float slow_wave = t_global * SPEED_LINE_WAVE;     // 线性波浪
    float slow_breath = t_global * SPEED_BREATH;      // 呼吸

    // 倾斜偏移
    float tilt_shift = (x_rot * 0.02) + (y_rot * 0.03);

    int mode = effect_mode;
    vec3 rainbow_light = vec3(0.0);
    float mask_total = 0.0;

    // ────── 径向虹光 (mode 6/7, 9) ──────
    if (mode == 6 || mode == 7 || mode == 9) {
        vec2 dir = global_uv - center;
        float dist = length(dir);
        float angle = atan(dir.y, dir.x) / (2.0 * 3.14159265);

        // 极慢旋转 + 倾斜影响
        angle += tilt_shift * 0.5 + slow_rot * 0.2;
        // 加入微弱的螺旋扭曲
        angle += sin(dist * 12.0 - slow_rot * 2.0) * 0.03;

        // 极慢的半径脉动
        float r_mod = 1.0 + sin(dist * 10.0 + slow_pulse) * 0.04;
        float r_mask = smoothstep(0.03 * r_mod, 0.35 * r_mod, dist);

        float hue_val = fract(angle * 6.0 + slow_hue);
        rainbow_light += pengo_rainbow_hue(hue_val) * r_mask;
        mask_total = max(mask_total, r_mask);
    }

    // ────── 线性虹光 (mode 8, 9) ──────
    if (mode == 8 || mode == 9) {
        // 基本滚动
        float line = global_uv.x * 5.0 + tilt_shift * 0.5;
        // 极慢的波浪调制
        line += sin(global_uv.y * 8.0 + slow_wave) * 0.15;
        // 双重模式加一点额外偏移
        if (mode == 9) {
            line += global_uv.y * 1.0 + cos(global_uv.x * 3.0 - slow_wave * 0.7) * 0.12;
        }
        float hue_lin = fract(line + slow_hue);
        vec3 lin_color = pengo_rainbow_hue(hue_lin);
        rainbow_light += lin_color;
        mask_total = max(mask_total, 1.0);
    }

    // ────── 全局呼吸效果 ──────
    float breath = 1.0 + sin(slow_breath * 2.0) * 0.04;  // 极细微的明暗波动

    // ────── 自适应叠加（保持高级感） ──────
    float high_h = max(tex.r, max(tex.g, tex.b));
    float low_h = min(tex.r, min(tex.g, tex.b));
    float delta_h = 0.2 + 0.3 * (high_h - low_h) + 0.1 * high_h;
    float weight = delta_h * intensity * mask_total * 0.7 * breath;

    // 加法混合 + 软钳制 + 微色调
    vec3 result = tex.rgb + rainbow_light * weight;
    result = mix(result, clamp(result, 0.0, 1.0), 0.9);
    result *= vec3(0.95, 0.85, 1.15);   // 保持轻微冷调

    COLOR = vec4(result, tex.a);
}