// PengoTarot/Patches/TickTackOverlayPatch.cs
#nullable enable
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using PengoTarot.Powers;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
    public static class TickTackOverlayPatch
    {
        internal const string FlashSfx = "event:/sfx/ui/clicks/ui_checkbox_on";
        internal const float MinFlashInterval = 0.1f;
        internal const float MaxFlashInterval = 0.5f;
        internal const float OverlayAlpha = 0.25f;

        static void Postfix(NCreature __instance)
        {
            var driver = new TickTackOverlayDriver(__instance);
            __instance.AddChildSafely(driver);
        }
    }

    internal partial class TickTackOverlayDriver : Node
    {
        private readonly NCreature _creature;
        private ColorRect? _overlay;
        private float _flashTimer;
        private bool _overlayVisible;

        public TickTackOverlayDriver(NCreature creature)
        {
            _creature = creature;
        }

        public override void _EnterTree()
        {
            _overlay = new ColorRect
            {
                Name = "TickTackOverlay",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Color = new Color(1f, 0f, 0f, 0f)
            };
            _creature.AddChildSafely(_overlay);
            _overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _overlay.ZIndex = 100;
        }

        public override void _Process(double delta)
        {
            if (_overlay == null) return;

            var creature = _creature.Entity;
            if (creature == null || !creature.IsPlayer)
            {
                _overlay.Color = new Color(1f, 0f, 0f, 0f);
                _flashTimer = 0f;
                return;
            }

            var tickTack = creature.GetPower<TickTackPower>();
            if (tickTack == null || tickTack.Amount <= 0)
            {
                _overlay.Color = new Color(1f, 0f, 0f, 0f);
                _flashTimer = 0f;
                return;
            }

            int amount = tickTack.Amount;
            float interval = Mathf.Clamp(amount * 0.08f,
                TickTackOverlayPatch.MinFlashInterval,
                TickTackOverlayPatch.MaxFlashInterval);

            _flashTimer -= (float)delta;
            if (_flashTimer <= 0f)
            {
                _flashTimer = interval;
                _overlayVisible = !_overlayVisible;

                if (_overlayVisible)
                {
                    _overlay.Color = new Color(1f, 0f, 0f, TickTackOverlayPatch.OverlayAlpha);
                }
                else
                {
                    _overlay.Color = new Color(1f, 0f, 0f, 0f);
                }
            }
        }
    }
}
