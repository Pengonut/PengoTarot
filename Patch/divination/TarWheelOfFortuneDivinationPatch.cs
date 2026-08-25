#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patches;

/// <summary>
/// 命运之轮占卜：新局获得两件普通/罕见遗物；每完成四场战斗，
/// 移除当前持有的、最早获得的普通/罕见/稀有/商店遗物。
/// </summary>
public static class TarWheelOfFortuneDivinationPatch
{
    private const int FlagIndex = 10;
    private const int CombatInterval = 4;
    private const string WarningIconPath = "res://images/ui/language_warning.png";
    private const float WarningIconSize = 24f;

    private sealed class WarningState
    {
        public Timer Timer = null!;
        public TextureRect? WarningIcon;
        public NRelicInventoryHolder? Target;
    }

    private static readonly ConditionalWeakTable<NRelicInventory, WarningState> WarningStates = new();

    private static bool IsEnabled()
        => ConfigFloatingWindowRunData.GetTarFlag(FlagIndex);

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewSingleplayer))]
    private static class SetUpNewSingleplayerPatch
    {
        [HarmonyPostfix]
        private static void Postfix(RunState state)
            => GrantStartingRelics(state);
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewMultiplayer))]
    private static class SetUpNewMultiplayerPatch
    {
        [HarmonyPostfix]
        private static void Postfix(RunState state, StartRunLobby lobby)
            => GrantStartingRelics(state);
    }

    private static void GrantStartingRelics(RunState state)
    {
        if (!IsEnabled())
            return;

        var obtainTasks = new List<Task>();
        foreach (var player in state.Players)
        {
            for (int i = 0; i < 2; i++)
            {
                // 单独掷普通/罕见（各 50%），明确不进入稀有与商店遗物池。
                var rarity = player.PlayerRng.Rewards.NextBool()
                    ? RelicRarity.Common
                    : RelicRarity.Uncommon;
                var relic = RelicFactory.PullNextRelicFromFront(
                    player,
                    rarity,
                    candidate => candidate.Rarity == rarity).ToMutable();
                obtainTasks.Add(RelicCmd.Obtain(relic, player));
            }
        }

        // Obtain 在首次 await 前已同步加入遗物栏；继续追踪 AfterObtained，避免吞掉异步异常。
        TaskHelper.RunSafely(Task.WhenAll(obtainTasks));
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    private static class AfterCombatVictoryPatch
    {
        [HarmonyPostfix]
        private static void Postfix(IRunState runState, ICombatState? combatState,
            CombatRoom room, ref Task __result)
        {
            __result = ResolveAfterVictory(__result, runState);
        }
    }

    private static async Task ResolveAfterVictory(Task originalTask, IRunState runState)
    {
        await originalTask;
        if (!RunManager.Instance.IsInProgress || !IsEnabled())
            return;

        int completedCombats = ConfigFloatingWindowRunData.RecordWheelOfFortuneCombat();
        if (completedCombats % CombatInterval != 0)
            return;

        foreach (var player in runState.Players)
        {
            // Relics 保持获得顺序；Starting/Ancient/Event 等不会进入候选。
            // Shop 虽是独立 rarity，但按需求允许被命运之轮移除。
            var oldestEligible = player.Relics.FirstOrDefault(IsRemovableRarity);
            if (oldestEligible != null)
                await RelicCmd.Remove(oldestEligible);
        }
    }

    private static bool IsRemovableRarity(RelicModel relic)
        => relic.Rarity is RelicRarity.Common
            or RelicRarity.Uncommon
            or RelicRarity.Rare
            or RelicRarity.Shop;

    /// <summary>为本地遗物栏添加命运之轮倒计时警告，不影响原版 FlowContainer 布局。</summary>
    [HarmonyPatch(typeof(NRelicInventory), nameof(NRelicInventory._Ready))]
    private static class NRelicInventoryReadyPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NRelicInventory __instance)
        {
            // 命运之轮未启用时不得创建任何计时器或视觉节点。
            // 开关是开局配置，局内不会从关闭切换为开启。
            if (!IsEnabled())
                return;

            if (WarningStates.TryGetValue(__instance, out _))
                return;

            var state = new WarningState
            {
                Timer = new Timer
                {
                    Name = "PengoTarotWheelRelicWarningTimer",
                    WaitTime = 0.05,
                    OneShot = false,
                    Autostart = true,
                },
            };
            WarningStates.Add(__instance, state);
            state.Timer.Timeout += () => UpdateWarning(__instance, state);
            __instance.TreeExiting += () => CleanupWarning(state);

            // NRelicInventory 是 FlowContainer，原版 GetBottomOfInventory 假定它的所有直接子节点
            // 都是 Control 类型的遗物 holder。Timer 若挂在 inventory 下会破坏该不变量，
            // 在多人状态栏定位时触发 Timer -> Control 的 InvalidCastException。
            __instance.GetTree().Root.AddChild(state.Timer);
        }
    }

    private static void CleanupWarning(WarningState state)
    {
        HideWarning(state);
        if (!GodotObject.IsInstanceValid(state.Timer))
            return;

        state.Timer.Stop();
        state.Timer.QueueFree();
    }

    private static void UpdateWarning(NRelicInventory inventory, WarningState state)
    {
        if (!GodotObject.IsInstanceValid(inventory) || !IsEnabled())
        {
            HideWarning(state);
            return;
        }

        var target = inventory.RelicNodes.FirstOrDefault(
            holder => GodotObject.IsInstanceValid(holder)
                      && IsRemovableRarity(holder.Relic.Model));
        if (target == null)
        {
            HideWarning(state);
            return;
        }

        EnsureWarningIcon(state, target);
        if (state.WarningIcon == null)
            return;

        int remainder = ConfigFloatingWindowRunData.WheelOfFortuneCombatCount % CombatInterval;
        int combatsRemaining = CombatInterval - remainder;
        double elapsedSeconds = Time.GetTicksMsec() / 1000.0;
        float pulse = (float)((1.0 - System.Math.Cos(
            elapsedSeconds * System.Math.Tau / combatsRemaining)) * 0.5);
        // 四战周期内依次将警告图标峰值限制为 10% / 20% / 30% / 40%，
        // 保留渐强提示但避免持续抢占玩家注意力。
        float maxOpacity = (remainder + 1) * 0.1f;

        state.WarningIcon.Visible = true;
        state.WarningIcon.Modulate = new Color(1f, 1f, 1f, pulse * maxOpacity);

        // 只在还剩一场时让遗物本身同步闪红；SelfModulate 不覆盖原版状态灰度。
        target.Relic.Icon.SelfModulate = combatsRemaining == 1
            ? Colors.White.Lerp(StsColors.redGlow, pulse)
            : Colors.White;
    }

    private static void EnsureWarningIcon(WarningState state, NRelicInventoryHolder target)
    {
        bool iconInvalid = state.WarningIcon == null
                           || !GodotObject.IsInstanceValid(state.WarningIcon);
        if (iconInvalid)
        {
            var texture = ResourceLoader.Load<Texture2D>(WarningIconPath);
            if (texture == null)
                return;

            state.WarningIcon = new TextureRect
            {
                Name = "PengoTarotWheelRelicWarning",
                Texture = texture,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Size = Vector2.One * WarningIconSize,
            };
            target.AddChild(state.WarningIcon);
        }
        else if (state.Target != target)
        {
            RestoreTargetColor(state);
            state.WarningIcon!.Reparent(target, keepGlobalTransform: false);
        }

        state.Target = target;
        state.WarningIcon!.Position = new Vector2(
            (target.Size.X - WarningIconSize) * 0.5f,
            target.Size.Y - 8f);
    }

    private static void HideWarning(WarningState state)
    {
        if (state.WarningIcon != null && GodotObject.IsInstanceValid(state.WarningIcon))
            state.WarningIcon.Visible = false;
        RestoreTargetColor(state);
        state.Target = null;
    }

    private static void RestoreTargetColor(WarningState state)
    {
        if (state.Target != null
            && GodotObject.IsInstanceValid(state.Target)
            && GodotObject.IsInstanceValid(state.Target.Relic.Icon))
        {
            state.Target.Relic.Icon.SelfModulate = Colors.White;
        }
    }
}
