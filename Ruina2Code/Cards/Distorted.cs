using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Ruina2.Ruina2Code.Cards;

[Pool(typeof(CurseCardPool))]
public class Distorted() : Ruina2Card(-1, CardType.Curse,
    CardRarity.Curse, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Innate, CardKeyword.Eternal];
    
    public override int MaxUpgradeLevel => 0;
}