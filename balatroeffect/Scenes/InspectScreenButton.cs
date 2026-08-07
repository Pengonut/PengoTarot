#nullable enable

using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace PengoTarot.BalatroEffect
{
    /// <summary>
    /// Shared state and logic between the original NInspectCardScreen entry button
    /// and the full NBalatroInspectScreen panel. No longer builds its own UI in-place.
    /// </summary>
    public partial class InspectScreen
    {
        // ── Shared state (public for NBalatroInspectScreen) ──────
        public static string CurrentCardId { get; set; } = "";
        public static int CurrentEffectIndex { get; set; }
        /// <summary>Separately 模式下当前高亮编辑的部件名（UI 态，不持久化）。</summary>
        public static string SeparatelyHighlight { get; set; } = "";

        // Tracks the current card index in the original NInspectCardScreen,
        // updated every time SetCard is called. Used by the entry button to
        // open NBalatroInspectScreen at the correct card.
        private static int _currentOriginIndex;
        private static List<CardModel>? _currentOriginCards;

        public static string Tr(string key) =>
            new LocString("gameplay_ui", "BAL_" + key).GetFormattedText() ?? key;

        // ── Paginator (scene-driven) ────────────────────────────
        private static List<EffectRegistry.EffectDef> _pgEffects = new();
        private static int _pgCurrentIndex;
        private static Label? _pgLabel;
        private static bool _pgInitialized;

        public static void InitPaginator(Label label)
        {
            _pgLabel = label;
            if (_pgInitialized) return;
            EffectRegistry.Initialize();
            _pgEffects.Clear();
            foreach (var def in EffectRegistry.AllEffects)
            {
                _pgEffects.Add(def);
            }
            _pgCurrentIndex = 0;
            _pgInitialized = true;
            UpdatePaginatorLabel();
            OnPaginatorIndexChanged();
        }

        public static void UpdatePaginatorTarget(string cardId)
        {
            CurrentCardId = cardId;
            int currentMode = Config.GetCardEffectMode(cardId);
            int idx = _pgEffects.FindIndex(d => d.Mode == currentMode);
            _pgCurrentIndex = idx < 0 ? 0 : idx;
            UpdatePaginatorLabel();
            OnPaginatorIndexChanged();
        }

        public static void OnPaginatorNavigate(int delta)
        {
            int newIndex = _pgCurrentIndex + delta;
            if (newIndex < 0) newIndex = _pgEffects.Count - 1;
            else if (newIndex >= _pgEffects.Count) newIndex = 0;
            _pgCurrentIndex = newIndex;
            UpdatePaginatorLabel();
            OnPaginatorIndexChanged();

            Config.SetCardEffectMode(CurrentCardId, _pgEffects[_pgCurrentIndex].Mode);
            ApplyCurrentEffectToCheckedParts();
            RefreshCurrentScreen(); // 同步勾选框图标/不透明度（separately 自动勾选/清零后 UI 需即时刷新）
            ShaderController.RefreshAllCardsWithId(CurrentCardId);
        }

        /// <summary>将 effect 种类选择器（paginator）定位到指定 mode（仅更新 UI 与 CurrentEffectIndex，不写配置）。</summary>
        private static void SetPaginatorToMode(int mode)
        {
            int idx = _pgEffects.FindIndex(d => d.Mode == mode);
            if (idx < 0) return;
            _pgCurrentIndex = idx;
            UpdatePaginatorLabel();
            OnPaginatorIndexChanged();
        }

        private static void UpdatePaginatorLabel()
        {
            if (_pgLabel == null || !_pgInitialized) return;
            string key = "BAL_" + _pgEffects[_pgCurrentIndex].LocKey;
            string? text = LocManager.Instance?.GetTable("gameplay_ui").GetRawText(key);
            _pgLabel.Text = !string.IsNullOrEmpty(text) ? text : _pgEffects[_pgCurrentIndex].LocKey;
        }

        private static void OnPaginatorIndexChanged()
        {
            CurrentEffectIndex = _pgEffects[_pgCurrentIndex].Mode;
            RefreshScreenSlider();
        }


        // ── Entry button on original NInspectCardScreen ───────────
        private static readonly string _entryBtnScene = "res://balatroeffect/Scenes/balatro_entry_btn.tscn";

        public static void AddEntryButton(Control root, List<CardModel> cards, int index)
        {
            if (root.HasNode("BalatroEntryBtnRoot")) return;

            var scene = GD.Load<PackedScene>(_entryBtnScene);
            var rootNode = scene.Instantiate(PackedScene.GenEditState.Disabled);
            root.AddChild(rootNode);
            var btn = rootNode.GetNode<NButton>("Button");
            var btnLabel = btn.GetNode<Label>("Label");
            btnLabel.Text = Tr("BTN_ENTRY");
            btnLabel.ApplyLocaleFontSubstitution(FontType.Regular, "font");

            // ── 动画控制 ──
            // 初始状态
            btn.Modulate = new Color(1, 1, 1, 1.0f);
            btn.Scale = Vector2.One;
            // 设置缩放基点为按钮中心
            btn.PivotOffset = btn.Size / 2.0f;

            // 初始衰减 Tween：8 秒后 1 秒内 alpha → 0
            Tween? fadeOutTween = btn.CreateTween();
            fadeOutTween.TweenProperty(btn, "modulate:a", 0.0f, 1.0f).SetDelay(8.0f);

            Tween? scaleTween = null;

            // 鼠标进入
            btn.MouseEntered += () =>
            {
                // 终止衰减动画
                if (fadeOutTween?.IsValid() == true)
                    fadeOutTween.Kill();

                // 重新设定缩放中心（防止按钮尺寸变化）
                btn.PivotOffset = btn.Size / 2.0f;

                // 0.1 秒内透明度恢复至 1
                Tween enterAlphaTween = btn.CreateTween();
                enterAlphaTween.TweenProperty(btn, "modulate:a", 1.0f, 0.1f);

                // 缩放至 1.02 倍（0.1 秒）
                if (scaleTween?.IsValid() == true)
                    scaleTween.Kill();
                scaleTween = btn.CreateTween();
                scaleTween.TweenProperty(btn, "scale", new Vector2(1.02f, 1.02f), 0.1f);
            };

            // 鼠标离开
            btn.MouseExited += () =>
            {
                // 终止旧的透明度渐变
                if (fadeOutTween?.IsValid() == true)
                    fadeOutTween.Kill();

                // 2 秒后 1 秒内 alpha → 0
                fadeOutTween = btn.CreateTween();
                fadeOutTween.TweenProperty(btn, "modulate:a", 0.0f, 1.0f).SetDelay(2.0f);

                // 0.2 秒内缩放恢复至原始大小
                if (scaleTween?.IsValid() == true)
                    scaleTween.Kill();

                btn.PivotOffset = btn.Size / 2.0f; // 确保中心缩放
                scaleTween = btn.CreateTween();
                scaleTween.TweenProperty(btn, "scale", Vector2.One, 0.2f);
            };
            // ── 结束 ──

            WireEntryBtn(btn, root, cards);
        }

        private static void WireEntryBtn(NButton btn, Control root, List<CardModel> cards)
        {
            btn.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ =>
            {
                var rootOfBtn = btn.GetParent();
                NInspectCardScreen? inspect = null;
                for (Node? cur = rootOfBtn; cur != null; cur = cur.GetParent())
                    if (cur is NInspectCardScreen ins) { inspect = ins; break; }

                // Find current card from the inspect screen's displayed NCard
                string? currentId = null;
                if (inspect != null)
                {
                    var ncard = inspect.FindChild("Card", true, false) as NCard;
                    currentId = ncard?.Model?.Id.ToString();
                }

                // Get current card list from NCardLibrary
                var tree = Engine.GetMainLoop() as SceneTree;
                var library = tree?.Root.FindChild("CardLibraryScreen", true, false);
                var grid = library?.FindChild("CardGrid", true, false) as NCardLibraryGrid;
                List<CardModel> visible = grid?.VisibleCards != null
                    ? new List<CardModel>(grid.VisibleCards) : cards;

                // Find index of current card in the visible list
                int idx = 0;
                if (currentId != null)
                {
                    var found = visible.FindIndex(c => c.Id.ToString() == currentId);
                    if (found >= 0) idx = found;
                }

                inspect?.Close();

                var screen = NBalatroInspectScreen.Create();
                if (screen != null)
                {
                    tree?.Root.AddChild(screen);
                    screen.Open(_currentOriginCards ?? cards, _currentOriginIndex, false, inspect);
                }
            }));
        }


        // ── Public action methods (called by NBalatroInspectScreen) ──
        /// <summary>切换编辑模式（normal/separately/fully）。进入 separately 默认高亮第一个部件。</summary>
        public static void OnEditModeSelected(string mode)
        {
            if (string.IsNullOrEmpty(CurrentCardId)) return;
            Config.SetEditMode(CurrentCardId, mode);
            if (mode == Config.ModeSeparately)
            {
                SeparatelyHighlight = Config.AllPartNames.Length > 0 ? Config.AllPartNames[0] : "";
                SetPaginatorToMode(Config.GetEffect(CurrentCardId, SeparatelyHighlight));
            }
            RefreshCurrentScreen();
            ShaderController.RefreshAllCardsWithId(CurrentCardId);
        }

        /// <summary>部件行点击入口（替换旧 OnPartToggled）：按当前编辑模式分派。</summary>
        public static void OnPartClicked(string partName)
        {
            if (string.IsNullOrEmpty(CurrentCardId)) return;

            switch (Config.GetEditMode(CurrentCardId))
            {
                case Config.ModeFully:
                    // fully 模式：整卡效果由 paginator 控制，部件点击无效
                    return;

                case Config.ModeSeparately:
                    bool isChecked = Config.GetEffect(CurrentCardId, partName) > 0;
                    if (isChecked && SeparatelyHighlight == partName)
                    {
                        // 点已高亮的已勾选 → 清零（高亮保留，待切 effect 自动勾选）
                        Config.SetEffect(CurrentCardId, partName, 0);
                    }
                    else
                    {
                        // 转移/建立高亮；未勾选者待切 effect 自动勾选
                        SeparatelyHighlight = partName;
                    }
                    // 高亮变化后：paginator 定位到高亮部件的 effect（便于继续编辑）
                    SetPaginatorToMode(Config.GetEffect(CurrentCardId, SeparatelyHighlight));
                    RefreshCurrentScreen();
                    ShaderController.RefreshAllCardsWithId(CurrentCardId);
                    return;

                default: // normal：勾选/取消切换
                    bool checkedState = Config.GetEffect(CurrentCardId, partName) > 0;
                    Config.SetEffect(CurrentCardId, partName, checkedState ? 0 : CurrentEffectIndex);
                    RefreshCurrentScreen();
                    ShaderController.RefreshAllCardsWithId(CurrentCardId);
                    return;
            }
        }

        public static void OnCopyCard()
        {
            if (string.IsNullOrEmpty(CurrentCardId)) return;
            string json = Config.ExportCardPreset(CurrentCardId);
            DisplayServer.ClipboardSet(json);
        }

        public static void OnPasteCard()
        {
            if (string.IsNullOrEmpty(CurrentCardId)) return;
            string? clipboard = DisplayServer.ClipboardGet();
            if (!string.IsNullOrEmpty(clipboard) && Config.ImportCardPreset(CurrentCardId, clipboard))
            {
                UpdatePaginatorTarget(CurrentCardId); // 定位 paginator 到粘贴的 mode
                RefreshCurrentScreen();
                ShaderController.RefreshAllCardsWithId(CurrentCardId); // 立即刷新卡牌特效，否则需切换一次才显示
            }
        }

        public static void OnMenuAction(int id)
        {
            switch (id)
            {
                case 0:
                    Config.ClearEffect(CurrentCardId);
                    SeparatelyHighlight = Config.AllPartNames.Length > 0 ? Config.AllPartNames[0] : "";
                    UpdatePaginatorTarget(CurrentCardId);
                    RefreshCurrentScreen();
                    ShaderController.RefreshAllCardsWithId(CurrentCardId);
                    break;
                case 1:
                case 5:
                    Config.ApplyAllAuthorPresets();
                    RefreshCurrentScreen();
                    break;
                case 2:
                    ApplyToVisibleCards();
                    break;
                case 3:
                    DisplayServer.ClipboardSet(Config.ExportPreset());
                    break;
                case 4:
                    string? clip = DisplayServer.ClipboardGet();
                    if (!string.IsNullOrEmpty(clip) && Config.ImportPreset(clip))
                        RefreshCurrentScreen();
                    break;
            }
        }

        /// <summary>
        /// 将当前卡的完整配置（编辑模式 + 部件 + 整卡 + 强度）应用到当前检查界面（NBalatroInspectScreen）的卡列表。
        /// 旧实现依赖 NCardLibrary 补丁维护的静态 VisibleCards，因 async DisplayCards 的 Postfix 在
        /// FilterCards 之前执行导致数据滞后一页、且与检查界面实际列表不一致，已改为直接读实例卡列表。
        /// </summary>
        private static void ApplyToVisibleCards()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            var screen = tree?.Root.FindChild("BalatroInspectScreen", true, false) as NBalatroInspectScreen;
            var cards = screen?.CurrentCards;
            if (cards == null || cards.Count == 0)
            {
                GD.Print("[BalatroEffect] 应用给当前卡列表失败：检查界面卡列表为空");
                return;
            }
            var source = Config.GetEntry(CurrentCardId);
            if (source == null)
            {
                GD.Print("[BalatroEffect] 应用给当前卡列表失败：当前卡无有效配置");
                return;
            }
            foreach (var card in cards)
            {
                string id = card.Id.ToString();
                Config.ReplaceCardEffects(id, source);
            }
            RefreshCurrentScreen();
            foreach (var card in cards)
                ShaderController.RefreshAllCardsWithId(card.Id.ToString());
        }

        /// <summary>Find the active NBalatroInspectScreen and refresh its panel state.</summary>
        private static void RefreshCurrentScreen()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            var screen = tree?.Root.FindChild("BalatroInspectScreen", true, false) as NBalatroInspectScreen;
            screen?.RefreshPanelState();
        }

        // ── NCardLibrary patches (for "apply to visible cards") ──
        // 已移除（2026-08-01）：原 ApplyToVisibleCards 依赖下面两个补丁维护静态 VisibleCards。
        // 移除原因：
        //   1. NCardLibrary.DisplayCards 是 async，Harmony Postfix 在同步段（await Task.Yield 之前）
        //      就执行，读到的 _grid.VisibleCards 是上一页旧值 → 数据滞后一页。
        //   2. VisibleCards 是"图鉴网格当前页"，与 NBalatroInspectScreen 检查界面实际显示的卡列表
        //      （_cards）可能不一致；OnSubmenuClosed 还会清空它导致菜单静默无效果。
        // 现改为直接从当前 NBalatroInspectScreen 实例读取卡列表（见 ApplyToVisibleCards）。
        // 如需恢复旧行为，取消下面注释并补回静态 VisibleCards 字段即可：
        //
        // [HarmonyPatch(typeof(NCardLibrary), "DisplayCards")]
        // static class ApplyToVisibleCardsPatch
        // {
        //     public static void Postfix(NCardLibrary __instance, NCardLibraryGrid ____grid)
        //     {
        //         VisibleCards = new List<CardModel>(____grid.VisibleCards);
        //     }
        // }
        //
        // [HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary.OnSubmenuClosed))]
        // static class ClearVisibleCardsPatch
        // {
        //     public static void Postfix() => VisibleCards = new List<CardModel>();
        // }


        /// <summary>
        /// 切换 effect 后应用：按编辑模式分派。
        ///  normal  → 应用到所有已勾选部件；
        ///  separately → 只改高亮部件（未勾选则自动勾选；切到 None 则清零）；
        ///  fully   → 整卡 effect（FullCardEffect）。
        /// </summary>
        private static void ApplyCurrentEffectToCheckedParts()
        {
            if (string.IsNullOrEmpty(CurrentCardId)) return;

            switch (Config.GetEditMode(CurrentCardId))
            {
                case Config.ModeFully:
                    Config.SetEffect(CurrentCardId, "FullCard", CurrentEffectIndex);
                    break;

                case Config.ModeSeparately:
                    string part = SeparatelyHighlight;
                    if (string.IsNullOrEmpty(part))
                        part = Config.AllPartNames.Length > 0 ? Config.AllPartNames[0] : "";
                    if (string.IsNullOrEmpty(part)) break;
                    Config.SetEffect(CurrentCardId, part, CurrentEffectIndex);
                    break;

                default: // normal：应用到所有已勾选部件
                    foreach (var p in Config.AllPartNames)
                        if (Config.GetEffect(CurrentCardId, p) > 0)
                            Config.SetEffect(CurrentCardId, p, CurrentEffectIndex);
                    break;
            }
        }

        private static void RefreshScreenSlider()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            var slider = tree?.Root.FindChild("BalatroEffectsSlider", true, false) as NSlider;
            if (slider != null)
            {
                slider.SetBlockSignals(true);
                slider.Value = Config.GetIntensity(CurrentCardId) * 100.0;
                slider.SetBlockSignals(false);
            }
        }

        // ── Patch: track current card in original NInspectCardScreen ──
        [HarmonyPatch(typeof(NInspectCardScreen), "Open")]
        static class OpenPatch
        {
            public static void Postfix(int index, List<CardModel> cards)
            {
                if (cards is null || index < 0 || index >= cards.Count) return;
                _currentOriginIndex = index;
                _currentOriginCards = cards;
            }
        }

        [HarmonyPatch(typeof(NInspectCardScreen), "SetCard")]
        static class SetCardPatch
        {
            public static void Postfix(NInspectCardScreen __instance, int index, List<CardModel> ____cards)
            {
                if (____cards is null || index < 0 || index >= ____cards.Count) return;
                _currentOriginIndex = index;
                _currentOriginCards = ____cards;
                AddEntryButton(__instance, ____cards, index);

                // 修复：原版检查界面的插画 shader 不显示。
                // UpdateVisuals 每次会把 _portrait.Material 清为 null，其 Postfix 触发的 ApplyShader
                // 可能发生在卡布局完成前（Portrait 尺寸未定 → UV 参数错误），或之后被再次清空。
                // 延迟一帧重应用，确保材质最终为本 mod 的 shader 且 UV 正确。
                var card = Traverse.Create(__instance).Field("_card").GetValue<NCard>();
                if (card == null || !GodotObject.IsInstanceValid(card) || card.Model == null) return;
                var tree = Engine.GetMainLoop() as SceneTree;
                if (tree == null) return;
                void Reapply()
                {
                    tree.ProcessFrame -= Reapply;
                    if (GodotObject.IsInstanceValid(card))
                        ShaderController.ApplyShader(card);
                }
                tree.ProcessFrame += Reapply;
            }
        }
    }
}