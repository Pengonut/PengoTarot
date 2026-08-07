---
name: configfw
description: 'custom 难度配置系统 (configFW / configfloatingwindow)：PengoTarot 的选人界面难度配置浮动面板、三层数据模型（json 长期偏好 / run 本局真相 / save 存档快照）、存档注入与读档隔离（编辑配置不污染已有存档）、多人配置同步（实时广播 + 读档广播 + LoadRunLobby 注册）、塔罗卡包价格机制（愚者开关/女祭司抵消/教皇降价/价格下限）、入口按钮注入与远程光标层级。Use when: 修改 configFW/ 或 Data/merchantroom/ 或 Patch/merchantroom/ 下与难度配置/塔罗包价格/商店注入相关的逻辑、排查本局配置不生效/不同步/读档被污染/编辑配置影响已有存档、多人配置界面不一致。'
argument-hint: '如：排查商店不出现塔罗包、多人配置不同步、价格逻辑'
user-invocable: true
---

# configFW — custom 难度配置系统（塔罗/星球 + 22 难度开关）

PengoTarot 的 configfloatingwindow 体系：选人界面入口按钮 → 浮动面板，配置本局 custom 难度（左侧塔罗/星球大开关 + 右侧 3 列 7/8/7 共 22 个难度开关）。选人界面可编辑，游戏过程只读。多人下仅主机可改、客机跟随。

> ⚠️ 修改本体系**先读本 skill + 关键文件**，不要凭旧认知改：多人同步、快照时机、价格机制都经过多轮修正，`AGENTS.md` 目录结构里 `configFW/` 的说明可能滞后。

## 何时使用

- 修改 `configFW/`（面板脚本、门面、patch、场景、本地化）
- 修改 `Data/merchantroom/MerchantTarotEntry.cs`（塔罗包价格/抽取）或 `Patch/merchantroom/`（商店注入）
- 排查：商店不出现塔罗包、本局配置不生效/不同步、读档被旧配置污染、多人配置界面不一致
- **实现 22 个难度/占卜功能开关**（新增战斗/地图/商店/事件效果、需要局内可写且跨存档保留的计数）——先读下面「开发指引」两节

## 当前文件布局（已核实 2026-08-04，piggyback 已于 08-03 移除）

