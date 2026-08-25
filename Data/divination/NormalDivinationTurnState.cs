#nullable enable

using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PengoTarot.Data.Divination;

/// <summary>
/// 普通房占卜的确定性回合状态。状态直接从双方都会记录的战斗历史推导，
/// 不保存无法随完整战斗状态同步的私有布尔字段。
/// </summary>
public static class NormalDivinationTurnState
{
    public static int CountCardsPlayedThisTurn(Creature owner, CardType type)
    {
        if (owner.Player == null)
            return 0;

        return CombatManager.Instance.History.CardPlaysStarted.Count(
            (CardPlayStartedEntry entry) =>
                entry.HappenedThisTurn(owner.CombatState)
                && entry.Actor == owner
                && entry.CardPlay.Card.Type == type);
    }

    public static bool HasPlayedCardThisTurn(Creature owner, CardType type)
        => CountCardsPlayedThisTurn(owner, type) > 0;
}
