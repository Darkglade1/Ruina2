using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class PriceOfTime() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars["CardCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CardCounter",0)];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner.IsAlive && Owner.Monster is AbstractMultiIntentMonster monster)
        {
            monster.RollMove(CombatState.PlayerCreatures);
            NCreature? creatureNode = Owner.GetCreatureNode();
            if (creatureNode == null || !CombatState.IsLiveCombat())
                return;
            await TaskHelper.RunSafely(creatureNode.RefreshIntents());
        }
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        DynamicVars["CardCounter"].BaseValue++;
        if (DynamicVars["CardCounter"].IntValue >= Amount)
        {
            DynamicVars["CardCounter"].BaseValue = 0M;
            Flash();
            if (Owner.IsAlive && Owner.Monster is AbstractMultiIntentMonster monster)
            {
                await monster.PerformMultiIntentMove();
                monster.RollMove(CombatState.PlayerCreatures);
                NCreature? creatureNode = Owner.GetCreatureNode();
                if (creatureNode == null || !CombatState.IsLiveCombat())
                    return;
                await TaskHelper.RunSafely(creatureNode.RefreshIntents());
            }
        }
        InvokeDisplayAmountChanged();
    }
}