| 文件 | 职责 |
|------|------|
| `configFW/Scripts/ConfigFloatingWindowConfig.cs` | 全局 JSON 配置模型（**默认值**，改动即写 `user://mod_configs/PengoTarot/ConfigFloatingWindow.json`；静态构造懒加载一次，改 JSON 需重启） |
| `configFW/Scripts/ConfigFloatingWindowRunData.cs` | **run 层·本局真相**（静态类）：`GetTarFlag`、`SnapshotFromDefaults/Reset/Apply/FromJson/AdjustTarotPrice`、**`SaveConfig`（本局存档配置快照，不可变）+ `SetSaveConfigFromRunData`**；存档 JSON 拆 `cfg`（不可变配置）/`run`（poff、markers 可变量）；静态构造按 json 初始化；静态默认 `_flags` 前 6 项 true |
| `configFW/Scripts/ConfigFloatingWindow.cs` | 静态门面：入口注入、面板开关、`BroadcastConfig()`、`AttachPanelAboveCursor()`（光标层级）、多人消息注册 |
| `configFW/Scripts/NConfigFloatingWindow.cs` | 面板脚本：`Bind*`（一律读 RunData）、`RefreshFromRunData()`、`AddHover`、`AnimateRightButtons`、`Open(remote, editable)` |
| `configFW/Scripts/NConfigFloatingWindowEntryButton.cs` | 入口按钮（纯图片、可拖动、`ApplySavedState`） |
| `configFW/Scripts/ConfigFloatingWindowDataMessage.cs` | 配置分发消息（INetMessage，主机→客机，`FromRunData/ApplyToRunData`） |
| `configFW/Scripts/ConfigFloatingWindowStateMessage.cs` | 面板开关同步消息（isOpen） |
| `configFW/Scripts/ConfigFloatingWindowLoc.cs` | 本地化注入（gameplay_ui 表，`BAL_CFW_*`，4 语言）；已并入 `local/LocManager.cs` 的 `LocManagerGetTablePatch` 首次 GetTable 自动注入（无需各处显式调 `Inject()`）；标记占卜动态描述（`BuildSettingsDescription`/`BuildMapDescriptionKey`） |
| `configFW/Scripts/NConfigFloatingWindow.cs` | 面板本体：flag 底部 hint 显示 `FlagHintText`（标记占卜动态描述）+ `ToLabelBbcode`（把 `[gold]` 等游戏色标转 RichTextLabel 可渲染的 `[color=#...]`）；**`_hintLabel` 是 RichTextLabel**（Godot 4.5 的 Label 已移除 `BbcodeEnabled`，普通 Label 无法解析色标，tscn 里 HintLabel 已改 type="RichTextLabel" + `bbcode_enabled=true`，字体主题项用 `normal_font`）；**CenterContainer 坑**：`HintCenter` 是 CenterContainer，其布局对子控件取 `minimum_size`（RichTextLabel 默认最小宽 0）→ 文本被挤成竖直一条线，**必须给 HintLabel 设 `size_flags_horizontal=3`（ExpandFill，tscn + 代码双设）** 才分配整行宽度；`fit_content` 高度自适应 + CenterContainer 垂直居中不变。不引用游戏 `MegaRichTextLabel`（避免 DLL 依赖），`[gold]` 走 ToLabelBbcode 转换；**中文字体/卡顿（2026-08-06）**：kreon_bold 无中文字形 → Godot 系统 fallback 落日文字体显示异体字 + glyph 懒生成致首次 hover 卡顿；用 `FontManager`（`MegaCrit.Sts2.Core.Localization.Fonts`）`GetSubstituteFont(lang, FontType)` 按语言替换（zhs→Noto 简体，v0.107/0.110 都有）；启动时 `ScheduleHintFontsWarmUp()`（透明标签挂 tree.Root 逐帧绘制全部文本，`normal_font_size` 与 hint 一致 24，glyph 按字体+字号缓存）；⚠️ **Steam Deck 启动崩溃（2026-08-06）**：预热原在 `Initialize()` 同步栈 `BuildWarmUpChunks()` 查 `BAL_CFW_*`（`new LocString().GetFormattedText()`），本地化表若在 PatchAll 前已加载缓存 → 键缺失抛 `LocException` 无 try/catch → mod 初始化崩溃（Windows 时序侥幸，Linux/SteamOS 触发，与语言无关）。**修复**：`ScheduleHintFontsWarmUp` 只订阅 ProcessFrame、首帧才构建文本；`BuildWarmUpChunks` 前置 `ConfigFloatingWindowLoc.Inject()` + 用 `LocString.GetIfExists`（缺失返回 null）；预热全路径 try/catch 静默终止（空队列退出）——预热是纯优化，绝不能影响启动；**SettingsToggle 点击/hover 范围（2026-08-06）**：只允许点「文本 Label」或「勾选框 TickIcon」区域内切换/放大（`GetLocalMousePosition()` + `GetRect().HasPoint` 判断，避免整行空白误触；hover 放大由 TickIcon/Label 各自 `MouseFilter=Pass` + `MouseEntered/Exited` 实现，与点击范围一致）；文案 `BAL_CFW_SETTINGS_TOGGLE` =「仅在游戏设置界面显示入口悬浮窗…」4 语言；**左下角 TIPS（2026-08-06）**：`ConfigFloatingWindowLoc` 提供 `GetMainTips/GetThanksTips(lang)` 随机池（**仅中文**有内容，其余语言空数组=不显示，可扩展）；`NConfigFloatingWindow.BindTipLabel()` 每次打开 `_configOpensThisLaunch++`，≥`ThanksUnlockOpens(3)` 次后「感谢」并入池（每次启动清零）；`Random.Shared.Next(total)` 抽选，文本 `TIPS：xxx`，**最近 10 条不重复**（`_recentTips` 列表剔除，池不足时回退全池）；tscn 左下角 `TipLabel`（anchor 左 0~0.6、底 1，autowrap_mode=3 超屏自动换行）；**CenterPanel 与 SettingsToggle 整体上移 0.0467 屏高**（`anchor_top 0.166667→0.12` / `0.833333→0.786667`）给 TIPS 腾出底部空间 |
| `configFW/Patches/CharacterSelectEntryPatch.cs` | 选人界面入口注入（`NCharacterSelectScreen.OnSubmenuOpened/Closed`） |
| `configFW/Patches/NRunEntryPatch.cs` | 游戏过程入口（`NRun._Ready`） |
| `configFW/Patches/RunSaveInjectPatch.cs` | 生命周期：开局快照+固定 `SaveConfig`、存档注入 `_pengotarot_cfw`（cfg 不可变/run 可变）、读档提取、`SetUpSaved*` Prefix 按 save 重写 run、开局/读档广播 |
| `configFW/Patches/MultiplayerLoadLobbySyncPatch.cs` | 多人读档：`LoadRunLobby` 构造/清理时注册/注销配置消息 handler（客机收主机读档广播） |
| `configFW/Scenes/*.tscn` | `configfloatingwindow.tscn` + `configfloatingwindow_entry_btn.tscn` |

