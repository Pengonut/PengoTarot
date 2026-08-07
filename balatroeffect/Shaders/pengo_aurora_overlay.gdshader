shader_type canvas_item;

uniform float x_rot : hint_range(-180, 180) = 0.0;
uniform float y_rot : hint_range(-180, 180) = 0.0;
uniform int effect_mode = 17;
uniform float intensity : hint_range(0, 1, 0.01) = 1.0;
uniform float seed = 0.0;
uniform vec2 uv_offset = vec2(0.0);
uniform vec2 uv_scale = vec2(1.0);

uniform vec4 aurora_color : source_color = vec4(0.84, 0.84, 0.90, 1.0);

// ────── 防冲突噪声 (同前) ──────
mat2 pengo_aurora_mm2(float a) {
    float c = cos(a), s = sin(a);
    return mat2(vec2(c, s), vec2(-s, c));
}
const mat2 pengo_aurora_m2 = mat2(vec2(0.95534, 0.29552), vec2(-0.29552, 0.95534));

float pengo_aurora_tri(float x) {
    return clamp(abs(fract(x) - 0.5), 0.01, 0.49);
}
vec2 pengo_aurora_tri2(vec2 p) {
    return vec2(pengo_aurora_tri(p.x) + pengo_aurora_tri(p.y),
                pengo_aurora_tri(p.y + pengo_aurora_tri(p.x)));
}
float pengo_aurora_triNoise2d(vec2 p, float spd) {
    float z = 1.8, z2 = 2.5, rz = 0.0;
    p *= pengo_aurora_mm2(p.x * 0.06);
    vec2 bp = p;
    for (float i = 0.0; i < 5.0; i++) {
        vec2 dg = pengo_aurora_tri2(bp * 1.85) * 0.75;
        dg *= pengo_aurora_mm2(TIME * spd);
        p -= dg / z2;
        bp *= 1.3;
        z2 *= 0.45;
        z *= 0.42;
        p *= 1.21 + (rz - 1.0) * 0.02;
        rz += pengo_aurora_tri(p.x + pengo_aurora_tri(p.y)) * z;
        p *= (pengo_aurora_m2 * -1.0);
    }
    return clamp(1.0 / pow(rz * 29.0, 1.3), 0.0, 0.55);
}

void fragment() {
    vec4 original = texture(TEXTURE, UV);
    vec2 global_uv = UV * uv_scale + uv_offset;

    // ── 非对称视线方向 (透视投影) ──
    // 屏幕坐标映射到摄像机前方平面
    vec3 ro = vec3(0.0, 0.0, -6.7);
    vec3 rd = normalize(vec3(global_uv - 0.5, 1.2)); // 1.2 控制视角宽度

    // 极慢时间 + 倾斜微调
    float t = TIME * 0.04 + seed;
    float tilt = (x_rot * 0.015) + (y_rot * 0.015);
    rd.yz *= pengo_aurora_mm2(0.3 + tilt);      // 轻微旋转打破对称残余
    rd.xz *= pengo_aurora_mm2(sin(t * 0.07) * 0.1 + tilt * 0.5);

    // 射线行进
    vec3 col = vec3(0.0);
    float transmittance = 1.0;
    const int STEP_COUNT = 25;
    float step_size = 0.45;

    for (int i = 0; i < STEP_COUNT; i++) {
        float dist = float(i) * step_size + 0.8;
        vec3 pos = ro + rd * dist;
        vec2 p = pos.xz;                     // 采样平面

        float density = pengo_aurora_triNoise2d(p * 0.9, 0.06);
        density *= smoothstep(0.0, 0.2, dist) * smoothstep(12.0, 4.0, dist);

        vec3 grad = sin(vec3(2.15, -0.5, 1.2) + pos.y * 0.8 + t * 0.15) * 0.5 + 0.5;
        vec3 sample_color = aurora_color.rgb * grad * density;

        float absorption = 0.6;
        col += transmittance * sample_color * absorption;
        transmittance *= 1.0 - density * absorption;
    }

    col = clamp(col * 1.5, 0.0, 1.0);

    // 叠加
    float alpha = intensity * max(col.r, max(col.g, col.b));
    vec3 result = original.rgb + col * alpha;
    result = mix(result, clamp(result, 0.0, 1.0), 0.9);
    result *= vec3(0.95, 0.88, 1.12);

    COLOR = vec4(result, original.a);
}