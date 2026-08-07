---
name: divination
description: '占卜标记系统 + 地图标记视觉 + 全部 6 个标记占卜战斗效果 + 塔罗奖励（divination）：PengoTarot 的「占卜」难度开关体系。含 TarotMarkerSystem（地图节点标记：配置/ApplyMarkers/查询/计数/失效/跨幕保留/序列化）、NMapPointMarkerIconPatch（节点视觉：soul_beam、sparkle、旋转图标、数字角标、逆位图标切换）、TarotReward 自定义奖励栏、以及战车/力量/隐者/正义/倒吊人/死神的战斗 Power/Affliction。Use when: 改 Data/divination/ 或 Patch/divination/ 下的标记/战斗效果/塔罗奖励逻辑、调地图节点特效/角标/逆图、排查塔罗奖励发放与读档重建。'
argument-hint: '如：调战车首次易伤、改正义每回合第一张消耗、数字角标位置、塔罗奖励三选一/跳过、失效跨幕保留'
user-invocable: true
---

# divination — 占卜标记系统 + 战斗效果 + 塔罗奖励

PengoTarot 的「占卜」难度开关体系：在地图上**标记房间**（TarotMarkerSystem）→ 节点显示视觉特效（数字角标/逆位图标）→ 进入被标记房间的战斗触发**战斗效果**（Power/Affliction）→ 打完被标记房间**发放塔罗奖励**（TarotReward）。全部 6 个标记占卜（战车/力量/隐者/正义/倒吊人/死神）已实现；恋人=精英标记放大器、命运之轮=已移除标记。

> ⚠️ 修改本体系先读本 skill + 关键文件；图层/加载/缩放/塔罗奖励序列化都有踩坑。

## 何时使用

- 修改 `Data/divination/TarotMarkerSystem.cs`、`Patch/divination/`、`Data/tarotcard/TarotReward.cs`
- 调各占卜战斗效果、塔罗奖励、地图节点视觉/角标/逆图
- 排查：塔罗奖励不发放/读档重建失败、标记跨幕不保留、数字角标不刷新、失效不切逆图

## 文件布局

| 文件 | 职责 |
|------|------|
| `Data/divination/TarotMarkerSystem.cs` | 标记核心：`Configs` 配置、`MarkState{ActIndex,Coords,CompletedCount,RewardsAwarded,Expired}`、`ApplyMarkers`、查询接口、`OnMarkedCombatVictory` 发放、序列化 |
| `Patch/divination/DivinationMarkerPatch.cs` | `Hook.ModifyGeneratedMapLate` Postfix → 每幕地图生成后应用标记 |
| `Patch/divination/NMapPointMarkerIconPatch.cs` | 节点视觉：sparkle + 连接线 + 旋转图标 + **数字角标** + **失效切逆图** + 总览 |
| `Patch/divination/EliteDivinationPowerPatch.cs` | `Hook.BeforeCombatStart` → 战车/力量/隐者给**敌人**挂 Power |
| `Data/divination/EliteDivinationSharedState.cs` | 战车/力量**房间级共享计数**（CWT 按 ICombatState，战斗结束回收） |
| `Patch/divination/NormalDivinationPowerPatch.cs` | `Hook.BeforeCombatStart` → 正义/倒吊人/死神给**玩家**挂 Power |
| `Patch/divination/AfterCombatVictoryTarotRewardPatch.cs` | `Hook.AfterCombatVictory` Postfix → `OnMarkedCombatVictory` 发放塔罗奖励 |
| `Patch/divination/RewardFromSerializableTarotPatch.cs` | `Reward.FromSerializable` Prefix 重建 TarotReward（读档） |
| `src/Core/Models/Powers/Tar*ReversedPower.cs` | 战车/力量/正义/倒吊人/死神的战斗 Power |
| `src/Core/Models/Afflictions/Tar*ReversedAffliction.cs` | 正义/倒吊人/死神的侵蚀标记（纯标记，无逻辑） |
| `Data/tarotcard/TarotReward.cs` | 自定义奖励栏类型（动态 RewardType + 三选一） |
| `Data/tarotcard/TarotEffectExecutor.cs` | 塔罗效果执行公共工具（商店与奖励共用） |
| `Patch/enchantments/HandCardHolder_DivinationIconPatch.cs` | 手牌右上角侵蚀逆图标 + `NHandCardHolder.get_ShouldGlowRed` 红提示 |
| `Patch/enchantments/PowerIconPath_Patch.cs` | Power 图标映射（逆塔罗附魔图标） |

