using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.Act3.PunshingBird;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class Punishment() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != null && target == Owner && props.IsPoweredAttack() && cardSource != null)
        {
            if (Owner.Monster is PunishingBird bird)
            {
                bird.Enraged = true;
                StartPulsing();
            }
        }
        return Task.CompletedTask;
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            StopPulsing();
        }
    }
}