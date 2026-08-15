using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;
using Lock = Ruina2.Ruina2Code.Powers.Act3.Lock;

namespace Ruina2.Ruina2Code.Monsters.Act3.PunshingBird;

public sealed class Keeper : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 40);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 48);
    
    private int RingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int RingHits => 2;
    private int SmackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    
    private int BlockAmt => 12;
    private int DebuffAmt => 1;

    protected override string VisualsPath => "Keeper/keeper.tscn".MonsterImagePath();

    private const string RING = "RING";
    private const string SMACK = "SMACK";
    private const string CUCKOO = "CUCKOO";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Lock>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetRingState()
    {
        return new MoveState(RING, Ring, new MultiAttackIntent(RingDamage, RingHits));
    }

    private MoveState GetSmackState()
    {
        return new MoveState(SMACK, Smack, new SingleAttackIntent(SmackDamage));
    }

    private MoveState GetCuckooState()
    {
        return new MoveState(CUCKOO, Cuckoo, new DefendIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetRingState();
        var state2 = GetSmackState();
        var state3 = GetCuckooState();
        
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
        if (!LastMove(stateMachine, RING)) {
            possibilities.Add(RING);
        }
        if (!LastMove(stateMachine, SMACK)) {
            possibilities.Add(SMACK);
        }
        if (!LastMove(stateMachine, CUCKOO)) {
            possibilities.Add(CUCKOO);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Ring(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < RingHits; i++)
        {
            await AttackAnimation(targets);
            await DamageCmd.Attack(RingDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task Smack(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(SmackDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Cuckoo(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await ResetIdle(1.0f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Smack", Sfx.BluntBlow, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Ring", Sfx.BossBirdSpecial);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Ring", "Smack"], controller);
    }
}