using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Ruina2.Ruina2Code.Monsters.Act1.Laetitia;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class SurprisePresent() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Owner)
        {
            bool laetitiaAlive = false;
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy.Monster is Laetitia && enemy.IsAlive)
                {
                    laetitiaAlive = true;
                }
            }
            if (laetitiaAlive)
            {
                await CreatureCmd.Add<WitchFriend>(CombatState, "minion" + Amount);
            }
        }
    }
}