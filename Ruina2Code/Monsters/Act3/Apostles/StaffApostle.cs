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

namespace Ruina2.Ruina2Code.Monsters.Act3.Apostles;

public sealed class StaffApostle : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 46, 42);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 46);
    
    private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int AttackBuffDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int StrAmt => 2;
    private int DebuffAmt => 1;
    private int BlockAmt => 8;

    protected override string VisualsPath => "StaffApostle/staff_apostle.tscn".MonsterImagePath();

    private const string ATTACK = "ATTACK";
    private const string ATTACK_BUFF = "ATTACK_BUFF";
    private const string BLOCK_DEBUFF = "BLOCK_DEBUFF";

    private MoveState GetAttackState()
    {
        return new MoveState(ATTACK, Attack, new SingleAttackIntent(AttackDamage));
    }

    private MoveState GetAttackBuffState()
    {
        return new MoveState(ATTACK_BUFF, AttackBuff, new SingleAttackIntent(AttackBuffDamage), new BuffIntent());
    }
    
    private MoveState GetDefendDebuffState()
    {
        return new MoveState(BLOCK_DEBUFF, BlockDebuff, new DefendIntent(), new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetAttackState();
        var state2 = GetAttackBuffState();
        var state3 = GetDefendDebuffState();
        
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
        if (CombatState.HittableEnemies.Count == 1)
        {
            return ATTACK;
        }
        else
        {
            if (LastMove(stateMachine, ATTACK_BUFF))
            {
                return BLOCK_DEBUFF;
            }
            else
            {
                return ATTACK_BUFF;
            }
        }
    }
    
    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(AttackDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackBuff(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(AttackBuffDamage)
            .FromMonster(this)
            .Execute(null);
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, StrAmt, Creature,  null);
        }
        await ResetIdle();
    }
    
    private async Task BlockDebuff(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await CreatureCmd.GainBlock(enemy, BlockAmt, ValueProp.Move, null);
        }
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.ApostleWand, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Block"], controller);
    }
}