shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 18;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

const float pixel_min = 0.001;
const float pixel_max = 0.05;

void fragment() {
    vec4 original = texture(TEXTURE, UV);
    
    // 全局 UV（对齐跨部件）
    vec2 global_uv = UV * uv_scale + uv_offset;
    
    float pixel_size = mix(pixel_min, pixel_max, intensity);
    
    // 像素步长（保持宽高比基于全局 1:1，不依赖纹理像素尺寸）
    // 可以使用固定比率，或者根据 TEXTURE_PIXEL_SIZE 的平均值，但这里简单使用 1:1
    vec2 pixel_step = vec2(pixel_size);
    
    // 全局 UV 量化
    vec2 uv_rounded = floor(global_uv / pixel_step) * pixel_step + pixel_step * 0.5;
    
    // 映射回局部 UV 采样（因为纹理是局部采样的，但全局我们使用原始 UV？不，量化是基于全局坐标，但采样仍需使用局部 UV，因为纹理坐标是局部的。
    // 我们需要把全局量化后的坐标转成局部 UV。
    // 逆向公式： UV = (global_uv - uv_offset) / uv_scale
    vec2 local_uv = (uv_rounded - uv_offset) / uv_scale;
    
    vec4 tex = texture(TEXTURE, local_uv);
    
    // 边缘抗锯齿（基于局部 UV）
    vec2 uv_delta = fwidth(UV);
    vec2 edge = smoothstep(vec2(0.0), uv_delta * 2.0, UV) *
                smoothstep(vec2(0.0), uv_delta * 2.0, vec2(1.0) - UV);
    float edge_factor = edge.x * edge.y;
    tex.a *= edge_factor;
    
    COLOR = vec4(tex.rgb, original.a);
}