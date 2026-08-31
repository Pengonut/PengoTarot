#nullable enable

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// configfloatingwindow 的「本局配置」生命周期 patch：
    /// - 新局（单机/多人）开始时把 JSON 默认值快照进运行时配置；
    /// - 多人开局由主机把配置广播给客户端；
    /// - 保存 run 存档时把配置注入 SerializableRun 的 JSON（字段 _pengotarot_cfw）；
    /// - 加载 run 存档时从 JSON 提取配置恢复运行时配置。
    /// 采用 RitsuLib 的思路但大幅简化：不依赖任何第三方库。
    /// </summary>
    public static class RunSaveInjectPatch
    {
        // ── 新局快照 ────────────────────────────────────────────
        [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewSingleplayer))]
        public static class SetUpNewSingleplayerPatch
        {
            public static void Postfix()
            {
                // 新单机局：按全局 JSON 重写本局配置，并固定「本局存档配置快照」。
                // 此后该局的 _pengotarot_cfw.cfg 不再因外部编辑而变（存档配置不可变）。
                ConfigFloatingWindowRunData.SnapshotFromDefaults();
                ConfigFloatingWindowRunData.SetSaveConfigFromRunData();
            }
        }

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewMultiplayer))]
        public static class SetUpNewMultiplayerPatch
        {
            public static void Postfix(StartRunLobby lobby)
            {
                // 本局配置已在选人界面打开时从默认值快照（ConfigFloatingWindow.OnCharacterSelectOpened），
                // 此处不再重快照（避免覆盖主机最后一次编辑）；固定本局存档配置快照后，把当前配置广播给客户端。
                // 客户端不本地快照（等主机广播），消息可靠保序，进入游戏前必收到。
                ConfigFloatingWindowRunData.SetSaveConfigFromRunData();
                if (lobby.NetService.Type == NetGameType.Host)
                    lobby.NetService.SendMessage(ConfigFloatingWindowDataMessage.FromRunData());
            }
        }

        /// <summary>
        /// 主机在 embark（点开始游戏）时广播本局配置，让客户端在进入游戏前拿到最终值。
        /// 注意：不在此重快照（选人界面打开时已快照，编辑实时更新 RunData），
        /// 直接广播当前 RunData 才能保证「最后一次主机修改」被同步。
        /// </summary>
        [HarmonyPatch(typeof(NCharacterSelectScreen), "OnEmbarkPressed")]
        public static class CharacterSelectEmbarkPatch
        {
            public static void Postfix(NCharacterSelectScreen __instance)
            {
                var lobby = __instance.Lobby;
                if (lobby?.NetService == null) return;
                if (lobby.NetService.Type == NetGameType.Host)
                    lobby.NetService.SendMessage(ConfigFloatingWindowDataMessage.FromRunData());
            }
        }

        // ── 读档（继续游戏）：按 save 重写 run，隔离外部编辑污染 ──
        /// <summary>
        /// 继续单机局：在 RunManager.SetUpSavedSingleplayer 重写盘（IncrementNumReloads）之前，
        /// 无条件从存档重新提取本局配置。这样选人/设置界面编辑造成的 RunData 污染不会带进已有存档，
        /// 也不会被写回存档（存档配置始终以存档为准）。
        /// </summary>
        [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpSavedSingleplayer))]
        public static class SetUpSavedSingleplayerPatch
        {
            public static void Prefix()
            {
                var manager = GetRunSaveManager(SaveManager.Instance);
                if (manager != null)
                    ExtractFromSave(manager, true, RunSaveManager.runSaveFileName);
            }
        }

        /// <summary>
        /// 继续多人局：仅主机按自己的多人存档重新提取配置（防止外部编辑污染），
        /// 并在读档后把本局配置广播给客机（客机通过 LoadRunLobby 创建时已注册 handler）。
        /// 客机不本地提取（本地 mp 存档可能缺失/过期），只等主机广播覆盖本机 RunData。
        /// </summary>
        [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpSavedMultiplayer))]
        public static class SetUpSavedMultiplayerPatch
        {
            public static void Prefix(LoadRunLobby lobby)
            {
                if (lobby.NetService.Type != NetGameType.Host) return;
                var manager = GetRunSaveManager(SaveManager.Instance);
                if (manager != null)
                    ExtractFromSave(manager, true, RunSaveManager.multiplayerRunSaveFileName);
            }

            public static void Postfix(LoadRunLobby lobby)
            {
                if (lobby.NetService.Type != NetGameType.Host) return;
                lobby.NetService.SendMessage(ConfigFloatingWindowDataMessage.FromRunData());
            }
        }

        // ── 保存：写入 run 存档时注入配置 JSON ─────────────────
        [HarmonyPatch(typeof(GodotFileIo), nameof(GodotFileIo.WriteFile), typeof(string), typeof(byte[]))]
        public static class WriteFilePatch
        {
            public static void Prefix(string path, ref byte[] bytes)
            {
                bytes = TryInjectRunData(path, bytes);
            }
        }

        [HarmonyPatch(typeof(GodotFileIo), nameof(GodotFileIo.WriteFileAsync), typeof(string), typeof(byte[]))]
        public static class WriteFileAsyncPatch
        {
            public static void Prefix(string path, ref byte[] bytes)
            {
                bytes = TryInjectRunData(path, bytes);
            }
        }

        // 云存档（CloudSaveStore）也注入：LocalStore 与 CloudStore 都写注入后的字节
        [HarmonyPatch(typeof(CloudSaveStore), nameof(CloudSaveStore.WriteFile), typeof(string), typeof(byte[]))]
        public static class CloudSaveStoreWriteFilePatch
        {
            public static void Prefix(string path, ref byte[] bytes)
            {
                bytes = TryInjectRunData(path, bytes);
            }
        }

        [HarmonyPatch(typeof(CloudSaveStore), nameof(CloudSaveStore.WriteFileAsync), typeof(string), typeof(byte[]))]
        public static class CloudSaveStoreWriteFileAsyncPatch
        {
            public static void Prefix(string path, ref byte[] bytes)
            {
                bytes = TryInjectRunData(path, bytes);
            }
        }

        // ── 加载：读取 run 存档后提取配置 ───────────────────────
        [HarmonyPatch(typeof(RunSaveManager), nameof(RunSaveManager.LoadRunSave))]
        public static class LoadRunSavePatch
        {
            public static void Postfix(RunSaveManager __instance, ReadSaveResult<SerializableRun> __result)
            {
                // LoadRunSave 也可能被继续游戏以外的查询路径调用。局内运行数据已是最新真相，
                // 不得再用磁盘上一次保存的旧快照覆盖；真正继续游戏由上面的
                // SetUpSavedSingleplayer Prefix 显式调用 ExtractFromSave。
                if (RunManager.Instance.IsInProgress) return;
                ExtractFromSave(__instance, __result.Success && __result.SaveData != null, RunSaveManager.runSaveFileName);
            }
        }

        [HarmonyPatch(typeof(RunSaveManager), nameof(RunSaveManager.LoadMultiplayerRunSave))]
        public static class LoadMultiplayerRunSavePatch
        {
            public static void Postfix(RunSaveManager __instance, ReadSaveResult<SerializableRun> __result)
            {
                // 同单机：真正多人继续游戏由 SetUpSavedMultiplayer Prefix 恢复。
                if (RunManager.Instance.IsInProgress) return;
                ExtractFromSave(__instance, __result.Success && __result.SaveData != null, RunSaveManager.multiplayerRunSaveFileName);
            }
        }

        // ── 核心逻辑 ────────────────────────────────────────────
        private static byte[] TryInjectRunData(string path, byte[] bytes)
        {
            var name = Path.GetFileName(path);
            if (name != RunSaveManager.runSaveFileName && name != RunSaveManager.multiplayerRunSaveFileName)
                return bytes;

            try
            {
                if (JsonNode.Parse(Encoding.UTF8.GetString(bytes)) is not JsonObject root)
                    return bytes;
                // 尚无本局存档配置快照（未开局/未读档）时不注入
                if (ConfigFloatingWindowRunData.SaveConfig == null)
                    return bytes;
                // 仅当存档中已有字段且与当前本局配置一致时跳过重写，保留原始字节：
                // 避免每次写盘都把整个 run 存档 parse + 重序列化（大多数写盘不含配置变化）。
                JsonObject current = ConfigFloatingWindowRunData.ToJson();
                if (root[ConfigFloatingWindowRunData.SaveFieldName] is JsonNode existing
                    && JsonNode.DeepEquals(existing, current))
                    return bytes;
                root[ConfigFloatingWindowRunData.SaveFieldName] = current;
                return Encoding.UTF8.GetBytes(root.ToJsonString());
            }
            catch
            {
                return bytes;
            }
        }

        /// <summary>
        /// 从 run 存档文件提取本局配置并恢复运行时配置。
        /// 必须走 ISaveStore.ReadFile（内部 GetFullPath 把相对路径拼成 user:// 绝对路径）；
        /// 不能直接 Godot.FileAccess.Open(GetRunSavePath(...)) —— 那返回的是相对路径，会打不开，
        /// 导致读档必然失败而回落到默认配置（重启后继续游戏配置丢失的 bug）。
        /// </summary>
        /// <remarks>
        /// 防御第三方回档 mod（如 Rewind）：配置字段 _pengotarot_cfw 是写盘字节层注入的
        /// （patch GodotFileIo/CloudSaveStore.WriteFile），内存 SerializableRun 对象本身不含该字段。
        /// Rewind 用 RunManager.ToSave() 拿内存对象自存 checkpoint 时字段会丢，回档写出的存档无字段。
        /// 因此「本局已开局（SaveConfig 非 null）却读到无字段存档」= 字段被第三方丢，不是真正的旧存档；
        /// 此时从 JSON 长期偏好重新快照恢复用户配置，避免回档后配置被清成内置默认；
        /// 再固定快照，后续正常写盘会把字段写回存档（自愈）。真正的冷启动旧存档（进程内 SaveConfig 为 null）
        /// 仍按原逻辑 Reset 内置默认，隔离外部编辑污染。
        /// </remarks>
        private static void ExtractFromSave(RunSaveManager manager, bool success, string fileName)
        {
            if (!success) return;
            try
            {
                string path = RunSaveManager.GetRunSavePath(SaveManager.Instance.CurrentProfileId, fileName);
                var store = GetSaveStore(manager);
                if (store == null) return;
                string? json = store.ReadFile(path);
                if (string.IsNullOrEmpty(json)) return;
                if (JsonNode.Parse(json) is not JsonObject root) return;
                if (root[ConfigFloatingWindowRunData.SaveFieldName] is JsonObject cfw)
                    ConfigFloatingWindowRunData.FromJson(cfw);
                else if (ConfigFloatingWindowRunData.SaveConfig == null)
                    // 冷启动旧存档（mod 更新前开始、进程内尚未开局）没有本局配置字段：
                    // 重置为内置默认，隔离外部 JSON / 选人界面快照的残留，避免污染进行中的旧存档。
                    ConfigFloatingWindowRunData.Reset();
                else
                    // 本局已开局（SaveConfig 非 null）却读到无字段存档：
                    // 多为第三方回档 mod（Rewind）丢字段 —— 从 JSON 长期偏好重新快照，防止配置被清空。
                    ConfigFloatingWindowRunData.SnapshotFromDefaults();
                // 读档后：本局存档配置以存档为准（隔离外部编辑污染；后续写盘也注入这份快照）
                ConfigFloatingWindowRunData.SetSaveConfigFromRunData();
            }
            catch
            {
                // 存档解析失败不阻塞游戏加载
            }
        }

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_saveStore")]
        private static extern ref ISaveStore GetSaveStore(RunSaveManager manager);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_runSaveManager")]
        private static extern ref RunSaveManager GetRunSaveManager(SaveManager manager);
    }
}
