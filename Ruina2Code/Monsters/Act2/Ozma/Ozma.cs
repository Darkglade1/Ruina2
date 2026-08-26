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
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.Ozma;

public sealed class Ozma : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 99, 90);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 103, 94);
    
    private int HinderDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int SquashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int DRAW_DEBUFF => 1;
    private int ParalysisAmt => 2;
    private int BlockAmt => 9;

    protected override string VisualsPath => "Ozma/ozma.tscn".MonsterImagePath();

    private const string FADING_MEMORIES = "FADING_MEMORIES";
    private const string POWDER_OF_LIFE = "POWDER_OF_LIFE";
    private const string HINDER = "HINDER";
    private const string SQUASH = "SQUASH";

    private MoveState GetFadingMemoriesState()
    {
        return new MoveState(FADING_MEMORIES, FadingMemories, new DebuffIntent());
    }

    private MoveState GetPowderOfLifeState()
    {
        return new MoveState(POWDER_OF_LIFE, PowderOfLife, new DefendIntent(), new BuffIntent());
    }

    private MoveState GetHinderState()
    {
        return new MoveState(HINDER, Hinder, new SingleAttackIntent(HinderDamage), new DebuffIntent());
    }
    
    private MoveState GetSquashState()
    {
        return new MoveState(SQUASH, Squash, new SingleAttackIntent(SquashDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetFadingMemoriesState();
        var state2 = GetPowderOfLifeState();
        var state3 = GetHinderState();
        var state4 = GetSquashState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (CombatState.RoundNumber == 1)
        {
            return FADING_MEMORIES;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, HINDER)) {
                possibilities.Add(HINDER);
            }
            if (!LastMove(stateMachine, SQUASH)) {
                possibilities.Add(SQUASH);
            }
            if (!LastMove(stateMachine, POWDER_OF_LIFE) && !LastMoveBefore(stateMachine, POWDER_OF_LIFE)) {
                possibilities.Add(POWDER_OF_LIFE);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task Squash(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(SquashDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task Hinder(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(HinderDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, ParalysisAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task PowderOfLife(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        foreach (var creature in CombatState.HittableEnemies)
        {
            await CreatureCmd.GainBlock(creature, BlockAmt, ValueProp.Move, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, StrengthAmount, Creature,  null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task FadingMemories(IReadOnlyList<Creature> targets)
    {
        await DebuffAnimation(targets);
        await PowerCmd.Apply<Oblivion>(new ThrowingPlayerChoiceContext(), targets, DRAW_DEBUFF, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slam", Sfx.GreedSlam, targets);
    }
    
    private async Task BuffAnimation()
    {
        await AnimationAction("Raise", Sfx.OzmaGuard);
    }
    
    private async Task DebuffAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Thud", Sfx.OzmaFin, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Raise", "Slam", "Thud"], controller);
    }
}