# BalatroEffect 效果器架构文档

> **时效性标注**：本文档最后核对并更新于 **2026-07-31**，已对照 `balatroeffect/Scripts/`、`Scenes/`、`Shaders/` 实际代码核实。
> **权威来源**：`.github/skills/balatroeffect/SKILL.md`（维护最新状态）。本文档与之不一致时，以 skill 为准。
> **AI 代理提示**：改动 balatroeffect 前请调用 `balatroeffect` skill。

## 概述

BalatroEffect 是 PengoTarot 模组的卡牌视觉特效系统，灵感来自 Balatro 的卡牌效果。核心功能：为杀戮尖塔2的卡牌提供部件级和全卡级的动态 shader 特效（闪箔、负片、多彩、镭射、各向异性虹光、VHS、CRT 等），配合 3D 透视倾斜（Tilt）效果。

---

## 渲染管线

### 层级结构

```
Body (Control, 卡牌根节点)
├─ TiltContainer (SubViewportContainer, 1200×1200, pos=(-600,-600))
│  └─ SubViewport (1200×1200)
│     └─ BalatroTiltRoot (Control, pos=(600,600))
│        ├─ ShaderPartContainer "BalatroShaderPart_Frame"
│        │  └─ SubViewport (部件尺寸)
│        │     └─ Control
│        │        └─ [原始 Frame 节点]
│        ├─ ShaderPartContainer "BalatroShaderPart_Portrait"
│        │  └─ SubViewport (部件尺寸)
│        │     └─ Control
│        │        └─ [原始 Portrait 节点]
│        ├─ Shadow, Highlight, ... (其他 CardTemplateNames 节点)
│        └─ Enchantment, etc.
├─ FullCardEffectContainer (仅 FullCard 模式, 1200×1200, pos=(-600,-600))
│  └─ SubViewport (1200×1200)
│     └─ TiltContainer (pos=(0,0))
│        └─ SubViewport (1200×1200)
│           └─ BalatroTiltRoot (pos=(600,600))
│              └─ [所有卡牌子节点]
└─ CardVfxContainer (z-index 0, 始终在最底层)
```

### 四种渲染路径

| 路径 | 条件 | 行为 |
|------|------|------|
| **Path 1: Inspect** | 卡牌在 NInspectCardScreen 或 NBalatroInspectScreen 内 | `ShaderPartContainer` 直接在 Body 上，无 TiltContainer，无 3D 倾斜 |
| **Path 2: FullCard** | `Config.GetEffect(cid, "FullCard") > 0` | TiltContainer + FullCardEffectContainer 常驻，全卡 shader |
| **Path 3a: 部件效果** | 卡牌有部件效果（非 FullCard） | `ShaderPartContainer` 直接在 Body 上（inspect 风格），hover 时懒创建 TiltContainer |
| **Path 3b: 仅动态效果** | 无部件效果，`GlobalDynamicEffect=true` | TiltContainer 懒加载，hover 创建 unhover 销毁 |

---

## 坐标系

### 1200×1200 容器系统

所有 TiltContainer 和 FullCardEffectContainer 使用 1200×1200 的 SubViewport：

- **TiltContainer**: `Size=(1200,1200)`, `Position=(-600,-600)`, `PivotOffset=(600,600)` — 居中于卡牌 Body
- **TiltRoot**: `Position=(600,600)` — 卡牌子节点的挂载点
- **卡片内容**: 480×480，位于 TiltRoot 内，从 `(0,0)` 到 `(480,480)` 相对 TiltRoot

### UV 坐标系（shader 内部）

**部件效果**使用 480 基准的 UV 空间：
```
uv_offset = (partPos + RefPoint) / 480
uv_scale  = partSize / 480
```

- Body 上 (WrapPartsInspect): `RefPoint = 240` (UvRefHalf)
- TiltContainer 内 (WrapParts): `RefPoint = 600` (tilt root position)

**FullCard** 效果通过特殊映射对齐 Body 部件坐标系：
```
uv_scale  = 2.5    // (1.5-0.5)/(0.9-0.5)，将 1200 纹理中卡牌区域(UV 0.5→0.9)映射到 Body 空间(0.5→1.5)
uv_offset = -0.75  // 0.5 - 0.5*2.5
foil_alt   = -1.25 // 0.5*2.5 + x = 0 → x = -1.25
```

### Shader 内 global_uv 计算

```glsl
// 部件和 FullCard 通用
global_uv = UV * uv_scale + uv_offset;
// global_uv 范围: Body 空间 0.5→1.5（卡片区域）
```

---

## Shader 分类

### 旧 5 个基础 shader（modes 1-5）

