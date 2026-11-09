using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Ruina2.Ruina2Code.Monsters.Act2;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class CounterAttack() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars["NumCounters"].IntValue;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("NumCounters",0)];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack)
        {
            if (Owner.Monster is Kim kim)
            {
                Flash();
                await kim.Claim(cardPlay.Card.Owner.Creature);
                DynamicVars["NumCounters"].BaseValue++;
                InvokeDisplayAmountChanged();
            }
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            if (SkipNextDurationTick)
            {
                SkipNextDurationTick = false;
            }
            else
            {
                await PowerCmd.Remove(this);
            }
        }
    }
}