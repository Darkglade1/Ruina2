using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.Evolve;

namespace Ruina2.Ruina2Code.Events.Act1;

public class NestSchool() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(ChallengingHomework, HoverTipFactory.FromCardWithCardHoverTips<ChallengingHomework>()),
        Option(DifficultHomework, HoverTipFactory.FromCardWithCardHoverTips<DifficultHomework>()),
        Option(EasyHomework)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Card1", ModelDb.Card<ChallengingHomework>().Title),
        new StringVar("Card2", ModelDb.Card<DifficultHomework>().Title)
    ];
    
    
    public async Task ChallengingHomework()
    {
        var card = Owner!.RunState.CreateCard(ModelDb.Card<ChallengingHomework>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("CHALLENGING_HOMEWORK"));
    }

    public async Task DifficultHomework()
    {
        var card = Owner!.RunState.CreateCard(ModelDb.Card<DifficultHomework>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("DIFFICULT_HOMEWORK"));
    }
    
    public async Task EasyHomework()
    {
        CardCreationOptions options = new CardCreationOptions([Owner!.Character.CardPool], CardCreationSource.Other, CardRarityOddsType.RegularEncounter);
        var cardReward = new CardReward(options, 3, Owner);
        await RewardsCmd.OfferCustom(Owner, new List<Reward>(1)
        {
            cardReward
        });
        SetEventFinished(PageDescription("EASY_HOMEWORK"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}