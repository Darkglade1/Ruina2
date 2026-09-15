using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Ri() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            Flash();
            await PowerCmd.Apply<DrawCardsNextTurnPower>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, Amount, Owner,  null);
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<Gon>(new ThrowingPlayerChoiceContext(), Owner, Tiph.GON, Owner,  null);
        }
    }
}