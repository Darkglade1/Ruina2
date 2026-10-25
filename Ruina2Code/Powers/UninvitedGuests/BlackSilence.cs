using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class BlackSilence() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature == Target)
        {
            if (CombatManager.Instance.History.CardPlaysStarted.Count(e =>
                    e.Actor == Target && e.CardPlay.IsFirstInSeries && e.HappenedThisTurn(CombatState)) == 2 && (card.Type == CardType.Attack || card.Type == CardType.Skill))
            {
                return playCount + 1;
            }
        }
        return playCount;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        if (card.Type == CardType.Attack)
        {
            if (Owner.Monster is AbstractAllyMonster allyMonster)
            {
                await allyMonster.AllowApplyPowersToAllies();
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner,  null);
                await allyMonster.DisableApplyPowersToAllies();
            }
        }
    }
}