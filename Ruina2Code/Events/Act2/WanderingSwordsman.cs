using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.CardSelection;
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
using Ruina2.Ruina2Code.Encounters.Act2;

namespace Ruina2.Ruina2Code.Events.Act2;

public class WanderingSwordsman() : Ruina2Event()
{
    public override EventLayoutType LayoutType => EventLayoutType.Combat;
    
    public override EncounterModel CanonicalEncounter
    {
        get => ModelDb.Encounter<KimEncounter>();
    }

    public override bool IsShared => true;
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Duel),
        Option(Flee, HoverTipFactory.FromCardWithCardHoverTips<Shame>())
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Card", ModelDb.Card<Shame>().Title),
        new CardsVar(2)
    ];
    
    public async Task Duel()
    {
        int num = 3;
        List<Reward> rewardList = new List<Reward>(num);
        CollectionsMarshal.SetCount(rewardList, num);
        CollectionsMarshal.AsSpan(rewardList)[0] = new RelicReward(RelicRarity.Uncommon, Owner!);
        CollectionsMarshal.AsSpan(rewardList)[1] = new RelicReward(RelicRarity.Uncommon, Owner!);
        CollectionsMarshal.AsSpan(rewardList)[2] = new GoldReward(50, Owner!);
        EnterCombatWithoutExitingEvent<KimEncounter>(rewardList, false);
    }

    public async Task Flee()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars.Cards.IntValue);
        await CardPileCmd.RemoveFromDeck((await CardSelectCmd.FromDeckForRemoval(Owner!, prefs)).ToList());
        var card = Owner!.RunState.CreateCard(ModelDb.Card<Shame>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("FLEE"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}