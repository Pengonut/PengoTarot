// PengoTarot/Powers/TickTackPower.cs
#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.GameActions;
using ActionSynchronizerCombatState = MegaCrit.Sts2.Core.Entities.Multiplayer.ActionSynchronizerCombatState;

namespace PengoTarot.Powers
{
    public sealed class TickTackPower : PowerModel
    {
        private const string FlashSfx = "event:/sfx/ui/clicks/ui_checkbox_on";
        private const float MinFlashInterval = 0.2f;
        private const float MaxFlashInterval = 1.0f;
        private const float TickInterval = 1.0f;
        private static readonly Color RedTint = new(1f, 0.2f, 0.2f, 1f);

        private Godot.Timer? _redFlashTimer;
        private Godot.Timer? _tickTimer;
        private Godot.Timer? _vfxTimer;
        private Tween? _flashTween;
        private NCreature? _creatureNode;
        private readonly List<float> _flashIntervals = new();
        private int _flashIndex;

        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

        public override Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            BuildFlashSchedule();

            Callable.From(() =>
            {
                if (!base.IsMutable) return;
                var node = NCombatRoom.Instance?.GetCreatureNode(Owner);
                if (node == null || !GodotObject.IsInstanceValid(node)) return;

                _creatureNode = node;
                StartRedFlash(node);
                StartVfxTimer(node);

                var netType = RunManager.Instance.NetService?.Type;
                if (netType == NetGameType.Host || netType == NetGameType.Singleplayer)
                    StartTickTimer(node);
            }).CallDeferred();

            return Task.CompletedTask;
        }