## 标记系统（TarotMarkerSystem）

- **配置表**（`Configs`）：`MarkerConfig(FlagIndex, TargetType, CountPerAct, MinRow, MarkAll)`：
  - 恋人(6)=全部精英（**放大器**，见下）、战车(7)/力量(8)/隐者(9)=每幕1精英、正义(11)/倒吊人(12)=每幕3普通(**row≥5**，2026-08-06 从 3 改 5)、死神(13)=每幕3普通；**命运之轮(10) 已从 Configs 移除**（暂不标记）
- **恋人放大器**：`LoversFlagIndex=6`；`PickCoords` 里开启时精英类（战车/力量/隐者）标记改为 MarkAll（所有精英）。恋人在 Configs（MarkAll 精英，提供图标），但 `OnMarkedCombatVictory` **跳过恋人**（无战斗效果/不计数/不发奖励）
- **跨幕保留**：`ApplyMarkers` 只检查开关 `GetTarFlag`（**不管失效**）→ 失效的占卜仍每幕标记（供逆图显示）；失效的标记不计数/不发奖励/不触发效果（那些路径走 `GetMarkedFlagsAt`/`IsFlagEnabled`，含失效检查）
- `ApplyMarkers`：确定性伪随机 `Rng(actIndex*1000+flag*17)`（同幕各端一致）；已是本幕且坐标有效则保留（读档恢复）
- **查询接口**：
  - `GetMarkedFlagsAt(coord)`（未失效，战斗效果判定用）/ `GetDisplayedFlagsAt(coord)`（含失效，图标显示用）
  - `GetCompletedCount(flag)` / `GetProgressForDisplay(flag)`（**角标数字**：精英类累计、普通类 `%RewardInterval`）
  - `GetMarkerIconPath` / `GetReversedMarkerIconPath`（正位路径 `_upright_`→`_reversed_`）
  - `IsFlagEnabled`（= GetTarFlag && !Expired）/ `IsExpired` / `GetMarkedCoords` / `RecordCompletion` / `Expire`
- **发放**：`OnMarkedCombatVictory(coord, room, players)` → 对每个标记占卜 `CompletedCount++` → 精英类完成 `RewardInterval`(2) 个发 1 次 + `Expire`；普通类每完成 `RewardInterval` 个发 1 次（`RewardsAwarded` 防重复，持久化）。`RewardInterval=2` 常量（发放与角标共用）
- **持久化**：markers 并入 `_pengotarot_cfw.markers`（含 `RewardsAwarded`）
  - **防御性补标（2026-08-06）**：`TarotMarkerSystem.TryRemarker(runState)` + 新文件 `Patch/divination/AfterCombatRemarkerPatch.cs`（挂 `Hook.AfterCombatVictory` Postfix，与塔罗奖励同触发点）——某些读档类 mod 破坏性代码会让回档后标记全消失；每次战斗胜利后检查：**有开启标记占卜但当前幕无任何标记坐标** → 调 `ApplyMarkers` 补标。幂等（坐标都在时无副作用、正常游玩零开销）；失效占卜也算「开启」以补逆图显示。`ApplyMarkers` 本身幂等：`st.ActIndex==actIndex && st.Coords.All(map.HasPoint)` 时保留、否则重新 PickCoords
## 塔罗奖励（TarotReward）

- **`TarotReward : Reward`**（`Data/tarotcard/`）：动态 `RewardType = 0x4000_0000 + flagIndex`（高位区间避开原版枚举，**借鉴 RitsuLib DynamicEnumValueMinter 思路但自研**，不依赖 RitsuLib）；`IconPath=GetMarkerIconPath(flag)`；`Description` 键 `BAL_CFW_TAROT_REWARD_DESC`；构造函数调 `ConfigFloatingWindowLoc.Inject()`（防未开面板时描述键缺失）
- **点击 `OnSelect`**：`CollectDefs`（来源占卜的**正位+逆位**，`availabilityCheck` 过滤，全无则 `WHEEL_OF_FORTUNE_UPRIGHT` **强制兜底**）→ `CardSelectCmd.FromChooseACardScreen(canSkip:true)` 三选一（**可跳过**）→ `TarotEffectExecutor.ExecuteEffectAndEnchant`
- **发放触发**：`Hook.AfterCombatVictory` Postfix → `runState.CurrentMapCoord` → `TarotMarkerSystem.OnMarkedCombatVictory` → `CombatRoom.AddExtraReward(player, new TarotReward(player, flagIndex))`
- **读档重建**：`Reward.FromSerializable` Prefix 识别动态 RewardType → 反查 flagIndex → `new TarotReward(player, flag)`，`return false` 跳过原版 switch（未知类型会抛 NotImplementedException）
- **`TarotEffectExecutor`**：从 `MerchantTarotEntry` 提取的 `ExecuteEffectAndEnchant`/`NotifyStateChange` 公共静态，商店与奖励共用
- 奖励的塔罗 = **来源占卜**的正位+逆位（战车→CHARIOT_UPRIGHT/REVERSED），不是随机全池

