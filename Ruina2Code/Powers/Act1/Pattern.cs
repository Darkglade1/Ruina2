using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Pattern() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars["DamageCounter"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("DamageCounter",0)];
    
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer == Owner && props.IsPoweredAttack())
        {
            DynamicVars["DamageCounter"].BaseValue += result.UnblockedDamage;
            if (DynamicVars["DamageCounter"].BaseValue >= Amount)
            {
                Flash();
                await PowerCmd.Remove<HelperTempStr>(Owner);
                DynamicVars["DamageCounter"].BaseValue = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
    
    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            DynamicVars["DamageCounter"].BaseValue = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}