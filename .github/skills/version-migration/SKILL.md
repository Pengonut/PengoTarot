---
name: version-migration
description: 'PengoTarot 的游戏版本升级/迁移流程。游戏分正式版 + beta 版两个版本，本 mod 目标 = 同时支持「正式版 + 最新 beta 版」；几乎只做 beta 版升级，少数时候 beta 版直接合并入正式版（版本号相同）。当新 beta 版发布时：用新旧版本解包源码 +（可选）社区更新总结做 API 差异排查（改名/移除/签名变更是否影响 mod）、改版本号字段（build/PengoTarot.CompatDefines.targets 支持版本列表与 STS2_AT_LEAST_* 条件编译符号、pack.ps1 $Versions/$VersionDllRoots、loader/Bootstrap.cs KnownVersions、loader/PengoTarot.Loader.csproj Sts2DllRoot）、必要时写 #if 分支或反射、重新编译验证（单版本构建 + pack.ps1 完整打包 + Loader dry-run）。Use when: 游戏出新版本要升级 PengoTarot 支持、新增/删除支持的游戏 API 版本、改支持版本列表/条件编译符号、排查某个游戏版本下编译失败或功能失效、需要写 STS2_AT_LEAST_* 条件编译分支。'
argument-hint: '如：游戏出了 0.111.0 新 beta，帮我升级支持；把支持版本从 0.110.0 切到 0.111.0；新版本下某个功能失效帮我查版本差异'
user-invocable: true
---

# version-migration — PengoTarot 游戏版本升级/迁移

把 PengoTarot 从当前支持的游戏版本升级到新的游戏版本（主要是新 beta 版）。**绝大部分工作只是改版本号字段 + 重新编译**，很少需要深度 bug 排查。

## 何时使用

- 游戏发布了新的 beta 版（或 beta 合并进正式版），需要让 PengoTarot 支持新版本
- 新增 / 删除支持的游戏 API 版本
- 排查某个游戏版本下编译失败 / 功能失效（多为 API 变更导致）
- 需要在源码里写 `STS2_AT_LEAST_*` 条件编译分支

## 版本策略（重要）

- 游戏分**正式版 + beta 版**两个版本；PengoTarot 只需同时支持「**正式版 + 最新 beta 版**」。
- **几乎总是只做 beta 版升级**（正式版基本不动）。
- **新 beta 发布后，旧 beta 变体被替换**：支持列表 = 正式版 + 新 beta，旧 beta 的条目与变体一并移除（不累积）。
- 少数时候 beta 版会**直接合并入正式版**，此时正式版与 beta 版版本号相同 → **保留双版本编译的基本框架，但字面上只支持 1 个版本**（`_PengoTarotSupportedApiVersions` 等列表只写一个版本号即可；框架/loader 仍按多版本结构组织，只是当前只有一个变体）。
- 用户会提供**旧版本 + 新版本的解包源码**（放在 `d:\[Tool] Godot\` 下），有时附带**社区总结的更新内容**。
- 大前提：**绝大部分问题只需重新编译即可解决**；只有 mod 实际用到的 API 发生 改名/移除/签名变更 时才需要改代码。

## 当前支持状态（写本 skill 时的基线，随升级更新）

| 版本 | 角色 | 参考源码 | 编译 DLL |
|---|---|---|---|
| 0.107.0 | 正式版 | `d:\[Tool] Godot\STS2v0.107\` | `d:\[Tool] Godot\STS2dll\v0.107\`（sts2.dll / 0Harmony.dll / GodotSharp.dll） |
| 0.111.0 | 最新 beta 版 | `d:\[Tool] Godot\STS2v0.111\` | `d:\[Tool] Godot\STS2dll\v0.111\`（同上） |

> 注：`STS2dll\` 下还有历史遗留 `v0.109\`、`v0.110\`（旧 beta，约等于 v0.110 API），**不参与**当前支持列表。`STS2v0.110\` 源码仍在 `d:\[Tool] Godot\` 下可作参考。

## 迁移案例：0.110.0 → 0.111.0（2026-08-14，一次通过）

- 社区总结 `sts2110_vs_sts2111_api_diff.md` 列出的破坏性变更（`CardCmd.Exhaust` 返回 `Task<CardPileAddResult?>`、`ModManager.Copy*` 加 `IModManagerFileIo` 参数、`CombatManager` 若干 public→internal、网络握手重构、VFX 命名空间迁移等）**全部不影响 mod**——grep 确认 mod 源码未引用这些 API，或用法兼容（如 `await CardCmd.Exhaust(...)` 忽略返回值）。
- 实际只需改版本号字段 + 重新编译，`pack.ps1` 一次通过（Loader dry-run + 0.107.0 + 0.111.0 全部成功）。
- 结论：**再次验证「绝大部分问题只需重新编译即可解决」**；迁移前先对照社区总结 grep mod 源码确认受影响面，可避免无谓改动。

## 版本号字段清单（改版本必须同步的全部位置）

升级支持版本时，**以下 5 处都要改**，漏一处会导致打包/Loader 找不到变体或构建被拦。

> 默认策略是**替换**：新 beta 发布 → 从这些列表里**移除旧 beta、加入新 beta**（最终 = 正式版 + 新 beta）。
> 合并场景（beta 合并入正式版、版本号相同）：列表**只保留一个字面版本**，但保留双版本编译框架（loader 的 `lib/<ver>/` 结构、`KnownVersions` 数组等仍按多版本组织，只是当前只有一个变体）。

1. **`build/PengoTarot.CompatDefines.targets`** — 支持版本列表 + 条件编译符号
   - `_PengoTarotSupportedApiVersions="0.107.0;0.110.0"`（分号分隔；替换后变 `0.107.0;0.111.0`）
   - `_PengoTarotSupportedApiList="|0.107.0|0.110.0|"`（带竖线，用于校验；同步替换）
   - `DefineConstants` 增加新版条件：`<DefineConstants Condition="$([MSBuild]::VersionGreaterThanOrEquals('$(Sts2ApiCompat)', '0.111.0'))">$(DefineConstants);STS2_AT_LEAST_0_111_0</DefineConstants>`（**旧符号如 `STS2_AT_LEAST_0_110_0` 保留**——新版本 ≥ 旧版本时仍然生效，`#if STS2_AT_LEAST_0_110_0` 分支对 0.111 照样编译）
   - 校验 Target `ValidatePengoTarotCompatTarget` 自动读上面的列表，不在列表内的 `Sts2ApiCompat` 会编译报错

