using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Brainwash() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Afflictions.Brainwash>();
    
    private bool hasTriggered;
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Target == cardPlay.Card.Owner.Creature && (cardPlay.Card.TargetType == TargetType.AnyEnemy || cardPlay.Card.TargetType == Afflictions.Brainwash.AnyRuinaAlly) && (cardPlay.Card.Type == CardType.Attack || cardPlay.Card.Type == CardType.Skill))
        {
            if (!hasTriggered && cardPlay.IsFirstInSeries)
            {
                Flash();
                hasTriggered = true;
                if (cardPlay.Card.Affliction == null)
                {
                    await CardCmd.Afflict<Afflictions.Brainwash>(cardPlay.Card, 1);
                }
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
    
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation cardLocation)
    {
        if (Target == card.Owner.Creature && (card.TargetType == TargetType.AnyEnemy || card.TargetType == Afflictions.Brainwash.AnyRuinaAlly) &&
            (card.Type == CardType.Attack || card.Type == CardType.Skill))
        {
            if (!hasTriggered)
            {
                return new CardLocation(card.Owner, PileType.Draw, CardPilePosition.Random);
            }
        }
        return cardLocation;
    }
}