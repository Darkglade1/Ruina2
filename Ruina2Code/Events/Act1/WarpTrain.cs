using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Events.Act1;

public class WarpTrain() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Open),
        Option(Interact)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("Gold", 75),
        new ("GoldCost", 25)
    ];
    
    public override bool IsAllowed(IRunState runState) => base.IsAllowed(runState) && runState.Players.All(p => p.Gold >= DynamicVars["GoldCost"].IntValue);
    
    public async Task Open()
    {
        await PlayerCmd.GainGold(DynamicVars["Gold"].IntValue, Owner!);
        SetEventFinished(PageDescription("OPEN"));
    }

    public async Task Interact()
    {
        await PlayerCmd.LoseGold(DynamicVars["GoldCost"].BaseValue, Owner!, GoldLossType.Spent);
        CardModel? original = (await CardSelectCmd.FromDeckForTransformation(Owner!, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))).FirstOrDefault();
        if (original != null)
        {
            await CardCmd.TransformToRandom(original, Rng, CardPreviewStyle.EventLayout);
        }
        SetEventFinished(PageDescription("INTERACT"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Asiyah;
    }
}