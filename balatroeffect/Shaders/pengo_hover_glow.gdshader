shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 15;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// 高光大小和硬度
uniform float glow_radius : hint_range(0.1, 0.9) = 0.3;
uniform float glow_falloff : hint_range(0.1, 2.0) = 0.5; // 边缘柔和度

void fragment() {
    vec4 original = texture(TEXTURE, UV);
    // 使用全局 UV，以便高光在整卡上位置一致
    vec2 global_uv = UV * uv_scale + uv_offset;

    // 虚拟鼠标（从倾斜角推导，注意符号可能需要匹配实际倾斜方向）
    vec2 mouse = vec2(
        clamp(x_rot / 90.0, -1.0, 1.0),
        clamp(y_rot / 90.0, -1.0, 1.0)
    ) * 0.5 + 0.5;

    // 计算像素到鼠标的距离
    float dist = length(global_uv - mouse);
    // 径向高光，使用 smoothstep 产生柔光
    float glow = 1.0 - smoothstep(glow_radius - glow_falloff, glow_radius + glow_falloff, dist);
    glow *= intensity;

    // 叠加高光
    vec3 result = original.rgb + vec3(glow);
    result = clamp(result, 0.0, 1.0);

    COLOR = vec4(result, original.a);
}