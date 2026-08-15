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
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class Pinocchio : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 190, 170);
    public override int MaxInitialHp => MinInitialHp;
    
    private int LearnDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int LearnHits => 2;
    private int LieDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int StrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int DebuffAmt => 1;
    private int BlockAmt => 20;
    private int ArtifactAmt => 2;

    protected override string VisualsPath => "Pinocchio/pinocchio.tscn".MonsterImagePath();

    private const string LEARN = "LEARN";
    private const string LIE = "LIE";
    private const string FIB = "FIB";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        Sfx.PinoOn.Play();
        await PowerCmd.Apply<Learn>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, ArtifactAmt, Creature, null);
    }

    private MoveState GetLearnState()
    {
        return new MoveState(LEARN, Learn, new MultiAttackIntent(LearnDamage, LearnHits));
    }

    private MoveState GetLieState()
    {
        return new MoveState(LIE, Lie, new SingleAttackIntent(LieDamage), new BuffIntent());
    }

    private MoveState GetFibState()
    {
        return new MoveState(FIB, Fib, new DefendIntent(), new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLearnState();
        var state2 = GetLieState();
        var state3 = GetFibState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state3;
        state3.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task Learn(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < LearnHits; i++)
        {
            await PierceAnimation(targets);
            await DamageCmd.Attack(LearnDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task Lie(IReadOnlyList<Creature> targets)
    {
        await PierceAnimation(targets);
        await DamageCmd.Attack(LieDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Fib(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.BluntBlow, targets);
    }
    
    private async Task BlockAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Block", Sfx.PinoLie, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Block"], controller);
    }
}