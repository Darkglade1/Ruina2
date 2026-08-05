using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Fear() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars["AttacksPlayed"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("AttacksPlayed",0), new ("StrengthLoss", 1)];
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack)
        {
            DynamicVars["AttacksPlayed"].BaseValue++;
            if (DynamicVars["AttacksPlayed"].BaseValue >= Amount)
            {
                Flash();
                DynamicVars["AttacksPlayed"].BaseValue = 0;
                await PowerCmd.Apply<FearTempStrLoss>(new ThrowingPlayerChoiceContext(), Owner, DynamicVars["StrengthLoss"].BaseValue, Owner,  null);
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
            DynamicVars["AttacksPlayed"].BaseValue = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}