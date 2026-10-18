using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act4;

public class Ensemble() : Ruina2Event()
{
    private int heal;
    public override bool IsShared => true;
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var healPercent = AscensionHelper.GetValueIfAscension(AscensionLevel.WearyTraveler, 80, 100);
        heal = (int)((Owner!.Creature.MaxHp - Owner!.Creature.CurrentHp) * (healPercent / 100f));
        
        var option = Option(MakeAStand, HoverTipFactory.FromRelic<BookOfSomeone>());
        option.Description.Add("ActualHeal", heal);
        
        var hoverTips = HoverTipFactory.FromRelic<BookOfSomeone>();
        hoverTips = hoverTips.Concat(HoverTipFactory.FromRelic<BlueScarf>());
        var optionShort = Option(MakeAStandShort, hoverTips);
        optionShort.Description.Add("ActualHeal", heal);
        
        return [option, optionShort, Option(Leave)];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [ 
        new StringVar("Relic", ModelDb.Relic<BookOfSomeone>().Title.GetFormattedText()),
        new StringVar("Scarf", ModelDb.Relic<BlueScarf>().Title.GetFormattedText())
    ];
    
    public async Task MakeAStand()
    {
        await CreatureCmd.Heal(Owner!.Creature, heal);
        await RelicCmd.Obtain(ModelDb.Relic<BookOfSomeone>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("MAKE_A_STAND"));
    }
    
    public async Task MakeAStandShort()
    {
        await CreatureCmd.Heal(Owner!.Creature, heal);
        await RelicCmd.Obtain(ModelDb.Relic<BookOfSomeone>().ToMutable(), Owner!);
        await RelicCmd.Obtain(ModelDb.Relic<BlueScarf>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("MAKE_A_STAND"));
    }
    
    public async Task Leave()
    {
        if (LocalContext.IsMe(Owner))
        {
            await RunManager.Instance.WinRun();   
        }
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is UninvitedGuests;
    }
}