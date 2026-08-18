
#nullable enable
using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Cards;       
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.BalatroEffect;
using PengoTarot.ConfigFW;
using PengoTarot.Network;
using PengoTarot.Patch.Card;
using PengoTarot.Relics;


namespace PengoTarot
{
    [ModInitializer(nameof(Initialize))]
    public static class ModInitializer
    {
#if STS2_AT_LEAST_0_111_0
        private const string CompatVersion = "0.111.0";
#elif STS2_AT_LEAST_0_110_0
        private const string CompatVersion = "0.110.0";
#else
        private const string CompatVersion = "0.107.0";
#endif

        public static TarotSynchronizer? TarotSync { get; private set; }

        public static void Initialize()
        {
            Log.Info($"[PengoTarot] Initializing (compat target: STS2 v{CompatVersion})");

            var harmony = new Harmony("PengoTarot.Pengo");
            harmony.PatchAll();

            ModHelper.AddModelToPool(typeof(EventRelicPool), typeof(StargazerKit));
            Config.Load();
            EnchantmentConfig.Load();
            EffectRegistry.Initialize();

            GetNodeCallRewriter.ScheduleAfterAllModsLoaded();
            MintyRestHPRenderRewriter.ScheduleAfterModsLoaded();

            NConfigFloatingWindow.ScheduleHintFontsWarmUp();

            Log.Info($"[PengoTarot] Initialization complete. Loaded for STS2 v{CompatVersion}.");
        }


        [HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
        public static class InitTarotSyncPatch
        {
            static void Postfix(RunManager __instance)
            {
                if (__instance.IsSingleplayerOrFakeMultiplayer)
                    return;
                TarotSync?.Dispose();
                TarotSync = null;
                var state = __instance.DebugOnlyGetState();
                if (state == null) return;
                TarotSync = new TarotSynchronizer(
                    __instance.RunLocationTargetedBuffer,
                    __instance.NetService,
                    state,
                    __instance.NetService.NetId);
            }
        }
    }

    
    public static class NodeExtensions
    {
        public static void OnReady(this Node node, Action action)
        {
            if (node.IsNodeReady())
                action();
            else
                node.Ready += action;
        }
    }
}