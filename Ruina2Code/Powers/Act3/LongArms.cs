using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Monsters.Act3.Twilight;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class LongArms() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;
     
    public override async Task AfterSideTurnEndLate(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            bool triggered = false;
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy.Monster is LongEgg || enemy.Monster is Twilight)
                {
                    var debuffs = new List<PowerModel>();
                    foreach (var power in enemy.Powers)
                    {
                        if (power.Type == PowerType.Debuff)
                        {
                            debuffs.Add(power);
                        }
                    }
                    foreach (var debuff in debuffs)
                    {
                        triggered = true;
                        await PowerCmd.Remove(debuff);
                    }
                }
            }
            if (triggered)
            {
                Flash();
            }
        }
    }
}