using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Ruina2.Ruina2Code.Monsters.Act1;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Expression() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars.Cards.IntValue;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        DynamicVars.Cards.BaseValue++;
        if (DynamicVars.Cards.IntValue >= Amount)
        {
            Flash();
            DynamicVars.Cards.BaseValue = 0;
            if (Owner.Monster != null && Owner.Monster is ShyLook monster)
            {
                if (monster.MoveStateMachine != null)
                {
                    monster.MoveStateMachine._performedFirstMove = true;
                }
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