2. **`pack.ps1`** — 打包版本列表 + DLL 路径映射
   - `$Versions = @("0.107.0", "0.110.0")`（最后一个 = 最新，Loader dry-run 用它；替换后变 `@("0.107.0", "0.111.0")`）
   - `$VersionDllRoots` 哈希表：**删旧 beta 映射、加新版本** → DLL 目录（**无 patch 后缀**，如 `D:\[Tool] Godot\STS2dll\v0.111`）

3. **`loader/Bootstrap.cs`** — Loader 运行时扫描的变体版本
   - `private static readonly string[] KnownVersions = ["0.110.0", "0.107.0"];`（**最新在前**；替换后变 `["0.111.0", "0.107.0"]`；只按此列表扫描 `lib/<ver>/PengoTarot.dll`，无 JSON manifest）

4. **`PengoTarot.csproj`** — 默认构建目标（`<Sts2ApiCompat>` 默认 `0.110.0` → `0.111.0`）。**必须改**：默认值若还指向已移除的旧 beta，`pack.ps1`/IDE 无参构建会被 targets 校验拦截。

5. **`ModInitializer.cs`** — `CompatVersion` 日志常量的 `#if` 链新增新版本分支（`STS2_AT_LEAST_0_111_0` → `"0.111.0"`，旧分支保留为 `#elif`）。仅影响初始化日志文案。

另外：`loader/PengoTarot.Loader.csproj` 的 `<Sts2DllRoot>` 保持**最旧支持版本**（正式版 v0.107），**一般不动**；仅当正式版本身升级时才改。Loader 只对版本相关 API 用反射，不能直接引用改名 API（否则构建期失败）。

## 迁移流程

