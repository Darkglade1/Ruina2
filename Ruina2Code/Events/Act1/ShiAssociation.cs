using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act1;

public class ShiAssociation() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Rush, HoverTipFactory.FromRelic<Overexertion>()),
        Option(Prepare)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Relic", ModelDb.Relic<Overexertion>().Title.GetFormattedText()),
        new CardsVar(1)
    ];
    
    public async Task Rush()
    {
        await RelicCmd.Obtain(ModelDb.Relic<Overexertion>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("RUSH"));
    }

    public async Task Prepare()
    {
        CardModel? card = (await CardSelectCmd.FromDeckForUpgrade(Owner!, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, DynamicVars.Cards.IntValue))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Upgrade(card);
        }
        SetEventFinished(PageDescription("PREPARE"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}