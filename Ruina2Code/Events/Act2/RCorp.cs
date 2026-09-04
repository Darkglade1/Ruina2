using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Events.Act2;

public class RCorp() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Killer).ThatDoesDamage(DynamicVars["UpgradeCost"].IntValue),
        Option(Coward).ThatDecreasesMaxHp(DynamicVars["RemoveCost"].IntValue)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("UpgradeCards", 2),
        new ("UpgradeCost", 18),
        new("RemoveCards", 1),
        new("RemoveCost", 4),
    ];
    
    public override bool IsAllowed(IRunState runState) => base.IsAllowed(runState) && runState.Players.All(p => p.Creature.CurrentHp > DynamicVars["UpgradeCost"].IntValue);
    
    public async Task Killer()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["UpgradeCost"].IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        var cards = (await CardSelectCmd.FromDeckForUpgrade(Owner!, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, DynamicVars["UpgradeCards"].IntValue)));
        foreach (var card in cards)
        {
            CardCmd.Upgrade(card);
        }
        SetEventFinished(PageDescription("KILLER"));
    }

    public async Task Coward()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature,  DynamicVars["RemoveCost"].IntValue, false);
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars["RemoveCards"].IntValue);
        await CardPileCmd.RemoveFromDeck((await CardSelectCmd.FromDeckForRemoval(Owner, prefs)).ToList());
        SetEventFinished(PageDescription("COWARD"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}