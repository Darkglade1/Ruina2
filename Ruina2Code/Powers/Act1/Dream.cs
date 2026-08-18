using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Monsters.Act1.BlackSwan;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Dream() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new ("TotalBrothers", 4)];

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature.Monster is Brother)
        {
            if (Owner.Monster is BlackSwan swan)
            {
                int numAliveBrothers = 0;
                foreach (var enemy in CombatState.HittableEnemies)
                {
                    if (enemy.Monster is Brother && enemy.IsAlive)
                    {
                        numAliveBrothers++;
                    }
                }
                if (numAliveBrothers == 0 && swan.NumActiveBrothers >= 4)
                {
                    Flash();
                    swan.Enraged = true;
                    await CreatureCmd.Stun(Owner, BlackSwan.SHRIEK);
                }
            }
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            if (Owner.Monster is BlackSwan swan && swan.NumActiveBrothers < DynamicVars["TotalBrothers"].BaseValue)
            {
                Flash();
                Sfx.SwanRevive.Play();
                swan.NumActiveBrothers++;
                await CreatureCmd.Add<Brother>(CombatState, "slot" + swan.NumActiveBrothers);
            }
        }
    }
}