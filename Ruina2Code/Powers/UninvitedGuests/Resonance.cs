using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Argalia;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;
public class Resonance() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars["CardsPlayed"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CardsPlayed",0)];
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        DynamicVars["CardsPlayed"].BaseValue++;
        if (DynamicVars["CardsPlayed"].BaseValue >= Amount)
        {
            DynamicVars["CardsPlayed"].BaseValue = 0;
            if (Owner.Monster is Argalia argalia)
            {
                await argalia.ShiftIntents();
            }
        }
        InvokeDisplayAmountChanged();
    }
}