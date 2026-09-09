using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class LaetitiaPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature == Owner && (card.EnergyCost.GetResolved() == 0 || card.EnergyCost.GetResolved() == 1))
        {
            if (CombatManager.Instance.History.CardPlaysStarted.Count(e => e.Actor == Owner && e.CardPlay.IsFirstInSeries && e.HappenedThisTurn(CombatState) && (e.CardPlay.Card.EnergyCost.GetResolved() == 0 || e.CardPlay.Card.EnergyCost.GetResolved() == 1)) < Amount)
            {
                return playCount + 1;
            }
        }
        return playCount;
    }

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        return Task.CompletedTask;
    }
}