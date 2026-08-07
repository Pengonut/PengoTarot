using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
#if STS2_AT_LEAST_0_110_0
using MegaCrit.Sts2.Core.Entities.Players;
#endif
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace PengoTarot
{
    internal sealed class SilentPlayerChoiceContext : PlayerChoiceContext
    {
#if STS2_AT_LEAST_0_110_0
        public override ulong? OwnerId => null;

        public override Task SignalPlayerChoiceBegun(Player chooser, PlayerChoiceOptions options)
            => Task.CompletedTask;
#else
        public override Task SignalPlayerChoiceBegun(PlayerChoiceOptions options)
            => Task.CompletedTask;
#endif

        public override Task SignalPlayerChoiceEnded()
            => Task.CompletedTask;
    }
}