        public override Task AfterRemoved(Creature oldOwner)
        {
            StopRedFlash();
            StopTickTimer();
            StopVfxTimer();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 玩家回合方结束时（敌方回合即将开始前），无条件清空 power，不结束回合。
        /// </summary>
        public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Player && IsMutable && Amount > 0)
            {
                _ = RemoveSilently();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 清空 power 但不触发 EndTurn；由敌方回合切换时调用。
        /// </summary>
        private async Task RemoveSilently()
        {
            if (!IsMutable || Amount <= 0) return;
            StopTickTimer();
            await PowerCmd.Remove(this);
        }

        private void StartRedFlash(NCreature nCreature)
        {
            _redFlashTimer = new Godot.Timer { OneShot = true };
            _redFlashTimer.Timeout += OnFlashTick;
            nCreature.AddChildSafely(_redFlashTimer);
            ScheduleNextFlash();
        }

        private void StopRedFlash()
        {
            _redFlashTimer?.QueueFree();
            _redFlashTimer = null;

            _flashTween?.Kill();
            _flashTween = null;

            _flashIntervals.Clear();
            _flashIndex = 0;

            if (_creatureNode != null && GodotObject.IsInstanceValid(_creatureNode))
            {
                var visuals = _creatureNode.Visuals;
                if (visuals != null && GodotObject.IsInstanceValid(visuals))
                    visuals.Modulate = Colors.White;
            }
            _creatureNode = null;
        }

        private void OnFlashTick()
        {
            if (_creatureNode == null || !GodotObject.IsInstanceValid(_creatureNode))
            {
                StopRedFlash();
                return;
            }

            var visuals = _creatureNode.Visuals;
            if (visuals == null || !GodotObject.IsInstanceValid(visuals)) return;

            bool isRed = visuals.Modulate != Colors.White;
            Color target = isRed ? Colors.White : RedTint;

            var flashTween = visuals.CreateTween();
            flashTween.TweenProperty(visuals, "modulate", target, 0.12f)
                .SetEase(Tween.EaseType.InOut);
            _flashTween = flashTween;

            // Play SFX on every color toggle for a precise, accelerating rhythm
            if (Owner != null && LocalContext.IsMe(Owner))
                SfxCmd.Play(FlashSfx);

            ScheduleNextFlash();
        }

        private void ScheduleNextFlash()
        {
            if (_redFlashTimer == null) return;

            float interval;
            if (_flashIndex < _flashIntervals.Count)
            {
                interval = _flashIntervals[_flashIndex];
                _flashIndex++;
            }
            else
            {
                interval = MinFlashInterval;
            }

            _redFlashTimer.WaitTime = interval;
            _redFlashTimer.Start();
        }

        /// <summary>
        /// Two-phase flash schedule:
        /// Phase 1 (first 50% duration): fixed 1.0s intervals — no acceleration.
        /// Phase 2 (last 50% duration): geometric decay from 1.0s to MinFlashInterval.
        /// The resulting sequence is strictly monotonic decreasing, independent of Amount changes.
        /// </summary>
        private void BuildFlashSchedule()
        {
            _flashIntervals.Clear();
            _flashIndex = 0;

            int startAmount = Amount;
            if (startAmount <= 0) return;

            float totalDuration = startAmount * TickInterval;

            // Phase 1: fixed 1.0s intervals for the first half of total duration
            float halfDuration = totalDuration * 0.5f;
            int phase1Count = Mathf.FloorToInt(halfDuration / MaxFlashInterval);
            for (int i = 0; i < phase1Count; i++)
                _flashIntervals.Add(MaxFlashInterval);

            // Phase 2: geometric decay from MaxFlashInterval to MinFlashInterval
            float remaining = totalDuration - phase1Count * MaxFlashInterval;
            if (remaining <= 0f) return;

            float targetR = (remaining - MaxFlashInterval) / (remaining - MinFlashInterval);
            float logRatio = Mathf.Log(MinFlashInterval / MaxFlashInterval);
            int phase2Count = Mathf.RoundToInt(logRatio / Mathf.Log(targetR)) + 1;
            phase2Count = Mathf.Max(phase2Count, 2);

            float r = Mathf.Pow(MinFlashInterval / MaxFlashInterval, 1f / (phase2Count - 1));
            float d = MaxFlashInterval;
            for (int i = 0; i < phase2Count; i++)
            {
                _flashIntervals.Add(d);
                d = Mathf.Max(d * r, MinFlashInterval);
            }
        }

        private void StartTickTimer(NCreature nCreature)
        {
            _tickTimer = new Godot.Timer { OneShot = false, WaitTime = TickInterval };
            _tickTimer.Timeout += OnTickTimeout;
            nCreature.AddChildSafely(_tickTimer);
            _tickTimer.Start();
        }

        private void StopTickTimer()
        {
            if (_tickTimer == null) return;
            _tickTimer.Stop();
            _tickTimer.QueueFree();
            _tickTimer = null;
        }

        private void OnTickTimeout()
        {
            if (!base.IsMutable || Amount <= 0)
            {
                StopTickTimer();
                return;
            }

            var synchronizer = RunManager.Instance.ActionQueueSynchronizer;
            if (synchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
                return;

            var player = Owner?.Player;
            if (player == null) return;

            synchronizer.RequestEnqueue(new TickTackGameAction(player));
        }

        private void StartVfxTimer(NCreature nCreature)
        {
            _vfxTimer = new Godot.Timer { OneShot = false, WaitTime = TickInterval };
            _vfxTimer.Timeout += OnVfxTick;
            nCreature.AddChildSafely(_vfxTimer);
            _vfxTimer.Start();
        }

        private void StopVfxTimer()
        {
            if (_vfxTimer == null) return;
            _vfxTimer.Stop();
            _vfxTimer.QueueFree();
            _vfxTimer = null;
        }

        private void OnVfxTick()
        {
            // Skip the last tick to avoid a spurious "power removed" VFX
            // that can occur when Amount drops from 1 → 0
            if (!base.IsMutable || Amount <= 1) return;
            if (_creatureNode == null || !GodotObject.IsInstanceValid(_creatureNode)) return;
            if (Owner == null || !LocalContext.IsMe(Owner)) return;

            ShakeCreature(_creatureNode, Amount);
            var vfx = NPowerAppliedVfx.Create(this, Amount, isBuff: false);
            if (vfx != null)
            {
                _creatureNode.AddChildSafely(vfx);
                vfx.GlobalPosition = NCombatRoom.Instance!.Size / 2;
            }
        }

        private static void ShakeCreature(NCreature nCreature, int remaining)
        {
            float intensity = Mathf.Clamp(30f / remaining, 5f, 30f);
            float duration = 0.3f;

            var originalPos = nCreature.Position;
            var tween = nCreature.CreateTween();
            tween.TweenMethod(Callable.From((float t) =>
            {
                nCreature.Position = originalPos + Vector2.Right * intensity * Mathf.Sin(t * 12f) * Mathf.Sin(t * 1.5f);
            }), 0f, Mathf.Pi * 2f, duration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);

            tween.TweenCallback(Callable.From(() => nCreature.Position = originalPos));
        }
    }
}
