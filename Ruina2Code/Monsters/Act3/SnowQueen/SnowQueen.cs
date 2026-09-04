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
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.SnowQueen;

public sealed class SnowQueen : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 290, 260);
    public override int MaxInitialHp => MinInitialHp;
    
    private int FrigidDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 24);
    private int IceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 32, 29);
    private int StrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);
    private int FrozenAmt => 1;
    
    private int PlatingAmt => 7;
    private int DebuffAmt => 2;
    private int BlockAmt => 16;

    private int MaxBlizzards = 2;

    protected override string VisualsPath => "SnowQueen/snow_queen.tscn".MonsterImagePath();

    private const string BLIZZARD = "BLIZZARD";
    private const string FRIGID_GAZE = "FRIGID_GAZE";
    private const string ICE_SPLINTERS = "ICE_SPLINTERS";
    private const string FROZEN_THRONE = "FROZEN_THRONE";
    
    private MoveState GetBlizzardState()
    {
        return new MoveState(BLIZZARD, Blizzard, new DebuffIntent(), new BuffIntent());
    }

    private MoveState GetFrigidGazeState()
    {
        return new MoveState(FRIGID_GAZE, FrigidGaze, new SingleAttackIntent(FrigidDamage), new DefendIntent());
    }

    private MoveState GetIceSplintersState()
    {
        return new MoveState(ICE_SPLINTERS, IceSplinters, new SingleAttackIntent(IceDamage));
    }

    private MoveState GetFrozenThroneState()
    {
        return new MoveState(FROZEN_THRONE, FrozenThrone, new BuffIntent(), new DefendIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBlizzardState();
        var state2 = GetFrigidGazeState();
        var state3 = GetIceSplintersState();
        var state4 = GetFrozenThroneState();
        
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
        if ((CombatState.RoundNumber == 1 || ThreeTurnCooldownHasPassedForMove(stateMachine, BLIZZARD)) && stateMachine.StateLog.FindAll(m => m.Id == BLIZZARD).Count < MaxBlizzards)
        {
            return BLIZZARD;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, FRIGID_GAZE)) {
                possibilities.Add(FRIGID_GAZE);
            }
            if (!LastMove(stateMachine, ICE_SPLINTERS)) {
                possibilities.Add(ICE_SPLINTERS);
            }
            if (!LastMove(stateMachine, FROZEN_THRONE) && !LastMoveBefore(stateMachine, FROZEN_THRONE)) {
                possibilities.Add(FROZEN_THRONE);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task Blizzard(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<PromiseOfWinter>(new ThrowingPlayerChoiceContext(), Creature, FrozenAmt, Creature, null);
        await ResetIdle(1.0f);
    }
    
    private async Task FrigidGaze(IReadOnlyList<Creature> targets)
    {
        await StrikeAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await DamageCmd.Attack(FrigidDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task IceSplinters(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(IceDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task FrozenThrone(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, PlatingAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SnowAttack, targets);
    }
    
    private async Task StrikeAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.SnowAttackFar, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.SnowBlizzard, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Blunt", "Special"], controller);
    }
}