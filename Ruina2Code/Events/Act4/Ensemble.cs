using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act4;

public class Ensemble() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Option(MakeAStand,HoverTipFactory.FromRelic<BookOfSomeone>())];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [ 
        new StringVar("Relic", ModelDb.Relic<BookOfSomeone>().Title.GetFormattedText())
    ];
    
    public async Task MakeAStand()
    {
        await RelicCmd.Obtain(ModelDb.Relic<BookOfSomeone>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("MAKE_A_STAND"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is UninvitedGuests;
    }
}