using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Ruina2.Ruina2Code.Cards.Evolve;

[Pool(typeof(QuestCardPool))]
public abstract class EvolveCard(int cost) :
    Ruina2Card(cost, CardType.Quest, CardRarity.Quest, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    public override int MaxUpgradeLevel => 0;
    
    protected virtual int BaseCardPlays => 0;
    private int _currentCardPlaysLeft = 0;
    private int _cardPlaysDone = 0;
    
    [SavedProperty]
    public int CurrentCardPlaysLeft
    {
        get => _currentCardPlaysLeft;
        set
        {
            AssertMutable();
            _currentCardPlaysLeft = value;
            DynamicVars.Cards.BaseValue = _currentCardPlaysLeft;
        }
    }

    [SavedProperty]
    public int CardPlaysDone
    {
        get => _cardPlaysDone;
        set
        {
            AssertMutable();
            _cardPlaysDone = value;
        }
    }
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        BuffFromPlay(1);
        if (!(DeckVersion is EvolveCard deckVersion))
            return;
        deckVersion.BuffFromPlay(1);
        if (deckVersion.CurrentCardPlaysLeft <= 0 && PileType.Deck.GetPile(Owner).Cards.Contains(deckVersion))
        {
            await RewardEffect();
            await CardPileCmd.RemoveFromDeck([deckVersion]);
        }
    }
    
    public void BuffFromPlay(int cardPlays)
    {
        CardPlaysDone += cardPlays;
        UpdateCardPlays();
    }

    public void UpdateCardPlays() => CurrentCardPlaysLeft = BaseCardPlays - CardPlaysDone;

    protected abstract Task RewardEffect();
}