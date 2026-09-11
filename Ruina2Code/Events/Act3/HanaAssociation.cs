using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act3;

public class HanaAssociation() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var tips = HoverTipFactory.FromRelic<FourTrigrams>().ToList();
        tips.Add(HoverTipFactory.FromCard<Regret>());
        return [Option(Myself, tips), Option(You)];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [ 
        new StringVar("Relic", ModelDb.Relic<FourTrigrams>().Title.GetFormattedText()),
        new StringVar("Curse", ModelDb.Card<Regret>().Title),
        new CardsVar(1)
    ];
    
    public async Task Myself()
    {
        await RelicCmd.Obtain(ModelDb.Relic<FourTrigrams>().ToMutable(), Owner!);
        await CardPileCmd.AddCurseToDeck<Regret>(Owner!);
        SetEventFinished(PageDescription("MYSELF"));
    }

    public async Task You()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars.Cards.IntValue);
        await CardPileCmd.RemoveFromDeck((await CardSelectCmd.FromDeckForRemoval(Owner!, prefs)).ToList());
        SetEventFinished(PageDescription("YOU"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}