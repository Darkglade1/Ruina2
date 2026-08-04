using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.Act2.RoadHome;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class EasilyDistracted() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars["AttackedCount"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("AttackedCount",0)];

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != null && props.IsPoweredAttack() && target == Owner)
        {
            DynamicVars["AttackedCount"].BaseValue++;
            if (DynamicVars["AttackedCount"].BaseValue >= Amount)
            {
                Flash();
                DynamicVars["AttackedCount"].BaseValue = 0;
                if (Owner.Monster is RoadHome road)
                {
                    road.CancelIntent();
                }
            }
            InvokeDisplayAmountChanged();
        }
    }
    
    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && Amount > 1)
        {
            DynamicVars["AttackedCount"].BaseValue = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}