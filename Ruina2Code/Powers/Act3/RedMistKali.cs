using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Powers.Act3;
public class RedMistKali() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override string CustomPackedIconPath => "red_mist.png".PowerImagePath();
    public override string CustomBigIconPath => "red_mist.png".BigPowerImagePath();

    private int strengthToGain = 0;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker == Owner && command.DamageProps.IsPoweredAttack())
        {
            var damageResults = command.Results.SelectMany(r => r);
            foreach (var damageResult in damageResults)
            {
                if (damageResult.UnblockedDamage > 0 && damageResult.Receiver.IsPlayer)
                {
                    Flash();
                    strengthToGain += Amount;
                    return;
                }
            }
        }
    }
    
    public override async Task AfterSideTurnEndLate(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner) && strengthToGain > 0)
        {
            Flash();
            await PowerCmd.Apply<KaliTempStr>(new ThrowingPlayerChoiceContext(), Owner, strengthToGain, Owner,  null);
            strengthToGain = 0;
        }
    }
}