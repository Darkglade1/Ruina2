using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Ruina2.Ruina2Code.Cards.Evolve;

public class RustyChest() : EvolveCard(3)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(BaseCardPlays), new("Gold", 400)];
    protected override int BaseCardPlays => 2;

    protected override async Task RewardEffect()
    {
        await PlayerCmd.GainGold(DynamicVars["Gold"].IntValue, Owner);
    }
}