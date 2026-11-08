using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act3;

public class Wish() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Option(Wish1, HoverTipFactory.FromRelic<DeathWish>()), Option(Wish2, HoverTipFactory.FromRelic<GlitteringWish>()), Option(Wish3, HoverTipFactory.FromRelic<CunningWish>())];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [ 
        new StringVar("Relic1", ModelDb.Relic<DeathWish>().Title.GetFormattedText()),
        new StringVar("Relic2", ModelDb.Relic<GlitteringWish>().Title.GetFormattedText()),
        new StringVar("Relic3", ModelDb.Relic<CunningWish>().Title.GetFormattedText()),
    ];
    
    public async Task Wish1()
    {
        await RelicCmd.Obtain(ModelDb.Relic<DeathWish>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("WISH"));
    }

    public async Task Wish2()
    {
        await RelicCmd.Obtain(ModelDb.Relic<GlitteringWish>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("WISH"));
    }
    
    public async Task Wish3()
    {
        await RelicCmd.Obtain(ModelDb.Relic<CunningWish>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("WISH"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}