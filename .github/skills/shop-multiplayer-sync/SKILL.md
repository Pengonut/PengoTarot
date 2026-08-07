---
name: shop-multiplayer-sync
description: '商店多人同步 (Shop Multiplayer Sync)：PengoTarot 塔罗卡包在商人房间（merchant）的多人购买/效果同步机制、相关游戏多人系统知识点，以及「客户端购买塔罗卡包后进下一房间黑屏/掉线」类问题的排查经验。Use when: 修改 Data/merchantroom/ 或 network/ 下的多人购买同步逻辑、排查商人购买相关的多人掉线/黑屏/desync、需要理解 RewardSynchronizer / PlayerChoiceSynchronizer / RunLocationTargetedMessageBuffer / CombatStateSynchronizer / SyncWithSerializedPlayer / PlayerCmd 同步等游戏多人机制。'
argument-hint: '如：排查塔罗卡包购买后客户端黑屏'
user-invocable: true
---

# 商店多人同步（塔罗卡包）

PengoTarot 的塔罗卡包（MerchantTarotEntry）是**商人房间里的自定义可购买项**，通过一套自研消息在多人下同步购买与效果。本文记录其架构、游戏本体多人机制的关键知识点，以及截至 2026-08-02 的排查结论。

> ⚠️ 结论先行：目前**没有定位到 mod 侧的确定 bug**。已把大量候选方向逐一排除，黑屏可能单纯是游戏问题。排查过程与「已排错项」见文末，请勿重复踩坑。

## 何时使用

- 修改 / 排查 `Data/merchantroom/` 下的塔罗卡包购买逻辑（`MerchantTarotEntry` / `NMerchantTarot`）
- 修改 / 排查 `network/` 下的多人同步消息（`TarotSynchronizer` / `TarotPurchaseRequestMessage` / `ForcePlayerSyncMessage`）
- 排查「客户端购买塔罗卡包后进下一房间黑屏 / 下一场战斗掉线」等多人问题
- 需要确认某个游戏多人 API 是否自动同步（`PlayerCmd` / `CardCmd` / `RelicCmd` / `RewardSynchronizer` 等）

## 当前文件布局（已核实 2026-08-02）

| 文件 | 职责 |
|------|------|
| `Data/merchantroom/MerchantTarotEntry.cs` | 塔罗卡包购买逻辑：`OnTryPurchase` 分单/多人；`DoMultiplayerPurchase`（发消息+本端执行）、`ExecuteNetworkedPurchase`（购买方）、`ExecuteNetworkedPurchaseStatic`（远端）、`ExecuteEffectAndEnchant`（附魔/立即效果/`_SUB`）、`NotifyStateChange`（立即效果后全量 force-sync） |
| `Data/merchantroom/NMerchantTarot.cs` | 卡包 UI 槽（点击购买、悬停、开包动画） |
| `Data/merchantroom/TarotEntryHolder.cs` | `MerchantInventory → MerchantTarotEntry` 映射 |
| `Data/merchantroom/PlayerChoiceContext.cs` | `SilentPlayerChoiceContext`（`SignalPlayerChoiceBegun/Ended` 均为空操作，等价于游戏内 `BlockingPlayerChoiceContext`） |
| `network/TarotSynchronizer.cs` | 注册/分发 `TarotPurchaseRequestMessage`、`ForcePlayerSyncMessage` 处理器 |
| `network/TarotPurchaseRequestMessage.cs` | 购买请求（`goldCost` + `cachedDefIds`），`ShouldBuffer=true` |
| `network/ForcePlayerSyncMessage.cs` | 全量玩家状态同步（`SerializablePlayer`），`ShouldBuffer=false` |
| `Patch/merchantroom/MerchantInventory_AllEntries_Patch.cs` | 把塔罗卡包塞进 `MerchantInventory.AllEntries` |
| `Patch/merchantroom/MerchantInventory_CreateForNormalMerchant_Patch.cs` | 商人创建时给每个 `MerchantInventory` 建 `MerchantTarotEntry` |
| `Patch/merchantroom/NMerchantInventory_Initialize_Patch.cs` | 商人 UI 里挂载 `NMerchantTarot` 槽 |

## 多人购买架构（mod 方案 vs 游戏方案）

**游戏本体**（`MerchantCardEntry` / `MerchantRelicEntry` 等）：
- 购买流程**只在购买方机器上执行**，通过 `*Cmd`（`PlayerCmd.LoseGold` / `RelicCmd.Obtain` / `CardPileCmd.Add`）改状态；
- 用 `RunManager.Instance.RewardSynchronizer.SyncLocalGoldLost()` / `SyncLocalObtainedCard()` / `SyncLocalObtainedRelic()` 把"奖励/金币变化"**复制给远端**（用于地图历史与奖励展示，非确定性上下文专用）。

