using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Afflictions;

public class Frozen : Ruina2Affliction
{
    public override bool HasExtraCardText => true;
    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card == Card && card.Affliction is Frozen)
        {
            return false;
        }
        return true;
    }
}