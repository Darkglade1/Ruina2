using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class TickingTime() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars["CardCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CardCounter",0)];

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        DynamicVars["CardCounter"].BaseValue++;
        InvokeDisplayAmountChanged();
        if (DynamicVars["CardCounter"].IntValue >= Amount)
        {
            DynamicVars["CardCounter"].BaseValue = 0M;
            InvokeDisplayAmountChanged();
            Flash();
            Sfx.SilenceStop.Play(1.0f, 0.9f);
            foreach (var player in CombatState.Players)
            {
                PlayerCmd.EndTurn(player, false);
            }
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            DynamicVars["CardCounter"].BaseValue = 0M;
            InvokeDisplayAmountChanged();
        }
    }
}