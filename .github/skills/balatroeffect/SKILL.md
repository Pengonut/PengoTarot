---
name: balatroeffect
description: '卡牌视觉特效系统 (BalatroEffect)：杀戮尖塔2卡牌的 shader 特效（Foil/负片/多彩/镭射/各向异性虹光/VHS/CRT 等）与 3D 透视倾斜 (Tilt)。Use when: 修改或新增卡牌特效 shader、调试 UV 坐标 / global_uv / Tilt 倾斜、注册效果模式 (EffectRegistry)、改检查界面 (NBalatroInspectScreen / NBalatroInspectEnchantScreen / InspectScreenButton)、改附魔特效配置 (EnchantmentConfig)、附魔预览选卡/排序、排查懒加载 Tilt、性能节流、出牌旋转动画、卡牌特效配置 (Config)。'
argument-hint: '如：新增一个 VHS 风格特效模式 / 调试星芒 shader 坐标'
user-invocable: true
---

# BalatroEffect 特效系统

PengoTarot 的卡牌视觉特效子系统。核心：为卡牌提供**部件级**与**全卡级**动态 shader 特效，配合 3D 透视倾斜（Tilt）。源码均在 `balatroeffect/` 下，C# 主逻辑在 `balatroeffect/Scripts/`，shader 在 `balatroeffect/Shaders/`，界面在 `balatroeffect/Scenes/`。

> ⚠️ **重要**：`balatroeffect/ARCHITECTURE.md` 有参考价值，但**部分章节已过时**（缺新子系统、个别说法与代码不符）。**以本 SKILL 为准**。已知差异见文末「ARCHITECTURE.md 过时点」。

## 何时使用

- 修改 / 新增 / 调试卡牌特效 shader（`Shaders/*.gdshader`）
- 注册新的效果模式（`EffectRegistry`）
- 修改检查界面（`NBalatroInspectScreen` / `NBalatroInspectEnchantScreen` / `InspectScreenButton` / `.tscn`）
- 修改附魔特效配置 / 附魔预览选卡 / 附魔排序（`EnchantmentConfig` / `NBalatroInspectEnchantScreen`）
- 排查 Tilt 倾斜、懒加载、性能节流、出牌旋转动画相关 bug
- 新增卡面部件、修改效果配置持久化

## 当前文件布局（已核实 2026-08-01）

| 文件 | 职责 |
|------|------|
| `Scripts/Config.cs` | 效果配置持久化（JSON，版本 5）+ **性能安全阀** + **附魔运行时 overlay**（`SetCardEnchantmentOverlay` / `_previewEnchantCards`；6 个读取方法路由：运行时卡效果为底附魔覆盖、预览只读附魔配置） |
| `Scripts/EnchantmentConfig.cs` | **附魔特效配置持久化**（独立 `BalatroEffectEnchantments.json`，键=附魔 id）+ `Changed` 结构变更事件（触发全局重应用） |
| `Scripts/EffectRegistry.cs` | 效果模式注册（mode → shader），`EffectDef` 带 `Aligns` 标志 |
| `Scripts/ShaderController.cs` | 卡牌效果分发 + 懒加载 Tilt + 性能节流清理 + 附魔配置订阅（`EnchantmentConfig.Changed → RefreshAllCards`、`_enchHandlers` 挂 `EnchantmentChanged`、`ApplyShader` 设 overlay、`InInspect` 含附魔屏） |
| `Scripts/Patches/EasePatches.cs` | `NCardHolder` hover 弹性动画（transpiler） |
| `Scripts/Patches/GlobalDynamicPatch.cs` | 卡牌图鉴侧栏「全局3D效果」开关 |
| `Scripts/Patches/GetNodeCallRewriter.cs` | 把第三方 `GetNode` 调用改写为递归 `FindChild`（延迟到所有 Mod 加载后） |
| `Scripts/CardPlayEffect/CardPlayPatch.cs` | 出牌生命周期补丁（拖出 / 打出 / 取消） |
| `Scripts/CardPlayEffect/CardPlayTracker.cs` | 出牌 360° 旋转动画状态与缓动 |
| `Scripts/Localization/LocExtension.cs` | 特效 UI 文案本地化注入（`BAL_*` 键） |
| `Scripts/ShaderWrappers/TiltWrapper.cs` | TiltContainer 生命周期 + 子节点过滤 |
| `Scripts/ShaderWrappers/PartWrapper.cs` | ShaderPartContainer 包裹/解包 + `_Process` 动态更新 |
| `Scripts/ShaderWrappers/FullCardWrapper.cs` | FullCardEffectContainer + UV 映射 |
| `Scenes/InspectScreenButton.cs` | 检查界面入口按钮 + 共享状态 + 菜单动作（替代旧 `Scripts/InspectScreen.cs`） |
| `Scenes/NBalatroInspectScreen.cs` | 自定义卡牌检查界面（场景驱动，`IScreenContext`，含 `%EnchantTickbox` 切换附魔屏） |
| `Scenes/NBalatroInspectEnchantScreen.cs` | **附魔特效检查界面**（场景驱动，独立于卡牌屏）+ 附魔排序 + 方法1预览选卡 |
| `Scenes/balatro_entry_btn.tscn` / `balatro_inspect_screen.tscn` | 界面场景（卡牌屏含 `%EnchantTickbox` 勾选进入附魔屏） |
| `Scenes/balatro_inspect_enchant_screen.tscn` | 附魔屏场景（继承卡牌屏 + 覆写根脚本 + `%EnchantNameLabel` 名称标签） |
| `Shaders/` | 各效果 shader |

