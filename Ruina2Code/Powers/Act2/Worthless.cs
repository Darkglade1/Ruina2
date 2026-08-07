using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class Worthless() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker == Owner && command.DamageProps.IsPoweredAttack())
        {
            int totalDamage = 0;
            var damageResults = command.Results.SelectMany(r => r);
            foreach (var damageResult in damageResults)
            {
                if (damageResult.Receiver.IsPlayer)
                {
                    totalDamage += damageResult.UnblockedDamage + damageResult.OverkillDamage;
                }
            }
            if (totalDamage == 0)
            {
                await CreatureCmd.Kill(Owner);
            }
        }
    }
}