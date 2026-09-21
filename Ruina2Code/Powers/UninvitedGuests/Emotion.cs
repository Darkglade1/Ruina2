using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Emotion() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars["ExhaustCount"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("ExhaustCount",0), new("StrengthAmount", 0),
    new("FirstStrengthThreshold", 0), new("SecondStrengthThreshold", 0)];

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        DynamicVars["ExhaustCount"].BaseValue++;
        if (DynamicVars["ExhaustCount"].BaseValue >= Amount)
        {
            Flash();
            DynamicVars["ExhaustCount"].BaseValue = 0;
            if (Owner.Monster is AbstractAllyMonster allyMonster)
            {
                await allyMonster.AllowApplyPowersToAllies();
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, DynamicVars["StrengthAmount"].BaseValue, Owner,  null); 
                await allyMonster.DisableApplyPowersToAllies();
            }
        }
        InvokeDisplayAmountChanged();
    }
}