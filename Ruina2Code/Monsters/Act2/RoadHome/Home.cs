using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.RoadHome;

public sealed class Home : AbstractAllyMonster
{
    public override int MinInitialHp => 1;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "HomeIcon.png".UIImagePath();

    protected override string VisualsPath => "Home/home.tscn".MonsterImagePath();

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<WayHome>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        OtherSideTargetMonster = FindTarget<RoadHome>();
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        MoveState initialState = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        initialState.FollowUpState = initialState;
        return new MonsterMoveStateMachine([initialState], initialState);
    }


    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        return CombatState.PlayerCreatures[0];
    }
    
    public async Task OnRoadDeath()
    {
        await CreatureCmd.Kill(Creature);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is RoadHome road)
        {
            if (road.Creature.IsAlive)
            {
                await road.HomeDeath();
            }
        }
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle"], controller);
    }
}