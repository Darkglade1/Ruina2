using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Enchantments;

public class Vanishing : Ruina2Enchantment
{
    public override bool CanEnchant(CardModel card)
    {
        return base.CanEnchant(card) && !card.Keywords.Contains(CardKeyword.Exhaust) && card.Type != CardType.Power;
    }
    
    protected override void OnEnchant()
    {
        Card.AddKeyword(CardKeyword.Exhaust);
        Card.AssertMutable();
        if (Card.EnergyCost.CostsX)
            return;
        int num = Card.EnergyCost._base;
        int newBaseCost = Math.Max(Card.EnergyCost._base - Amount, 0);
        if (newBaseCost < num)
        {
            foreach (LocalCostModifier localModifier in Card.EnergyCost._localModifiers)
            {
                if (localModifier.Type == LocalCostType.Absolute && localModifier.Amount > newBaseCost)
                    localModifier.Amount = newBaseCost;
            }
        }
        Card.EnergyCost.SetCustomBaseCost(newBaseCost);
    }
}