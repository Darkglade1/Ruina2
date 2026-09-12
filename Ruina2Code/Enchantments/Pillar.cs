using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Enchantments;

public class Pillar : Ruina2Enchantment
{
    public override bool ShowAmount => true;

    public override bool CanEnchant(CardModel card) => base.CanEnchant(card) && card.GainsBlock;
    
    protected override void OnEnchant()
    {
        Card.AddKeyword(CardKeyword.Ethereal);
    }
    
    public override Decimal EnchantBlockAdditive(Decimal originalBlock) => Amount;
}