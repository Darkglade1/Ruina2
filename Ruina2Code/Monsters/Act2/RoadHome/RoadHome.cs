using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.RoadHome;

public sealed class RoadHome : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 130, 120);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 3;

    private int GoDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int HomingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 39, 35);
    private int StatusAmt => 2;

    protected override string VisualsPath => "RoadHome/road.tscn".MonsterImagePath();

    private const string LETS_GO = "LETS_GO";
    private const string HOMING_INSTINCT = "HOMING_INSTINCT";
    private const string NONE  = "NONE";

    public Creature? Cat;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Home>();
        Cat = FindTarget<ScaredyCat>();
        await PowerCmd.Apply<EasilyDistracted>(new ThrowingPlayerChoiceContext(), Creature, CombatState.Players.Count, Creature,  null);
    }

    private MoveState GetLetsGoState()
    {
        return new MoveState(LETS_GO, LetsGo, new RuinaSingleAttackIntent(GoDamage));
    }

    private MoveState GetHomingInstinctState()
    {
        return new MoveState(HOMING_INSTINCT, HomingInstinct, new RuinaSingleAttackIntent(HomingDamage), new StatusIntent(StatusAmt));
    }

    private MoveState GetNoneState()
    {
        return new MoveState(NONE, _ => Task.CompletedTask);
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLetsGoState();
        var state2 = GetHomingInstinctState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLetsGoState();
        var state2 = GetNoneState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 1);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private MonsterMoveStateMachine GenerateIntent3StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLetsGoState();
        var state2 = GetNoneState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 2);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsDead)
        {
            if (intentNum == 0)
            {
                return HOMING_INSTINCT;
            }
            else
            {
                return NONE;
            }
        }
        return LETS_GO;
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine(), GenerateIntent3StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsDead)
        {
            return CombatState.PlayerCreatures[0];
        }
        else if (OtherSideTargetMonster != null)
        {
            return OtherSideTargetMonster;
        }
        else
        {
            return CombatState.PlayerCreatures[0];
        }
    }

    private async Task LetsGo(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await DamageCmd.Attack(GoDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task HomingInstinct(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        var target = targets.FirstOrDefault(t => t.IsAlive);
        if (target != null)
        {
            var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
            if (targetNode != null)
            {
                var houseDropEffect = HouseDropEffect.Create(targetNode.VfxSpawnPosition);
                Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
                vfxContainer?.AddChildSafely(houseDropEffect);
                await WaitAnimation(1.0f);
            }
        }
        await DamageCmd.Attack(HomingDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Home home)
        {
            if (home.Creature.IsAlive)
            {
                await home.OnRoadDeath();
            }
        }
        if (creature == Creature && Cat?.Monster is ScaredyCat cat)
        {
            if (cat.Creature.IsAlive)
            {
                await cat.OnRoadDeath();
            }
        }
    }

    public async Task HomeDeath()
    {
        await PowerCmd.Remove<EasilyDistracted>(Creature);
    }

    public void CancelIntent()
    {
        if (NextMoves.Count > 0)
        {
            NextMoves.RemoveAt(NextMoves.Count - 1);
        }

        if (Targets.Count > 0)
        {
            Targets.RemoveAt(Targets.Count - 1);
        }
        NCreature? creatureNode = Creature.GetCreatureNode();
        if (creatureNode == null || !CombatState.IsLiveCombat())
            return;
        TaskHelper.RunSafely(creatureNode.RefreshIntents());
    }

    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.MakeRoad, targets);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("House", Sfx.HouseAttack, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "House", "Special"], controller);
    }
}