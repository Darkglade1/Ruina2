using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Encounters.Act3;

namespace Ruina2.Ruina2Code.Events.Act3;

public class RedMist() : Ruina2Event()
{
    public override EncounterModel CanonicalEncounter
    {
        get => ModelDb.Encounter<KaliEncounter>();
    }

    public override bool IsShared => true;
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Fight),
        Option(Flee, HoverTipFactory.FromCardWithCardHoverTips<Guilty>())
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Card", ModelDb.Card<Guilty>().Title)
    ];
    
    public async Task Fight()
    {
        int num = 2;
        List<Reward> rewardList = new List<Reward>(num);
        CollectionsMarshal.SetCount(rewardList, num);
        CollectionsMarshal.AsSpan(rewardList)[0] = new RelicReward(RelicRarity.Rare, Owner!);
        CollectionsMarshal.AsSpan(rewardList)[1] = new GoldReward(100, Owner!);
        EnterCombatWithoutExitingEvent<KaliEncounter>(rewardList, false);
    }

    public async Task Flee()
    {
        var card = Owner!.RunState.CreateCard(ModelDb.Card<Guilty>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("FLEE"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}