## 三层数据模型（json / run / save，2026-08-04 重构）

- **json**（`ConfigFloatingWindowConfig`）= **长期偏好**（tarot/planet/flags/pmin/pmax/ShowInSettingsOnly）。首次启用用默认；**只被「首次启用」和「配置界面修改」覆写**。读档/多人分发/局内运行**不碰 json**。poff/markers **不在 json**。
- **run**（`ConfigFloatingWindowRunData` 静态）= **本局真相**。所有配置开关的游戏内容**只读 run**（已审计：商店/战斗/占卜/遗物 patch 全部读 RunData，无人直接读 json）。run 变动原因：启动从 json 初始化（静态构造）、UI 改（json+run）、选人界面开局按 json 重写、读档按 save 重写、客机按主机消息重写。
- **save**（run 存档的 `_pengotarot_cfw`）= 读档时**覆写 run**。配置部分（`cfg`）**只在创建存档时初始化、此后不可变**；`run` 部分（poff 价格偏移、markers 占卜标记）是**可变量**，随局内变化持久化（读档恢复）。
- 面板 Bind*/RefreshFromRunData 一律显示 run（**绝不显示 json**，历史教训）。

## 开发指引：接入 run 数据 & 存档可写数值（实现 22 个开关必读）

### A. 游戏逻辑读配置：只读 run，绝不读 json

实现任一难度/占卜开关时，战斗/地图/商店/事件逻辑一律这样读：

```csharp
// 难度开关：索引见 FlagNames（0 愚者 ~ 21 世界）
bool enabled = ConfigFloatingWindowRunData.GetTarFlag(index);

// 左侧大开关
bool tarotOn = ConfigFloatingWindowRunData.TarotEnabled;
bool planetOn = ConfigFloatingWindowRunData.PlanetEnabled;

// 价格（塔罗包）
int min = ConfigFloatingWindowRunData.TarotPriceMin;
int max = ConfigFloatingWindowRunData.TarotPriceMax;
int off = ConfigFloatingWindowRunData.TarotPriceOffset;
```

硬性规则：
1. **游戏逻辑绝不读 `ConfigFloatingWindowConfig`（json）**——那是长期偏好/默认值来源，只被开局快照与配置界面写入。
2. **局外隔离**：patch 判断开关时加 `RunManager.Instance.IsInProgress` 守卫（参考 `Patch/card/AscendersBaneTowerPatch.cs` 的 `ShouldApply()`），避免主菜单/图鉴等非跑局场景误读。
3. **总开关门控**：`GetTarFlag(i)` 在 `TarotEnabled==false` 时恒为 false（右侧全部失效）。若某开关设计上应独立于塔罗总开关，需自行判断 `TarotEnabled`（一般不需要）。
4. **FlagNames 索引固定**（`NConfigFloatingWindow.FlagNames` 顺序）：0 愚者、1 魔术师、2 女祭司、3 皇后、4 皇帝、5 教皇、6 恋人、7 战车、8 力量、9 隐者、10 命运之轮、11 正义、12 倒吊人、13 死神、14 节制、15 恶魔、16 高塔、17 星星、18 月亮、19 太阳、20 审判、21 世界。新增效果前先确认用的索引正确。

### B. 需要「局内可写、跨存档/继续保留」的数值 → 放 save 的 run 部分

先分清三种存放位置：

| 数值类型 | 存放 | 说明 |
|---|---|---|
| 玩家长期偏好（开关是否开启） | json（Config） | 配置界面编辑；只进 `cfg` 快照 |
| 本局固定配置 | run 的 cfg 部分 | 开局按 json 快照后不可变；进存档 `cfg` |
| **局内可写、需持久化的状态**（如价格偏移、占卜标记进度） | **run 的 run 部分** | 每次写盘实时保存、读档恢复；**不进 json、不进 cfg** |

