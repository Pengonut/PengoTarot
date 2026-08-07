shader_type canvas_item;

// ── 系统必需 uniform ──
uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 20;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform float use_black_bg = 0.0;   // 背景模式（如检查界面黑块）：不叠加原图（无纹理时为白），以黑为底显示星云
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// ── 星星参数 ──
uniform float density : hint_range(0.0, 300.0, 1.0) = 200.0;          // 提高默认密度
uniform int layer_parascale : hint_range(0, 5, 1) = 2;
uniform float speedx : hint_range(-10.0, 10.0, 0.1) = 0.2;
uniform float speedy : hint_range(-10.0, 10.0, 0.1) = 0.0;
uniform float star_horizontal_wave : hint_range(0.0, 10.0, 0.1) = 0.3;
uniform float star_size : hint_range(0.0, 100.0, 0.1) = 4.0;
uniform float twinkle_effect : hint_range(0.0, 1.0, 0.1) = 0.6;
uniform float twinkle_speed : hint_range(0.0, 100.0, 0.1) = 0.3;
uniform float star_softness : hint_range(0.0, 1.0, 0.01) = 0.2;

uniform float fbm_strength : hint_range(0.0, 3.0, 0.01) = 0.12;
uniform float fbm_mix : hint_range(0.0, 1.0, 0.01) = 0.07;
uniform float fbm_freq : hint_range(0, 200, 1) = 4;
uniform float fbm_amp : hint_range(0, 10.0, 0.01) = 1.5;
uniform float fbm_speed : hint_range(-10.0, 10.0, 0.01) = 5.0;
uniform float warp_scale : hint_range(0, 10.0, 0.01) = 2.0;

uniform float cloud_freq : hint_range(0, 100, 1) = 40;
uniform float cloud_amp : hint_range(0, 1.0, 0.01) = 0.02;
uniform float cloud_speed : hint_range(0, 10.0, 0.01) = 5.0;

uniform float cloud_color_ci1 : hint_range(0, 1.0, 0.01) = 0.75;
uniform float cloud_color_ci2 : hint_range(0, 1.0, 0.01) = 0.45;
uniform float cloud_color_ci3 : hint_range(0, 1.0, 0.01) = 0.9;
uniform float cloud_color_shiftx : hint_range(-100, 100.0, 0.5) = 0.0;
uniform float cloud_color_shifty : hint_range(-100, 100.0, 0.5) = 0.0;

// ── 防冲突函数前缀 pengo_starcloud_ ──
float pengo_starcloud_onedx(float x) {
    return 1.0 / (abs(x) + star_softness);
}
float pengo_starcloud_onedx2(float x) {
    return pengo_starcloud_onedx(x) * pengo_starcloud_onedx(x);
}
float pengo_starcloud_getBetaW(float x, float f, float size) {
    return size * x * 3.14159265 / f;
}
float pengo_starcloud_getBetaH(float y, float f, float size) {
    return size * y * 3.14159265 / f;
}
float pengo_starcloud_getI(vec2 uv, float f, vec2 SIZE) {
    return pengo_starcloud_onedx2(pengo_starcloud_getBetaW(uv.x, f, SIZE.x)) *
           pengo_starcloud_onedx2(pengo_starcloud_getBetaH(uv.y, f, SIZE.y));
}
float pengo_starcloud_random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43759.5453123);
}
float pengo_starcloud_noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    float a = pengo_starcloud_random(i);
    float b = pengo_starcloud_random(i + vec2(1.0, 0.0));
    float c = pengo_starcloud_random(i + vec2(0.0, 1.0));
    float d = pengo_starcloud_random(i + vec2(1.0, 1.0));
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}
float pengo_starcloud_fbm(vec2 st) {
    float value = 0.0;
    float freq = fbm_freq;
    float amp = fbm_amp;
    for (int i = 0; i < 5; i++) {
        value += amp * pengo_starcloud_noise(st * freq);
        freq *= 2.7;
        amp *= 0.5;
    }
    return value;
}
vec2 pengo_starcloud_clouduv(vec2 uv, float time, vec2 centers[6], vec2 flowDir, float flowSpeed) {
    const int count = 3;
    vec2 cloud_shiftspeed = time * flowDir * flowSpeed * fbm_speed;
    vec2 p = uv * warp_scale - cloud_shiftspeed;
    vec2 warp = vec2(
        pengo_starcloud_fbm(p + vec2(5.2, 1.3) + time * flowDir * 1.2),
        pengo_starcloud_fbm(p + vec2(-2.8, 3.1) - time * flowDir * 0.9)
    );
    uv += (warp - 0.5) * fbm_strength;
    for (int i = 0; i < count; i++) {
        vec2 toC = uv - centers[i];
        float r = length(toC) + 1e-6;
        float rip = sin(r * cloud_freq - time * cloud_speed) * cloud_amp;
        uv += normalize(toC) * rip;
    }
    return uv;
}
vec2 pengo_starcloud_rotate(vec2 uv, float addtheta) {
    float theta = atan(uv.y, uv.x) + addtheta;
    float r = length(uv);
    return vec2(r * cos(theta), r * sin(theta));
}

