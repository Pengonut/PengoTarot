#nullable enable
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(ModelDb), "get_AllCardPools")]
    public static class ModelDb_AllCardPools_Patch
    {
        static void Postfix(ref IEnumerable<CardPoolModel> __result)
        {
            var tarotPool = ModelDb.CardPool<TarotPool>();
            var planetPool = ModelDb.CardPool<PlanetPool>();
            __result = __result.Concat(new CardPoolModel[] { tarotPool, planetPool });
        }
    }
}