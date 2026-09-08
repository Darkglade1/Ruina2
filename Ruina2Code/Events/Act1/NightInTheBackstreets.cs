using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act1;

public class NightInTheBackstreets() : Ruina2Event()
{
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var tips = HoverTipFactory.FromRelic<LifeFibers>().ToList();
        tips.Add(HoverTipFactory.FromCard<Injury>());

        return [Option(Extract, tips), Option(Observe)];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(18),
        new StringVar("Relic", ModelDb.Relic<LifeFibers>().Title.GetFormattedText()),
        new StringVar("Curse", ModelDb.Card<Injury>().Title),
    ];
    
    public async Task Extract()
    {
        await RelicCmd.Obtain(ModelDb.Relic<LifeFibers>().ToMutable(), Owner!);
        await CardPileCmd.AddCurseToDeck<Injury>(Owner!);
        SetEventFinished(PageDescription("EXTRACT"));
    }

    public async Task Observe()
    {
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.IntValue);
        SetEventFinished(PageDescription("OBSERVE"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}