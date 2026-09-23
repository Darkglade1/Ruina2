using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class ContractMightPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => !this.IsCanonical ? this._cardsPlayedThisTurn : 0;
    private int _cardsPlayedThisTurn;

    public bool ShouldPreventCardPlay => this._cardsPlayedThisTurn >= Amount;

    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        return card.Owner != this.Owner.Player || !this.ShouldPreventCardPlay;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == this.Owner.Player)
            ++this._cardsPlayedThisTurn;
        this.InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
    
    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner))
            return Task.CompletedTask;
        _cardsPlayedThisTurn = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}