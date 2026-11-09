using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Ruina2.Ruina2Code.Cards;

[Pool(typeof(CurseCardPool))]
public class LightInjury() : Ruina2Card(-1, CardType.Curse,
    CardRarity.Curse, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new ("Combats", maxCombats)];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
    public override bool CanBeGeneratedByModifiers => false;
    public override int MaxUpgradeLevel => 0;
    
    public const int maxCombats = 3;
    private int _combatsSeen;
    
    [SavedProperty]
    public int CombatsSeen
    {
        get => _combatsSeen;
        set
        {
            AssertMutable();
            _combatsSeen = value;
            DynamicVars["Combats"].BaseValue = (maxCombats - CombatsSeen);
        }
    }

    public override async Task AfterCombatEnd(CombatRoom _)
    {
        CardPile? pile = Pile;
        if ((pile != null ? (pile.Type != PileType.Deck ? 1 : 0) : 1) != 0)
            return;
        CombatsSeen++;
        if (CombatsSeen < maxCombats || Pile?.Type != PileType.Deck)
            return;
        await CardPileCmd.RemoveFromDeck(this);
    }
}