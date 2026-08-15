using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class Sin() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CostIncrease", 1)];
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.EnergyCost.GetResolved() <= Amount)
        {
           Flash();
           cardPlay.Card.EnergyCost.AddThisCombat(DynamicVars["CostIncrease"].IntValue);
        }
    }
}