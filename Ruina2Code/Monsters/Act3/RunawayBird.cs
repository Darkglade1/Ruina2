using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class RunawayBird : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 36);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 46, 42);

    private int SweepDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int StatusAmt => 2;

    protected override string VisualsPath => "RunawayBird/runaway_bird.tscn".MonsterImagePath();

    private const string SWEEP = "SWEEP";
    private const string SHRIEK = "SHRIEK";

    private MoveState GetSweepState()
    {
        return new MoveState(SWEEP, Sweep, new SingleAttackIntent(SweepDamage));
    }

    private MoveState GetShriekState()
    {
        return new MoveState(SHRIEK, Shriek, new StatusIntent(StatusAmt));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetSweepState();
        var state2 = GetShriekState();
        
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
        if (!LastTwoMoves(stateMachine, SWEEP)) {
            possibilities.Add(SWEEP);
        }
        if (!LastMove(stateMachine, SHRIEK)) {
            possibilities.Add(SHRIEK);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Sweep(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(SweepDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Shriek(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Sweep", Sfx.BirdSweep, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Shriek", Sfx.BirdShout, targets, 0.4f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Sweep", "Shriek"], controller);
    }
}