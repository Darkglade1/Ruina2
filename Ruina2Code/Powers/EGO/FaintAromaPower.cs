using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class FaintAromaPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != null && (command.Attacker == Owner || command.Attacker.PetOwner == Owner.Player) && command.ModelSource is CardModel && CombatManager.Instance.History.CardPlaysStarted.Count(e => (e.Actor == Owner || e.Actor.PetOwner == Owner.Player) && e.CardPlay.IsFirstInSeries && e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Type == CardType.Attack) == 1)
        {
            Flash();
            foreach (var result in command.Results)
            {
                foreach (var innerResult in result)
                {
                    if (innerResult.Props.IsPoweredAttack() && command.ModelSource is CardModel cardModel &&
                        cardModel.Type == CardType.Attack)
                    {
                        await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), innerResult.Receiver, innerResult.TotalDamage * (Amount / 100M), command.Attacker, null);
                    }
                }
            }
        }
    }
}