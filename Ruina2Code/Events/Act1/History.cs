using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act1;

public class History() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Closer, HoverTipFactory.FromRelic<ScorchingWound>()),
        Option(Safe)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("UpgradedCardsNum",15),
        new ("CardsNum",10),
        new CardsVar(1),
        new StringVar("Relic", ModelDb.Relic<ScorchingWound>().Title.GetFormattedText())
    ];
    
    public async Task Closer()
    {
        CardCreationOptions options = CardCreationOptions.ForNonCombatWithDefaultOdds([Owner!.Character.CardPool]);
        List<CardCreationResult> list = CardFactory.CreateForReward(Owner!, DynamicVars["UpgradedCardsNum"].IntValue, options).ToList();
        foreach (CardCreationResult card in list)
        {
            CardCmd.Upgrade(card.Card);
        }
        CardSelectorPrefs prefs = new CardSelectorPrefs(L10NLookup("RUINA2-HISTORY.pages.selectionScreenPrompt"), DynamicVars.Cards.IntValue);
        await SelectCardsToAddToDeckFromGrid(list, prefs);
        await RelicCmd.Obtain(ModelDb.Relic<ScorchingWound>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("CLOSER"));
    }

    public async Task Safe()
    {
        CardCreationOptions options = CardCreationOptions.ForNonCombatWithDefaultOdds([Owner!.Character.CardPool]);
        List<CardCreationResult> list = CardFactory.CreateForReward(Owner!, DynamicVars["CardsNum"].IntValue, options).ToList();
        CardSelectorPrefs prefs = new CardSelectorPrefs(L10NLookup("RUINA2-HISTORY.pages.selectionScreenPrompt"), DynamicVars.Cards.IntValue);
        await SelectCardsToAddToDeckFromGrid(list, prefs);
        SetEventFinished(PageDescription("SAFE"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}