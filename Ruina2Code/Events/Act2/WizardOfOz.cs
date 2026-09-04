using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Ruina2.Ruina2Code.Cards.EGO.Act2;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act2;

public class WizardOfOz() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Option(Accept, HoverTipFactory.FromRelic<FalsePresent>()), Option(Refuse, [HoverTipFactory.FromCard<FalseThrone>(), HoverTipFactory.FromCard<Injury>()])];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [ 
        new StringVar("Relic", ModelDb.Relic<FalsePresent>().Title.GetFormattedText()),
        new StringVar("Card", ModelDb.Card<FalseThrone>().Title),
        new StringVar("Curse", ModelDb.Card<Injury>().Title),
    ];
    
    public async Task Accept()
    {
        await RelicCmd.Obtain(ModelDb.Relic<FalsePresent>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("ACCEPT"));
    }

    public async Task Refuse()
    {
        var card = Owner!.RunState.CreateCard(ModelDb.Card<FalseThrone>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        await CardPileCmd.AddCurseToDeck<Injury>(Owner!);
        SetEventFinished(PageDescription("REFUSE"));
    }
}