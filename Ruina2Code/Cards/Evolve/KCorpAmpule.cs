using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Ruina2.Ruina2Code.Cards.Evolve;

public class KCorpAmpule() : EvolveCard(1)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(BaseCardPlays)];
    protected override int BaseCardPlays => 4;

    protected override async Task RewardEffect()
    {
        await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp);
    }
}