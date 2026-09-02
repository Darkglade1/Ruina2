using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs.History;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class BookOfEgo() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Ancient;
    
    public override bool HasUponPickupEffect => true;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Decay>();

    public override async Task AfterObtained()
    {
        var egoCards1 = EGOCardPool.GetAct1EgoCards();
        egoCards1.StableShuffle(Owner.PlayerRng.Rewards);
        var egoCards2 = EGOCardPool.GetAct2EgoCards();
        egoCards2.StableShuffle(Owner.PlayerRng.Rewards);
        var egoCards3 = EGOCardPool.GetAct3EgoCards();
        egoCards3.StableShuffle(Owner.PlayerRng.Rewards);
        
        List<CardModel> options = new List<CardModel>();
        options.Add(egoCards1[0]);
        options.Add(egoCards2[0]);
        options.Add(egoCards3[0]);
        CardModel? chosenCard = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), options, Owner, true);
        List<CardModel> cards = new List<CardModel>(1)
        {
            Owner.RunState.CreateCard<Decay>(Owner)
        };
        if (chosenCard != null)
        {
            cards.Insert(0, Owner.RunState.CreateCard(chosenCard, Owner));
        }
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cards, PileType.Deck));
        foreach (CardModel card in options)
        {
            if (card != chosenCard)
            {
                Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(Owner.NetId).CardChoices.Add(new CardChoiceHistoryEntry(Owner.RunState.CreateCard(card, Owner), false));
            }
        }
    }
}