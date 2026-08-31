# AGENTS.md — PengoTarot

杀戮尖塔2 (Slay the Spire 2) 的 Mod 项目：添加 44 张塔罗牌 + 40 个附魔。
技术栈：Godot 4.5.1 + C# (net9.0)、Harmony 补丁、支持多版本游戏 API（0.107.0 / 0.111.0）。
代码库使用中文注释/文档，回复与代码注释请保持中文。

## 常用命令

在项目根目录（本目录）运行：

- **单版本构建**：`dotnet build -c Release /p:Sts2ApiCompat=0.107.0`（或 `0.111.0`）
- **完整打包**：`powershell -ExecutionPolicy Bypass -File pack.ps1` → 输出到 `dist/PengoTarot/`
- **工具脚本**：`python tools/<脚本名>`（资产生成/图片转换/UID 修复，详见 `tools/README.md`）

## 多版本兼容机制（核心！）

- `PengoTarot.csproj` 通过 `Sts2ApiCompat` 属性切换目标游戏 API（默认 `0.111.0`）
- 条件编译符号：`STS2_AT_LEAST_0_107_0` / `STS2_AT_LEAST_0_108_0` / `STS2_AT_LEAST_0_110_0` / `STS2_AT_LEAST_0_111_0`
  （由 `build/PengoTarot.CompatDefines.targets` 根据版本自动生成，源码里用 `#if STS2_AT_LEAST_0_110_0` 写版本分支）
- 各版本 DLL 输出到 `.godot/mono/temp/bin/<Configuration>/<Sts2ApiCompat>/`
- `loader/` 是独立子项目（`PengoTarot.Loader.csproj`）：运行时检测宿主版本，用 `AssemblyLoadContext` 加载 `lib/<version>/PengoTarot.dll`
- **改任何 API 调用前**，务必在两个版本的游戏源码中核对签名。参考源码：
  - `d:\[Tool] Godot\STS2v0.107\`（v0.107）
  - `d:\[Tool] Godot\STS2v0.111\`（v0.111）
- **Loader 对版本相关 API 只能用反射**，不能直接引用。`pack.ps1` 会针对最新版本做 dry-run 编译验证，直接引用改名 API 会在构建期失败。

## 目录结构

| 目录 | 作用 |
|---|---|
| `src/Core/Models/` | 卡牌 / 附魔 / Power / 遗物模型（`Cards/` `Enchantments/` `Powers/` `Relics/`） |
| `Patch/` | Harmony 补丁（`card/` `enchantments/` `hover/` `stargazer/` 等按子系统分目录） |
| `balatroeffect/` | 卡牌视觉特效系统（shader + 3D 倾斜）。**改这里前先调用 `balatroeffect` skill**（`.github/skills/balatroeffect/SKILL.md`，已核实最新状态；`balatroeffect/ARCHITECTURE.md` 部分过时） |
| `loader/` | 变体 DLL 加载器（版本检测、脚本注册、序列化缓存重建） |
| `network/` | 多人同步（`TarotSynchronizer`、自定义网络消息） |
| `local/` | 本地化（`LocManager.cs` + 各语言 `LocHelper_<Lang>.cs`） |
| `Data/` | 数据定义（`TarotDef` / `TarotDeck` 等） |
| `tools/` | Python 资产生成脚本，用法见 `tools/README.md` |

## 关键约定与陷阱

- **禁止使用 ZIndex 调整图层（项目级硬约束）**：任何 `.cs`、`.tscn`、shader/视觉特效代码以及运行时创建的节点，都不得通过 `ZIndex`、`ZAsRelative` 或等价的 Z 轴排序属性改变绘制层级；不得把它们与节点顺序混用作“保险”。同一父节点内统一使用场景树的兄弟顺序（如 `MoveChild`）控制前后关系；跨父节点时应调整节点挂载位置或建立合适的专用容器。修改视觉层级后必须搜索改动范围，确认没有新增或改写上述禁用属性。只有确实无法通过节点树结构实现时，才可在说明原因并取得用户明确许可后例外使用。
- **变体 DLL 的 Godot 脚本注册**：变体 DLL 经 `AssemblyLoadContext` 加载后，Godot ScriptManager 不会自动识别其中的 C# 场景类（如 `NBalatroInspectScreen`），`loader/Bootstrap.cs` 通过反射调用 `LookupScriptsInAssembly` 注册。新增/修改 Godot 派生场景脚本后要检查该注册逻辑。
- **序列化缓存**：`loader/ModelIdSerializationCacheRebuildPatch.cs` 确保 Mod 模型 ID 进入 `ModelIdSerializationCache`。
- **csproj 排除项**：`Compile Remove` 排除了 `loader/**`、`src/Core/Nodes/GodotExtensions/**`、`src/Core/Helpers/**`、`src/Core/Assets/**` 及 `balatroeffect/Scripts/ShaderController_new.cs`。这些仅供 .tscn 预览或属于子项目，**不要**让主 DLL 的代码依赖它们。
- **资源打包**：`export_presets.cfg` 的 `exclude_filter` 是控制 PCK 内容的唯一机制（被排除的由游戏本体提供，未被排除的自动打包）。`tools/import_deps.py` / `export_deps.py` 仅管理被排除资源的编辑器预览副本。改 `.tscn` 后需在 Godot 编辑器重新导出 PCK。
- **`.tscn` 禁用 Unicode 特殊字符**（如 ◀▶），Godot 资源解析器不支持，改用 `<>`。
- **本地化**：通过 Harmony 补丁 `LocManager.GetTable` 按语言注入（`zhs` / `jpn` / `kor` / 其他→英语）。新增文案需同步更新各 `LocHelper_<Lang>.cs`。
- **多人同步**：改卡牌/附魔运行逻辑时需评估 `network/` 同步。`pack.ps1` 会在版本号加构建时间戳哈希，防止 DLL 不一致导致联机 desync。
- **Harmony 惯例**：`[HarmonyPatch]` + 静态嵌套类 + 静态 `Prefix` / `Postfix` / `Transpiler`。私有字段用 `Traverse` 访问，无需 Publicizer（参考 `Patch/hover/` 的 RandomForeseer 模式：Prefix+Postfix+ConditionalWeakTable）。
- **Changelog**：完成任何用户可感知的新功能、修复或行为调整后，必须主动更新 `CHANGELOG.json` 的当前未发布版本；纯重构、注释或内部维护且不改变用户体验的改动无需记录。不要等用户提醒，也不要擅自创建新版本号。

## 参考源码（只读，勿改）

- 游戏本体源码：`d:\[Tool] Godot\STS2v0.107\` / `d:\[Tool] Godot\STS2v0.111\`（`src/Core/` 为 C# 源码）
- 依赖框架：`d:\[Download] Edge\STS2-RitsuLib-0.4.62\`（`README.md` 与 `docs/` 有 API 文档）
- 编译用 DLL：`d:\[Tool] Godot\STS2dll\v0.107` / `v0.111`
- 游戏日志：`c:\Users\Pengo\AppData\Roaming\SlayTheSpire2\logs`

## 发布流程
注意，这些内容不需要替用户执行，在必要时提醒用户检查即可。
1. 运行 `pack.ps1`（自动构建 Loader + 两个版本 DLL + 生成 `PengoTarot.json`）
2. 在 Godot 编辑器导出 `PengoTarot.pck`，手动复制到 `dist/PengoTarot/`