文件：`balatro_effects_parts.gdshader`

- mode 1: Foil（闪箔）
- mode 2: Negative（负片）
- mode 3: Polychrome（多彩）
- mode 4: Holographic（镭射）
- mode 5: Foil Alt（闪箔偏）

**关键设计原则**（与自定义 shader 对齐）：
- `texture(TEXTURE, UV)` — 纹理采样始终用原始 UV
- `global_uv = UV * uv_scale + uv_offset` — 仅用于效果图案数学
- `tilt_shift = x_rot*0.02 + y_rot*0.03` — 倾斜仅用于微调动画

### TiltContainer shader

文件：`balatro_effects.gdshader`

- `effect_mode=0`：纯 3D 透视倾斜，无效果
- vertex shader：3D 透视变换 + VERTEX 扩展
- fragment shader：透视 UV 扭曲 + 纹理采样
- **无旋转时自动旁路**透视数学：`p = (UV-0.5, 1.0)`, `o = (0,0)`，不做 VERTEX 扩展

### 自定义 shader（modes 6+）

| Mode | Shader | 特点 |
|------|--------|------|
| 6,8,9 | `pengo_aniso_rainbow` | 各向异性虹光 |
| 10 | `pengo_vhs` | VHS 故障效果 |
| 11 | `pengo_crt` | CRT 扫描线 |
| 12 | `pengo_vhs2` | VHS 变体 |
| 13 | `pengo_sweep` | 扫光 |
| 15 | `pengo_hover_glow` | hover 发光 |
| 16 | `pengo_glitter` | 闪烁粒子 |
| 17 | `pengo_aurora_overlay` | 极光叠层 |
| 18 | `pengo_pixelate` | 像素化 |
| 19 | `pengo_outline` | 描边 |
| 20 | `pengo_starcloud` | 星芒 |
| 21 | `pengo_randomstars` | 随机星星 |

所有自定义 shader 遵循相同模式：`texture(TEXTURE, UV)` 采样 + `global_uv = UV * uv_scale + uv_offset` 效果计算。

- **Mode 7 和 14 空缺**，可用于新增效果
- **`Aligns` 标志**（`EffectDef.Aligns`）：标记需要对齐的 shader（如虹光/星星类），注册时指定
- **`TEXTURE_PIXEL_SIZE` 使用情况（已核实）**：`pengo_vhs`（像素化）、`pengo_outline`（texel_size）、`balatro_effects.gdshader`（Tilt 透视）与 `balatro_effects_parts.gdshader` 在使用；`pengo_randomstars` 已改用固定 1/480。不要在新 shader 中用它在卡片空间做坐标计算。

---

## 懒加载 Tilt 机制

### 设计目的

TiltContainer 包含一个 1200×1200 的 SubViewport，创建成本高。手牌/抽牌堆/弃牌堆中可能有几十张卡，全部常驻 TiltContainer 会严重影响性能。

### 实现

```
ApplyShader → 注册到 _hoverCards 字典
SceneTree.ProcessFrame → OnHoverProcess 每帧检测
  hover 检测到 → CreateTilt + 移入子节点
  unhover → 移出子节点 + DestroyTilt（当前 TiltRemoveDelayMsec = 0，无延迟）
_hoverCards 为空 → UnhookHoverWatcher（零开销）
```

### HasInlineEffects 标志

Path 3a 的卡牌在 Body 上预先应用了 `ShaderPartContainer`。hover 时：
1. `CreateTilt(fullCard=false)` — 按 `CardTemplateNames` 白名单移动子节点
2. `ShaderPartContainer` 不在白名单中 → 手动移入 tilt root
3. 按原始 Body 子节点逻辑顺序重排（`LogicalName` 映射）
4. unhover 时全部移回 Body，不调用 `UnwrapParts`（保留 ShaderPartContainer）

### CardHoverState

```csharp
private sealed class CardHoverState
{
    public string Cid = "";
    public TiltWrapper.TiltContainer? Tilt;
    public ulong LeaveTimeMsec;
    public bool HasInlineEffects; // true = Body 上预应用了效果
}
```

---

## Config 持久化系统

### 数据结构

```csharp
ConfigData {
    Version = 5,
    Cards: Dictionary<string, CardEffectEntry>,  // cardId → 配置
    IntensitySettings: Dictionary<int, double>,   // mode → 强度 (0-1)
    GlobalDynamicEffect: bool,                    // 全局动态倾斜开关
    EnableShaderInNonCombat: bool,                // 非战斗启用特效
    ShownTestWarning: bool,                       // 已展示测试警告
}

AllPartNames（8 个部件）: Portrait / Frame / TitleBanner / TypePlaque /
                        PortraitBorder / EnergyIcon / StarIcon / FullCard

CardEffectEntry {
    Mode: int,                    // 当前效果模式
    Parts: Dictionary<string, int>, // 部件名 → 效果模式
    FullCardEffect: int,          // 全卡效果模式 (0=关闭)
}
```