**mod 塔罗卡包**：
- 购买方发 `TarotPurchaseRequestMessage`（`goldCost` + `cachedDefIds`，经 `RunLocationTargetedMessageBuffer` 按位置投递）；
- **两端各自重跑**购买流程：购买方走 `ExecuteNetworkedPurchase`（`isLocalBuyer=true`），远端走 `ExecuteNetworkedPurchaseStatic`（`isLocalBuyer=false`）；
- 选牌通过 `PlayerChoiceSynchronizer` 同步（`FromChooseACardScreen` / `FromDeckForEnchantment` / `FromDeckForTransformation` 均多人感知）；
- 立即效果（命运之轮 / 塔 / `_SUB` 等）只在购买方执行，随后 `NotifyStateChange` 发 `ForcePlayerSyncMessage` 全量同步购买方状态给远端。

## 游戏多人机制关键知识点（参考 v0.109 ≈ v0.110，v0.107 同）

### `PlayerCmd` / `CardCmd` / `RelicCmd` 是同步操作
- `PlayerCmd.LoseGold`、`CardCmd.Enchant`、`CardCmd.Transform`、`RelicCmd.Obtain` 等 `*Cmd` 方法**本身会跨端同步**，不需要手动补消息。商人里额外调 `RewardSynchronizer.*` 主要是为了地图历史/奖励展示，不是状态同步的必要条件。
- 结论：**不要因为"远端没扣钱"就断定金币 desync**。

### `CombatStateSynchronizer`（战斗开始前状态同步）
- `StartSync()`（进战斗房间动画前）：每端广播自己的 `SerializablePlayer`（含金币/牌组/遗物/RNG）；host 额外广播 `SyncRngMessage`（全 RNG + 共享遗物袋）。
- `WaitForSync()`（进战斗前）：远端用收到的数据 `player.SyncWithSerializedPlayer(...)` 覆盖；非 host 再加载 host 的 RNG。
- 含义：**任何商人期间的本地状态/金币/RNG 分歧，都会在下一场战斗开始时被重同步抹平**。所以商人期间的小分歧不会导致"下一场战斗掉线"，除非分歧发生在同步点之后。

### `SyncWithSerializedPlayer`（全量覆盖）
- `Player.SyncWithSerializedPlayer(SerializablePlayer)`：清空并重建 牌组/遗物/药水，覆盖 金币/HP/MaxHP/能量/RNG/Odds/遗物袋/发现表/ExtraFields。要求 `NetId` 与 `CharacterId` 一致，否则抛 `InvalidOperationException`。
- 这是 mod `ForcePlayerSyncMessage` 用的机制，也是游戏战斗同步用的机制。

### `PlayerChoiceSynchronizer`（多人选牌同步）
- 每端本地维护 `_choiceIds[playerSlot]` 计数器。`ReserveChoiceId(player)` 取当前值并 +1。
- 购买方 `SyncLocalChoice` 发 `PlayerChoiceMessage`（带 choiceId + 结果下标）；远端 `WaitForRemoteChoice` 等对应 choiceId。
- **下标语义**：`PlayerChoiceResult.FromIndex(cards.IndexOf(result))` → 远端在自己那份 `cards` 列表按下标取值。**两端 `cards` 列表必须一致**，否则同一下标取到不同卡。RNG 同步时 `DrawThreeUnique` 结果一致，故正常。
- choiceId 计数器是 run state 的一部分，随战斗全量状态（`NetFullCombatState.nextChoiceIds`）与回放（`CombatReplay.choiceIds`）同步/回退。
- 潜在挂起点：若远端 `WaitForRemoteChoice` 等不到对应 choiceId（消息丢失或计数器不一致），任务永久挂起 → 客户端表现可能就是"黑屏卡住"。目前未证实。

### `RunLocationTargetedMessageBuffer`（按地图位置投递）
- 注册 `IRunLocationTargetedMessage` 类型的处理器；消息若目标位置未访问过则**入队**，`OnLocationChanged` 访问到该位置后按接收顺序投递。
- `ShouldBuffer` 属性**不影响**本缓冲（它只影响 `NetMessageBus` 在加载期的全局缓冲）。位置是否访问过才是投递条件。
- 含义：mod 的 `TarotPurchaseRequestMessage` 在购买方与远端都位于商人房间时才会被远端处理，避免跨房间错序。

### `NetMessageBus`（加载期全局缓冲）
- `_isBufferingMessages=true` 且 `message.ShouldBuffer=true` 时先入队，释放时按序补发。`ShouldBuffer=false` 的消息加载期也立即投递。

### `RewardSynchronizer`（商人奖励复制，非确定性上下文）
- 只在商人等非确定性上下文用；处理 `RewardObtainedMessage` / `GoldLostMessage` / `CardRemovedMessage`，战斗进行中先缓存、战斗结束再补发。主要用于把购买结果（获得卡/遗物/药水/金币、失去金币、删牌）同步到远端的地图历史与奖励展示。

