using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Afflictions;

public class Judas : Ruina2Affliction
{
    public override bool HasExtraCardText => true;
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource != null && cardSource == Card && Card.Affliction is Judas && props.IsPoweredAttack())
        {
            return 2;
        }
        return 1;
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == Card && cardPlay.Card.Affliction is Judas)
        {
            await CreatureCmd.Damage(choiceContext, Card.Owner.Creature, Amount,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Card, cardPlay);
        }
    }
}