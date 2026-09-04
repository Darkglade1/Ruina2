using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Events.Act2;

public class ZweiAssociation() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Training),
        Option(Scouting).ThatDecreasesMaxHp(DynamicVars["ScoutingCost"].IntValue)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("GoldCost", 100),
        new("ScoutingCost", 7),
    ];
    
    public override bool IsAllowed(IRunState runState) => base.IsAllowed(runState) && runState.Players.All(p => p.Gold >= DynamicVars["GoldCost"].IntValue);
    
    public async Task Training()
    {
        await PlayerCmd.LoseGold(DynamicVars["GoldCost"].BaseValue, Owner!, GoldLossType.Spent);
        CardReward cardReward = new CardReward(CardCreationOptions.ForNonCombatWithDefaultOdds([Owner!.Character.CardPool],  (Func<CardModel, bool>) (c => c.Rarity == CardRarity.Rare)).WithFlags(CardCreationFlags.NoRarityModification), 3, Owner);
        await RewardsCmd.OfferCustom(Owner, new List<Reward>(1)
        {
            cardReward
        });
        SetEventFinished(PageDescription("TRAINING"));
    }

    public async Task Scouting()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature,  DynamicVars["ScoutingCost"].IntValue, false);
        var egoCards = EGOCardPool.GetAct2EgoCards();
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
        SetEventFinished(PageDescription("SCOUTING"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}