using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers;

public class Paralysis() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    private static decimal BASE_REDUCTION = 0.5M;
    private static decimal STACK_REDUCTION = 0.25M;
    private static int MAX_STACKS = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new ("DamageReduction", BASE_REDUCTION * 100)];
    
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (Owner.CombatState != null && power is Paralysis && Owner == power.Owner)
        {
            if (power.Amount > MAX_STACKS)
            {
                power.Amount = MAX_STACKS;
            }
            DynamicVars["DamageReduction"].BaseValue = (BASE_REDUCTION + STACK_REDUCTION * (Amount - 1)) * 100;
        }
    }
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer == Owner && props.IsPoweredAttack()) {
            return (BASE_REDUCTION - (STACK_REDUCTION * (Amount - 1)));
        }
        return 1M;
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            await PowerCmd.Remove<Paralysis>(Owner);
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            await PowerCmd.Remove<Paralysis>(Owner);
        }
    }
}