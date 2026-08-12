using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act3.Heart;

public sealed class LungsOfCraving : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 46, 42);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 46);
    
    private int FerventDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int FerventHits => 2;
    private int RetractingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int BlockAmt => 7;

    protected override string VisualsPath => "Lungs/lungs.tscn".MonsterImagePath();

    private const string FERVENT_BEATS = "FERVENT_BEATS";
    private const string RETRACTING_BEATS = "RETRACTING_BEATS";

    private MoveState GetFerventBeatsState()
    {
        return new MoveState(FERVENT_BEATS, FerventBeats, new MultiAttackIntent(FerventDamage, FerventHits));
    }

    private MoveState GetRetractingBeatsState()
    {
        return new MoveState(RETRACTING_BEATS, RetractingBeats, new SingleAttackIntent(RetractingDamage), new DefendIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetFerventBeatsState();
        var state2 = GetRetractingBeatsState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        List<string> possibilities = new List<string>();
        if (!LastTwoMoves(stateMachine, FERVENT_BEATS)) {
            possibilities.Add(FERVENT_BEATS);
        }
        if (!LastTwoMoves(stateMachine, RETRACTING_BEATS)) {
            possibilities.Add(RETRACTING_BEATS);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task FerventBeats(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < FerventHits; i++)
        {
            await AttackAnimation(targets);
            await DamageCmd.Attack(FerventDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task RetractingBeats(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await DamageCmd.Attack(RetractingDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.WoodStrike, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce"], controller);
    }
}