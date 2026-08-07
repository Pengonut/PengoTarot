shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 19;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// 线条颜色（可自行调整）
uniform vec4 line_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

// 边缘检测阈值（越大检测到的边缘越多）
const float EDGE_THRESHOLD = 0.07;
// 线条平滑度
const float EDGE_SMOOTH = 0.03;

void fragment() {
    vec4 original = texture(TEXTURE, UV);
    
    // 采样步长（基于纹理像素尺寸）
    vec2 texel_size = TEXTURE_PIXEL_SIZE;
    
    // 采样右方和下方像素
    vec4 right = texture(TEXTURE, UV + vec2(texel_size.x, 0.0));
    vec4 down  = texture(TEXTURE, UV + vec2(0.0, texel_size.y));
    
    // 计算颜色距离（RGB 空间）
    float r_dist = length(original.rgb - right.rgb);
    float b_dist = length(original.rgb - down.rgb);
    float max_dist = max(r_dist, b_dist);
    
    // 边缘程度：平滑过渡
    float edge = smoothstep(EDGE_THRESHOLD, EDGE_THRESHOLD + EDGE_SMOOTH, max_dist);
    
    // 线条透明度，受 intensity 控制
    float alpha = edge * intensity * line_color.a;
    
    // 叠加线条颜色到原图上
    vec3 result = mix(original.rgb, line_color.rgb, alpha);
    
    COLOR = vec4(result, original.a);
}