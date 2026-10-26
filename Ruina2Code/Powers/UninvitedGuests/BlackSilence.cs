using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Cards.EnemyCards;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class BlackSilence() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public override int DisplayAmount => Math.Min(CombatManager.Instance.History.CardPlaysFinished.Count(e =>
        e.Actor == Target && e.HappenedThisTurn(CombatState)), 3);

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource != null && cardSource.Owner.Creature == Target)
        {
            if (props.IsPoweredAttack() && EnemyCard.EnemyCardOwner.Get(cardSource) == null)
            {
                if (CombatManager.Instance.History.CardPlaysFinished.Count(e =>
                        e.Actor == Target && e.HappenedThisTurn(CombatState)) == 2)
                {
                    return 2M;
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
        if (cardSource != null && cardSource.Owner.Creature == Target)
        {
            if (props.IsPoweredCardOrMonsterMoveBlock() && EnemyCard.EnemyCardOwner.Get(cardSource) == null)
            {
                if (CombatManager.Instance.History.CardPlaysFinished.Count(e =>
                        e.Actor == Target && e.HappenedThisTurn(CombatState)) == 2)
                {
                    return 2M;
                }
            }
        }
        return 1M;
    }
    
    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        if (card.Owner.Creature == Target && CombatManager.Instance.History.CardPlaysFinished.Count(e =>
                e.Actor == Target && e.HappenedThisTurn(CombatState)) == 2)
        {
            modifiedCost = originalCost - DynamicVars.Energy.IntValue;
            return true;
        }
        modifiedCost = originalCost;
        return false;
    }
    
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Target)
        {
            if (DisplayAmount == 2)
            {
                StartPulsing();
            }
            else
            {
                StopPulsing();
            }
            if (DisplayAmount <= 3)
            {
                InvokeDisplayAmountChanged();
            }
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