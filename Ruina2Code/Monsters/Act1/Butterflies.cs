using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class Butterflies : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 13, 12);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 17, 15);

    private int TranquilityDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int StatusAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int ParalysisAmt => 1;

    protected override string VisualsPath => "Butterflies/butterflies.tscn".MonsterImagePath();

    private const string TRANQUILITY = "TRANQUILITY";
    private const string LIBERATION = "LIBERATION";

    private MoveState GetTranquilityState()
    {
        return new MoveState(TRANQUILITY, Tranquility, new SingleAttackIntent(TranquilityDamage));
    }

    private MoveState GetLiberationState()
    {
        return new MoveState(LIBERATION, Liberation, new DebuffIntent(), new StatusIntent(StatusAmt));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetTranquilityState();
        var state2 = GetLiberationState();
        
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
        if (!LastTwoMoves(stateMachine, TRANQUILITY)) {
            possibilities.Add(TRANQUILITY);
        }
        if (!LastMove(stateMachine, LIBERATION)) {
            possibilities.Add(LIBERATION);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Tranquility(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(TranquilityDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Liberation(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, ParalysisAmt, Creature,  null);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.ButterflyAtk, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", null, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Special"], controller);
    }
}