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

namespace Ruina2.Ruina2Code.Monsters.Act1.ScorchedGirl;

public sealed class MatchFlame : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 13, 12);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 17, 15);
    
    private int KindleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int BlockAmt => 6;

    protected override string VisualsPath => "MatchFlame/match_flame.tscn".MonsterImagePath();

    private const string BROKEN_HOPE = "BROKEN_HOPE";
    private const string KINDLE = "KINDLE";

    private MoveState GetBrokenHopeState()
    {
        return new MoveState(BROKEN_HOPE, BrokenHope, new DefendIntent());
    }

    private MoveState GetKindleState()
    {
        return new MoveState(KINDLE, Kindle, new SingleAttackIntent(KindleDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBrokenHopeState();
        var state2 = GetKindleState();
        
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
        if (!LastMove(stateMachine, BROKEN_HOPE)) {
            possibilities.Add(BROKEN_HOPE);
        }
        if (!LastTwoMoves(stateMachine, KINDLE)) {
            possibilities.Add(KINDLE);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task BrokenHope(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        bool foundGirl = false;
        foreach (var creature in CombatState.HittableEnemies)
        {
            if (creature.Monster is ScorchedGirl)
            {
                await CreatureCmd.GainBlock(creature, BlockAmt, ValueProp.Move, null);
                foundGirl = true;
            }
        }

        if (!foundGirl)
        {
            await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task Kindle(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(KindleDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.MatchSizzle, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Block", "Pierce"], controller);
    }
}