新增一个「可写数值」的完整步骤（以 poff 为模板，参考 `ConfigFloatingWindowRunData`）：

```csharp
// 1) 字段 + 读取 + 修改方法
private static int _tarotPriceOffset;                       // 已有模板
public static int TarotPriceOffset => _tarotPriceOffset;
public static void AdjustTarotPrice(int delta) => _tarotPriceOffset += delta;

// 2) 新局重置：SnapshotFromDefaults / Reset 里清零或设初值
_tarotPriceOffset = 0;

// 3) ToJson 的 run 分支写入（cfg 分支不动——cfg 不可变）
["run"] = new JsonObject
{
    ["poff"] = _tarotPriceOffset,
    ["markers"] = TarotMarkerSystem.ToJson(),
    // 新增：["你的key"] = _你的字段,
};

// 4) FromJson 的 run 分支读回
if (obj["run"] is JsonObject run)
{
    _tarotPriceOffset = TryGet(run["poff"], 0);
    TarotMarkerSystem.FromJson(run["markers"] as JsonObject);
    // 新增：_你的字段 = TryGet(run["你的key"], 默认值);
}
```

> ⚠️ 规则：
> - **只改 run 部分**；不要动 `cfg`（SaveConfig 不可变，动它会破坏「编辑配置不污染存档」）。
> - **不要放进 json**（Config 类）。
> - **多人注意**：`ConfigFloatingWindowDataMessage` 只携带配置（tarot/planet/pmin/pmax/flags），**不携带 run 可变量**。若该可变量必须两端一致，要么确定性生成（如 markers 用固定种子 RNG，两端自洽），要么走游戏自带多人同步（如购买→RewardSynchronizer/PlayerCmd）。**不要**为了同步往配置消息里塞局内状态。

### C. 实现一个新占卜/难度开关的检查清单

1. **索引确认**：在 `NConfigFloatingWindow.FlagNames` 里找对应索引；本地化键 `BAL_CFW_FLAG_<NAME>_DESC`（全大写）在 `ConfigFloatingWindowLoc.cs` 补 4 语言。
2. **读开关**：游戏逻辑读 `GetTarFlag(index)`，按需加 `RunManager.Instance.IsInProgress` 守卫。
3. **跨存档状态**：若该占卜有「进度/标记/计数」要跨存档保留 → 走 B 节 run 部分模式。
4. **子系统联动**：若影响商店/价格/地图 → 先读对应子系统已有模式（`merchantroom` / `divination` / `Patch/card` 的 Tower 示例）与 skill。

## 快照 / 生命周期（三层模型最终约定）

- **启动**：run 静态构造 `SnapshotFromDefaults()` 从 json 初始化（用户偏好）。
- **开局（新局）**：`OnCharacterSelectOpened`（主机/单机）快照 + 广播；`SetUpNewSingleplayer`/`SetUpNewMultiplayer` Postfix **按 json 重写 run + `SetSaveConfigFromRunData()`**（固定本局存档配置快照，此后 `cfg` 不可变）。`OnEmbarkPressed` 不重快照（只广播）。
- **读档（继续）**：`SetUpSavedSingleplayer`/`SetUpSavedMultiplayer` **Prefix** 无条件 `ExtractFromSave`（按 save 重写 run + 重设 SaveConfig）——这是**隔离「配置界面编辑污染已有存档」的核心**；单机无条件，多人仅主机提取（客机等主机广播）。
- **写盘**：`WriteFile/WriteFileAsync`（GodotFileIo + CloudSaveStore）patch 注入 `_pengotarot_cfw = {v, cfg: SaveConfig, run:{poff, markers}}`；`SaveConfig==null`（未开局/未读档）跳过。`UnmappedMemberHandling=Skip` → 卸载 mod 后字段被忽略，不导致读档失败。
- **读档提取**：`ExtractFromSave` 读 `_pengotarot_cfw` → `FromJson`（新格式 cfg/run 拆分 + **旧扁平格式兼容**）；无字段（旧局）→ `Reset()` 内置默认；随后 `SetSaveConfigFromRunData()`。

## 价格机制（2026-08-04 最终）

