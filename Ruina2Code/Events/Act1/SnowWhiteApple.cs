using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act1;

public class SnowWhiteApple() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Accept, HoverTipFactory.FromCardWithCardHoverTips<SporeMind>()),
        Option(Reject, HoverTipFactory.FromRelic<Malice>()).ThatDecreasesMaxHp(DynamicVars["MaxHPLoss"].IntValue)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("MaxHP", 12),
        new("MaxHPLoss", 8),
        new StringVar("Curse", ModelDb.Card<SporeMind>().Title),
        new StringVar("Relic", ModelDb.Relic<Malice>().Title.GetFormattedText())
    ];
    
    public async Task Accept()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars["MaxHP"].IntValue);
        await CardPileCmd.AddCurseToDeck<SporeMind>(Owner!);
        SetEventFinished(PageDescription("ACCEPT"));
    }

    public async Task Reject()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature,  DynamicVars["MaxHPLoss"].IntValue, false);
        await RelicCmd.Obtain(ModelDb.Relic<Malice>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("REJECT"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}