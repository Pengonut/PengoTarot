// pengo_perspective_warp.gdshaderinc
// Fragment-shader perspective warp, extracted from balatro_effects.gdshader vertex shader.
// Warps a 0-1 UV coordinate as if the card is tilted by x_rot/y_rot degrees around center.
// Usage: vec2 warped = perspective_warp_uv(card_uv, x_rot, y_rot);
// When no rotation is applied, the input UV is returned unchanged (no clipping / scaling).

vec2 perspective_warp_uv(vec2 uv, float x_rot, float y_rot) {
    // Bypass perspective math when no tilt — prevents incorrect clipping
    // and scaling caused by UV offsets from the 1200×1200 container system.
    if (abs(x_rot) < 0.001 && abs(y_rot) < 0.001)
        return uv;

    const float fov = 90.0;

    float sin_b = sin(y_rot / 180.0 * 3.14159265);
    float cos_b = cos(y_rot / 180.0 * 3.14159265);
    float sin_c = sin(x_rot / 180.0 * 3.14159265);
    float cos_c = cos(x_rot / 180.0 * 3.14159265);

    mat3 inv_rot_mat = mat3(
        vec3(cos_b, 0.0, -sin_b),
        vec3(sin_b * sin_c, cos_c, cos_b * sin_c),
        vec3(sin_b * cos_c, -sin_c, cos_b * cos_c)
    );

    float t = tan(fov / 360.0 * 3.14159265);
    vec3 p = inv_rot_mat * vec3((uv - 0.5), 0.5 / t);
    float v = (0.5 / t) + 0.5;
    p.xy *= v * inv_rot_mat[2].z;
    vec2 o = v * inv_rot_mat[2].xy;

    // cull_back
    if (p.z <= 0.0)
        return uv;

    vec2 uv_skew = (p.xy / p.z) - o;

    // Clamp to valid range instead of discarding — preserves full card size
    // while still providing perspective warp for the tilt effect.
    return clamp(uv_skew + 0.5, 0.0, 1.0);
}
