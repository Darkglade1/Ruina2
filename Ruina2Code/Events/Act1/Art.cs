using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.EGO;
using Ruina2.Ruina2Code.Potions;

namespace Ruina2.Ruina2Code.Events.Act1;

public class Art() : Ruina2Event()
{
    private bool isEgoCardRandom;
    private bool isEgoCardUpgraded;
    private string EGOCardKey;
    private PotionModel chosenPotion;
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<PotionModel> list = Owner!.Potions.ToList();
        list.StableShuffle(Owner.PlayerRng.Rewards);
        chosenPotion = list[0];
        if (chosenPotion.Rarity == PotionRarity.Rare)
        {
            isEgoCardRandom = false;
            isEgoCardUpgraded = true;
            EGOCardKey = "CUT_SUPPLY_RARE";
        } else if (chosenPotion.Rarity == PotionRarity.Common)
        {
            isEgoCardRandom = true;
            isEgoCardUpgraded = false;
            EGOCardKey = "CUT_SUPPLY_COMMON";
        }
        else
        {
            isEgoCardRandom = false;
            isEgoCardUpgraded = false;
            EGOCardKey = "CUT_SUPPLY_UNCOMMON";
        }

        var cutSupplyOption = new EventOption(this, CutSupply,
            $"{Id.Entry}.pages.INITIAL.options.{EGOCardKey}");
        cutSupplyOption.Description.Add("ChosenPotion", chosenPotion.Title.GetFormattedText());

        return [Option(BringDrink, [HoverTipFactory.FromPotion<EgoPotion>()]), cutSupplyOption];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("NumPotions", 2),
        new StringVar("Potion", ModelDb.Potion<EgoPotion>().Title.GetFormattedText())
    ];
    
    public override bool IsAllowed(IRunState runState)
    {
        return base.IsAllowed(runState) && runState.Players.All(p => p.Potions.Count() >= 1);
    }
    
    public async Task BringDrink()
    {
        Owner!.CanUseOrRemovePotions = true;
        await RewardsCmd.OfferCustom(Owner!, new List<Reward>(DynamicVars["NumPotions"].IntValue)
        {
            new PotionReward(ModelDb.Potion<EgoPotion>().ToMutable(), Owner!),
            new PotionReward(ModelDb.Potion<EgoPotion>().ToMutable(), Owner!)
        });
        SetEventFinished(PageDescription("BRING_DRINK"));
    }

    public async Task CutSupply()
    {
        await PotionCmd.Discard(chosenPotion);
        if (isEgoCardRandom)
        {
            var egoCards = EGOCardPool.GetAct1EgoCards();
            egoCards.StableShuffle(Owner!.PlayerRng.Rewards);
            var card = Owner.RunState.CreateCard(egoCards[0], Owner);
            if (isEgoCardUpgraded)
            {
                CardCmd.Upgrade(card);
            }
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        }
        else
        {
            var egoCards = EGOCardPool.GetAct1EgoCards();
            egoCards.StableShuffle(Owner!.PlayerRng.Rewards);
            var cards = new List<CardModel>()
            {
                Owner.RunState.CreateCard(egoCards[0], Owner),
                Owner.RunState.CreateCard(egoCards[1], Owner),
                Owner.RunState.CreateCard(egoCards[2], Owner)
            };
            if (isEgoCardUpgraded)
            {
                foreach (var card in cards)
                {
                    CardCmd.Upgrade(card);
                }
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
        }
        SetEventFinished(PageDescription("CUT_SUPPLY"));
    }
    
    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        Owner!.CanUseOrRemovePotions = false;
        return Task.CompletedTask;
    }

    protected override void OnEventFinished() => Owner!.CanUseOrRemovePotions = true;
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}