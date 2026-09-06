using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Events.Act1;

public class GalaxyChild() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Accept),
        Option(Reject, HoverTipFactory.FromCardWithCardHoverTips<Decay>())
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Curse", ModelDb.Card<Decay>().Title)
    ];
    
    public async Task Accept()
    {
        await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(Owner!, RelicRarity.Common).ToMutable(), Owner!);
        SetEventFinished(PageDescription("ACCEPT"));
    }

    public async Task Reject()
    {
        var egoCards = EGOCardPool.GetAct1EgoCards();
        egoCards.StableShuffle(Owner!.PlayerRng.Rewards);
        var cards = new List<CardModel>()
        {
            Owner.RunState.CreateCard(egoCards[0], Owner),
            Owner.RunState.CreateCard(egoCards[1], Owner),
            Owner.RunState.CreateCard(egoCards[2], Owner)
        };
        CardReward cardReward = new CardReward(cards, CardCreationSource.Other, Owner,
            CardCreationOptions
                .ForNonCombatWithDefaultOdds([ModelDb.CardPool<EGOCardPool>()],
                    (Func<CardModel, bool>)(c => c.Rarity == CardRarity.Rare))
                .WithFlags(CardCreationFlags.NoRarityModification));
        await RewardsCmd.OfferCustom(Owner, new List<Reward>(1)
        {
            cardReward
        });
        await CardPileCmd.AddCurseToDeck<Decay>(Owner!);
        SetEventFinished(PageDescription("REJECT"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}