## 核心概念

### 1. 四种渲染路径（`ShaderController.ApplyShader`）

| 路径 | 条件 | 行为 |
|------|------|------|
| **Path 1: Inspect** | 卡牌在 `NInspectCardScreen` / `NBalatroInspectScreen` 内 | `ApplyShaderInspect`：`ShaderPartContainer` 直接包在 Body 上，无 Tilt、无 3D 倾斜 |
| **Path 2: FullCard** | `Config.GetEffect(cid, "FullCard") > 0` | 立即建 Tilt + FullCardEffectContainer（常驻） |
| **Path 3a: 部件效果** | 有部件效果（非 FullCard） | 部件直接包在 Body（inspect 风格），hover 时懒创建 Tilt |
| **Path 3b: 仅动态效果** | 无部件效果，`GlobalDynamicEffect=true` | Tilt 懒加载，hover 创建、unhover 销毁 |

`ApplyShader` 会对每张卡注册 `ModelChanged` 处理器（`_handlers` 字典，用旧的先注销）。

### 2. 坐标系（改动前必读）

**1200×1200 容器**（TiltContainer / FullCardEffectContainer）：
- `TiltContainer`: `Size=(1200,1200)`，`Position=(-600,-600)`，`PivotOffset=(600,600)`，居中于卡牌 Body
- `TiltRoot`: `Position=(600,600)` — 卡牌子节点挂载点
- 卡片内容 480×480，位于 TiltRoot 内 `(0,0)`→`(480,480)`

**部件效果 UV 空间**（480 基准，`UvRefSize=(480,480)`，`UvRefHalf=(240,240)`）：
```
uv_offset = (partPos + RefPoint) / 480
uv_scale  = partSize / 480
```
- Body 上 (`WrapPartsInspect`): `RefPoint = 240`（UvRefHalf）
- Tilt 内 (`WrapParts`): `RefPoint = 600`（tilt root 位置）

**FullCard UV 映射**（`FullCardWrapper.CreateFullCard` 已核实）：
```
uv_scale = 2.5    // (1.5-0.5)/(0.9-0.5)，把 1200 纹理中卡牌区域(UV 0.5→0.9)映射到 Body 空间(0.5→1.5)
uv_offset = -0.75 // 0.5 - 0.5*2.5
foil_alt_uv_offset = -1.25 // 0.5*2.5 + x = 0
```

**shader 内 global_uv**（所有 shader 通用）：
```glsl
global_uv = UV * uv_scale + uv_offset;  // 范围：Body 空间 0.5→1.5（卡片区域）
```

### 3. 效果模式注册（`EffectRegistry`）

`EffectDef(int Mode, Shader? Shader, string LocKey, bool Aligns)`。`Initialize()` 里注册，mode → shader 映射（**mode 14 空缺，可用来加新效果**；mode 7 已被 aniso_fixed 占用）：

