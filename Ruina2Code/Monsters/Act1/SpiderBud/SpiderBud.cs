using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1.SpiderBud;

public sealed class SpiderBud : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 38, 35);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 42, 38);
    
    private int CatchingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int BlockAmt => 5;

    protected override string VisualsPath => "SpiderBud/spider_bud.tscn".MonsterImagePath();

    private const string PROTECTIVE_INSTINCTS = "PROTECTIVE_INSTINCTS";
    private const string COCOON = "COCOON";
    private const string CATCHING_FOOD = "CATCHING_FOOD";

    public bool SpiderlingJustDied = false;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        Sfx.SpiderDown.Play();
        await PowerCmd.Apply<Hunt>(new ThrowingPlayerChoiceContext(), Creature, CombatState.Players.Count, Creature,  null);
    }

    private MoveState GetProtectiveInstinctsState()
    {
        return new MoveState(PROTECTIVE_INSTINCTS, ProtectiveInstincts, new DefendIntent());
    }

    private MoveState GetCocoonState()
    {
        return new MoveState(COCOON, Cocoon, new DebuffIntent());
    }
    
    private MoveState GetCatchingFoodState()
    {
        return new MoveState(CATCHING_FOOD, CatchingFood, new SingleAttackIntent(CatchingDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetProtectiveInstinctsState();
        var state2 = GetCocoonState();
        var state3 = GetCatchingFoodState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (SpiderlingJustDied || CombatState.HittableEnemies.Count == 1)
        {
            SpiderlingJustDied = false;
            return CATCHING_FOOD;
        }
        else
        {
            if (LastMoveIgnoringMove(stateMachine, PROTECTIVE_INSTINCTS, CATCHING_FOOD))
            {
                return COCOON;
            }
            else
            {
                return PROTECTIVE_INSTINCTS;
            }
        }
    }
    
    private async Task ProtectiveInstincts(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await CreatureCmd.GainBlock(enemy, BlockAmt, ValueProp.Move, null);
        }
        await ResetIdle();
    }
    
    private async Task Cocoon(IReadOnlyList<Creature> targets)
    {
        await DebuffAnimation(targets);
        await PowerCmd.Apply<Webbed>(new ThrowingPlayerChoiceContext(), targets, 1, Creature,  null);
        await ResetIdle();
    }
    
    private async Task CatchingFood(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(CatchingDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.SpiderStrongAtk, targets, 0.7f);
    }
    
    private async Task DebuffAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", null, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Special", Sfx.SpiderProtect, 2.0f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Special"], controller);
    }
}