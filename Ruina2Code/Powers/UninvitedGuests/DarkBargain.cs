using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class DarkBargain() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("DamageBonus", 0), new("Increase",0)];

    private decimal damageBonusToGain;

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer == Owner && props.IsPoweredAttack() && target != null && target.Monster is AbstractRuinaMonster)
        {
            return 1.0M + (DynamicVars["DamageBonus"].BaseValue / 100.0M);
        }
        return 1M;
    }
    
    public override Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer == Owner && target.Player != null && props.IsPoweredAttack())
        {
            if (result.WasFullyBlocked)
            {
                Flash();
                damageBonusToGain += DynamicVars["Increase"].BaseValue;
            }
        }
        return Task.CompletedTask;
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            DynamicVars["DamageBonus"].BaseValue += damageBonusToGain;
            damageBonusToGain = 0;
        }
    }
}