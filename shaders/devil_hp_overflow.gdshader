shader_type canvas_item;
// 恶魔（Devil）超额生命：把血条前景 %HpForeground 的右端（超出最大生命值的部分）
// 做「色相偏移」染成暖橙 —— 保留前景贴图的阴影/明暗细节（不做纯色覆盖）。
// 由 TarDevilDivinationPatch 在恶魔开启且玩家 CurrentHp > MaxHp 时挂到 %HpForeground 上。
//
// max_hp_edge：MaxHp 位置（节点局部像素坐标，从前景左端算起）。
// 用 VERTEX（varying local_pos）取节点局部坐标 —— %HpForeground 是 NinePatchRect，
// UV 被九宫格限制（垂直方向不覆盖全节点），不能直接用 UV 判断位置。
//
// 左端保持默认（TEXTURE * COLOR，跟随游戏 SelfModulate 的红/蓝/紫）；
// 右端（local_pos.x > max_hp_edge）：把色相替换为 target_hue（暖橙），保留原饱和度/明度
// （即保留贴图阴影）。交界处用「颜色插值」（红色→橙色 mix，alpha 保持不透明）：
// ⚠️ 不能 alpha 淡出 —— 橙色下面没有红色可露（下面是血条背景），淡出会露出背景形成
// 明显分界线（用户实测反馈）；颜色插值 smoothstep 缓入缓出才无交界线。
// 水波流动（用户追加）：超额区内用多频正弦叠加（TIME 驱动）产生流动的聚集区，
// 聚集处橙色略提亮（更明显），随 TIME 聚散偏移；flow_strength 控制强度（0=关闭）。
uniform float max_hp_edge : hint_range(0.0, 4096.0) = 1000000.0;
uniform float fade_range : hint_range(0.0, 256.0) = 28.0;
uniform float target_hue : hint_range(0.0, 1.0) = 0.092;   // 暖橙 #F59C2E 的色相（约 33°）
uniform float flow_strength : hint_range(0.0, 1.0) = 0.3;   // 流动聚集强度

varying vec2 local_pos;

vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void vertex()
{
    local_pos = VERTEX;
}

void fragment()
{
    vec4 base = texture(TEXTURE, UV) * COLOR;
    if (local_pos.x > max_hp_edge)
    {
        // 色相替换为暖橙（保留前景明暗/阴影细节）
        vec3 hsv = rgb2hsv(base.rgb);
        hsv.x = target_hue;
        vec3 orange = hsv2rgb(hsv);
        // 交界处颜色从红色平滑插值到橙色（smoothstep 缓入缓出，无硬交界线）；
        // alpha 保持不透明 —— 淡出会露出血条背景形成分界线，故不做 alpha 渐变
        float t = smoothstep(0.0, fade_range, local_pos.x - max_hp_edge);
        vec3 color = mix(base.rgb, orange.rgb, t);

        // 水波流动：多频正弦叠加（横/纵 + 不同速度/相位）形成流动的聚集区，
        // 聚集处橙色略提亮（更明显），并随 TIME 移动/聚散偏移。
        // ⚠️ 提亮要乘以 t（交界过渡浓度因子）：交界处橙浓度极低，流动提亮不能盖过
        // 它应有的低浓度（否则交界处浓度异常提升）——只有橙浓度高的区域才充分提亮。
        float w = sin(local_pos.x * 0.05 + TIME * 2.0)
                + sin(local_pos.x * 0.11 - TIME * 1.4 + 2.5)
                + 0.4 * sin(local_pos.x * 0.22 + TIME * 2.8)
                + 0.5 * sin(local_pos.y * 0.13 + TIME * 1.1);
        float flow = 0.5 + 0.5 * clamp(w * 0.4, -1.0, 1.0);
        flow *= flow;   // 平方让峰更集中（小部分聚集区更明显）
        color *= 1.0 + flow_strength * flow * t;

        COLOR = vec4(color, base.a);
    }
    else
    {
        COLOR = base;
    }
}