### `MerchantRoom` 逐玩家建库存
- `EnterInternal` 对每个玩家 `CreateForNormalMerchant(player)`（含 mod patch 建的 `MerchantTarotEntry`），`GetLocalInventory()` 只取本地玩家那份。**每个玩家有自己的商人/自己的塔罗卡包**；商人状态不跨房间持久化（每次进入重建）。
- 商人内 `CreateForNormalMerchant` 消耗各玩家 `PlayerRng.Shops`（定价/刷牌），两端各自确定性执行 → RNG 保持同步（这是"三张塔罗两端一致"的前提）。

## 已排错项（2026-08-02 调查结论，勿重复排查）

| # | 候选方向 | 结论 |
|---|---------|------|
| 1 | RNG 不同步导致两端三张塔罗不同、下标取到不同卡 | **排除**。RNG 由战斗同步兜底、商人生成确定性消耗；且反馈者引用的"RNG 不同步"是**另一位用户的历史反馈**，与本次黑屏无关、已很久无人反馈 |
| 2 | 三张塔罗"不同"是 bug、应强制两端一致（`cachedDefIds` 恒为 null） | **排除**。三张塔罗随机是正常设计；两端在 RNG 同步下本就一致 |
| 3 | 两端重跑购买流程会 desync | **排除（基本）**。`*Cmd` 自动同步、选择由 `PlayerChoiceSynchronizer` 同步、choiceId 计数两端一致，重跑没有实质问题 |
| 4 | 远端没扣金币 → 金币 desync | **排除**。`PlayerCmd.LoseGold` 本身是同步操作，且 `CombatStateSynchronizer` 会在战斗开始前重同步 |
| 5 | `SilentPlayerChoiceContext` 空操作导致选择未纳入游戏选择跟踪 | **排除**。等价于游戏自带 `BlockingPlayerChoiceContext`（同样是空操作），是合法用法 |
| 6 | `ForcePlayerSyncMessage`（`ShouldBuffer=false`）投递时机错乱 | **排除（未证实）**。`ShouldBuffer` 不影响 `RunLocationTargetedMessageBuffer`，位置访问过即投递；立即效果路径依赖它收敛，未见明显缺陷 |

## 已知 bug 反馈与现状

Steam 创意工坊反馈（2026-08 前后）：
> 打开塔罗卡包（多人）会让其他玩家在下一场战斗开始时掉线；重新读档不会掉线，直到再次开卡包并开战。
> 看起来 bug 对主机玩家已修复，但客户端玩家在购买塔罗卡包并进入下一房间后仍会看到黑屏。
> 主机尝试「保存并退出」后，主菜单只显示「加入」没有「主持」。

用户澄清：
- 「主菜单没有 Host 选项」是**游戏本身的 bug**（切换存档位即可自愈），与 mod 无关，不用排查。
- 黑屏只影响**客户端**（无论谁是主机）；mod 侧未能定位到确定触发点。

**当前结论**：mod 的购买同步架构与游戏机制吻合，已排除 RNG/金币/两端重跑/上下文等方向；剩余未证实怀疑点是**客户端 `WaitForRemoteChoice` 挂起**（选择消息与本地 choiceId 不匹配），但纯代码无法确定触发条件，黑屏可能单纯是游戏问题。

## 排查指引（下次遇到同类问题）

1. **先区分"状态分歧"还是"挂起"**：状态分歧（checksum/desync 报错、掉线）≠ 挂起（黑屏卡住、无报错）。黑屏优先查 `WaitForRemoteChoice` / 异步任务是否未返回。
2. **确认触发范围**：是否与具体塔罗类型有关（普通附魔 / `_SUB` / 立即效果）？黑屏是购买方还是客户端？
3. **看游戏日志**：`c:\Users\Pengo\AppData\Roaming\SlayTheSpire2\logs` 的 `godot*.log`（`PlayerChoiceSynchronizer` / `RunLocationTargetedMessageBuffer` / `TaskHelper.RunSafely` 异常）、`ritsulib_state_divergence_*.zip`（状态分歧归档，RitsuLib 生成）。
   - ⚠️ 这些日志是**本机**的；排查他人反馈时无意义，仅用于自己复现。
4. **复现要点**：需双开/联机真实复现（mod 商人卡包 → 购买 → 进下一房间）；单机/假多人不触发网络路径。
5. **候选修复思路（未实施，仅备忘）**：若确认是 choiceId 不匹配挂起，可考虑让远端在 `WaitForRemoteChoice` 超时/校验失败时优雅退出而非挂死；若确认是消息时序，可对齐 `ShouldBuffer` 与投递顺序。
