using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Events.Act1;

public class YourBook() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Overcome).ThatDoesDamage(DynamicVars.HpLoss.IntValue),
        Option(Succumb, HoverTipFactory.FromCardWithCardHoverTips<Metamorphosis>())
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(12),
        new StringVar("Card", ModelDb.Card<Metamorphosis>().Title)
    ];
    
    public async Task Overcome()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.HpLoss.IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        var egoCards = EGOCardPool.GetAct1EgoCards();
        egoCards.StableShuffle(Owner!.PlayerRng.Rewards);
        var card = Owner.RunState.CreateCard(egoCards[0], Owner);
        CardCmd.Upgrade(card);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("OVERCOME"));
    }

    public async Task Succumb()
    {
        var card = Owner!.RunState.CreateCard(ModelDb.Card<Metamorphosis>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("SUCCUMB"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}