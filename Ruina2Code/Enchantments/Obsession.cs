using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Enchantments;

public class Obsession : Ruina2Enchantment
{
    public override bool HasExtraCardText => true;
    
    public override bool ShouldGlowRed => true;
    
    public override bool CanEnchant(CardModel card)
    {
        return base.CanEnchant(card) && !card.Keywords.Contains(CardKeyword.Unplayable);
    }
    
    protected override void OnEnchant()
    {
        Card.AddKeyword(CardKeyword.Innate);
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card.Owner != Card.Owner)
            return true;
        CardPile? pile = Card.Pile;
        return (pile != null ? (pile.Type != PileType.Hand ? 1 : 0) : 1) != 0 || card.Enchantment is Obsession || autoPlayType != AutoPlayType.None;
    }
   
}