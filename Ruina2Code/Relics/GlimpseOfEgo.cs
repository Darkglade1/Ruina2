using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class GlimpseOfEgo() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Ancient;
    
    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        var egoCards = EGOCardPool.GetAct1EgoCards();
        egoCards.AddRange(EGOCardPool.GetAct2EgoCards());
        egoCards.AddRange(EGOCardPool.GetAct3EgoCards());
        egoCards.StableShuffle(Owner.PlayerRng.Rewards);
        var card = Owner.RunState.CreateCard(egoCards[0], Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
    }
}