## 战斗效果（6 个标记占卜，已实现）

| 占卜 | 挂载 | 效果 |
|------|------|------|
| 战车(7) | 精英敌人 `TarChariotReversedPower` | 敌人对玩家造成**首次**未格挡伤害 → 玩家获得 `VulnerablePower` 1 层（**房间级共享**：所有敌人共享每玩家一次，全玩家触发后移除所有敌人身上 power） |
| 力量(8) | 精英敌人 `TarStrengthReversedPower` | 同上 → `WeakPower` 1 层（房间级共享） |
| 隐者(9) | 敌人 `PlatingPower`（游戏自带，无自定义 power） | Amount=MaxHp×10%，自带第 1 回合格挡（**青蛙骑士同款**） |
| 正义(11) | 玩家 `TarJusticeReversedPower` | **每回合第一张**攻击牌打出后 `CardCmd.Exhaust`；触发后清除其余攻击牌侵蚀（图标消失），`AfterPlayerTurnStart` 重置+重新标记 |
| 倒吊人(12) | 玩家 `TarHangedManReversedPower` | 同上，技能牌 |
| 死神(13) | 玩家 `TarDeathReversedPower` | 打出能力牌 → `PlayerCmd.EndTurn(canBackOut:false)`（参照 VoidForm） |

- **Power 通用**：`Type=Buff`（精英，对敌方正面）/`Debuff`（玩家）、`StackType=Single`（不可堆叠）、`ExtraHoverTips` 额外显示关键词（战车/力量→Vulnerable/Weak，正义/倒吊人→Exhaust）
- **战车/力量房间级共享**（`Data/divination/EliteDivinationSharedState.cs`）：所有敌人共享每玩家一次的触发计数（`ConditionalWeakTable<ICombatState, HashSet<ulong>>`，战斗结束自动回收）；某玩家已触发后其他敌人再打不再触发；**所有玩家都触发过后** → 遍历 `combat.Enemies` 逐个 `PowerCmd.Remove<T>` 移除敌人身上的该 power
- **挂载**：两个 `BeforeCombatStart` patch（`EliteDivinationPowerPatch` 敌人、`NormalDivinationPowerPatch` 玩家），`ThrowingPlayerChoiceContext` + `async void`（防死锁）；`GetMarkedFlagsAt(coord)` 判断（未失效才生效）
- **侵蚀 Affliction**（正义/倒吊人/死神）：`Tar*ReversedAffliction` **纯标记**（`CanAfflictCardType` 限制类型、无逻辑、**不提供 overlay** → 卡牌 UI 走默认 overlay，无缺特效报错）；效果逻辑在 Power（AfterCardPlayed）
- **手牌视觉**：`HandCardHolder_DivinationIconPatch` 在右上角显示正义/倒吊人/死神**逆附魔图标**（`AfflictionChanged`→`Flash` 刷新）；死神标记卡牌 `NHandCardHolder.get_ShouldGlowRed` → 打出提示变红（替换默认蓝色 `playableColor`）

## 节点视觉（NMapPointMarkerIconPatch）— 踩坑大全

### 图层（低→高）
1. **soul_beam 连接线**（节点 `IconContainer` 内置底）
2. **节点图标**（地图点 Icon）
3. **恋人图标**（`IconContainer` 内、Icon 之上，被容器连带缩放）
4. **外侧旋转图标**（全局图标层）
5. **数字角标**（图标右下角，白字+黑描边）
6. **sparkle 金光**（节点内、Icon 之上，常驻）

