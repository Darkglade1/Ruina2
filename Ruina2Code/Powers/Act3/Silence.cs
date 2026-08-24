using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Monsters.Act3.SilentGirl;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class Silence() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    protected override object InitInternalData() => new Data();

    public bool IsReviving => GetInternalData<Data>().isReviving;
    
    public void DoRevive() => GetInternalData<Data>().isReviving = false;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (wasRemovalPrevented || creature != Owner || !(creature.Monster is SilentGirl monster))
            return;
        GetInternalData<Data>().isReviving = true;
        await monster.TriggerDeadState();
    }
    
    public override bool ShouldAllowHitting(Creature creature)
    {
        return creature != Owner || !IsReviving;
    }

    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        return creature != Owner;
    }

    public override bool ShouldStopCombatFromEnding() => true;

    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

    public override bool ShouldOwnerDeathTriggerFatal() => false;
    
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
    
    public class Data
    {
        public bool isReviving;
    }
}