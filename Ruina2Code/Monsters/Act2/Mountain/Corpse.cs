using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.mountain;

public sealed class Corpse : AbstractMultiIntentMonster
{
    public override int MinInitialHp => 40;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "CorpseIcon.png".UIImagePath();

    protected override string VisualsPath => "Corpse/corpse.tscn".MonsterImagePath();

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        if (CombatState.Players.Count > 1)
        {
            await PowerCmd.Apply<MultiplayerAlly>(new ThrowingPlayerChoiceContext(), Creature, AbstractAllyMonster.GetAllyMultiplayerDamageModifier(CombatState), Creature,  null);
        }
        await PowerCmd.Apply<Corpses>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        MoveState emptyState = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        emptyState.FollowUpState = emptyState;
        MoveState initialState = emptyState;
        initialState.FollowUpState = initialState;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { initialState },
            initialState
        );
    }


    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        return CombatState.PlayerCreatures[0];
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle"], controller);
    }
}