### 关键实现与踩坑
- **全局图标层**：惰性创建挂 `TheMap/Points` 之后（`NMapScreen.Instance.GetNodeOrNull("TheMap")`，`MoveChild(layer, points.GetIndex()+1)`）→ 高于所有节点、随地图滚动、低于 NGame UI。节点坐标用 `GetGlobalRect().GetCenter()`；**节点 `_ExitTree` 必须清理全局层图标**（防换幕孤儿）
- **加载游戏本体场景必须 `GD.Load<PackedScene>`，不用 `PreloadManager.Cache.GetScene`**（Cache 只缓存启动预加载，非预加载返回 null，特效全不显示）
- **sparkle**（`card_sparkles_vfx.tscn`）：必须 `LocalCoords=true`（否则粒子世界空间模拟，地图滚动滞留）
- **soul_beam**（`kin_priest_beam_vfx.tscn`）：不调 `Fire()`，自己控制 `BeamHolder.Visible/Scale` + `StaticParticles`；**必须反向补偿 `%IconContainer` 的原版呼吸缩放**（`NNormalMapPoint._Process` 里 `Sin*0.25+1.2`）→ `vfx.Scale = 1/container.Scale`，否则光束随节点放大/缩小
- **图层置底用 `MoveChild(vfx,0)`，不用 `ZIndex=-1`**（同父级 ZIndex 均 0，靠置入顺序）
- **图标缩放用 `Sprite2D`**（pivot 默认中心），`TextureRect.PivotOffset` 在全局层不可靠
- **总览模式**：`NConfigFloatingWindowEntryButton` 的 `BeginDrag()` → `SetOverview(true)`；`DockIn()` → `SetOverview(false)`（不要以贴边状态切换触发）
- **数字角标**：`OrbitItem.Badge`（Label，白字 + 黑描边 `outline_size` 5、`font_size` 17、**无底框**、右下角 `Position(IconSize-18, IconSize-22)`）；`OnProcess` 每帧检查 `GetProgressForDisplay` 变化刷新 + 失效时 `ApplyReversedTexture` 切逆图并隐藏角标；**与图标绑死同中心缩放**：`PivotOffset = 图标中心 − Position`（`new Vector2(IconSize/2,IconSize/2) - label.Position`，注意 PivotOffset 是相对角标左上角、必须补偿 Position 才等于图标中心）+ `Badge.Scale=iconScale*hov` 跟随 Sprite，绕图标中心同节奏放大/缩小
- **失效切逆图**：`ApplyReversedTexture`（`GetReversedMarkerIconPath`）；失效的占卜跨幕仍显示逆图（`GetDisplayedFlagsAt` + `ApplyMarkers` 不管失效）；`OrbitItems` 已从元组重构为 `OrbitItem` 类（含 Badge/LastShown/WasExpired）
- **恋人图标**：固定左下角、不加连接线、不显示角标（不参与计数）、**大小为其他图标 0.8 倍**（`LoversScale=0.8f`）；**作为 `%IconContainer` 子节点**（`Position` 用容器局部坐标在 OnReady 设好：容器中心 OrbitSize/2 + 左下偏移 - IconSize/2）；**hitbox 恒定**：`li.Icon.Scale = 1/container.Scale` 反向补偿容器呼吸 → 命中区恒为 IconSize，不随节点放大/缩小；**呼吸 C 与 hover H（%Icon 1→1.45）都集中到 sprite**：`li.Sprite.Scale = (IconSize/IconBaseTexSize)*iconScale*hov*LoversScale*c*h`（c=容器呼吸、h=%Icon hover），视觉照常缩放、锚点=恋人自身中心（`PivotOffset=(IconSize/2,IconSize/2)`）
- 旋转/残留常量：`OrbitSpeed=0.48`、`InitialPhase=3π/4`、`MinVisible=0.06`、`IconSize=48`

### 可调常量（NMapPointMarkerIconPatch 顶部）
`OrbitSize`、`IconSize`(48)、`OrbitRadius`(76.8)、`OrbitSpeed`(0.48)、`InitialPhase`、`SparkleScale/SparkleAlpha`、`PopDuration`、`LoversOffset`、`LoversFlag`(6)、`MinVisible`、`BeamLengthScale`(0.2)/`BeamThinScale`(0.16)/`BeamAngleOffset`(-π)、角标 `font_size`(17)/`outline_size`(5)/`Position`

## 本地化