### 部件互斥规则

- 勾选 FullCard → 自动关闭所有单个部件
- 勾选任意单个部件 → 自动关闭 FullCard
- 同一张卡的所有部件使用同一效果模式（不支持不同部件不同效果）

### 性能节流（性能安全阀，NEW）

`Config` 内置安全阀，防止大量特效导致卡顿：

- `IsPerformanceThrottled` 为 true 时，**所有特效读取都返回关闭态**（`GetEffect` / `GetIntensity` / `GlobalDynamicEffect` / `EnableShaderInNonCombat` / `HasAnyEffect`）
- 触发源：`GetNode` 补丁在性能超阈值时调用 `Config.SetPerformanceThrottled(true)`，触发 `PerformanceThrottled` 事件
- `ShaderController.OnPerformanceThrottled` 延迟一帧（`SceneTree.ProcessFrame`）执行 `PerformCleanupAfterThrottle`：清空 `_hoverCards`、卸载 hover watcher、递归遍历场景树移除所有特效容器
- 任何用户配置写入都会自动 `ResetPerformanceThrottle()`

---

## Inspect 界面

### 入口（场景驱动）

InspectScreen 类现位于 `Scenes/InspectScreenButton.cs`（旧 `Scripts/InspectScreen.cs` 已移除）。通过 Harmony Patch（`SetCardPatch`）在原始 `NInspectCardScreen.SetCard` 后添加入口按钮，按钮场景为 `Scenes/balatro_entry_btn.tscn`。

- 点击时追踪 `_currentOriginIndex` / `_currentOriginCards`（动态追踪当前浏览的卡牌索引，而非闭包捕获的初始值）
- 按钮有淡出 + hover 恢复动画（8 秒后 1 秒内 alpha→0，hover 恢复并缩放到 1.02）

### NBalatroInspectScreen

自定义 inspect 界面，场景驱动（`Scenes/balatro_inspect_screen.tscn`）：
- **左 2/3**: 卡牌显示 + 左右切换箭头
- **右 1/3**: 效果配置面板 (Paginator + 强度滑条 `BalatroEffectsSlider` + 部件勾选框 + 菜单)
- 面板 UI 在 `.tscn` 中定义，`InitializeFromScene()` 组装，通过 `FindChild` 查找

### 生命周期

- `Open()`: 动画进入，设置卡片，刷新面板状态，绑定热键（cancel/pauseAndBack/left/right）
- `Close()`: 动画退出 → 返回原始 inspect（使用当前 `_index`）→ `QueueFree()`
- 所有 UI 操作直接读写 `Config`，面板本身不缓存状态

### 关键修复

**二次打开界面按钮失效**：`Close()` 只设 `Visible=false` 不销毁。`QueueFree()` 确保旧实例被清理，静态 `FindChild` 查找不会误命中。

---

## ShaderPartContainer 动态更新

```csharp
// _Process 每帧执行:
1. 在父链中查找 TiltContainer
2. 从 Config 读取当前效果模式
3. 更新 EffectMode / Intensity
4. 若 Shader 类型变化，创建新 ShaderMaterial 并迁移 UV 参数
5. 若有 TiltContainer，同步 x_rot/y_rot 实现闪烁效果
6. 若无 TiltContainer，清零 x_rot/y_rot（防止残留值导致 warp 异常）
```

---

## 运行时动画补丁（NEW）

### 出牌旋转动画（`Scripts/CardPlayEffect/`）

- `CardPlayPatch`：`NPlayerHand.StartCardPlay`（拖出开始）、`CardModel.OnPlayWrapper`（打出）、`NCardPlay.Cleanup`（取消）
- `CardPlayTracker`：`AttackTime=0.12s` 快速上升，`ReleaseTime=0.40s` 缓慢淡出，振幅 `RotationAmplitude=15°`，轴模式 `Time.GetTicksMsec() & 3`
- `PartWrapper` / `FullCardWrapper` 的 `_Process` 中**出牌旋转优先于 Tilt 倾斜闪烁**

### Hover 弹性动画（`Scripts/Patches/EasePatches.cs`）

`NCardHolder.DoCardHoverEffects` transpiler：hover 时注入 `AnimateEase`（缩放 + 随机旋转回弹），受 `Config.GlobalDynamicEffect` 控制。

## 全局补丁（NEW，`Scripts/Patches/`）

