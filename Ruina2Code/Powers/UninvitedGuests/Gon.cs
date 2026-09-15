using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Gon() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || !props.IsPoweredAttack())
            return 1M;
        return 1.0M - (Amount / 100.0M);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            Flash();
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<Geon>(new ThrowingPlayerChoiceContext(), Owner, Tiph.GEON, Owner,  null);
        }
    }
}