| Mode | Shader | LocKey |
|------|--------|--------|
| 0 | 无 | `OPTION_NONE` |
| 1 | `balatro_effects_parts`（Foil 闪箔） | `OPTION_FOIL` |
| 2 | `balatro_effects_parts`（Foil Alt 闪箔偏） | `OPTION_FOIL_ALT` |
| 3 | `balatro_effects_parts`（Polychrome 多彩） | `OPTION_POLYCHROME` |
| 4 | `balatro_effects_parts`（Holographic 镭射） | `OPTION_HOLOGRAPHIC` |
| 5 | `balatro_effects_parts`（Negative **负片-A**：原 shader 负片效果，带蓝光泽） | `OPTION_NEGATIVE` |
| 6 | `balatro_effects_parts`（Negative B **负片-B**：简单反色 `1.0-rgb`，原 NegativeShaderPatch 效果） | `OPTION_NEGATIVE_BLUE` |
| 7 | `pengo_aniso_rainbow`（fixed） | `OPTION_ANISO_FIXED`（aligns） |
| 8, 9 | `pengo_aniso_rainbow`（stripe/dual） | `OPTION_ANISO_STRIPE/DUAL`（aligns） |
| 10 | `pengo_vhs` | `OPTION_VHS`（aligns） |
| 11 | `pengo_crt` | `OPTION_CRT` |
| 12 | `pengo_vhs2` | `OPTION_VHS2` |
| 13 | `pengo_sweep` | `OPTION_SWEEP` |
| 15 | `pengo_hover_glow` | `OPTION_HOVER_GLOW`（aligns） |
| 16 | `pengo_glitter` | `OPTION_GLITTER`（aligns） |
| 17 | `pengo_aurora_overlay` | `OPTION_AURORA`（aligns） |
| 18 | `pengo_pixelate` | `OPTION_PIXELATE` |
| 19 | `pengo_outline` | `OPTION_OUTLINE` |
| 20 | `pengo_starcloud` | `OPTION_STARCLOUD`（aligns） |
| 21 | `pengo_randomstars` | `OPTION_RANDOMSTARS`（aligns） |

> ⚠️ **mode 编号持久化**：卡/附魔配置里存的 mode 编号是**持久化**的。2026-08-02 将部分效果重排为 1闪箔 2闪箔偏 3多彩 4镭射 5负片-A 6负片-B，aniso_fixed 顺延到 7。Config 升 v8、EnchantmentConfig 升 v2。**Config 迁移为链式 v5→v6→v7→v8**（`Config.UpgradeToCurrent`：`V5ToV6` 强度导入 → `V6ToV7` 推断 EditMode → `V7ToV8` mode 重排，`RemapModeV7ToV8` 2→5/5→2/6→7），`Load`/`ImportPreset` 统一走该链，避免老玩家（v5/v6）跳过 v7→v8 的 mode 重排。改 mode 编号务必同时更新 shader effect_mode + EffectRegistry + 本地化 + 链式迁移 + 作者预设。

**shader 统一约定**：`texture(TEXTURE, UV)` 采样用原始 UV；`global_uv` 只用于效果图案数学；`tilt_shift = x_rot*0.02 + y_rot*0.03` 只用于微调动画。`effect_mode==5`（Foil Alt）用 `foil_alt_uv_offset`，其余用 `uv_offset`。

**卡面部件**（`Config.AllPartNames`，8 个）：`Portrait`、`Frame`、`TitleBanner`、`TypePlaque`、`PortraitBorder`、`EnergyIcon`、`StarIcon`、`FullCard`。互斥规则：勾 FullCard 自动清空所有部件；勾任意部件自动清空 FullCard（`InspectScreenButton.OnPartToggled`）。

> ⚠️ **效果类型（mode）是卡片级唯一的**：一张卡只能有一种效果，部件只是该效果作用在卡面上的部位。勾选部件统一写入当前 paginator 选中的 mode（`OnPartToggled` 用 `CurrentEffectIndex`）；切换效果会把所有已勾选部件一并切换（`OnPaginatorNavigate` → `ApplyCurrentEffectToCheckedParts`）。数据层不强制（`ImportCardPreset` 可导入任意 Parts），但正常 UI 流程不会产生部件间不同 mode。**强度同理：每卡一个强度**，内嵌于卡牌条目 `CardEffectEntry.Intensity`（v6 起，旧的全局按 mode 的 `IntensitySettings` 已废除，v5 配置在 `Load` 时自动迁移：把全局强度按卡导入）。**无效条目自动清理**：`Parts` 为空且非 FullCard（`FullCardEffect==0`）的卡牌条目即使 `Mode`/`Intensity` 非默认也会被移除（`Config.PruneInvalidEntries`，加载/迁移/导入后调用；`SetEffect`/`ClearEffect` 写入后同样清理）——因为只有 `Mode` 而无实际部件/FullCard 的卡不渲染任何效果。

### 4. 懒加载 Tilt（性能关键）

TiltContainer 含 1200×1200 SubViewport，创建成本高，几十张手牌不能常驻：
```
ApplyShader → 注册到 _hoverCards
SceneTree.ProcessFrame → OnHoverProcess 每帧检测
  hover → CreateTilt + 移入子节点
  unhover → 移出 + DestroyTilt（TiltRemoveDelayMsec = 0，当前无延迟）
_hoverCards 为空 → UnhookHoverWatcher（零开销）
```
`CardHoverState`：`Cid` / `Tilt` / `LeaveTimeMsec` / `HasInlineEffects`（true = Body 上预应用了部件效果）。

