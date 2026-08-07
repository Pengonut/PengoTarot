shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 11;           // 建议占用 11，可自定义
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// ─── 防冲突随机函数 ───
vec2 pengo_crt_random(vec2 uv) {
    uv = vec2( dot(uv, vec2(127.1, 311.7)),
               dot(uv, vec2(269.5, 183.3)) );
    return -1.0 + 2.0 * fract(sin(uv) * 43758.5453123);
}

// ─── Perlin 风格噪声（用于滚动带失真） ───
float pengo_crt_noise(vec2 uv) {
    vec2 i = floor(uv);
    vec2 f = fract(uv);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix( mix( dot( pengo_crt_random(i + vec2(0.0,0.0)), f - vec2(0.0,0.0) ),
                     dot( pengo_crt_random(i + vec2(1.0,0.0)), f - vec2(1.0,0.0) ), u.x),
                mix( dot( pengo_crt_random(i + vec2(0.0,1.0)), f - vec2(0.0,1.0) ),
                     dot( pengo_crt_random(i + vec2(1.0,1.0)), f - vec2(1.0,1.0) ), u.x), u.y) * 0.5 + 0.5;
}

void fragment() {
    vec4 original = texture(TEXTURE, UV);

    // ─── 像素化：根据强度降低虚拟分辨率 ───
    float pixel_count = mix(256.0, 32.0, intensity);   // 强度越高，像素块越少
    vec2 uv_pix = floor(UV * pixel_count) / pixel_count;

    // ─── 倾斜影响（让滚动速度 / 强度随鼠标变化） ───
    float tilt = (x_rot * 0.02) + (y_rot * 0.03);

    // ─── 红蓝通道分离（色差） ───
    float aberr = 0.03 * intensity;
    vec2 uv_red = uv_pix + vec2(aberr, 0.0);
    vec2 uv_green = uv_pix;
    vec2 uv_blue = uv_pix - vec2(aberr, 0.0);

    // ─── 滚动带失真（结合 Perlin 噪声） ───
    float roll_size = 15.0;
    float roll_speed = 8.0 + 0.5 * tilt;                // 倾斜改变滚动速度
    float roll_line = smoothstep(0.3, 0.9, sin(UV.y * roll_size - TIME * roll_speed));
    roll_line *= roll_line * smoothstep(0.3, 0.9, sin(UV.y * roll_size * 1.8 - TIME * roll_speed * 1.8));

    float distort = 0.05 * intensity;
    vec2 roll_offset = vec2(roll_line * distort * (1.0 - UV.x), 0.0);

    // 采样时叠加滚动偏移
    vec3 tex_color;
    tex_color.r = texture(TEXTURE, uv_red + roll_offset).r;
    tex_color.g = texture(TEXTURE, uv_green + roll_offset).g;
    tex_color.b = texture(TEXTURE, uv_blue + roll_offset).b;

    // ─── 扫描线（固定 240 行） ───
    float scan_opacity = 0.5 * intensity;
    float scanline = smoothstep(0.25, 0.5, abs(sin(UV.y * 240.0 * 3.14159265)));
    tex_color = mix(tex_color, tex_color * vec3(scanline), scan_opacity);

    // ─── 格栅（仅在强度较高时启用） ───
    if (intensity > 0.4) {
        float grille = 0.3 * intensity;
        float gr = smoothstep(0.85, 0.95, abs(sin(UV.x * 480.0 * 3.14159265)));
        float gg = smoothstep(0.85, 0.95, abs(sin(1.05 + UV.x * 480.0 * 3.14159265)));
        float gb = smoothstep(0.85, 0.95, abs(sin(2.1  + UV.x * 480.0 * 3.14159265)));
        tex_color.r = mix(tex_color.r, tex_color.r * gr, grille);
        tex_color.g = mix(tex_color.g, tex_color.g * gg, grille);
        tex_color.b = mix(tex_color.b, tex_color.b * gb, grille);
    }

    // ─── 条带噪声（基于 Perlin 噪声的横向条纹） ───
    float noise_opacity = 0.4 * intensity;
    float noise_field = pengo_crt_noise(UV * vec2(2.0, 200.0) + vec2(10.0, TIME * 5.0));
    noise_field = smoothstep(0.4, 0.5, noise_field);
    float band_noise = roll_line * noise_field * scanline *
                       clamp(pengo_crt_random(floor(UV * 240.0) / 240.0 + vec2(TIME * 0.8, 0.0)).x + 0.8, 0.0, 1.0);
    tex_color = mix(tex_color, tex_color + vec3(band_noise), noise_opacity);

    // ─── 静态噪声 ───
    float static_intensity = 0.06 * intensity;
    tex_color += pengo_crt_random(floor(UV * 240.0) / 240.0 + fract(TIME)).x * static_intensity;

    // ─── VHS 风格褪色 + 对比度 ───
    float saturation = 0.5 * intensity;
    vec3 gray = vec3(dot(tex_color, vec3(0.333)));
    tex_color = mix(tex_color, gray, saturation);

    float contrast = 1.0 + 0.2 * intensity;
    float midpoint = pow(0.5, 2.2);
    tex_color = clamp((tex_color - midpoint) * contrast + midpoint, 0.0, 1.0);

    // ─── 最终混合：滑块控制效果总强度 ───
    vec3 final_color = clamp(tex_color, 0.0, 1.0);
    COLOR = vec4(mix(original.rgb, final_color, intensity), original.a);
}