- **powers 表**（`local/LocHelper*.cs` 4 语言）：`TAR_<NAME>_REVERSED_POWER` 的 title/description/smartDescription（战车/力量=「首次」、正义/倒吊人=「每回合第一张被消耗」、死神=「能力牌结束回合」）
- **afflictions 表**：`TAR_<NAME>_REVERSED_AFFLICTION` 的 title/description/extraCardText（死神 extraCardText 对齐 VoidForm「结束你的回合。」）
- **开关描述**（`configFW/Scripts/ConfigFloatingWindowLoc.cs` `BAL_CFW_FLAG_*_DESC` 4 语言）：恋人=所有精英敌人房间共享塔罗标记（已去「开启后」与具体标记名）、关键词 gold（易伤/虚弱/覆甲/格挡/消耗/能力牌/结束你的回合/塔罗包/Tarot packs/タロットパック/타로 팩）、**所有阿拉伯数字 `[blue]`**（含 `{Count}` 占位符、`10%`、`175~200` 等；中文数词「一次」、日文「一回」、韩文「한 번」等「获得一次」类量词用文字不标）、全句号、保留「开发中」标记；普通类已删「（前3层不出现）」
- **动态 hovertip 文本（2026-08-06）**：`ConfigFloatingWindowLoc` 的 `IsMarkedDivination`(7/8/9/11/12/13) / `IsEliteDivination`(7/8/9) / `BuildSettingsDescription`(设置界面底部 hint：标记类游戏内追加 `BAL_CFW_PROGRESS_LINE`「当前已完成{Count}。」或精英已完成≥`TarotMarkerSystem.RewardInterval`(2) 时换 `BAL_CFW_EXPIRED_LINE`「已失效。」，非游戏状态 `!RunManager.Instance.IsInProgress` 不显示动态行) / `BuildMapDescriptionKey`(地图 hovertip：精英三态 `BAL_CFW_MAP_<NAME>_0/_1/_EXP` 按 `GetCompletedCount` 0/1/≥2、普通两态 `_0/_1` 按 `GetProgressForDisplay`(重置计数 %2))；接入点：`NConfigFloatingWindow.AddHover`→`FlagHintText`、`NMapPointMarkerIconPatch.ShowIconTip`；动态值用 LocString `{Count}` 占位符 + `Add("Count", n)` 每次 hover 重新求值（游戏原版机制：HoverTip.Description 是固定 string，动态 = 重建 tip，无每帧刷新）
- **地图 hovertip 词条 tip（2026-08-06）**：`NMapPointMarkerIconPatch.ExtraTipsForFlag` 在动态描述后追加词条 hovertip 堆叠显示——战车→`HoverTipFactory.FromPower<VulnerablePower>()`、力量→`WeakPower`、隐者→`PlatingPower`、正义/倒吊人→`FromKeyword(CardKeyword.Exhaust)`；多 tip 用 `CreateAndShow(icon, IEnumerable<IHoverTip>)` + `TipsWithExtras`；地图奖励句统一「完成下一场战斗后」（`BAL_CFW_MAP_*_1`，4 语言）
- **配置面板 hint 的 bbcode（2026-08-06）**：`NConfigFloatingWindow._hintLabel` 是普通 `Label`，不解析游戏 `[gold]` 色标 → 设 `BbcodeEnabled=true` + `ToLabelBbcode` 把 `[gold]→[color=#EFC851]`/`[red]→#FF5555`/`[blue]→#87CEEB`/`[purple]→#EE82EE`/`[green]→#7FFF00` 转标准 bbcode（色值取自 StsColors）
- **本地化注入链（2026-08-06 统一）**：`ConfigFloatingWindowLoc.Inject()` 已并入 `local/LocManager.cs` 的 `LocManagerGetTablePatch`（patch `LocManager.GetTable` Postfix，首次 GetTable 时与 `TarLocHelper.*InjectAll` + `LocExtension.Inject` 一起自动注入，`_injected` 防递归/防重复）；各处**不再需要**显式调 `Inject()`（幂等 `_injected` 保留作防御）；读取用 `LocString.GetIfExists`（缺 key 返回 null 不抛异常）

## 多版本兼容坑

- `CardSelectCmd` 在 `MegaCrit.Sts2.Core.Commands`（勿漏 using）
- `LocalContext` 在两版本都在 `MegaCrit.Sts2.Core.Context`（不是 GameActions.Multiplayer）
- `CardModel.AddKeyword/RemoveKeyword` 两版本存在；`AfterPlayerTurnStart(PlayerChoiceContext, Player)` 存在
- 改跨版本代码核对 `STS2v0.107`/`STS2v0.110` 源码

## 已完成 / 待办

- ✅ 6 个标记占卜战斗效果 + 塔罗奖励 + 数字角标 + 失效逆图 + 跨幕保留 + 恋人放大器 + 命运之轮移除
- ⏳ 非标记占卜（恶魔 15/星星 17/月亮 18/太阳 19/审判 20/世界 21）仍为「开发中」未实现
- ⏳ 塔罗奖励多人发放深度：目前按「拥有者执行 + ForceSync」（基础版），RewardsSetSynchronizer 深度集成后续迭代

