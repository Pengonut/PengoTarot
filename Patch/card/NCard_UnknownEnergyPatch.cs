#nullable enable

using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;
using PengoTarot.Cards;

namespace PengoTarot.Patch.Card
{
    /// <summary>
    /// 卡牌未遇到（未解锁）时，游戏会在左上角显示 "?" 能量图标
    /// （见 <see cref="NCard.UpdateEnergyCostVisuals"/> 的 Visibility != Visible 分支）。
    /// 我们 mod 的塔罗牌 (TarCard) / 星球牌 (PlanetCard) 基础能量为 -1（无能量消耗），
    /// 此时再显示 "?" 会误导玩家以为存在隐藏费用，因此对这类卡隐藏 "?" 能量显示。
    ///
    /// 生效条件（三者同时满足）：
    ///   - 卡牌处于未解锁状态（Visibility != ModelVisibility.Visible）
    ///   - 卡牌来自本 mod（TarCard / PlanetCard）
    ///   - 卡牌基础能量 Canonical 为 -1
    ///
    /// 实现：隐藏 _energyIcon，并将 _energyLabel 文本置空（与游戏自身隐藏星费用
    /// UpdateStarCostVisuals 的做法一致）。不直接设 _energyLabel.Visible=false，
    /// 以免卡牌恢复可见后 UpdateEnergyCostVisuals 不会重新显示它。
    /// 使用 Harmony 嵌套类模式，确保 PatchAll 100% 发现。
    /// </summary>
    public static class NCard_UnknownEnergyPatch
    {
        [HarmonyPatch(typeof(NCard), "UpdateEnergyCostVisuals")]
        public static class UpdateEnergyCostVisuals
        {
            [HarmonyPostfix]
            internal static void Postfix(NCard __instance)
            {
                // 仅在未解锁的 "?" 分支生效，可见状态保持游戏原逻辑
                if (__instance.Visibility == ModelVisibility.Visible) return;

                var model = __instance.Model;
                if (model is not (TarCard or PlanetCard)) return;
                if (model.EnergyCost.Canonical >= 0) return; // 基础能量不是 -1 的卡不处理

                var energyIcon = Traverse.Create(__instance).Field("_energyIcon").GetValue<TextureRect>();
                if (energyIcon != null) energyIcon.Visible = false;

                var energyLabel = Traverse.Create(__instance).Field("_energyLabel").GetValue<MegaLabel>();
                if (energyLabel != null) energyLabel.SetTextAutoSize(string.Empty);
            }
        }
    }
}