**子节点过滤**（`TiltWrapper`，改动前必读）：
- `SkipNames` = `CardVfxContainer`、`RareGlow`、`UncommonGlow` — 永远留在 Body
- `CardTemplateNames` 白名单 — 只有这些才进 Tilt SubViewport
- `ShouldIncludeExternalNode` — 非模板节点按名称过滤，含 `vfx/effect/particle/anim/trail/glow/sparkle` 关键词的排除
- **`ShaderPartContainer`（`BalatroShaderPart_*`）不在白名单**，`CreateTilt(fullCard=false)` 后需手动处理（`HasInlineEffects` 流程：记录 Body 完整逻辑顺序快照 → 移动后按 `OriginalBodyNames` 重排）

### 5. 性能节流（NEW，ARCHITECTURE.md 没有）

`Config` 内置安全阀，防止大量特效导致卡顿：
- `IsPerformanceThrottled` 为 true 时**所有特效读取返回关闭态**（`GetEffect`/`GetIntensity`/`GlobalDynamicEffect`/`EnableShaderInNonCombat`/`HasAnyEffect`）
- 触发源：`GetNode` 补丁在性能超阈值时调用 `Config.SetPerformanceThrottled(true)`，触发 `PerformanceThrottled` 事件
- `ShaderController.OnPerformanceThrottled` 延迟一帧（`ProcessFrame`）执行 `PerformCleanupAfterThrottle`：清空 `_hoverCards`、卸载 hover watcher、递归遍历场景树 `RemoveAllContainers` + `CleanupInspect`
- 任何用户配置写入都会自动 `ResetPerformanceThrottle()`

### 6. 检查界面

- **入口**：`InspectScreenButton.SetCardPatch`（Harmony）在原始 `NInspectCardScreen.SetCard` 后添加入口按钮（场景 `balatro_entry_btn.tscn`），并追踪 `_currentOriginIndex` / `_currentOriginCards`。按钮有 8 秒淡出 + hover 恢复 + 1.02 缩放动画
- **背景星云（2026-08-02）**：卡牌屏/附魔屏共用父场景 `CardArea/CardAreaBg`（黑色半透明块）。`pengo_starcloud.gdshader` 加了默认关闭的 `uniform float use_black_bg`（=1 时不叠加原图——无纹理 ColorRect 采样为白、否则背景会变白——黑底显示星云且 alpha=1）；`CardAreaBg` 挂 ShaderMaterial 设 `use_black_bg=1.0`。卡牌用法默认 0 完全不受影响。**拉伸修复**：背景块宽扁（~853×720），星云按归一化 UV 生成会被横向拉成椭圆。⚠️ **`SIZE` 在该渲染环境不可用**（shader 编译失败：`表达式中的标识符未知："SIZE"`）→ 改用 **`uv_scale`** 补偿：`ShaderController.ApplyStarcloudBgAspect`（内部 `uv_scale=(w/maxDim, h/maxDim)` 短边归一化，同 `HoverTipShaderPatch` 的做法）按节点实际尺寸设置，卡牌屏/附魔屏 `InitializeFromScene` 各调一次（`GetNodeOrNull<ColorRect>("CardArea/CardAreaBg")`）。面板 `PanelArea/PanelBg`（深青色）未加。
- **NBalatroInspectScreen**：场景驱动（`balatro_inspect_screen.tscn`），左 2/3 卡牌 + 左右箭头，右 1/3 配置面板（Paginator + 强度滑条 `BalatroEffectsSlider` + 部件勾选框 + 菜单）
- **生命周期**：`Open()` 动画进入 → `SetCard` → 绑定热键；`Close()` 动画退出 → 返回原始 inspect（当前 `_index`）→ `QueueFree()`
- **静态状态**：`InspectScreen` 的静态字段（`CurrentCardId` / `CurrentEffectIndex` / `VisibleCards`）多实例共享，旧实例必须 `QueueFree()`
- **菜单动作**（`OnMenuAction`）：清空 / 加载作者预设 / 应用到可见卡牌 / 导入导出全局（剪贴板）。**作者预设（全局预设，2026-08-02 改造）**：`Config.ApplyAllAuthorPresets()` 同时应用卡牌预设 `res://balatroeffect/Assets/author_preset.json` + 附魔预设 `res://balatroeffect/Assets/author_enchant_preset.json`，**按 id 合并**（预设 id 覆盖对应条目、其余保留，不整文件替换）；在 JSON **首次初始化** / **v5、v6→v7 升级**后调用；`Config.ImportPreset`（含剪贴板导入）也改为按 id 合并。**负片（Sub 塔罗卡 + Sub 附魔）已内置为作者预设**（`Portrait` = mode 2 负片，共 10+10 条），原 `Patch/enchantments/NegativeShaderPatch.cs` 已注释保留。

### 7. 出牌旋转动画（NEW）