- 基础价：`TarotPriceMin/Max`（175~200，Config 默认）。
- 构造：`_cost = rng.NextInt(baseMin, baseMax+1) + TarotPriceOffset + (教皇 GetTarFlag(5) ? -100 : 0)`（最低价 175+0-100=75，天然不为负，无下限 clamp）
- 购买后 `AdjustPriceAfterPurchase`：**塔罗包默认 +50（无条件，不绑愚者）** + 女祭司 `GetTarFlag(2)` 时 -50 抵消 → 默认配置下价格不变；女祭司关则每次 +50 涨价。
- **愚者 `GetTarFlag(0)` 只控制「商店是否出现塔罗包」**（`Patch/merchantroom/` 5 个 patch 判断），不参与价格。
- 其他难度索引（`FlagNames` 顺序）：0 愚者、1 魔术师（抽取 +2）、2 女祭司（-50 抵消）、3 皇后（逆位）、4 皇帝（角色专属）、5 教皇（-100）。
- 价格偏移 `TarotPriceOffset`：新局 `SnapshotFromDefaults` 与多人 `Apply` 都归零（防跨局污染）；offset 只会 ≥0（默认+50，女祭司-50 抵消），价格不会为负，无需下限。
- **poff/markers 存在 save 的 `run` 部分（可变量）**，随局内变化写盘、读档恢复；**不在 json**。

## 多人同步（可靠广播，无 piggyback）

- **实时广播**：选人界面编辑 → `BroadcastConfig()` → `ConfigFloatingWindowDataMessage.FromRunData()`；客机 `OnDataMessage` → `ApplyToRunData()` + `_openPanel.RefreshFromRunData()`（实时刷新已开面板）。主机 `OpenPanel` 时 SendState + BroadcastConfig 双发。
- **5 条可靠广播路径**（`ConfigFloatingWindowDataMessage`，Reliable 保序，最终一致）：
  1. 选人界面打开（主机/单机）`OnCharacterSelectOpened` → `BroadcastConfig()` 广播初始值；
  2. 选人界面编辑实时 `BroadcastConfig()`；
  3. embark（`RunSaveInjectPatch.CharacterSelectEmbarkPatch` Postfix，主机）广播最终值；
  4. `RunSaveInjectPatch.SetUpNewMultiplayerPatch` Postfix（主机）广播兜底；
  5. **读档**：`RunSaveInjectPatch.SetUpSavedMultiplayerPatch` Postfix（主机）广播存档配置。
  已移除原「开局 piggyback」（`LobbyBeginRunPiggybackPatch`，08-03 删除）。
- **多人读档客机注册**：`MultiplayerLoadLobbySyncPatch` patch `LoadRunLobby` 主构造（客户端构造链式调用主构造，一个 Postfix 覆盖主机+客机）→ `RegisterSyncForLoadLobby` 注册配置 handler；`CleanUp` → 注销。客机因此能收到主机读档广播并覆盖本机 RunData。
- **消息注册**：**切勿**在 `ModInitializer.Initialize()` 里调 `MessageTypes.Initialize()`（ModManager 未完成会抛异常且中断 PatchAll）。正确机制是 loader 的 `ReflectionHelperModTypesPatch` 注入变体类型，游戏 `ExecuteEssential` 自动注册。

## 只读（客机/局内）三处关键

1. `Open(editable=false)` → 所有按钮 `Disable()`；`NClickableControl` 点击检查 `_isEnabled`，Disabled 后 Released 不发。
2. `PlayRightButtonsShow`：**只在 `_editable` 时 `Enable()`**（否则动画回调会覆盖 Open 的 Disable，导致客机可点击切换）。
3. `AddHover.MouseEntered`：**只在 `_editable` 时恢复白色**（只读 hover 保持压暗，避免「像点亮」的误导）。

## 远程光标层级

- 面板挂 `NGame.Instance` 下，用 `AttachPanelAboveCursor()` 把面板 `MoveChild` 到 `RemoteCursorContainer` 之前 → 光标靠树序自然在面板上层。
- **不要移动光标容器本身**（RemoveChild 触发 `_ExitTree → Deinitialize` 会 Dispose 共享 `PeerInputSynchronizer`、破坏原生光标逻辑）；**不要直接改 z_index**。

## 关键踩坑（勿重复）

