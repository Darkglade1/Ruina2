using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Enchantments;

public class SteadyHeart : Ruina2Enchantment
{
    public override bool CanEnchant(CardModel card)
    {
        return base.CanEnchant(card) && !card.Keywords.Contains(CardKeyword.Exhaust) && card.Type != CardType.Power;
    }
    
    protected override void OnEnchant()
    {
        Card.AddKeyword(CardKeyword.Retain);
        Card.AddKeyword(CardKeyword.Exhaust);
    }
}