- `CardPlayPatch`：`NPlayerHand.StartCardPlay`（拖出开始）、`CardModel.OnPlayWrapper`（打出）、`NCardPlay.Cleanup`（取消）
- `CardPlayTracker`：`AttackTime=0.12s` 快速上升，`ReleaseTime=0.40s` 缓慢淡出，振幅 `RotationAmplitude=15°`，`BaseSpeed=2.0 rad/s`，轴模式 `Time.GetTicksMsec() & 3`
- `PartWrapper` / `FullCardWrapper` 的 `_Process` 中：**出牌旋转优先于 Tilt 倾斜闪烁**

### 8. 附魔特效检查界面（NEW，2026-08-01）

独立于卡牌屏的「查看附魔特效」界面（`NBalatroInspectEnchantScreen`，场景 `balatro_inspect_enchant_screen.tscn`，继承卡牌屏布局 + 覆写根脚本）。左右箭头切换**附魔**（而非卡牌），右侧面板直连 `EnchantmentConfig`（与卡牌编辑器完全解耦）。

- **入口**：卡牌屏 `NBalatroInspectScreen` 的 `%EnchantTickbox`（CardArea 下方，`BAL_VIEW_ENCHANTMENTS`）勾选 → `SwitchToEnchant()` 存上下文（`_cards/_index/_viewAllUpgraded/_originInspect`）→ `Close()`（用 `_suppressOriginReturn` 抑制返回原版）→ 新建附魔屏 `Open(...)`
- **返回链**：附魔屏取消 → **先 `Config.ClearAllEnchantOverlays()` 再重建卡牌屏**；卡牌屏 esc → 原版 inspect
- **附魔列表**（`BuildEnchantList`）：遍历 `ModelDb.DebugEnchantments`，排除 `.Mocks` 命名空间 + 找不到可附魔卡的；随后**稳定排序**（`GetPengoTarotOrderKey`）：
  1. 原版附魔（非 `PengoTarot.` 命名空间）→ 2. 塔罗主牌 0-21（40 个，无命运之轮/高塔，正逆交替）→ 3. 负片(Sub)（恶魔/星星/月亮/太阳/世界，正逆交替，共 10）→ 4. 星球（PlanetDeck 顺序：水金地火木土天海冥 X 谷神星 阋神星，共 12）→ 5. 其他 mod。按类型名匹配（`Tar<Name>...Enchantment` / `Tar<Name>...SubEnchantment` / `Planet<Name>Enchantment`），**Sub 分支必须先于主牌判断**（防 `TarDevil` 误配）
- **方法1 预览选卡**（`FindDisplayCard` → `FindNearestDisplayCard`）：优先卡牌屏序列 `_returnCards`，以 `_returnIndex` 为中心**向前向后交替**找最近 `CanEnchant` 卡（0, +1, -1, +2, -2...）；兜底 `ModelDb.AllCards`
- **安全预览**（`TryShowEnchantment`）：clone + `EnchantInternal(mutableEnch, 1m)` + `IsEnchantmentPreview=true`（绕开 `CardCmd.Enchant` 抛错）→ 单独 try/catch `mutableEnch.ModifyCard()`（Death/Sun 等 `OnEnchant` 依赖战斗上下文在裸克隆上抛 NRE 时仅打日志继续显示）；外层 catch 打印完整堆栈
- **失败即剔除**（`SetEnchantment` → `RemoveEnchantmentAt`）：某附魔无法显示就从 `_enchantments` / `_displayCardCache` 移除，索引指向下一个，保证前后导航顺畅
- **导航 UI（2026-08-02 图标列）**：卡与面板之间纵向 **9 格附魔图标列**（`%EnchantList`，VBoxContainer，槽位在 `InitializeEnchantList` 动态生成，图标用 `EnchantmentModel.Icon`）。**循环滚动**（`WrapIndex` 取模，`SetEnchantment` 不再 Clamp）：选中居中 1.05 放大、全亮，两侧按 `0.8^d` 距离衰减亮/透明；点击任意格跳转（点最下/最上格 = "第 9 个跳到中间"，可循环快速跳转）。原左右箭头**旋转 90° 复用**为上下切换（`LeftArrow` 朝上=上一个、`RightArrow` 朝下=下一个，语义不变 `SetEnchantment(_index∓1)`），置于图标列上下两端；`UpdateArrowVisibility` 改循环（附魔数 >1 始终显示）；卡滑入动画改垂直方向。图标尺寸 55×55（`ListSlotSize`），箭头调小至 88×88 置于列上下端（`pivot_offset=(44,44)`，Icon 在 Godot 里加了 `flip_h` 调方向）；**悬停放大 1.12 并恢复全亮**（`MouseEntered`→`HoverScaleTo`+`Modulate=White`，`MouseExited`→kill tween+`RefreshEnchantList` 恢复距离衰减）；**点击图标与上下箭头反馈一致**：卡白闪+垂直滑动 tween（点上方图标→从 `-120y` 滑入、点下方→`+120y`；箭头为 `±100y`），并统一播放 `ui_click`（`PlayUiClickSfx()` → `SfxCmd.Play("event:/sfx/ui/clicks/ui_click")`，命名空间 `MegaCrit.Sts2.Core.Commands`）。**音效来源真相（2026-08-02 定位）**：`NButton.OnPress()` 点击会自动播放 `ClickedSfx`（`event:/sfx/ui/clicks/ui_click`）；上下箭头是 NButton 所以原生有声，图标列是自定义 `Control`（无 OnPress）需手动播同一音效，箭头不要再手动加音效（否则双重播放）。点当前选中格（`real==_index`）无反馈。注意：tscn 被 Godot 编辑器重写过（uid/内联材质/`index` 属性），改 tscn 前先读当前内容。
- **运行时自动包裹**：`ShaderController.ApplyShader` 调 `Config.SetCardEnchantmentOverlay(cid, card.Model.Enchantment?.Id, card.Model.IsEnchantmentPreview)`，每帧按 cid 合并：运行时卡效果为底、附魔逐字段覆盖；**预览只读附魔配置**（无配置=无效果，保证预览卡干净）
- **名称标签**：tscn `%EnchantNameLabel`（卡上方，只显示附魔名 `mutableEnch.Title.GetFormattedText()`），不显示 hovertip
- **更多选项菜单**（2026-08-02 启用）：附魔屏 `%MenuBtnHolder` + 动态 `PopupMenu`（参考卡牌屏），目前只有「清空当前附魔效果」（`BAL_MENU_CLEAR_ENCHANT`，4 语言）；`OnEnchantMenuAction(0)` → `EnchantmentConfig.ClearEffect(enchId)` + `RefreshPanelState()` + 兜底 `ShaderController.ApplyShader(_card)`；`UpdateEnchantMenuAvailability` 在当前附魔无效果时禁用该项

