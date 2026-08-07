shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 17;
uniform float intensity : hint_range(0, 3.0, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

// ═══════ 碎片密度（两层） ═══════
const float cell_density_1 = 13.0;
const float cell_density_2 = 21.0;

// ═══════ 碎片大小（UV空间） ═══════
const float radius_uv_min = 0.03 * 1.4;
const float radius_uv_max = 0.12 * 1.4;


// ═══════ 大碎片透明度═══════
const float BIG_OPACITY = 0.5;

// ═══════ 死亡比例 ═══════
const float DEAD_RATIO = 0.6;

// ═══════ 光晕参数 ═══════
const float GLOW_RADIUS = 0.08;
const float GLOW_INTENSITY = 0.3;

// ═══════ 倾斜与随机抑制 ═══════
const float light_strength = 0.85;
const float random_suppression = 0.65;
const float random_threshold = 0.5;

// ═══════ 时间响应 ═══════
const float time_speed = 0.06;

// ═══════ 双层混合权重 ═══════
const float layer1_weight = 0.55;
const float layer2_weight = 0.45;

// ═══════ 子碎片参数 ═══════
const float SUB_SPARKLE_STRENGTH = 0.6;
const float SUB_GLOW_STRENGTH = 0.3;
const float SUB_SIZE_BASE = 0.24;       // 相对于细胞大小的系数
const float SUB_GLOW_RADIUS = 0.04;     // 子光晕半径（UV空间）

// ─── 随机函数 ───
float pengo_hash1(vec2 st) {
    return fract(sin(dot(st, vec2(12.9898, 78.233))) * 43758.5453123);
}
vec2 pengo_hash2(vec2 st) {
    return vec2(pengo_hash1(st), pengo_hash1(st + vec2(1.345, 5.678)));
}
vec2 pengo_sparkle_random2(vec2 st) {
    st = vec2( dot(st, vec2(127.1, 311.7)),
               dot(st, vec2(269.5, 183.3)) );
    return -1.0 + 2.0 * fract(sin(st) * 43758.5453123);
}

// ─── 计算一个Voronoi层，包含主碎片和子碎片（几何小碎片） ───
void compute_glitter(vec2 global_uv, float density,
                     out float sparkle_out, out vec3 color_out, out float glow_out) {
    vec2 i = floor(global_uv * density);
    vec2 f = fract(global_uv * density);
    float minDist = 1.0;
    vec2 nearestPoint;
    vec2 nearestCell;

    for (int y = -1; y <= 1; y++) {
        for (int x = -1; x <= 1; x++) {
            vec2 neighbor = vec2(float(x), float(y));
            vec2 point = pengo_sparkle_random2(i + neighbor + seed) * 0.5 + 0.5;
            vec2 diff = neighbor + point - f;
            float d = length(diff);
            if (d < minDist) {
                minDist = d;
                nearestPoint = point;
                nearestCell = i + neighbor;
            }
        }
    }

    float dist_uv = minDist / density;
    float frag_seed = fract(sin(dot(nearestCell, vec2(12.9898, 78.233))) * 43758.5453123);
    float frag_radius = mix(radius_uv_min, radius_uv_max, frag_seed);

    // 死亡检测
    float alive = frag_seed > DEAD_RATIO ? 1.0 : 0.0;

    // 光线对齐因子（当倾角为零时使用默认方向避免全黑）
    vec2 light_dir_raw = vec2(-x_rot, -y_rot);
    float len_dir = length(light_dir_raw);
    vec2 light_dir = len_dir > 0.01 ? normalize(light_dir_raw) : vec2(0.707, 0.707);

    vec2 frag_center = (nearestCell + nearestPoint) / density;
    vec2 dir_to_center = frag_center - 0.5;
    float light_alignment = max(0.0, dot(normalize(dir_to_center + 1e-4), light_dir));
    float light_factor = mix(0.5, 1.0, light_alignment);

    // 随机抑制（效果打折）
    float slow_time = TIME * time_speed + seed;
    float suppression_rand = pengo_hash1(nearestCell + floor(slow_time * 3.7));
    float effective_light = light_factor * (1.0 - random_suppression * 0.6);

    // 时间调制（波动小，整体偏亮）
    float time_mod = sin(slow_time * 0.6 + frag_seed * 9.0) * 0.12 + 0.88;

    // 主闪光
    float sparkle = 0.0;
    if (dist_uv < frag_radius && alive > 0.5) {
        float edge = smoothstep(frag_radius, frag_radius - 0.005, dist_uv);
        sparkle = edge * effective_light * time_mod;
    }
    sparkle_out = sparkle * BIG_OPACITY;

    // 主光晕
    float glow = 0.0;
    if (alive > 0.5) {
        glow = exp(-dist_uv * dist_uv / (GLOW_RADIUS * GLOW_RADIUS)) * GLOW_INTENSITY;
        glow *= effective_light * time_mod;
    }
    glow_out = glow * BIG_OPACITY;

    // 子碎片（仅在细胞存活时计算）
    float sub_sparkle = 0.0;
    float sub_glow = 0.0;
    if (alive > 0.5) {
        float cell_size = 1.0 / density;
        vec2 cell_center = frag_center;   // (nearestCell + nearestPoint) / density

        // 子碎片数量 4~8
        float sub_count_f = 4.0 + floor(pengo_hash1(nearestCell + vec2(5.3, 2.7)) * 5.0);
        int sub_count = int(clamp(sub_count_f, 4.0, 8.0));

        for (int j = 0; j < 8; j++) {
            if (j >= sub_count) break;

            // 子碎片随机种子
            vec2 seed_j = nearestCell + vec2(float(j) * 3.7, 1.3);

            // 子中心偏移（限制在细胞内部）
            vec2 sub_offset = (pengo_hash2(seed_j) - 0.5) * cell_size * 0.7;

            // 子碎片形状参数：旋转角度和拉伸比
            float sub_angle = pengo_hash1(seed_j + 1.2) * 6.28318;
            float aspect = 0.5 + pengo_hash1(seed_j + 2.3) * 1.0;   // 0.5 ~ 1.5

            // 子碎片尺寸
            float sub_size = cell_size * SUB_SIZE_BASE * (0.7 + 0.6 * pengo_hash1(seed_j + 3.4));

            // 当前像素到子中心的向量
            vec2 diff = global_uv - (cell_center + sub_offset);
            // 逆旋转
            float c = cos(-sub_angle);
            float s = sin(-sub_angle);
            vec2 diff_rot = vec2(diff.x * c - diff.y * s, diff.x * s + diff.y * c);

            // 菱形距离（非圆形，形状锐利）
            float d_sub = abs(diff_rot.x) * aspect + abs(diff_rot.y) / aspect;

            // 形状掩码（边缘稍硬，避免模糊）
            float shape = 1.0 - smoothstep(sub_size * 0.7, sub_size, d_sub);

            if (shape < 0.001) continue;

            // 子碎片独立的光线响应（随机反射方向 → 类似糖果纸）
            vec2 sub_reflect_dir = normalize(pengo_hash2(seed_j + 4.7) * 2.0 - 1.0);
            float sub_light = max(0.0, dot(sub_reflect_dir, light_dir));
            // 混合主光因子以保持整体接近
            float effective_sub_light = mix(light_factor, sub_light, 0.7);

            // 子碎片时间相位偏移
            float sub_time_mod = sin(slow_time * 0.8 + frag_seed * 12.0 + float(j) * 1.5) * 0.2 + 0.8;

            // 子闪光贡献
            float sub_sp = shape * effective_sub_light * sub_time_mod * SUB_SPARKLE_STRENGTH;
            sub_sparkle += sub_sp;

            // 子光晕贡献（更小的光晕半径）
            float dist_to_sub = length(diff);
            float sub_glow_contrib = exp(-dist_to_sub * dist_to_sub / (SUB_GLOW_RADIUS * SUB_GLOW_RADIUS))
                                     * SUB_GLOW_STRENGTH * effective_sub_light * sub_time_mod;
            sub_glow += sub_glow_contrib;
        }
    }

    // 将子碎片叠加到主输出上
    sparkle_out += sub_sparkle;
    glow_out += sub_glow;

    // 颜色：高饱和、慢速漂移
    float hue = fract(frag_seed * 7.13 + slow_time * 0.03);
    vec3 col = vec3(
        0.5 + 0.5 * sin(6.2831 * hue),
        0.5 + 0.5 * sin(6.2831 * (hue + 0.333)),
        0.5 + 0.5 * sin(6.2831 * (hue + 0.667))
    );
    vec3 saturated = (col - 0.5) * 3.5 + 0.65;
    color_out = clamp(saturated, 0.0, 1.0);
}

// ─── 片段着色器 ───
void fragment() {
    vec4 original = texture(TEXTURE, UV);
    vec2 global_uv = UV * uv_scale + uv_offset;

    float sparkle1, sparkle2, glow1, glow2;
    vec3 color1, color2;
    compute_glitter(global_uv, cell_density_1, sparkle1, color1, glow1);
    compute_glitter(global_uv, cell_density_2, sparkle2, color2, glow2);

    // 颜色混合（用主闪光加权）
    vec3 mixed_color = (color1 * sparkle1 + color2 * sparkle2) /
                       max(sparkle1 + sparkle2, 1e-4);

    // 总强度：主闪光 + 光晕 + 子碎片贡献已在内部累加
    float total_sparkle = (sparkle1 * layer1_weight + sparkle2 * layer2_weight) * intensity;
    float total_glow    = (glow1    * layer1_weight + glow2    * layer2_weight) * intensity;

    // 高亮区域略微向白色偏移（更白更亮的光感）
    float bright_boost = clamp(total_sparkle * 0.2 + total_glow * 0.15, 0.0, 1.0);
    mixed_color = mix(mixed_color, vec3(1.0), bright_boost);

    // ─── 直接叠加（与 starcloud 相同风格） ───
    float sparkle_weight = 1.2;
    vec3 result = original.rgb;
    result += mixed_color * total_sparkle * sparkle_weight;   // 主闪光
    result += mixed_color * total_glow    * 0.7;              // 泛光光晕

    // 全局提亮与柔和处理
    result = clamp(result, 0.0, 1.0);
    result = mix(result, clamp(result * 1.08, 0.0, 1.0), 0.95);
    COLOR = vec4(result, original.a);
}