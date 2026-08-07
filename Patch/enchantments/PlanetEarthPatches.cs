// PengoTarot/Patches/PlanetEarthPatches.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using PengoTarot.Powers;

namespace PengoTarot.Patches
{
    public static class PlanetEarthPatches
    {
        private static readonly FieldInfo _playerField = AccessTools.Field(typeof(PlayerCombatState), "_player");
        private static readonly FieldInfo _maxEnergyBackingField = AccessTools.Field(typeof(Player), "<MaxEnergy>k__BackingField");

        // 防止递归
        private static bool _syncingEnergy;
        private static bool _syncingMax;

        // 能量值同步
        [HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.GainEnergy))]
        internal static class GainEnergy_Patch
        {
            static void Postfix(PlayerCombatState __instance) => SyncEnergy(__instance);
        }

        [HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.LoseEnergy))]
        internal static class LoseEnergy_Patch
        {
            static void Postfix(PlayerCombatState __instance) => SyncEnergy(__instance);
        }

        // 最大能量上限共享（通过重写结果实现）
        [HarmonyPatch(typeof(PlayerCombatState), "get_MaxEnergy")]
        internal static class MaxEnergy_Get_Patch
        {
            static void Postfix(PlayerCombatState __instance, ref int __result)
            {
                if (_syncingMax) return;
                Player? player = _playerField.GetValue(__instance) as Player;
                if (player == null) return;

                var power = player.Creature.GetPower<PlanetEarthPower>();
                if (power == null || power.PairedPlayers.Count == 0) return;

                // 安全获取不含地球的上限之和
                _syncingMax = true;
                PlanetEarthPower.DisableEarthModifier = true;
                try
                {
                    var seen = new HashSet<Player>();
                    int totalNormal = 0;

                    // 包含自己
                    seen.Add(player);
                    totalNormal += player.PlayerCombatState!.MaxEnergy;

                    // 遍历所有唯一配对者的正常上限（不含地球）
                    foreach (var paired in power.PairedPlayers)
                    {
                        if (paired == null || paired.PlayerCombatState == null || !seen.Add(paired))
                            continue;
                        totalNormal += paired.PlayerCombatState.MaxEnergy;
                    }

                    __result = totalNormal;
                }
                finally
                {
                    PlanetEarthPower.DisableEarthModifier = false;
                    _syncingMax = false;
                }
            }
        }

        private static void SyncEnergy(PlayerCombatState state)
        {
            if (_syncingEnergy) return;
            Player? player = _playerField.GetValue(state) as Player;
            if (player == null) return;

            var power = player.Creature.GetPower<PlanetEarthPower>();
            if (power == null || power.PairedPlayers.Count == 0) return;

            int myEnergy = state.Energy;
            _syncingEnergy = true;
            try
            {
                foreach (var paired in power.PairedPlayers)
                {
                    var targetState = paired?.PlayerCombatState;
                    if (targetState != null && targetState.Energy != myEnergy)
                        targetState.Energy = myEnergy;
                }
            }
            finally
            {
                _syncingEnergy = false;
            }
        }
    }
}