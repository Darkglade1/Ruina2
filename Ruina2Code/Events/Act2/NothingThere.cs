using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act2;

public class NothingThere() : Ruina2Event()
{
    private int goldReward;
    private int MIN_GOLD_REWARD = 80;
    private int MAX_GOLD_REWARD = 120;
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var tips = HoverTipFactory.FromRelic<Goodbye>().ToList();
        tips.Add(HoverTipFactory.FromCard<SporeMind>());
        goldReward = Owner!.PlayerRng.Rewards.NextInt(MIN_GOLD_REWARD, MAX_GOLD_REWARD + 1);
        var nearOption = Option(Near);
        nearOption.Description.Add("Gold", goldReward);

        return [Option(Far, tips), nearOption];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [ 
        new StringVar("Relic", ModelDb.Relic<Goodbye>().Title.GetFormattedText()),
        new StringVar("Curse", ModelDb.Card<SporeMind>().Title),
    ];
    
    public async Task Far()
    {
        await RelicCmd.Obtain(ModelDb.Relic<Goodbye>().ToMutable(), Owner!);
        await CardPileCmd.AddCurseToDeck<SporeMind>(Owner!);
        SetEventFinished(PageDescription("FAR"));
    }

    public async Task Near()
    {
        await PlayerCmd.GainGold(goldReward, Owner!);
        SetEventFinished(PageDescription("NEAR"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}