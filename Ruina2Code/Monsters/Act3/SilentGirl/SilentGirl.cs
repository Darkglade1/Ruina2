using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
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

namespace Ruina2.Ruina2Code.Monsters.Act3.SilentGirl;

public sealed class SilentGirl : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 330, 300);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    
    private int BrokenDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int BrokenHits => 2;
    private int StrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int DebuffAmt => 1;

    protected override string VisualsPath => "SilentGirl/silent_girl.tscn".MonsterImagePath();

    private const string BROKEN = "BROKEN";
    private const string LEER = "LEER";
    private const string SUPPRESS = "SUPPRESS";
    private const string NONE = "NONE";
    
    public MoveState? _suppressState;
    public MoveState? SuppressState
    {
        get => _suppressState;
        set
        {
            AssertMutable();
            _suppressState = value;
        }
    }

    public int Phase = 1;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Silence>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        SetToSide(CombatSide.Player);
    }

    private MoveState GetBrokenState()
    {
        return new MoveState(BROKEN, Broken, new MultiAttackIntent(BrokenDamage, BrokenHits));
    }

    private MoveState GetLeerState()
    {
        return new MoveState(LEER, Leer, new BuffIntent(), new DebuffIntent());
    }

    private MoveState GetSuppressState()
    {
        return new MoveState(SUPPRESS, Suppress, new UnknownIntent());
    }
    
    private MoveState GetNoneState()
    {
        return new MoveState(NONE, _ => Task.CompletedTask);
    }
    
    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBrokenState();
        var state2 = GetLeerState();
        var state3 = GetNoneState();
        SuppressState = GetSuppressState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        SuppressState.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(SuppressState);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (Phase == 2)
        {
            if (LastMove(stateMachine, BROKEN) && LastMoveBefore(stateMachine, BROKEN))
            {
                return LEER;
            }
            else
            {
                return BROKEN;
            }
        }
        else
        {
            return NONE;
        }
    }
    
    public override Creature DetermineTargetForIntent(int intentNum)
    {
        return CombatState.PlayerCreatures[0];
    }
    
    private async Task Broken(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < BrokenHits; i++)
        {
            if (i % 2 == 0) {
                await SpecialUpAnimation(targets);
            } else {
                await SpecialDownAnimation(targets);
            } 
            await DamageCmd.Attack(BrokenDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Leer(IReadOnlyList<Creature> targets)
    {
        await RangedAnimation(targets);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Suppress(IReadOnlyList<Creature> targets)
    {
        await PhaseChangeAnimation(targets);
        Phase = 2;
        await ResetIdle(1.0f);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature.Monster is Hammer || creature.Monster is Nail)
        {
            bool aliveHammerOrNail = false;
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy.Monster is Hammer || enemy.Monster is Nail)
                {
                    aliveHammerOrNail = true;
                }
            }

            if (!aliveHammerOrNail && SuppressState != null)
            {
                SetToSide(CombatSide.Enemy);
                await PowerCmd.Remove<Silence>(Creature);
                SetMoveImmediateMultiIntentMonster(SuppressState, 0);   
            }
        }
    }
    
    private async Task SpecialUpAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SpecialUp", Sfx.SilentHammer, targets);
    }
    
    private async Task SpecialDownAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SpecialDown", Sfx.SilentHammer, targets);
    }
    
    private async Task RangedAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.SilentEye, targets);
    }
    
    private async Task PhaseChangeAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.SilentPhaseChange, targets);
    }
    
    protected override async Task ResetIdle(float waitTime)
    {
        await WaitAnimation(waitTime);
        await CreatureCmd.TriggerAnim(Creature, "Idle" + Phase, 0);
    }
    
    protected override async Task ResetIdle()
    {
        await WaitAnimation();
        await CreatureCmd.TriggerAnim(Creature, "Idle" + Phase, 0);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle1", "Idle2", "Ranged", "SpecialUp", "SpecialDown"], controller);
    }
}