### 9. 界面文本字体 / 日式异体字（2026-08-06）

检查界面/入口按钮的 mod 文本若用 kreon（无中文字形）→ Godot 系统 fallback 落日文字体显示**日式异体字** + glyph 懒生成卡顿。统一用游戏 `FontManager`（`MegaCrit.Sts2.Core.Localization.Fonts`）按语言替换：
- **共享工具**：`Scripts/Localization/LocaleFontUtil.cs` → `LocaleFontUtil.GetLocaleFont(FontType)`（zhs→Noto 简体 / jpn→Noto CJK JP / kor→韩文；非 CJK 返回 null）
- **两个界面脚本**（`NBalatroInspectScreen` / `NBalatroInspectEnchantScreen`）：
  - `BoldFont`/`RegularFont` 属性改为 `LocaleFontUtil.GetLocaleFont(FontType.X) ?? kreon`
  - `_Ready` 里**先**调 `ApplyLocaleFontsToUi()`（`FindChildren("*","Label"/"RichTextLabel")` 逐个 `ApplyLocaleFontSubstitution`，**必须在 `AddThemeFontOverride(Bold/RegularFont)` 之前**，否则会被覆盖）
  - PopupMenu（`Window` 非 `Control`，无 `ApplyLocaleFontSubstitution`）创建后 `if (LocaleFontUtil.GetLocaleFont(FontType.Regular) is Font f) _menuPopup.AddThemeFontOverride("font", f)`
- **入口按钮**：`InspectScreenButton.AddEntryButton` 里 `btnLabel.ApplyLocaleFontSubstitution(...)`
- ⚠️ **zhs 字体类型语义（2026-08-06 关键）**：`FontType.Bold`→`source_han_serif_sc_bold`（**思源宋体，衬线**）、`FontType.Regular`→`noto_sans_mono_cjksc`（**Noto 黑体，无衬线**）。游戏原版「查看升级」Label 是 `MegaLabel`，其 `RefreshFont` 用 **`FontType.Regular`**（中文=黑体）。所以 mod 的 `%EnchantTickbox`「查看附魔」Label 要跟 **Regular**（`GetLocaleFont(FontType.Regular) ?? BoldFont`）而非 Bold（否则变成宋体，字体不对）；字体替换+黄字 `Color(0.937,0.784,0.318,1)`+黑描边 `outline_size=12`+`font_size=32`+`vertical_alignment=1` 对齐原版（原版 `inspect_card_screen.tscn` `%ShowUpgradeLabel`）
- 与 configFW 面板同一机制（v0.107/v0.110 都有 `FontManager`）

## 常见任务流程