// ── 片段着色器 ──
void fragment() {
    vec4 original = texture(TEXTURE, UV);
    vec2 global_uv = UV * uv_scale + uv_offset;
    // 背景模式（use_black_bg=1）下 uv_scale 由外部按节点宽高比设置（见 ShaderController.ApplyStarcloudBgAspect），
    // 避免宽扁背景块上星云被拉伸成椭圆；卡牌模式 uv_scale 由部件映射设置，互不影响。
    vec2 st = global_uv * 0.25;
    vec2 cuv = (st - 0.5) * 2.0;

    float tilt_x = x_rot * 0.00002;
    float tilt_y = y_rot * 0.00002;
    vec2 tilt_offset = vec2(tilt_x, tilt_y);

    float tilt_mag = length(vec2(x_rot, y_rot));
    vec2 flowDir = tilt_mag > 0.01 ? vec2(-y_rot, x_rot) / tilt_mag : vec2(0.0);
    float flowSpeed = clamp(tilt_mag / 4500.0, 0.0, 2.5);

    vec3 star_acc = vec3(0.0);
    for (int layer = 0; layer < 4; layer++) {
        float layerScale = exp(float(layer * layer_parascale + 1) * density * 0.002);
        vec2 layerSpeed = flowDir * flowSpeed * (1.0 + float(layer) * 0.3);
        float layerSize = star_size * (1.0 - float(layer) * 0.2);

        vec2 layerSt = st * layerScale;
        float flowAmount = length(layerSpeed);
        float flowTime = TIME * flowAmount;   // 持续增长
        layerSt -= flowDir * flowTime;

        vec2 gridSt = fract(layerSt);
        vec2 gridId = floor(layerSt);

        float randSeed = pengo_starcloud_random(gridId);
        vec2 snowPos = vec2(
            0.5 + 0.3 * sin(randSeed * 6.28 + TIME * star_horizontal_wave),
            0.5 + 0.2 * cos(randSeed * 12.56)
        );

        float dist = distance(gridSt, snowPos);
        float snowSize = layerSize * 0.018 * (0.5 + 0.5 * randSeed);
        float M = exp(-dist * dist / (snowSize * snowSize));

        float rotateAngle = 0.01 * (TIME * cos(randSeed * 120.0 + TIME * 0.05));

        // 直接使用局部坐标
        vec2 localOffset = (gridSt - snowPos) * (2.0 / layerScale);
        vec2 fst = pengo_starcloud_rotate(localOffset, rotateAngle);

        float snowflake = M * 0.5 * (pengo_starcloud_getI(fst, 0.8 - dist * 42.0, vec2(1.0 / snowSize)) + 1.0);

        float twinkle = max((1.0 - twinkle_effect) + twinkle_effect * (sin(randSeed * 100.0 + TIME * twinkle_speed) * cos(randSeed * 120.0 + TIME * (twinkle_speed + 2.0))), 0.0);
        snowflake *= twinkle;

        vec3 colortype = vec3(pengo_starcloud_random(gridId - 3.0),
                              pengo_starcloud_random(gridId + 7.0),
                              pengo_starcloud_random(gridId + 5.0));
        star_acc += snowflake * colortype;
    }

    // 云彩 UV（务必保留）
    vec2 centers[6];
    centers[0] = vec2(-0.3, 0.4);
    centers[1] = vec2(0.7, -0.5);
    centers[2] = vec2(-0.5, 0.7);

    cuv -= vec2(cloud_color_shiftx, cloud_color_shifty);
    vec2 fuv = pengo_starcloud_clouduv(cuv + tilt_offset * 0.2, TIME, centers, flowDir, flowSpeed);

    vec3 cloud_col = vec3(
        fuv.x * (1.0 - cloud_color_ci1) + fuv.y * cloud_color_ci2,
        fuv.x * (1.0 - cloud_color_ci3) + fuv.y * cloud_color_ci2,
        fuv.x * (1.0 - cloud_color_ci3) + fuv.y * cloud_color_ci1
    );

    vec3 effect_color = mix(star_acc, cloud_col, fbm_mix);
    float star_weight = intensity * 0.9;
    // 背景模式（use_black_bg=1）以黑为底，避免无纹理 ColorRect 采样为白导致背景变白
    vec3 base = mix(original.rgb, vec3(0.0), step(0.5, use_black_bg));
    vec3 result = base + effect_color * star_weight;
    result = clamp(result, 0.0, 1.0);
    result = mix(result, clamp(result * 1.05, 0.0, 1.0), 0.95);
    float out_a = mix(original.a, 1.0, step(0.5, use_black_bg));
    COLOR = vec4(result, out_a);
}