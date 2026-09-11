using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Events.Act3;

public class DistortedYan() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(GiveIn, HoverTipFactory.FromCardWithCardHoverTips<Distorted>()),
        Option(GiveUp).ThatDoesDamage(DynamicVars.HpLoss.IntValue)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Curse", ModelDb.Card<Distorted>().Title),
        new HpLossVar(8)
    ];
    
    
    public async Task GiveIn()
    {
        var egoCards = EGOCardPool.GetAct3EgoCards();
        egoCards.StableShuffle(Owner!.PlayerRng.Rewards);
        var cards = new List<CardModel>()
        {
            Owner.RunState.CreateCard(egoCards[0], Owner),
            Owner.RunState.CreateCard(egoCards[1], Owner),
            Owner.RunState.CreateCard(egoCards[2], Owner)
        };
        foreach (var card in cards)
        {
            CardCmd.Upgrade(card);
        }
        CardReward cardReward = new CardReward(cards, CardCreationSource.Other, Owner,
            CardCreationOptions
                .ForNonCombatWithDefaultOdds([ModelDb.CardPool<EGOCardPool>()],
                    (Func<CardModel, bool>)(c => c.Rarity == CardRarity.Rare))
                .WithFlags(CardCreationFlags.NoRarityModification));
        await RewardsCmd.OfferCustom(Owner, new List<Reward>(1)
        {
            cardReward
        });
        await CardPileCmd.AddCurseToDeck<Distorted>(Owner!);
        SetEventFinished(PageDescription("GIVE_IN"));
    }

    public async Task GiveUp()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.HpLoss.IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        SetEventFinished(PageDescription("GIVE_UP"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}