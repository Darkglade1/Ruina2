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
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class BurrowingHeaven : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 165, 150);
    public override int MaxInitialHp => MinInitialHp;
    
    private int HeavenDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 23, 21);
    private int WingsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int FrailAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int VulnerableAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StrDown => 2;
    private int DamageReduction => 50;

    protected override string VisualsPath => "BurrowingHeaven/burrowing_heaven.tscn".MonsterImagePath();

    private const string YOUR_OWN_HEAVEN = "YOUR_OWN_HEAVEN";
    private const string BLOODY_WINGS = "BLOODY_WINGS";
    private const string GAZE_OF_OTHERS = "GAZE_OF_OTHERS";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Unnerving>(new ThrowingPlayerChoiceContext(), Creature, DamageReduction, Creature, null);
    }

    private MoveState GetYourOwnHeavenState()
    {
        return new MoveState(YOUR_OWN_HEAVEN, YourOwnHeaven, new SingleAttackIntent(HeavenDamage));
    }

    private MoveState GetBloodyWingsState()
    {
        return new MoveState(BLOODY_WINGS, BloodyWings, new SingleAttackIntent(WingsDamage), new DebuffIntent());
    }

    private MoveState GetGazeOfOthersState()
    {
        return new MoveState(GAZE_OF_OTHERS, GazeOfOthers, new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetYourOwnHeavenState();
        var state2 = GetBloodyWingsState();
        var state3 = GetGazeOfOthersState();
        
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
        if (!LastMove(stateMachine, YOUR_OWN_HEAVEN)) {
            possibilities.Add(YOUR_OWN_HEAVEN);
        }
        if (!LastMove(stateMachine, BLOODY_WINGS)) {
            possibilities.Add(BLOODY_WINGS);
        }
        if (!LastMove(stateMachine, GAZE_OF_OTHERS) && !LastMoveBefore(stateMachine, GAZE_OF_OTHERS)) {
            possibilities.Add(GAZE_OF_OTHERS);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task YourOwnHeaven(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(HeavenDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task BloodyWings(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(WingsDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, FrailAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task GazeOfOthers(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), targets, -StrDown, Creature,  null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, VulnerableAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.WoodFinish, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.HeavenWakeStrong, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Special"], controller);
    }
}