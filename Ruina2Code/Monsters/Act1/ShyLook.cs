using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
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
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class ShyLook : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 74, 67);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 79, 72);
    
    private int AtkDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int AtkDefDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int AtkDebuffDamage =>  AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int BlockAmt => 13;
    private int AtkBlock => 6;
    private int DebuffAmt => 1;
    private int StrengthAmount => 3;
    private int CardThreshold => 4;

    protected override string VisualsPath => "ShyLook/shy_look.tscn".MonsterImagePath();
    
    private const string DEFEND = "1";
    private const string BUFF = "2";
    private const string DEBUFF_ATTACK = "3";
    private const string DEFEND_ATTACK = "4";
    private const string ATTACK = "5";

    private int phase = 1;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Expression>(new ThrowingPlayerChoiceContext(), Creature, CombatState.Players.Count * CardThreshold, Creature,  null);
    }

    private MoveState GetDefendState()
    {
        return new MoveState(DEFEND, Defend, new DefendIntent());
    }

    private MoveState GetBuffState()
    {
        return new MoveState(BUFF, Buff, new BuffIntent());
    }
    
    private MoveState GetDebuffAttackState()
    {
        return new MoveState(DEBUFF_ATTACK, DebuffAttack, new SingleAttackIntent(AtkDebuffDamage), new DebuffIntent());
    }
    
    private MoveState GetDefendAttackState()
    {
        return new MoveState(DEFEND_ATTACK, DefendAttack, new SingleAttackIntent(AtkDefDamage), new DefendIntent());
    }
    
    private MoveState GetAttackState()
    {
        return new MoveState(ATTACK, Attack, new SingleAttackIntent(AtkDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetDefendState();
        var state2 = GetBuffState();
        var state3 = GetDebuffAttackState();
        var state4 = GetDefendAttackState();
        var state5 = GetAttackState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;
        state5.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(state5);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        List<string> possibilities = new List<string>();
        if (!LastMove(stateMachine, DEFEND) && NextMove.Id != DEFEND) {
            possibilities.Add(DEFEND);
        }
        if (!LastMove(stateMachine, BUFF) && NextMove.Id != BUFF) {
            possibilities.Add(BUFF);
        }
        if (!LastMove(stateMachine, DEBUFF_ATTACK) && NextMove.Id != DEBUFF_ATTACK) {
            possibilities.Add(DEBUFF_ATTACK);
        }
        if (!LastMove(stateMachine, DEFEND_ATTACK) && NextMove.Id != DEFEND_ATTACK) {
            possibilities.Add(DEFEND_ATTACK);
        }
        if (!LastMove(stateMachine, ATTACK) && NextMove.Id != ATTACK) {
            possibilities.Add(ATTACK);
        }
        MainFile.Logger.Info("Current Move: " + NextMove.Id);
        foreach (var move in possibilities)
        {
            MainFile.Logger.Info(move);
        }
        var nextMove = possibilities[rng.NextInt(possibilities.Count)];
        var ok = int.TryParse(nextMove, out phase);
        if (ok)
        {
            ResetIdle();
        }
        else
        {
            phase = 1;
        }
        return nextMove;
    }
    
    private async Task Defend(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await ResetIdle();
    }
    
    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle();
    }
    
    private async Task DebuffAttack(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(AtkDebuffDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task DefendAttack(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await CreatureCmd.GainBlock(Creature, AtkBlock, ValueProp.Move, null);
        await DamageCmd.Attack(AtkDefDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(AtkDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt" + phase, Sfx.ShyAtk, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block" + phase, null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle1", "Idle2", "Idle3", "Idle4", "Idle5", "Blunt1", "Blunt2", "Blunt3", "Blunt4", "Blunt5", "Block1", "Block2", "Block3", "Block4", "Block5"], controller);
    }
    
    protected override async Task ResetIdle(float waitTime)
    {
        IsMassAttacking = false;
        await WaitAnimation(waitTime);
        if (Creature.GetCreatureNode() != null)
        {
            await CreatureCmd.TriggerAnim(Creature, "Idle" + phase, 0);
        }
    }
}