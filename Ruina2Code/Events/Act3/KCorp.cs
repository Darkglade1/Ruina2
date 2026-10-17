using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.Evolve;

namespace Ruina2.Ruina2Code.Events.Act3;

public class KCorp() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Use),
        Option(Save, HoverTipFactory.FromCardWithCardHoverTips<KCorpAmpule>())
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(30),
        new StringVar("Card", ModelDb.Card<KCorpAmpule>().Title)
    ];
    
    public async Task Use()
    {
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.IntValue);
        SetEventFinished(PageDescription("USE"));
    }

    public async Task Save()
    {
        var card = Owner!.RunState.CreateCard(ModelDb.Card<KCorpAmpule>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("SAVE"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}