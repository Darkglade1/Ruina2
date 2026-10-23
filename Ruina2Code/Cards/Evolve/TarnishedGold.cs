using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Ruina2.Ruina2Code.Cards.Evolve;

public class TarnishedGold() : EvolveCard(1)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(BaseCardPlays), new("Gold", 200)];
    protected override int BaseCardPlays => 3;

    protected override async Task RewardEffect()
    {
        await PlayerCmd.GainGold(DynamicVars["Gold"].IntValue, Owner);
    }
}