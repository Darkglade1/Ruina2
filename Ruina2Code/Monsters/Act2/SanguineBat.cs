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
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.Act2;

public sealed class SanguineBat : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 34, 31);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 41, 37);
    
    private int BloodsuckingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);
    private int BloodsuckingHits => 2;
    private int TeethDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int ParalysisAmt => 1;

    protected override string VisualsPath => "Bat/bat.tscn".MonsterImagePath();

    private const string BLOODSUCKING = "BLOODSUCKING";
    private const string DIGGING_TEETH = "DIGGING_TEETH";
    private const string AVID_THIRST = "AVID_THIRST ";

    private MoveState GetBloodsuckingState()
    {
        return new MoveState(BLOODSUCKING, Bloodsucking, new MultiAttackIntent(BloodsuckingDamage, BloodsuckingHits), new HealIntent());
    }

    private MoveState GetDiggingTeethState()
    {
        return new MoveState(DIGGING_TEETH, DiggingTeeth, new SingleAttackIntent(TeethDamage), new DebuffIntent());
    }

    private MoveState GetAvidThirstState()
    {
        return new MoveState(AVID_THIRST, AvidThirst, new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBloodsuckingState();
        var state2 = GetDiggingTeethState();
        var state3 = GetAvidThirstState();
        
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
        List<string> possibilities = new List<string>();
        if (!LastMove(stateMachine, BLOODSUCKING)) {
            possibilities.Add(BLOODSUCKING);
        }
        if (!LastMove(stateMachine, DIGGING_TEETH)) {
            possibilities.Add(DIGGING_TEETH);
        }
        if (!LastMove(stateMachine, AVID_THIRST) && !LastMoveBefore(stateMachine, AVID_THIRST)) {
            possibilities.Add(AVID_THIRST);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Bloodsucking(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < BloodsuckingHits; i++)
        {
            await AttackAnimation(targets);
            var attackCommand = await DamageCmd.Attack(BloodsuckingDamage)
                .FromMonster(this)
                .Execute(null);
            await VampireHeal(attackCommand);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task DiggingTeeth(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(TeethDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, ParalysisAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task AvidThirst(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack", Sfx.BAT_ATTACK, targets, 0.5f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack"], controller);
    }
}