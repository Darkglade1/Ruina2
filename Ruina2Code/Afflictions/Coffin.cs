using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Afflictions;

public class Coffin : Ruina2Affliction
{
    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card == Card && card.Affliction is Coffin)
        {
            return false;
        }
        return true;
    }
    
    public override void AfterApplied()
    {
        Card.AddKeyword(CardKeyword.Ethereal);
    }

    public override void BeforeRemoved()
    {
        Card.RemoveKeyword(CardKeyword.Ethereal);
    }
}