### A. 新增一个效果模式
1. 新建 `balatroeffect/Shaders/pengo_<name>.gdshader`，遵循统一约定（`texture(TEXTURE, UV)` + `global_uv` 计算）
2. 在 `EffectRegistry.Initialize()` 用 `Register(mode, LoadShader(...), "OPTION_<NAME>", aligns: ...)` 注册（用空缺的 mode 7 或 14）
3. 在 `Scripts/Localization/LocExtension.cs` 的 4 个语言字典各加 `"BAL_OPTION_<NAME>": "..."`（键前缀必须是 `BAL_`，`Tr()` 会拼 `BAL_` + key）
4. 若需要强度/倾斜联动，确认 shader 读取 `intensity`、`x_rot`、`y_rot` uniform
5. 重新构建：`dotnet build -c Release /p:Sts2ApiCompat=<版本>`

### B. 新增卡面部件
1. `Config.AllPartNames` 加部件名（注意 `FullCard` 是特殊部件，走 `FullCardEffect` 字段）
2. 确认该部件名在 `TiltWrapper.CardTemplateNames` 白名单内（否则不进 Tilt）
3. 确认 `PartWrapper.TextPartNames`（文本部件如 TitleLabel/DescriptionLabel 会跳过包裹）
4. 检查 `.tscn` 中卡牌模板的节点名与之一致

### C. 调试 UV / 坐标问题
1. 判断当前是哪种路径：inspect（RefPoint=240）还是 Tilt 内（RefPoint=600）还是 FullCard（2.5/-0.75）
2. 检查 `global_uv` 范围：卡片区域应在 0.5→1.5（Body 空间）
3. 确认 shader 未用 `TEXTURE_PIXEL_SIZE` 做卡片空间计算（见陷阱 1）

### D. 排查特效不显示
1. 是否 `Config.IsPerformanceThrottled`？（看游戏日志是否有性能节流触发）
2. 该卡是否在 `NInspectCardScreen` / `NBalatroInspectScreen` 内（走 inspect 路径）
3. `EffectRegistry` 中该 mode 的 shader 是否加载成功（`FileAccess.FileExists` 检查路径，失败会 `PrintErr`）
4. 部件名是否在 `AllPartNames` 且配置已写入（`Config.GetEffect`）
5. 改 `.tscn` 后是否重新导出 PCK；shader 路径是否在 `export_presets.cfg` 的 `exclude_filter`（被排除则运行时由游戏本体提供）
6. **只有插画（Portrait）不显示**：插画效果是直接设 `_portrait.Material`，会被游戏 `NCard.UpdateVisuals` / `NCard.OnReturnedFromPool` 清掉（其他部件是独立 `ShaderPartContainer` 节点不受影响）。确认 `EasePatches` 的两个 Postfix（`NCard_UpdateVisuals_ApplyShader_Patch`、`NCard_OnReturnedFromPool_Reapply`）与 `ShaderController.ScheduleReapply` 轮询存在；插画设置走 `UpdatePortraitInspect`（同时处理 Portrait + AncientPortrait，先古卡才不失效）

### E. 修改附魔检查界面（附魔屏）
1. 附魔列表/排序在 `NBalatroInspectEnchantScreen.BuildEnchantList` + `GetPengoTarotOrderKey`（我们 mod 的附魔按 原版→塔罗40→负片Sub→星球→其他mod 排）
2. 预览选卡在 `FindDisplayCard` / `FindNearestDisplayCard`（以卡牌屏当前索引为中心前后交替找最近可用卡）
3. 面板直连 `EnchantmentConfig`（别接到卡牌 `Config`）；结构变更记得触发 `EnchantmentConfig.Changed`
4. 改了运行时合并逻辑要保证预览隔离（`IsEnchantmentPreview` 时只读附魔配置）

## 常见陷阱（当前有效）

