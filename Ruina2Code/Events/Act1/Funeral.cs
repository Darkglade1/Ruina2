using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Events.Act1;

public class Funeral() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Black).ThatDoesDamage(DynamicVars["BlackCost"].IntValue),
        Option(White).ThatDecreasesMaxHp(DynamicVars["WhiteCost"].IntValue)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("TransformCards", 2),
        new ("BlackCost", 10),
        new("RemoveCards", 1),
        new("WhiteCost", 3),
    ];
    
    public async Task Black()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["BlackCost"].IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, DynamicVars["TransformCards"].IntValue);
        foreach (CardModel original in (await CardSelectCmd.FromDeckForTransformation(Owner!, prefs)).ToList())
        {
            await CardCmd.TransformToRandom(original, Rng, CardPreviewStyle.EventLayout);
        }
        SetEventFinished(PageDescription("BLACK"));
    }

    public async Task White()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature,  DynamicVars["WhiteCost"].IntValue, false);
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars["RemoveCards"].IntValue);
        await CardPileCmd.RemoveFromDeck((await CardSelectCmd.FromDeckForRemoval(Owner, prefs)).ToList());
        SetEventFinished(PageDescription("WHITE"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}