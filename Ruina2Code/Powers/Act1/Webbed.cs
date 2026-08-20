using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Webbed() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner.Player != null && side == CombatSide.Player)
        {
            if (participants.Contains(Owner))
            {
                await PowerCmd.Remove<Webbed>(Owner);   
            }
        }
    }
    
    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner.Creature == Owner && card.Type == CardType.Attack)
        {
            return false;
        }
        return true;
    }
}