1. **`TEXTURE_PIXEL_SIZE`**：依赖容器尺寸，不应在卡片空间坐标计算中使用。现状：`pengo_vhs`（像素化）、`pengo_outline`（texel_size）、`balatro_effects.gdshader`（Tilt 透视）、`balatro_effects_parts.gdshader` 在用；`pengo_randomstars` 已改用固定 1/480。
2. **`CardTemplateNames` 白名单**：`CreateTilt(fullCard=false)` 只移动白名单节点，`ShaderPartContainer` 不在其中需手动处理。
3. **WrapParts vs WrapPartsInspect**：UV offset 计算不同（RefPoint 600 vs 240）。Body 上部件用 inspect 风格，Tilt 内用 Tilt 风格。
4. **静态状态污染**：`InspectScreen` 静态字段多实例共享，旧实例务必 `QueueFree()`。
5. **Shader 残留旋转**：unhover 销毁 Tilt 后必须清零 `x_rot/y_rot`（`PartWrapper._Process` 已处理：无 tilt 父节点时置 0），否则 `perspective_warp_uv` 会用残留值扭曲。
6. **FullCard UV 映射**：1200 纹理中卡牌占比 UV 0.5→0.9，需 `uv_scale=2.5` 映射回 Body 坐标系。
7. **`pengo_perspective_warp.gdshaderinc`**：目前**只被 include、无实际调用**（已核实）。`balatro_effects_parts.gdshader` 的 `#include` 是遗留物。除非明确需要，新 shader 不要依赖它。
8. **插画（Portrait）shader 材质生命周期**（2026-08-01 定位）：插画效果直接设 `_portrait.Material`（其他部件是独立 `ShaderPartContainer`，游戏不会动它们）。游戏 **`NCard.UpdateVisuals` 每次把 `_portrait.Material` 清为 null**，**`NCard.OnReturnedFromPool`（池化取出）也会清空**，且它不触发 UpdateVisuals 的 Postfix。因此：
   - `EasePatches.NCard_UpdateVisuals_ApplyShader_Patch`（UpdateVisuals Postfix）：每次清后重设
   - `EasePatches.NCard_OnReturnedFromPool_Reapply` + `ShaderController.ScheduleReapply`：轮询等待 Model/Body/插画布局就绪后重应用（解决图鉴/战斗**首次生成**插画不显示；复用后正常是因为 `ReassignToCard → ApplyShader` 是最后一步）
   - `ScheduleReapply` 用 `SceneTree.ProcessFrame` 轮询，Model/Body 未就绪也重试，120 帧上限；正常卡零开销
   - 排查：先看 `_portrait.Material` 是否被清（null），再确认这两个 Postfix / 轮询是否在
   - **原版 inspect 点「查看升级」插画特效丢失**（2026-08-01）：`NInspectCardScreen.UpdateCardDisplay` 会把 `_card.Model` 换成 clone（`NCard.Model` setter 内 `Reload()` 清 `_portrait.Material = null`）。特效恢复原本依赖 `ModelChanged` 与 `UpdateVisuals` Postfix 两条隐式路径，升级预览路径一旦异常/时序错位就丢失且切回也不恢复。修复 = `EasePatches.NInspectCardScreen_UpdateCardDisplay_Reapply`（`NInspectCardScreen.UpdateCardDisplay` Postfix，用 `AccessTools` 取 `_card` 后强制 `ApplyShader`），与自建屏 `SetCard` 末尾的显式兜底一致。若再出现升级后特效丢失，优先确认此补丁在。
9. **附魔预览隔离**：预览模式（`IsEnchantmentPreview`）下 `Config` 读取只返回附魔配置、无配置则“无效果”，**绝不读卡自身**（保证预览卡不被自身特效污染）；运行时才是“卡效果为底、附魔覆盖”。
10. **附魔屏返回清理**：附魔屏关闭必须先 `Config.ClearAllEnchantOverlays()` 再重建卡牌屏，否则 overlay 会残留到同 id 的真实卡。
11. **附魔屏缩进**：`NBalatroInspectScreen.cs` 用**制表符**缩进；`NBalatroInspectEnchantScreen.cs` / `Config.cs` / `EnchantmentConfig.cs` 用 **4 空格**。编辑必须严格匹配（曾因混用损坏文件）。

## 参考

- 本 SKILL 为**权威来源**（已对照代码核实 2026-08-01）
- `balatroeffect/ARCHITECTURE.md` — 总体架构补充阅读，但部分过时（见下）
- `AGENTS.md` — 项目级约定（多版本兼容、构建命令等）

### ARCHITECTURE.md 过时点（核查结论）

1. **缺失整个新子系统**：`Scripts/Patches/`（EasePatches、GlobalDynamicPatch、GetNodeCallRewriter）、`Scripts/CardPlayEffect/`（CardPlayPatch、CardPlayTracker）、`Scripts/Localization/LocExtension.cs`、`Scenes/InspectScreenButton.cs`、`Scenes/balatro_entry_btn.tscn`
2. **`Scripts/InspectScreen.cs` 已不存在**：InspectScreen 类移到 `Scenes/InspectScreenButton.cs`，检查界面改为场景驱动
3. **性能节流机制缺失**：`Config` 的性能安全阀 + `ShaderController` 清理流程
4. **`EffectDef.Aligns` 标志缺失**（影响 shader 对齐判断）
5. **部件列表未给出**：`Config.AllPartNames`（8 个部件）
6. **`perspective_warp_uv` 描述不准确**：inc 文件仍被 include（遗留），但确实无调用；`TEXTURE_PIXEL_SIZE` 的实际使用情况与文档说法有出入（outline 也在用）
7. **Tilt 移除延迟已为 0**（`TiltRemoveDelayMsec = 0`），文档描述的「延迟销毁」已简化
