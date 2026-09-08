using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.Quests;

namespace Ruina2.Ruina2Code.Events.Act2;

public class Messenger() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Deliver, HoverTipFactory.FromCardWithCardHoverTips<Prescript>()),
        Option(Discard, HoverTipFactory.FromCardWithCardHoverTips<PoorSleep>())
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Quest", ModelDb.Card<Prescript>().Title),
        new StringVar("Curse", ModelDb.Card<PoorSleep>().Title),
        new("RemoveCards", 2)
    ];
    
    public async Task Deliver()
    {
        var card = Owner!.RunState.CreateCard(ModelDb.Card<Prescript>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("DELIVER"));
    }

    public async Task Discard()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars["RemoveCards"].IntValue);
        await CardPileCmd.RemoveFromDeck((await CardSelectCmd.FromDeckForRemoval(Owner!, prefs)).ToList());
        await CardPileCmd.AddCurseToDeck<PoorSleep>(Owner!);
        SetEventFinished(PageDescription("DISCARD"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}