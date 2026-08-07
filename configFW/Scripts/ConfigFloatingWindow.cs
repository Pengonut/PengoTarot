#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.ConfigFW
{
    /// <summary>
    /// configfloatingwindow 体系的静态入口/门面。
    /// 入口悬浮窗在选人界面（可编辑）与游戏过程（只读）共用同一场景，均可拖动。
    /// 点击入口时按当前上下文决定编辑/只读模式。
    /// 多人规则：选人界面仅主机可改（客户端隐藏入口）；游戏过程所有玩家可查看共享配置。
    /// 本局配置：RunData 为运行时真相；存档注入 _pengotarot_cfw（RunSaveInjectPatch）；
    /// 多人由 ConfigFloatingWindowDataMessage 可靠广播分发（RunSaveInjectPatch 的 4 条路径）。
    /// </summary>
    public static class ConfigFloatingWindow
    {
        private const string EntryBtnScenePath = "res://configFW/Scenes/configfloatingwindow_entry_btn.tscn";
        private const string EntryBtnRootName = "ConfigFWEntryBtnRoot";
        /// <summary>入口按钮图片（284×380 竖向）。</summary>
        private const string EntryImagePath = "res://configFW/Scenes/images/entry.webp";

        /// <summary>当前入口绑定的角色选择界面（patch 在 OnSubmenuOpened/Closed 时更新）。</summary>
        private static NCharacterSelectScreen? _boundScreen;
        /// <summary>当前打开的面板实例（null 表示未打开）。</summary>
        private static NConfigFloatingWindow? _openPanel;
        /// <summary>已注册消息 handler 的 NetService（防止重复注册）。</summary>
        private static INetGameService? _registeredNetService;
        /// <summary>当前入口根节点（选人界面或 NRun 下，AddEntryButton/AddRunEntryButton 时赋值）。</summary>
        private static Control? _currentEntryRoot;
        /// <summary>面板打开前入口根的可见状态（关闭时按此恢复，保留客户端原本的隐藏态）。</summary>
        private static bool _entryVisibleBeforeOpen;

        // ── Patch 生命周期入口 ─────────────────────────────────
        public static void OnCharacterSelectOpened(NCharacterSelectScreen screen)
        {
            _boundScreen = screen;
            // 仅在设置界面显示模式下，选人界面不注入入口
            if (!ConfigFloatingWindowConfig.ShowInSettingsOnly)
                AddEntryButton(screen);
            RefreshEntryButton(screen);
            // 让选人界面入口应用最新保存的位置/埋入状态（含从游戏带回的）
            if (screen.GetNodeOrNull<NConfigFloatingWindowEntryButton>($"{EntryBtnRootName}/Button") is { } btn)
                btn.ApplySavedState();
            RegisterSync(screen);

            // 选人界面打开（主机/单机）：以配置默认值为本局编辑起点，
            // 覆盖主菜单 LoadRunSave 阶段从磁盘旧存档恢复的值（否则进入游戏会显示旧配置）。
            // 多人主机同时广播初始值，客机一进选人界面面板显示即正确。
            if (screen.Lobby?.NetService?.Type != NetGameType.Client)
            {
                ConfigFloatingWindowRunData.SnapshotFromDefaults();
                BroadcastConfig();
            }
        }

        public static void OnCharacterSelectClosed(NCharacterSelectScreen screen)
        {
            if (_boundScreen == screen) _boundScreen = null;
            // 退出角色选择界面时，若面板还开着则关闭（不广播，lobby 即将清理）
            ClosePanel(broadcast: false);
            UnregisterSync();
        }

        /// <summary>游戏开始后（NRun 就绪）注入只读入口悬浮窗（所有界面显示）。</summary>
        public static void OnRunReady(NRun run)
        {
            // 仅在设置界面显示模式下，游戏过程中不注入入口
            if (!ConfigFloatingWindowConfig.ShowInSettingsOnly)
                AddRunEntryButton(run);
        }

        // ── 入口按钮注入（选人界面 + 游戏过程共用同一场景） ─────
        private static void AddEntryButton(NCharacterSelectScreen screen)
        {
            if (screen.HasNode(EntryBtnRootName)) return;
            var root = CreateEntry();
            screen.AddChildSafely(root);
            _currentEntryRoot = root;
        }

        private static void AddRunEntryButton(NRun run)
        {
            if (run.HasNode(EntryBtnRootName)) return;
            var root = CreateEntry();
            run.AddChildSafely(root);
            _currentEntryRoot = root;
        }

        private static Control CreateEntry()
        {
            var scene = GD.Load<PackedScene>(EntryBtnScenePath);
            var root = scene.Instantiate(PackedScene.GenEditState.Disabled);
            root.Name = EntryBtnRootName;
            return (Control)root;
        }

        /// <summary>选人界面每次打开时刷新入口：加载图片 + 客户端隐藏。</summary>
        private static void RefreshEntryButton(NCharacterSelectScreen screen)
        {
            var root = screen.GetNodeOrNull<Control>(EntryBtnRootName);
            if (root == null) return;

            // 入口图片：用户放到指定路径后，重新进入界面即生效
            if (ResourceLoader.Exists(EntryImagePath))
            {
                var tex = GD.Load<Texture2D>(EntryImagePath);
                var icon = root.GetNodeOrNull<TextureRect>("Button/Icon");
                if (tex != null && icon != null)
                    icon.Texture = tex;
            }

            var net = screen.Lobby?.NetService;
            bool isClient = net != null && net.Type == NetGameType.Client;
            root.Visible = !isClient;
        }

        // ── 入口点击 → 打开面板（按上下文决定编辑/只读） ────────
        public static void OnEntryClicked()
        {
            bool editable = IsInCharacterSelect() || IsInSettingsAwayFromRun();
            OpenPanel(remote: false, editable: editable);
        }

        private static bool IsInCharacterSelect()
        {
            var mainMenu = NGame.Instance?.MainMenu;
            return mainMenu != null && mainMenu.SubmenuStack.SubmenusOpen
                && mainMenu.SubmenuStack.Peek() is NCharacterSelectScreen;
        }

        /// <summary>在设置界面且不在游戏进程中：应可编辑配置。</summary>
        private static bool IsInSettingsAwayFromRun()
        {
            if (RunManager.Instance?.IsInProgress != false) return false;
            var mainMenu = NGame.Instance?.MainMenu;
            return mainMenu != null && mainMenu.SubmenuStack.SubmenusOpen
                && mainMenu.SubmenuStack.Peek() is NSettingsScreen;
        }

        // ── 面板开关（主机权威） ───────────────────────────────
        /// <summary>打开面板。remote=true 表示客户端只读（收到主机广播）；editable 控制是否可编辑。</summary>
        public static void OpenPanel(bool remote = false, bool editable = false)
        {
            if (_openPanel != null) return;
            var panel = NConfigFloatingWindow.Create();
            if (panel == null) return;
            // 挂到 NGame 下、远程光标容器之前（靠树序让光标显示在面板上层，不动光标、不改 z_index）
            AttachPanelAboveCursor(panel);
            panel.Open(remote, editable);
            _openPanel = panel;

            // 面板（模态）打开时隐藏入口悬浮按钮，避免遮挡/被全局输入误拖；关闭时恢复原可见状态
            HideEntryRoot();

            if (!remote && IsMultiplayer())
            {
                SendState(true);
                // 同时广播当前配置：客机面板打开即显示主机最新值（消息可靠保序，最终一致）
                BroadcastConfig();
            }
        }

        /// <summary>面板打开时隐藏入口根（记录打开前的可见状态，供关闭恢复）。</summary>
        private static void HideEntryRoot()
        {
            if (_currentEntryRoot == null || !GodotObject.IsInstanceValid(_currentEntryRoot)) return;
            _entryVisibleBeforeOpen = _currentEntryRoot.Visible;
            _currentEntryRoot.Visible = false;
        }

        /// <summary>
        /// 面板关闭后把入口根恢复为打开前的可见状态。
        /// 恢复的是打开前记录的值：客户端入口本就由 RefreshEntryButton 隐藏，不得强制显示。
        /// </summary>
        private static void RestoreEntryRoot()
        {
            if (_currentEntryRoot == null || !GodotObject.IsInstanceValid(_currentEntryRoot)) return;
            _currentEntryRoot.Visible = _entryVisibleBeforeOpen;
        }

        /// <summary>
        /// 把面板挂到 NGame 下、远程光标容器之前，靠树序让队友光标自然显示在面板上层。
        /// 不移动光标容器（避免触发 Deinitialize 破坏输入同步）、不改 z_index。
        /// 面板盖住主内容（RootSceneContainer），模态容器（ModalContainer）仍在面板之上（合理）。
        /// </summary>
        private static void AttachPanelAboveCursor(Control panel)
        {
            if (NGame.Instance is not { } game)
            {
                // 兜底：NGame 尚未就绪时挂到树根
                (Engine.GetMainLoop() as SceneTree)?.Root.AddChild(panel);
                return;
            }
            game.AddChild(panel);
            var container = game.RemoteCursorContainer;
            if (container != null && container.GetParent() == game)
            {
                // 把面板移到光标容器当前索引处，光标被自然推到面板之后（上层）
                game.MoveChild(panel, container.GetIndex());
            }
        }

        /// <summary>关闭面板。broadcast=false 时（客户端跟随/退出界面）不广播。</summary>
        public static void ClosePanel(bool broadcast = true)
        {
            if (_openPanel == null) return;
            _openPanel.Close();
            _openPanel = null;

            // 恢复入口（保留客户端原本的隐藏态，不强制显示）
            RestoreEntryRoot();

            if (broadcast && IsMultiplayer())
                SendState(false);
        }

        private static bool IsMultiplayer()
            => _boundScreen?.Lobby?.NetService.Type.IsMultiplayer() == true;

        // ── 设置界面入口注入（始终出现，不受 toggle 控制） ──
        /// <summary>设置界面打开时注入入口（始终生效，不受 ShowInSettingsOnly 影响）。</summary>
        public static void OnSettingsScreenOpened(Control settingsScreen)
        {
            if (settingsScreen.HasNode(EntryBtnRootName)) return;
            var root = CreateEntry();
            settingsScreen.AddChildSafely(root);
            _currentEntryRoot = root;
        }

        /// <summary>设置界面关闭时移除入口。</summary>
        public static void OnSettingsScreenClosed()
        {
            if (_currentEntryRoot == null) return;
            if (!GodotObject.IsInstanceValid(_currentEntryRoot)) return;
            _currentEntryRoot.QueueFree();
            _currentEntryRoot = null;
        }

        // ── 多人同步 ───────────────────────────────────────────
        private static void RegisterSync(NCharacterSelectScreen screen) => RegisterSync(screen.Lobby?.NetService);

        private static void RegisterSync(INetGameService? net)
        {
            if (net == null) return;
            if (_registeredNetService == net) return;
            _registeredNetService?.UnregisterMessageHandler<ConfigFloatingWindowStateMessage>(OnStateMessage);
            _registeredNetService?.UnregisterMessageHandler<ConfigFloatingWindowDataMessage>(OnDataMessage);
            net.RegisterMessageHandler<ConfigFloatingWindowStateMessage>(OnStateMessage);
            net.RegisterMessageHandler<ConfigFloatingWindowDataMessage>(OnDataMessage);
            _registeredNetService = net;
        }

        /// <summary>多人读档（LoadRunLobby 创建）时注册配置消息 handler，使客机能收到主机读档广播。</summary>
        public static void RegisterSyncForLoadLobby(INetGameService net) => RegisterSync(net);

        /// <summary>多人读档 lobby 关闭时注销配置消息 handler（仅当仍指向该 service）。</summary>
        public static void OnLoadLobbyClosed(INetGameService net)
        {
            if (_registeredNetService == net)
                UnregisterSync();
        }

        private static void UnregisterSync()
        {
            _registeredNetService?.UnregisterMessageHandler<ConfigFloatingWindowStateMessage>(OnStateMessage);
            _registeredNetService?.UnregisterMessageHandler<ConfigFloatingWindowDataMessage>(OnDataMessage);
            _registeredNetService = null;
        }

        private static void SendState(bool isOpen)
        {
            _registeredNetService?.SendMessage(new ConfigFloatingWindowStateMessage
            {
                version = ConfigFloatingWindowRunData.CurrentVersion,
                isOpen = isOpen,
            });
        }

        /// <summary>
        /// 广播本局配置给客机（选人界面编辑时调用，主机权威）。
        /// 客机通过 OnDataMessage 覆盖本机 RunData，实现实时跟随。
        /// </summary>
        public static void BroadcastConfig()
        {
            if (_registeredNetService == null) return;
            if (_registeredNetService.Type != NetGameType.Host) return;
            _registeredNetService.SendMessage(ConfigFloatingWindowDataMessage.FromRunData());
        }

        private static void OnStateMessage(ConfigFloatingWindowStateMessage message, ulong senderId)
        {
            if (_registeredNetService == null) return;
            if (senderId == _registeredNetService.NetId) return; // 忽略自己发出的回显

            if (message.isOpen)
                OpenPanel(remote: true);
            else
                ClosePanel(broadcast: false);
        }

        /// <summary>收到主机广播的本局配置：覆盖本机运行时配置（整局共享主机配置）。</summary>
        private static void OnDataMessage(ConfigFloatingWindowDataMessage message, ulong senderId)
        {
            if (_registeredNetService == null) return;
            if (senderId == _registeredNetService.NetId) return; // 忽略自己发出的回显

            message.ApplyToRunData();
            // 客户端面板若已打开（跟随主机），刷新显示以实时反映主机修改
            if (_openPanel != null)
                _openPanel.RefreshFromRunData();
        }
    }
}
