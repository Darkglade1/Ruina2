using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act3;

public class Lowell() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Option(Cute, HoverTipFactory.FromRelic<LowellsGift>()), Option(Practical)];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [ 
        new StringVar("Relic", ModelDb.Relic<LowellsGift>().Title.GetFormattedText()),
        new("GoldCost", 120),
        new CardsVar(1)
    ];
    
    public override bool IsAllowed(IRunState runState) => base.IsAllowed(runState) && runState.Players.All(p => p.Gold >= DynamicVars["GoldCost"].IntValue);
    
    public async Task Cute()
    {
        await PlayerCmd.LoseGold(DynamicVars["GoldCost"].BaseValue, Owner!, GoldLossType.Spent);
        await RelicCmd.Obtain(ModelDb.Relic<LowellsGift>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("CUTE"));
    }

    public async Task Practical()
    {
        CardModel? card = (await CardSelectCmd.FromDeckForUpgrade(Owner!, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, DynamicVars.Cards.IntValue))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Upgrade(card);
        }
        SetEventFinished(PageDescription("PRACTICAL"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}