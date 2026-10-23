using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Ruina2.Ruina2Code.Cards.Evolve;

public class Shovel() : EvolveCard(1)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(BaseCardPlays)];
    protected override int BaseCardPlays => 3;

    protected override async Task RewardEffect()
    {
        if (CombatState != null && CombatState.RunState.CurrentRoom is CombatRoom combatRoom)
        {
            combatRoom.AddExtraReward(Owner, new RelicReward(Owner));
        }
    }
}