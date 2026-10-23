using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Rewards;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.Evolve;

namespace Ruina2.Ruina2Code.Events.Act2;

public class BizarreMachine() : Ruina2Event()
{
    private CardModel? junk1;
    private CardModel? junk2;
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<CardModel> list = new List<CardModel>();
        list.Add(ModelDb.Card<ScrapIron>());
        list.Add(ModelDb.Card<Shovel>());
        list.Add(ModelDb.Card<BlemishedSteel>());
        list.Add(ModelDb.Card<TarnishedGold>());
        list.Add(ModelDb.Card<RustyChest>());
        list.StableShuffle(Rng);
        junk1 = Owner!.RunState.CreateCard(list[0], Owner!);
        junk2 = Owner!.RunState.CreateCard(list[1], Owner!);

        var junkOption1 = Option(Junk1,[HoverTipFactory.FromCard(junk1)]);
        var junkOption2 = Option(Junk2, [HoverTipFactory.FromCard(junk2)]);
        junkOption1.Description.Add("Card", junk1.Title);
        junkOption2.Description.Add("Card", junk2.Title);

        return [junkOption1, junkOption2, Option(Potion)];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];
    
    public async Task Junk1()
    {
        if (junk1 != null)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(junk1, PileType.Deck));
        }
        SetEventFinished(PageDescription("JUNK"));
    }
    
    public async Task Junk2()
    {
        if (junk2 != null)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(junk2, PileType.Deck));
        }
        SetEventFinished(PageDescription("JUNK"));
    }

    public async Task Potion()
    {
        var items = Owner!.Character.PotionPool.GetUnlockedPotions(Owner.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(Owner.UnlockState));
        PotionModel? potionModel = Owner!.PlayerRng.Rewards.NextItem(items);
        if (potionModel != null)
        {
            await RewardsCmd.OfferCustom(Owner, new List<Reward>(1)
            {
                new PotionReward(potionModel.ToMutable(), Owner)
            });
        }
        SetEventFinished(PageDescription("POTION"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}