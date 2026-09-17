using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Sharkskin() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars["Count"].IntValue;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Turns", 3), new PowerVar<ArtifactPower>(0), new("Count", 0)];

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target == Owner && props.IsPoweredAttack() && Owner.HasPower<ArtifactPower>())
        {
            return 1.0M - (Amount / 100.0M);
        }
        return 1M;
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            DynamicVars["Count"].BaseValue++;
            if (DynamicVars["Count"].BaseValue >= DynamicVars["Turns"].BaseValue)
            {
                Flash();
                DynamicVars["Count"].BaseValue = 0;
                await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Owner, DynamicVars["ArtifactPower"].BaseValue, Owner,  null);
            }
            InvokeDisplayAmountChanged();
        }
    }
}