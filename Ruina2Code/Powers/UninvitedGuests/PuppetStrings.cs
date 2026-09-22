using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Puppeteer;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class PuppetStrings() : Ruina2Power
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
        if (wasRemovalPrevented || creature != Owner || !(creature.Monster is Puppet puppet))
            return;
        puppet.IsReviving = true;
        await puppet.TriggerDeadState();
    }
    
    public override bool ShouldAllowHitting(Creature creature)
    {
        var isReviving = false;
        if (Owner.Monster is Puppet puppet)
        {
            isReviving = puppet.IsReviving;
        }
        return creature != Owner || !isReviving;
    }

    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        return creature != Owner;
    }

    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
    
    public override bool ShouldPowerBeRemovedOnDeath(PowerModel power)
    {
        if (power.Owner == Owner)
        {
            if (power.Type == PowerType.Debuff)
            {
                return true;
            }
        }
        return false;
    }
}