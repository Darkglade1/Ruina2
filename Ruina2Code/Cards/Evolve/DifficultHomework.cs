using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Ruina2.Ruina2Code.Cards.Evolve;

public class DifficultHomework() : EvolveCard(1)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(BaseCardPlays), new("ExtraCards", 2)];
    protected override int BaseCardPlays => 2;

    protected override async Task RewardEffect()
    {
        if (CombatState != null && CombatState.RunState.CurrentRoom is CombatRoom combatRoom)
        {
            for (int i = 0; i < DynamicVars["ExtraCards"].IntValue; i++)
            {
                combatRoom.AddExtraReward(Owner, new CardReward(CardCreationOptions.ForRoom(Owner, RoomType.Monster), 3, Owner));
            }
        }
    }
}