#nullable enable

using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    /// <summary>
    /// 读档/多人同步重建塔罗奖励：识别 TarotReward 的自定义 RewardType → 重建 <see cref="TarotReward"/>。
    /// 原版 <see cref="Reward.FromSerializable"/> 的 switch 对未知类型会抛 NotImplementedException，
    /// 因此必须在 Prefix 拦截（return false 跳过原方法）。
    /// </summary>
    [HarmonyPatch(typeof(Reward), nameof(Reward.FromSerializable))]
    public static class RewardFromSerializableTarotPatch
    {
        [HarmonyPrefix]
        static bool Prefix(SerializableReward save, Player player, ref Reward __result)
        {
            if (TarotReward.TryGetFlagFromRewardType(save.RewardType, out var flagIndex))
            {
                __result = new TarotReward(player, flagIndex);
                return false;
            }
            return true;
        }
    }
}