- `[Tool]` 方括号路径在 PowerShell 是通配符，需 `Set-Location -LiteralPath`。
- 用户文件可能在会话间隙被手动改过，**编辑前先读文件确认实际内容**；multi_replace 的 oldString 必须匹配实际文本。
- RunData 静态默认 `_flags` 前 6 项必须为 true（与 Config 默认对齐），否则未走快照路径下 `GetTarFlag(0)` 误判为关 → 商店塔罗包消失。
- `Reset()` 的 `_planetEnabled` 必须为 `false`（曾误设为 true，与 Config/静态默认不一致）。
- 本地化键 `BAL_CFW_FLAG_<NAME>_DESC` 全大写；`FlagNames` 是 PascalCase，匹配用 `ToUpperInvariant()`。
- `NuGetAudit` 已在 `PengoTarot.csproj` 关闭（`<NuGetAudit>false</NuGetAudit>`），避免离线构建 NU1900。
- **存档格式（2026-08-04 重构）**：`_pengotarot_cfw` 现为 `{v, cfg:{tarot,planet,pmin,pmax,flags}, run:{poff,markers}}`；`FromJson` 兼容旧扁平格式。**配置编辑只改 json+run，不碰 `SaveConfig`** → 不会污染已有存档。
- **「编辑配置污染已有存档」根因与修复**：根因是配置面板编辑写静态 RunData，而「继续游戏」不重新提取 + 写盘注入把污染写回存档。修复 = `SetUpSavedSingleplayer/Multiplayer` Prefix 无条件 `ExtractFromSave` + `TryInjectRunData` 只注入 `SaveConfig`（cfg 不可变）。
- **`SetUpSavedMultiplayer` 主机/客机都会调用**（`BeginRun` 两端触发）→ Prefix 必须加 `lobby.NetService.Type == Host` 守卫，否则客机会从本地（可能缺失/过期）mp 存档提取。
- **CS0136 作用域坑**：`FromJson` 新旧格式分支里若用同名 `arr` 变量会作用域冲突，旧格式的已改名 `legacyFlags`。
- **⚠️ Harmony patch 构造函数必须显式 `MethodType.Constructor`**（Harmony 2.4.2）：`[HarmonyPatch(typeof(X), ".ctor", new Type[]{...})]` 走 MethodType.Normal，Harmony 不会把 `.ctor` 字符串转成构造函数 → PatchAll 抛 `Undefined target method`，**整个 mod 初始化失败**（日志 `Failed to initialize PengoTarot: ... HarmonyException`，之后 mod 无任何 patch 生效）。正确写法：`[HarmonyPatch(typeof(X), MethodType.Constructor, new Type[]{...})]`（参考 RitsuLib 的 PatchTarget 用 `MethodType.Constructor`）。

## 标记类占卜 + 塔罗奖励（2026-08-06 已全实现，详见 divination skill）
- 标记类 6 个占卜（战车7/力量8/隐者9/正义11/倒吊人12/死神13）的战斗效果 + 塔罗奖励已实现，代码在 `Data/divination/` + `Patch/divination/` + `Data/tarotcard/TarotReward.cs`；恋人=精英标记放大器、命运之轮=移除标记
- **塔罗奖励发放**：打完被标记房间 → `Hook.AfterCombatVictory`（`AfterCombatVictoryTarotRewardPatch`）→ `TarotMarkerSystem.OnMarkedCombatVictory` → `CombatRoom.AddExtraReward(player, new TarotReward(player, flag))`
- **`TarotReward : Reward`**：动态 `RewardType=0x4000_0000+flagIndex`（高位区间自研，不依赖 RitsuLib）；读档重建 patch `Reward.FromSerializable`（Prefix return false 跳过原版 default 抛异常）；点击三选一（来源占卜正位+逆位，availabilityCheck 过滤，可跳过，全无则命运之轮正兜底）
- **完成/失效**：精英类（战车/力量/隐者）完成 2 个 → 失效 + 发 1 次塔罗奖励；普通类（正义/倒吊人/死神）每完成 2 个 → 发 1 次（`MarkState.RewardsAwarded` 防重复，持久化到 markers）
- **地图图标角标数字**显示完成进度：`TarotMarkerSystem.GetProgressForDisplay(flag)`（精英类累计、普通类 `%RewardInterval`）
- 改这些逻辑前先读 `.github/skills/divination/SKILL.md`