- `GetNodeCallRewriter.cs`：把所有第三方 `GetNode` 调用改写为递归 `FindChild`（仅当实例是卡牌 Body 时），由 `ModInitializer` 的 `ScheduleAfterAllModsLoaded` 延迟到所有 Mod 加载后执行。**性能节流的触发源之一**
- `GlobalDynamicPatch.cs`：在卡牌图鉴 `NCardLibrary` 侧栏加「全局3D效果」开关（复制 %Upgrades tickbox）

---

## 常见陷阱

1. **TEXTURE_PIXEL_SIZE**：依赖容器尺寸。不应在卡片空间坐标计算中使用（如星芒的 aspect 修正）。仅 VHS 像素化、outline 等需要实际像素信息的场景使用。

2. **CardTemplateNames 白名单**：`CreateTilt(fullCard=false)` 只移动白名单内的节点。`ShaderPartContainer`（`BalatroShaderPart_*`）不在其中，需手动处理。

3. **WrapParts vs WrapPartsInspect**：UV offset 计算不同（RefPoint 600 vs 240）。Body 上的部件用 Inspect 风格，TiltContainer 内的用 Tilt 风格。

4. **静态状态污染**：`InspectScreen.CurrentCardId` 等静态字段在多实例场景下需要谨慎管理。确保旧实例 `QueueFree()`。

5. **Shader 残留旋转**：`ShaderPartContainer._Process` 从 TiltContainer 抄写 x_rot/y_rot。unhover 后 TiltContainer 销毁，必须清零否则 `perspective_warp_uv` 用残留值继续扭曲。

6. **FullCard UV 映射**：FullCard 的 SubViewport 是 1200×1200 但卡牌在其中占比不同（UV 0.5→0.9），需要 uv_scale=2.5 映射回 Body 坐标系。

7. **perspective_warp_uv 遗留**：`balatro_effects_parts.gdshader` 顶部仍 `#include` `pengo_perspective_warp.gdshaderinc`，但**实际无调用**（遗留物）。纹理采样直接 `texture(TEXTURE, UV)`。新 shader 不要依赖它。

8. **性能节流**：特效突然消失时先检查 `Config.IsPerformanceThrottled`（见「性能节流」章节）。

---

## 文件索引

| 文件 | 职责 |
|------|------|
| `Scripts/Config.cs` | 效果配置持久化（JSON，版本 5）+ 性能安全阀 |
| `Scripts/EffectRegistry.cs` | 效果模式注册（mode → shader，含 `Aligns` 标志） |
| `Scripts/ShaderController.cs` | 卡牌效果分发 + 懒加载 Tilt + 性能节流清理 |
| `Scripts/Patches/EasePatches.cs` | NCardHolder hover 弹性动画（transpiler） |
| `Scripts/Patches/GlobalDynamicPatch.cs` | 卡牌图鉴「全局3D效果」开关 |
| `Scripts/Patches/GetNodeCallRewriter.cs` | GetNode → FindChild 改写（延迟到所有 Mod 加载后） |
| `Scripts/CardPlayEffect/CardPlayPatch.cs` | 出牌生命周期补丁（拖出/打出/取消） |
| `Scripts/CardPlayEffect/CardPlayTracker.cs` | 出牌 360° 旋转动画状态与缓动 |
| `Scripts/Localization/LocExtension.cs` | 特效 UI 文案本地化注入（`BAL_*` 键） |
| `Scripts/ShaderWrappers/TiltWrapper.cs` | TiltContainer 创建/销毁 + CardTemplateNames |
| `Scripts/ShaderWrappers/PartWrapper.cs` | ShaderPartContainer 包裹/解包 + _Process 动态更新 |
| `Scripts/ShaderWrappers/FullCardWrapper.cs` | FullCardEffectContainer + UV 映射 |
| `Scenes/InspectScreenButton.cs` | Inspect 入口按钮 + 共享状态（旧 `Scripts/InspectScreen.cs` 已移除） |
| `Scenes/NBalatroInspectScreen.cs` | 自定义 inspect 界面（场景驱动） |
| `Scenes/balatro_entry_btn.tscn` / `balatro_inspect_screen.tscn` | 界面场景 |
| `Shaders/balatro_effects.gdshader` | TiltContainer 3D 透视 shader (mode 0) |
| `Shaders/balatro_effects_parts.gdshader` | 基础 5 效果 (modes 1-5) |
| `Shaders/pengo_*.gdshader` | 自定义效果 shader (modes 6+) |
| `Shaders/pengo_perspective_warp.gdshaderinc` | 透视 warp 函数（当前仅被 include、无调用，遗留物） |

---

*最后更新：2026-07-31（核对代码后修订）*
