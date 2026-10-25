using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Cards.EnemyCards;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class BlueReverb() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    public override int DisplayAmount => CombatManager.Instance.History.CardPlaysFinished.Count(e =>
        e.Actor == Target && e.CardPlay.IsFirstInSeries && !e.CardPlay.IsAutoPlay && e.HappenedThisTurn(CombatState));
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource != null && cardSource.Owner.Creature == Target && (cardPlay == null || !cardPlay.IsAutoPlay))
        {
            if (props.IsPoweredAttack() && EnemyCard.EnemyCardOwner.Get(cardSource) == null)
            {
                if (CombatManager.Instance.History.CardPlaysFinished.Count(e =>
                        e.Actor == Target && e.CardPlay.IsFirstInSeries && !e.CardPlay.IsAutoPlay && e.HappenedThisTurn(CombatState)) % 2 == 1 && (cardPlay == null || cardPlay.IsFirstInSeries))
                {
                    return 0M;
                }
            }
        }
        return 1M;
    }

    public override Decimal ModifyBlockMultiplicative(
        Creature target,
        Decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource != null && cardSource.Owner.Creature == Target && (cardPlay == null || !cardPlay.IsAutoPlay))
        {
            if (props.IsPoweredCardOrMonsterMoveBlock() && EnemyCard.EnemyCardOwner.Get(cardSource) == null)
            {
                if (CombatManager.Instance.History.CardPlaysFinished.Count(e =>
                        e.Actor == Target && e.CardPlay.IsFirstInSeries && !e.CardPlay.IsAutoPlay && e.HappenedThisTurn(CombatState)) % 2 == 1 && (cardPlay == null || cardPlay.IsFirstInSeries))
                {
                    return 0M;
                }
            }
        }
        return 1M;
    }
    
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Target)
        {
            if (DisplayAmount % 2 == 1)
            {
                StartPulsing();
            }
            else
            {
                StopPulsing();
            }
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
    
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Target)
        {
            StopPulsing();
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}