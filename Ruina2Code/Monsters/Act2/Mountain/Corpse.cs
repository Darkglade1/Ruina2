using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Ruina2.Ruina2Code.Extensions;

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
        var node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (node != null)
        {
            node.Position = new Vector2(0, 200);
        }
        //await PowerCmd.Apply<Fury>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
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