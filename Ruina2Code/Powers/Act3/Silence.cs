using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class Silence() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override bool ShouldAllowHitting(Creature creature)
    {
        if (creature == Owner)
        {
            return creature.CombatState?.CurrentSide == CombatSide.Enemy;
        }
        return true;
    }
}