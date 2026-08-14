// PengoTarot: 检测到 Hextech 附魔 mod 同时安装时的一次性作者提示弹窗。
//
// 背景：Hextech 使用「组合代理替换 card.Enchantment」的多重附魔实现，
// 硬编码修复了原版附魔 is 判断，但与绝大多数附魔内容 mod（含 PengoTarot）不兼容。
// 本弹窗在进入选人界面（点击「新建游戏」）时触发一次，提示玩家可用
// MultiEnchantment 替代；确认后不再弹出（用 mod 自身 config 持久化，不依赖游戏 FTUE）。
//
// 实现：复用游戏原版 NGenericPopup（generic_popup.tscn）+ NModalContainer，
// 分两页显示（第一页「下一页」，第二页「我知道了，不再弹出」）。

#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch
{
    /// <summary>Hextech 冲突提示弹窗（一次性，进入选人界面触发）。</summary>
    public static class HextechWarningPatch
    {
        /// <summary>Hextech 本体与赞助者拓展包的 manifest id。</summary>
        private static readonly string[] HextechModIds = { "HextechRunes", "HextechRunesSponsorPack" };

        /// <summary>进程内防重入（AfterInitialized 可能被多次调用）。</summary>
        private static bool _showing;

        /// <summary>是否检测到 Hextech mod 已加载（按 manifest id 匹配）。</summary>
        public static bool IsHextechLoaded()
        {
            try
            {
                return ModManager.Mods.Any(mod =>
                    mod.state == ModLoadState.Loaded &&
                    mod.manifest?.id != null &&
                    HextechModIds.Contains(mod.manifest.id, StringComparer.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>进入选人界面（点击「新建游戏」）后触发一次提示弹窗。</summary>
        [HarmonyPatch(typeof(NCharacterSelectScreen), "AfterInitialized")]
        public static class CharacterSelect_AfterInitialized_Patch
        {
            static void Postfix()
            {
                bool loaded = IsHextechLoaded();
                bool dismissed = ConfigFloatingWindowConfig.HextechWarningDismissed;
                GD.Print($"[PengoTarot] HextechWarning: AfterInitialized postfix. showing={_showing}, hextechLoaded={loaded}, dismissed={dismissed}");
                if (_showing || !loaded || dismissed)
                    return;
                _showing = true;
                _ = ShowWarningAsync();
            }
        }

        private static async Task ShowWarningAsync()
        {
            try
            {
                var tree = Engine.GetMainLoop() as SceneTree;
                if (tree == null || NModalContainer.Instance == null)
                    return;

                // 第一页：只有「下一页」
                bool next = await ShowPageAsync(
                    "HEXTECH_WARNING_TITLE",
                    "HEXTECH_WARNING_PAGE1",
                    null,
                    "HEXTECH_WARNING_NEXT");
                if (!next) return;

                // 时序关键：WaitForConfirmation 的 SetResult 会同步恢复本 continuation（正处于第一页
                // 按钮的 Released signal 派发中）。此时同一 signal 的下一个处理器 NVerticalPopup.Close
                // → NModalContainer.Clear() 尚未执行。若不等待，第二页弹窗刚 Add 进容器就会被 Clear() 误删。
                // 等一帧让按钮 signal 派发（含 Clear）完全结束，容器 OpenModal 复位后再弹第二页。
                if (tree != null)
                    await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

                // 第二页：确认不再弹出（写入 mod 自身 config）
                await ShowPageAsync(
                    "HEXTECH_WARNING_TITLE",
                    "HEXTECH_WARNING_PAGE2",
                    null,
                    "HEXTECH_WARNING_ACK");

                ConfigFloatingWindowConfig.SetHextechWarningDismissed(true);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PengoTarot] Hextech warning popup failed: {ex.Message}");
            }
            finally
            {
                _showing = false;
            }
        }

        /// <summary>用原版 NGenericPopup 弹一页确认框，返回是否点击了确认按钮（走 main_menu_ui 本地化表）。</summary>
        private static async Task<bool> ShowPageAsync(string titleKey, string bodyKey, string? noButtonKey, string yesButtonKey)
        {
            // 不用 PreloadManager.Cache.GetScene（非预加载场景返回 null），直接 GD.Load。
            var scene = GD.Load<PackedScene>("res://scenes/ui/generic_popup.tscn");
            if (scene == null) return false;

            var popup = scene.Instantiate<NGenericPopup>(PackedScene.GenEditState.Disabled);
            if (popup == null) return false;

            var container = NModalContainer.Instance;
            if (container == null) return false;

            container.Add(popup);
            return await popup.WaitForConfirmation(
                new LocString("main_menu_ui", bodyKey),              // body
                new LocString("main_menu_ui", titleKey),             // header
                noButtonKey != null ? new LocString("main_menu_ui", noButtonKey) : null,
                new LocString("main_menu_ui", yesButtonKey));
        }
    }
}
