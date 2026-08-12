using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.Act2.Wrath;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class BlindFury() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars["HPLossCounter"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("HPLossCounter",0)];
    
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        DynamicVars["HPLossCounter"].BaseValue = Amount;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == Owner)
        {
            DynamicVars["HPLossCounter"].BaseValue -= result.UnblockedDamage;
            if (DynamicVars["HPLossCounter"].BaseValue <= 0)
            {
                DynamicVars["HPLossCounter"].BaseValue = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            if (DynamicVars["HPLossCounter"].BaseValue <= 0 && Owner.Monster is ServantOfWrath wrath)
            {
                Flash();
                DynamicVars["HPLossCounter"].BaseValue = Amount;
                InvokeDisplayAmountChanged();
                wrath.Enrage();
            }
        }
    }
}