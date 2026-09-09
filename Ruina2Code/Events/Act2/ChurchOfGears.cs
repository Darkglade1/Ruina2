using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Events.Act2;

public class ChurchOfGears() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Accept).ThatDoesDamage(DynamicVars["DupeCost"].IntValue),
        Option(Escape,[HoverTipFactory.Static(StaticHoverTip.Transform)]).ThatDoesDamage(DynamicVars["TransformCost"].IntValue)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("DupeCost", 16),
        new("TransformCost", 8),
    ];
    
    public override bool IsAllowed(IRunState runState) => base.IsAllowed(runState) && runState.Players.All(p => p.Creature.CurrentHp > DynamicVars["DupeCost"].IntValue);
    
    public async Task Accept()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["DupeCost"].IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("card_selection", "RUINA2-TO_DUPE"), 1);
        CardModel? mutableCard = (await CardSelectCmd.FromDeckGeneric(Owner!, prefs, Filter)).FirstOrDefault();
        if (mutableCard == null)
            return;
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(Owner.RunState.CloneCard(mutableCard), PileType.Deck));
        SetEventFinished(PageDescription("ACCEPT"));
    }
    
    public bool Filter(CardModel c) => c.Type != CardType.Quest;

    public async Task Escape()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["TransformCost"].IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        CardModel? original = (await CardSelectCmd.FromDeckForTransformation(Owner!, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))).FirstOrDefault();
        if (original != null)
        {
            CardModel cardForTransform = CardFactory.CreateRandomCardForTransform(original, false, Owner.RunState.Rng.Niche);
            CardCmd.Upgrade(cardForTransform);
            await CardCmd.Transform(original, cardForTransform);
        }
        SetEventFinished(PageDescription("ESCAPE"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}