using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class BookOfSomeone() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(30)];

    public override async Task AfterCombatVictory(CombatRoom _)
    {
        Flash();
        await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp * (DynamicVars.Heal.BaseValue / 100M));
    }
}