using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Events.Act1;

public class SingingMachine() : Ruina2Event()
{
    private CardModel? offeredCard;
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Option(OfferCard)];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("NumPotions", 2),
        new HpLossVar(10),
        new ("MaxHpLoss", 4)
    ];
    
    public override bool IsAllowed(IRunState runState)
    {
        return base.IsAllowed(runState) && runState.Players.All(p => PileType.Deck.GetPile(p).Cards.Any(card => card.Rarity != CardRarity.Basic));
    }
    
    public async Task OfferCard()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1);
        Func<CardModel, bool> filter = card => card.Rarity != CardRarity.Basic;
        offeredCard = (await CardSelectCmd.FromDeckForRemoval(Owner!, prefs, filter)).FirstOrDefault();
        if (offeredCard != null)
        {
            await CardPileCmd.RemoveFromDeck(offeredCard);
            if (offeredCard.Rarity == CardRarity.Curse || offeredCard.Type == CardType.Curse || offeredCard.Type == CardType.Status)
            {
                var curseOption = new EventOption(this, GetCurseBack,"RUINA2-SINGING_MACHINE.pages.options.CURSE");
                curseOption.Description.Add("Card", offeredCard.Title);
                var loseHpOption = new EventOption(this, LoseHP,"RUINA2-SINGING_MACHINE.pages.options.LOSE_HP");
                var loseMaxHpOption = new EventOption(this, LoseMaxHP,"RUINA2-SINGING_MACHINE.pages.options.LOSE_MAX_HP");
                SetEventState(L10NLookup("RUINA2-SINGING_MACHINE.pages.BAD_OFFERING.description"), [curseOption,  loseHpOption, loseMaxHpOption]);
            }
            else
            {
                EventOption firstOption;
                if (offeredCard.IsUpgraded)
                {
                    firstOption = new EventOption(this, DupeCard,"RUINA2-SINGING_MACHINE.pages.options.DUPE_CARD", HoverTipFactory.FromCard(offeredCard));
                }
                else
                {
                    firstOption = new EventOption(this, UpgradeCard,"RUINA2-SINGING_MACHINE.pages.options.UPGRADED_CARD", HoverTipFactory.FromCard(offeredCard, true));
                }
                firstOption.Description.Add("Card", offeredCard.Title);
                var relicOption = new EventOption(this, GetRelic,"RUINA2-SINGING_MACHINE.pages.options.RELIC");
                var potionOption = new EventOption(this, GetPotions,"RUINA2-SINGING_MACHINE.pages.options.POTIONS");
                relicOption.Description.Add("Rarity", GetRelicRarity(offeredCard).ToLocString().GetFormattedText());
                potionOption.Description.Add("Rarity", GetPotionRarity(offeredCard).ToLocString().GetFormattedText());
                SetEventState(L10NLookup("RUINA2-SINGING_MACHINE.pages.GOOD_OFFERING.description"), [firstOption,  relicOption, potionOption]);
            }
        }
    }
    
    public async Task UpgradeCard()
    {
        if (offeredCard != null)
        {
            var newCard = Owner!.RunState.CloneCard(offeredCard);
            CardCmd.Upgrade(newCard);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(newCard, PileType.Deck));
        }
        SetEventFinished(PageDescription("UPGRADED_CARD"));
    }
    public async Task DupeCard()
    {
        if (offeredCard != null)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(Owner!.RunState.CloneCard(offeredCard), PileType.Deck));
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(Owner!.RunState.CloneCard(offeredCard), PileType.Deck));
        }
        SetEventFinished(PageDescription("DUPE_CARD"));
    }
    
    public async Task GetRelic()
    {
        if (offeredCard != null)
        {
            await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(Owner!, GetRelicRarity(offeredCard)).ToMutable(), Owner!);
        }
        SetEventFinished(PageDescription("RELIC"));
    }
    
    public async Task GetPotions()
    {
        if (offeredCard != null)
        {
            IEnumerable<PotionModel> items = Owner!.Character.PotionPool.GetUnlockedPotions(Owner.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(Owner.UnlockState)).Where((Func<PotionModel, bool>) (p => p.Rarity == GetPotionRarity(offeredCard)));
            var potionModels = items.ToList();
            PotionModel? potion1 = Owner.PlayerRng.Rewards.NextItem(potionModels);
            PotionModel? potion2 = Owner.PlayerRng.Rewards.NextItem(potionModels);
            if (potion1 != null && potion2 != null)
            {
                await RewardsCmd.OfferCustom(Owner, new List<Reward>(2)
                {
                    new PotionReward(potion1.ToMutable(), Owner),
                    new PotionReward(potion2.ToMutable(), Owner)
                });
            }
        }
        SetEventFinished(PageDescription("POTIONS"));
    }

    public async Task GetCurseBack()
    {
        if (offeredCard != null)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(offeredCard, PileType.Deck));
        }
        SetEventFinished(PageDescription("CURSE"));
    }
    
    public async Task LoseHP()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.HpLoss.IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        SetEventFinished(PageDescription("LOSE_HP"));
    }
    
    public async Task LoseMaxHP()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature,  DynamicVars["MaxHpLoss"].IntValue, false);
        SetEventFinished(PageDescription("LOSE_HP"));
    }
    
    public PotionRarity GetPotionRarity(CardModel card)
    {
        switch (card.Rarity)
        {
            case CardRarity.Common:
            case CardRarity.Token:
            case CardRarity.Quest:
                return PotionRarity.Common;
            case CardRarity.Uncommon:
                return PotionRarity.Uncommon;
            case CardRarity.Rare:
            case CardRarity.Event:
            case CardRarity.Ancient:
                return PotionRarity.Rare;
            default:
                throw new InvalidOperationException($"Card {card.Id.Entry} has invalid rarity {card.Rarity}");
        }
    }
    
    public RelicRarity GetRelicRarity(CardModel card)
    {
        switch (card.Rarity)
        {
            case CardRarity.Common:
            case CardRarity.Token:
            case CardRarity.Quest:
                return RelicRarity.Common;
            case CardRarity.Uncommon:
                return RelicRarity.Uncommon;
            case CardRarity.Rare:
            case CardRarity.Event:
            case CardRarity.Ancient:
                return RelicRarity.Rare;
            default:
                throw new InvalidOperationException($"Card {card.Id.Entry} has invalid rarity {card.Rarity}");
        }
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}