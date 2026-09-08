using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Events.Act1;

public class DerFreischutz() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Accept, HoverTipFactory.FromRelic<SeventhBullet>()),
        Option(Reject).ThatDoesDamage(DynamicVars["HpLoss"].IntValue)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(7),
        new StringVar("Relic", ModelDb.Relic<SeventhBullet>().Title.GetFormattedText()),
        new CardsVar(1)
    ];
    
    public async Task Accept()
    {
        await RelicCmd.Obtain(ModelDb.Relic<SeventhBullet>().ToMutable(), Owner!);
        SetEventFinished(PageDescription("ACCEPT"));
    }

    public async Task Reject()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["HpLoss"].IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars.Cards.IntValue);
        await CardPileCmd.RemoveFromDeck((await CardSelectCmd.FromDeckForRemoval(Owner, prefs)).ToList());
        SetEventFinished(PageDescription("REJECT"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}