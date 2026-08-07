shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 10;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// 防冲突随机函数
float pengo_vhs_random(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453123);
}

void fragment() {
    vec4 original = texture(TEXTURE, UV);

    // 像素化基础 (利用纹理像素尺寸，单位换算成 UV)
    float base_pixel_size = 3.0;
    float effective_pixel = base_pixel_size / max(0.1, intensity);
    vec2 uv_step = effective_pixel * TEXTURE_PIXEL_SIZE;
    vec2 grid_uv = round(UV / uv_step) * uv_step;

    // 动态水平抖动 (受倾斜角影响)
    float tilt_shift = (x_rot * 0.02) + (y_rot * 0.03);
    float time_step = floor(TIME * 15.0) + seed;
    float glitch_amplitude = 0.005 * intensity;
    glitch_amplitude += abs(tilt_shift) * 0.01;
    float glitch = (pengo_vhs_random(vec2(time_step, floor(grid_uv.y * 30.0))) - 0.5)
                   * glitch_amplitude * 10.0;

    vec2 uv_shifted = grid_uv + vec2(glitch, 0.0);

    // 红蓝通道分离
    float split_amount = 0.01 * intensity;
    vec2 uv_red = uv_shifted + vec2(split_amount, 0.0);
    vec2 uv_blue = uv_shifted - vec2(split_amount, 0.0);

    vec4 tex = texture(TEXTURE, uv_shifted);
    vec4 tex_red = texture(TEXTURE, uv_red);
    vec4 tex_blue = texture(TEXTURE, uv_blue);

    // 构建色差颜色
    vec3 vhs_color;
    vhs_color.r = tex_red.r;
    vhs_color.g = tex.g;          // 保留原始绿色，不额外偏移
    vhs_color.b = tex_blue.b;

    // 最终混合
    vec3 final_color = mix(original.rgb, vhs_color, intensity * 0.8);
    // 轻微色彩漂白
    final_color = mix(final_color, vec3(dot(final_color, vec3(0.333))), intensity * 0.15);

    COLOR = vec4(final_color, original.a);
}