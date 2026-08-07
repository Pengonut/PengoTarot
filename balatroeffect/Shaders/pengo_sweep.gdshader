shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 13;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// 扫光专用参数（仍可暴露，但根据需要调节）
uniform float line_smoothness : hint_range(0.001, 0.1) = 0.045;
uniform float line_width : hint_range(0, 0.2) = 0.09;
uniform float brightness = 3.0;
uniform float rotation_deg : hint_range(-90, 90) = 30;
uniform float distortion : hint_range(1, 2) = 1.8;
uniform float speed = 0.7;
uniform float position_min = 0.25;
uniform float position_max = 0.5;

// 防冲突旋转函数
vec2 pengo_sweep_rotate_uv(vec2 uv, vec2 center, float rotation, bool use_degrees) {
    float angle = rotation;
    if (use_degrees) {
        angle = rotation * (3.1415926 / 180.0);
    }
    mat2 rot = mat2(
        vec2(cos(angle), -sin(angle)),
        vec2(sin(angle), cos(angle))
    );
    vec2 delta = uv - center;
    delta = rot * delta;
    return delta + center;
}

void fragment() {
    vec4 original = texture(TEXTURE, UV);

    // ─── 倾斜影响旋转速度或角度 ───
    float tilt = (x_rot * 0.02) + (y_rot * 0.03);
    float actual_rotation = rotation_deg + tilt * 0.24; // 倾斜微调旋转角度
    float actual_speed = speed + tilt * 0.005;

    // ─── 位置动画 ───
    // 使用 intensity 影响扫描线的位置范围（强度越高，扫描线覆盖范围越大）
    float range = position_max - position_min;
    float pos_now = position_min + range * (fract(TIME * actual_speed + seed));
    // 将 [0,1] 映射到 [ -2, 2 ] 用于 UV 偏移
    float remapped_time = mix(-2.0, 2.0, pos_now);

    // ─── 旋转 UV ───
    vec2 rot_uv = pengo_sweep_rotate_uv(UV, vec2(0.5), actual_rotation, true);
    vec2 off_uv = rot_uv + vec2(remapped_time, 0.0);

    // ─── 计算扫描线亮度 ───
    vec2 center_uv = UV - 0.5;
    float gradient = 1.0 - max(abs(center_uv.x), abs(center_uv.y)) * distortion;

    float line = abs(off_uv.x);
    line *= gradient;
    line = sqrt(line);

    float smooth_val = clamp(line_smoothness, 0.001, 1.0);
    float plus = line_width + smooth_val;
    float minus = line_width - smooth_val;

    float line_intensity = (line - plus) / (minus - plus); // 取值范围为负，但后面会用 brightness 拉伸
    line_intensity *= brightness;
    line_intensity = clamp(line_intensity, 0.0, 1.0);

    // ─── 最终合成 ───
    // 扫描线颜色为白色（可改），以加法叠加的方式点亮原图
    vec3 sweep_color = vec3(1.0);
    float sweep_alpha = line_intensity * intensity;

    vec3 result = original.rgb + sweep_color * sweep_alpha;
    result = clamp(result, 0.0, 1.0);

    COLOR = vec4(result, original.a);
}