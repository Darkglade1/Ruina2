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

namespace Ruina2.Ruina2Code.Monsters.Act1.QueenBee;

public sealed class QueenBee : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 32);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 36);
    
    private int HornetDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int BlockAmt => 9;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    protected override string VisualsPath => "QueenBee/queen_bee.tscn".MonsterImagePath();

    private const string BOOST_AGGRESSION = "BOOST_AGGRESSION";
    private const string BOOST_LOYALTY = "BOOST_LOYALTY";
    private const string HORNET_STRIKE = "HORNET_STRIKE";

    private MoveState GetBoostAggressionState()
    {
        return new MoveState(BOOST_AGGRESSION, BoostAggression, new BuffIntent());
    }

    private MoveState GetBoostLoyaltyState()
    {
        return new MoveState(BOOST_LOYALTY, BoostLoyalty, new DefendIntent());
    }
    
    private MoveState GetHornetStrikeState()
    {
        return new MoveState(HORNET_STRIKE, HornetStrike, new SingleAttackIntent(HornetDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBoostAggressionState();
        var state2 = GetBoostLoyaltyState();
        var state3 = GetHornetStrikeState();
        
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
            return HORNET_STRIKE;
        }
        else if (LastMove(stateMachine, BOOST_AGGRESSION))
        {
            return BOOST_LOYALTY;
        }
        {
            return BOOST_AGGRESSION;
        }
    }
    
    private async Task BoostAggression(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        bool foundDrone = false;
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.Monster is WorkerBee)
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, StrengthAmount, Creature,  null);
                foundDrone = true;
                break;
            }
        }
        if (!foundDrone)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task BoostLoyalty(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        bool foundDrone = false;
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.Monster is WorkerBee)
            {
                await CreatureCmd.GainBlock(enemy, BlockAmt, ValueProp.Move, null);
                foundDrone = true;
                break;
            }
        }
        if (!foundDrone)
        {
            await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task HornetStrike(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(HornetDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.QueenBeeStab, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.QueenBeeBuff, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Special"], controller);
    }
}