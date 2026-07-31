using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;

namespace Ruina2.Ruina2Code.Monsters.Act2.Wrath;

public sealed class Hermit : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 209, 190);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 220, 200);
    public override int NumIntents => 2;

    private int HoldDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int MakeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int BlockAmount => 11;
    private int DebuffAmt => 1;

    protected override string VisualsPath => "Hermit/hermit.tscn".MonsterImagePath();

    private const string HOLD_STILL = "HOLD_STILL";
    private const string MAKE_WAY = "MAKE_WAY";
    private const string CRACKLE  = "CRACKLE";
    private const string HELLO = "HELLO";

    public Creature? staff;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<ServantOfWrath>();
        staff = FindTarget<HermitStaff>();
    }

    private MoveState GetHoldStillState()
    {
        return new MoveState(HOLD_STILL, HoldStill, new RuinaSingleAttackIntent(HoldDamage), new RuinaDebuffIntent());
    }

    private MoveState GetMakeWayState()
    {
        return new MoveState(MAKE_WAY, MakeWay, new RuinaSingleAttackIntent(MakeDamage));
    }

    private MoveState GetCrackleState()
    {
        return new MoveState(CRACKLE, Crackle, new DefendIntent(), new BuffIntent());
    }

    private MoveState GetHelloState()
    {
        return new MoveState(HELLO, Hello, new SummonIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetHoldStillState();
        var state2 = GetMakeWayState();
        var state3 = GetHelloState();
        
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

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetHoldStillState();
        var state2 = GetMakeWayState();
        var state3 = GetCrackleState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove2, 0);

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
        if (staff == null || staff.IsDead)
        {
            return HELLO;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, HOLD_STILL)) {
                possibilities.Add(HOLD_STILL);
            }
            if (!LastTwoMoves(stateMachine, MAKE_WAY)) {
                possibilities.Add(MAKE_WAY);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private string SelectNextMove2(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (LastMove(stateMachine, HOLD_STILL))
        {
            return MAKE_WAY;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, HOLD_STILL)) {
                possibilities.Add(HOLD_STILL);
            }
            if (!LastTwoMoves(stateMachine, MAKE_WAY)) {
                possibilities.Add(MAKE_WAY);
            }
            if (!LastMove(stateMachine, CRACKLE) && !LastMoveBefore(stateMachine, CRACKLE)) {
                possibilities.Add(CRACKLE);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            return CombatState.PlayerCreatures[0];
        }
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task HoldStill(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(HoldDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        if (targets[0].IsPlayer)
        {
            await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        }
        else
        {
            await ApplyPowerAndSkipNextDurationTick<VulnerablePower>(targets, DebuffAmt);
        }
        await ResetIdle();
    }
    
    private async Task MakeWay(IReadOnlyList<Creature> targets)
    {
        await Attack2Animation(targets);
        await DamageCmd.Attack(MakeDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Crackle(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (!(enemy.Monster is AbstractAllyMonster))
            {
                await CreatureCmd.GainBlock(enemy, BlockAmount, ValueProp.Move, null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, StrengthAmount, Creature,  null);
            }
        }
        await ResetIdle(1.0f);
    }

    private async Task Hello(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await CreatureCmd.Add<HermitStaff>(CombatState, "staff");
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.Monster is HermitStaff && enemy.IsAlive)
            {
                staff = enemy;
            }
        }
        await ResetIdle(1.0f);
    }
    
    public override async Task BeforeDeath(Creature creature)
    {
        await base.BeforeDeath(creature);
        if (creature != Creature)
            return;

        var livingMinions = CombatState.GetTeammatesOf(Creature)
            .Where(t => t != Creature && t.IsAlive && t.Monster is HermitStaff)
            .ToList();

        foreach (var minion in livingMinions)
        {
            await CreatureCmd.Kill(minion);
        }
    }

    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack", Sfx.HermitAtk, targets);
    }
    
    private async Task Attack2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack2", Sfx.HermitStrongAtk, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", Sfx.HermitWand);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack", "Attack2", "Special"], controller);
    }
}