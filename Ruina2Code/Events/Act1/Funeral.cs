using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.EGO;

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
        new ("BlackCost", 12),
        new("RemoveCards", 1),
        new("WhiteCost", 3),
    ];
    
    public async Task Black()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["BlackCost"].IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        var egoCards = EGOCardPool.GetAct1EgoCards();
        egoCards.StableShuffle(Owner!.PlayerRng.Rewards);
        var card = Owner.RunState.CreateCard(egoCards[0], Owner);
        CardCmd.Upgrade(card);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
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