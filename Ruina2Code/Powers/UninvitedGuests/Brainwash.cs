using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Brainwash() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    
    private bool hasTriggered;
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Target == cardPlay.Card.Owner.Creature && cardPlay.Card.TargetType == TargetType.AnyEnemy && (cardPlay.Card.Type == CardType.Attack || cardPlay.Card.Type == CardType.Skill))
        {
            if (!hasTriggered)
            {
                Flash();
                hasTriggered = true;
                await CardCmd.Afflict<Afflictions.Brainwash>(cardPlay.Card, 1);
                StopPulsing();
            }
        }
    }
    
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Target)
        {
            hasTriggered = false;
            StartPulsing();
        }
        return Task.CompletedTask;
    }
}