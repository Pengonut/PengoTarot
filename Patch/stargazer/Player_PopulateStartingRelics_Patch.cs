#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;
using PengoTarot.Relics;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewMultiplayer))]
    public static class RunManager_SetUpNewMultiplayer_Patch
    {
        static void Postfix(RunState state)
        {
            // 星球牌关闭时，不发放观星套组（望远镜套装）——读本局配置
            if (!ConfigFloatingWindowRunData.PlanetEnabled)
                return;

            foreach (var player in state.Players)
            {
                if (player.GetRelic<StargazerKit>() == null)
                {
                    var kit = ModelDb.Relic<StargazerKit>().ToMutable();
                    player.AddRelicInternal(kit);
                }
            }
        }
    }
}