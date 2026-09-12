using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Events.Act3;

public class CryingChildren() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Unleash).ThatDoesDamage(DynamicVars.HpLoss.IntValue),
        Option(Suppress)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(13),
        new ("MaxHP", 6)
    ];
    
    public async Task Unleash()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.HpLoss.IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        var egoCards = EGOCardPool.GetAct3EgoCards();
        egoCards.StableShuffle(Owner!.PlayerRng.Rewards);
        var card = Owner.RunState.CreateCard(egoCards[0], Owner);
        CardModel? original = (await CardSelectCmd.FromDeckForRemoval(Owner!, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1))).FirstOrDefault();
        if (original != null)
        {
            await CardCmd.Transform(original, card);
        }
        SetEventFinished(PageDescription("UNLEASH"));
    }

    public async Task Suppress()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars["MaxHP"].IntValue);
        SetEventFinished(PageDescription("SUPPRESS"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}