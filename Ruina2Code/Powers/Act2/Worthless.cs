using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class Worthless() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker == Owner)
        {
            var totalDamage = command.Results.SelectMany(r => r)
                .Sum((Func<DamageResult, int>)(r => r.UnblockedDamage + r.OverkillDamage));
            if (totalDamage == 0)
            {
                await CreatureCmd.Kill(Owner);
            }
        }
    }
}