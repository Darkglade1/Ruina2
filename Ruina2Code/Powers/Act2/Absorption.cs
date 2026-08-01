using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Ruina2.Ruina2Code.Monsters.Act2.mountain;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class Absorption() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (wasRemovalPrevented || creature != Owner || !(creature.Monster is Mountain monster))
            return;
        monster.IsReviving = true;
        await monster.TriggerDeadState();
    }
    
    public override bool ShouldAllowHitting(Creature creature)
    {
        var isReviving = false;
        if (Owner.Monster is Mountain monster)
        {
            isReviving = monster.IsReviving;
        }
        return creature != Owner || !isReviving;
    }

    public override bool ShouldStopCombatFromEnding()
    {
        if (Owner.Monster is Mountain mountain && !mountain.CanLose)
        {
            return true;
        }
        return false;
    }

    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        return creature != Owner;
    }

    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
}