### 1. 准备新版本源码与 DLL
- 用户解包新版本源码 → 确认放在 `d:\[Tool] Godot\STS2v<新版本>\`
- 确认编译用 DLL 齐全：`d:\[Tool] Godot\STS2dll\v<新版本>\` 下要有 `sts2.dll`、`0Harmony.dll`、`GodotSharp.dll`（从新版本本体提取）

### 2. API 差异排查
- 对比**旧 beta 源码 vs 新 beta 源码**（`STS2v<旧>\src` vs `STS2v<新>\src`），或对照社区更新总结。
- 重点：**mod 实际引用过的 API** 是否 改名 / 移除 / 签名变更。参考 `AGENTS.md` 的「改任何 API 调用前，务必在两个版本的游戏源码中核对签名」。
- 判定：
  - 编译能过 → 无需改代码，直接跳步骤 5。
  - 编译报错 / 功能失效 → 定位到具体 API 差异，按步骤 4 处理。

### 3. 改版本号字段
- 按「版本号字段清单」改上面 4 处（targets / pack.ps1 / Bootstrap.cs；loader csproj 一般不动）。
- **默认 = 替换旧 beta**：4 处列表都是「删旧 beta、加新 beta」，最终支持列表 = 正式版 + 新 beta。
- **合并场景**（beta 合并入正式版、版本号相同）：列表只保留 1 个字面版本，但保留双版本编译框架（`lib/<ver>/` 目录、`KnownVersions` 结构不删）。
- 若新 beta 与旧 beta 之间有中间版本号（如旧 0.108 → 新 0.110），`DefineConstants` 里对应的 `STS2_AT_LEAST_0_1xx_0` 也需补齐（编译符号由 targets 的 `VersionGreaterThanOrEquals` 链自动推导，只需确保每个阈值版本有对应行）。

### 4. 处理 API 差异（仅当步骤 2 发现变更时）
- 源码内用 `#if STS2_AT_LEAST_0_<新>_0` 写版本分支（新 API 走新分支，旧 API 走 `#else`）。
- Loader 侧对版本相关 API 一律**反射**调用，不直接引用（见 `Bootstrap.cs` 里 `RegisterVariantAssembly` 的 `Mod.assembly → Mod.assemblies` 改名先例：v0.110 改名，Loader 用 `GetField` 反射兼容）。
- 排查某版本功能失效时：先在 `c:\Users\Pengo\AppData\Roaming\SlayTheSpire2\logs` 看游戏日志。

### 5. 重新编译 + 打包验证
- 单版本构建（逐个目标版本）：
  - `dotnet build -c Release /p:Sts2ApiCompat=0.107.0`
  - `dotnet build -c Release /p:Sts2ApiCompat=<新beta>`
- 完整打包（Loader 构建 + **对最新版 dry-run 验证** + 各版本 DLL + 生成 `dist/PengoTarot/` + `PengoTarot.json`）：
  - `powershell -ExecutionPolicy Bypass -File pack.ps1`
- **Loader dry-run 是硬校验**：Loader 编译于最旧版但必须跑在最新版上，若直接引用了改名 API 会在这一步构建期失败 → 按报错改用反射。
- 打包后注意把 `PengoTarot.pck` 手动复制进 `dist/PengoTarot/`（`.tscn` 改动后需在 Godot 编辑器重新导出 PCK）。

### 6. 游戏内验证
- **正式版 + 新 beta 版都要测**：进游戏跑一局，重点验证涉及新版本 API 的功能（附魔/卡牌/Power/占卜标记/商店塔罗包等）。
- 检查 `ModInitializer` 初始化日志无异常；多人场景验证 DLL 版本一致性（`pack.ps1` 会加时间戳哈希防 desync）。

## 关键坑

- **版本号字段 4 处必须同步**：targets 支持列表 / pack.ps1 `$Versions`+`$VersionDllRoots` / loader `KnownVersions`。漏掉 loader 的 `KnownVersions` 会导致运行时不扫描新变体；漏掉 targets 会导致构建该校验版本时直接报错。
- **DLL 目录名无 patch 后缀**：`0.110.0` → `v0.110`（`$VersionDllRoots` 与 csproj 的 `_PengoTarotCompatFolder` 都用 `Version.ToString(2)` 去掉 patch 段）。
- **Loader 只反射版本相关 API**：`Bootstrap.cs` 已处理 `Mod.assembly→Mod.assemblies` 改名先例；新增版本若再有 API 改名，Loader 侧照此模式加反射分支。
- **`KnownVersions` 顺序 = 最新在前**：`PickVariant` 会排序后选「<= 宿主版本的最高变体」，未知宿主版本时回退最新变体。
- **别动 `_PengoTarotSupportedApiVersions` 之外的格式**：`_SupportedApiList` 的 `|x|y|` 竖线格式是 `Contains` 校验用的，保持 `|0.107.0|0.110.0|` 风格。
- **PowerShell 5.1 中文/路径坑**：所有命令在 `D:\PengoTarot`（junction）下跑；`D:\[Tool] Godot\` 含方括号，PowerShell 会当通配符，走 pack.ps1 内部路径映射（`-LiteralPath` / csproj 直接写盘符）即可，终端里别用裸 `Set-Location` 进含 `[` 的路径。
