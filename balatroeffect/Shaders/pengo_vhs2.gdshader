shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 12;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// 防冲突随机函数
float pengo_vhs2_random(float s) {
    return fract(543.2543 * sin(dot(vec2(s, s), vec2(3525.46, -54.3415))));
}

void fragment() {
    vec4 original = texture(TEXTURE, UV);

    // ─── 倾斜影响：倾斜角度增大基础抖动力度和概率 ───
    float tilt = (x_rot * 0.02) + (y_rot * 0.03);
    float shake_power = 0.03 * intensity;
    float shake_rate = 0.2 + 0.3 * intensity; // 强度越高抖动越频繁
    float shake_speed = 5.0 + 0.3 * tilt;     // 倾斜改变抖动速度

    // 块大小固定，强度越高块越小（抖动更细碎）
    float shake_block_size = 30.5 / max(0.5, intensity);

    // 当前帧是否抖动
    float enable = float(
        pengo_vhs2_random(floor(TIME * shake_speed) + seed) < shake_rate
    );

    // 按 y 分块，每块独立随机偏移
    float block = floor(UV.y * shake_block_size) / shake_block_size;
    float shift_x = (pengo_vhs2_random(block + floor(TIME)) - 0.5) * shake_power * enable;

    vec2 uv_shift = UV + vec2(shift_x, 0.0);

    // 基本颜色采样
    vec3 tex_color = texture(TEXTURE, uv_shift).rgb;

    // 色差分离（仅在抖动帧生效）
    float color_rate = 0.01 * intensity;
    float r = mix(tex_color.r,
                  texture(TEXTURE, uv_shift + vec2(color_rate, 0.0)).r,
                  enable);
    float b = mix(tex_color.b,
                  texture(TEXTURE, uv_shift - vec2(color_rate, 0.0)).b,
                  enable);
    tex_color.r = r;
    tex_color.b = b;

    // ─── 终极混合 ───
    vec3 final_color = mix(original.rgb, tex_color, intensity);
    COLOR = vec4(final_color, original.a);
}