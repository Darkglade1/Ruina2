using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.SilentGirl;

public sealed class SilentGirl : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 280, 250);
    public override int MaxInitialHp => MinInitialHp;
    
    private int NailDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);
    private int NailAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StatusAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int HammerDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 24);
    private int BrokenDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int BrokenHits => 2;
    private int StrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int DebuffAmt => 1;

    protected override string VisualsPath => "SilentGirl/silent_girl.tscn".MonsterImagePath();

    private const string BROKEN = "BROKEN";
    private const string LEER = "LEER";
    private const string SUPPRESS = "SUPPRESS";
    private const string NAIL = "NAIL";
    private const string HAMMER = "HAMMER";
    
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
    
    private CreatureAnimator? nailAnimator;
    private CreatureAnimator? hammerAnimator;

    public override bool ShouldDisappearFromDoom => !Creature.HasPower<Silence>();
    
    public override async Task AfterAddedToRoom()
    {
        if (NCombatRoom.Instance != null)
        {
            NCreature? creatureNode = NCombatRoom.Instance.GetCreatureNode(Creature);
            if (creatureNode != null)
            {
                var nailVisuals = creatureNode.Visuals.GetNodeOrNull<Node2D>((NodePath) "%NailVisuals");
                if (nailVisuals != null)
                {
                    var otherSpineBody = new MegaSprite((Variant) (GodotObject) nailVisuals);
                    if (otherSpineBody?.GetSkeleton()?.GetData() == null)
                    {
                        otherSpineBody = null;
                    }
                    if (otherSpineBody != null)
                    {
                        var idle = new AnimState("Idle", true);
                        var pierce = new AnimState("Pierce");
                        var dead = new AnimState("Dead", true);
                        var animator = new CreatureAnimator(idle, otherSpineBody);
                        animator.AddAnyState("Idle", idle);
                        animator.AddAnyState("Pierce", pierce);
                        animator.AddAnyState("Dead", dead);
                        nailAnimator = animator;
                        nailAnimator.SetTrigger("Idle");
                    }
                }
                var hammerVisuals = creatureNode.Visuals.GetNodeOrNull<Node2D>((NodePath) "%HammerVisuals");
                if (hammerVisuals != null)
                {
                    var otherSpineBody = new MegaSprite((Variant) (GodotObject) hammerVisuals);
                    if (otherSpineBody?.GetSkeleton()?.GetData() == null)
                    {
                        otherSpineBody = null;
                    }
                    if (otherSpineBody != null)
                    {
                        var idle = new AnimState("Idle", true);
                        var blunt = new AnimState("Blunt");
                        var dead = new AnimState("Dead", true);
                        var animator = new CreatureAnimator(idle, otherSpineBody);
                        animator.AddAnyState("Idle", idle);
                        animator.AddAnyState("Blunt", blunt);
                        animator.AddAnyState("Dead", dead);
                        hammerAnimator = animator;
                        hammerAnimator.SetTrigger("Idle");
                    }
                }
            }
        }
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Silence>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }
    
    private MoveState GetNailState()
    {
        return new MoveState(NAIL, Nail, new SingleAttackIntent(NailDamage), new DebuffIntent(), new StatusIntent(StatusAmt));
    }
    
    private MoveState GetHammerState()
    {
        return new MoveState(HAMMER, Hammer, new SingleAttackIntent(HammerDamage), new BuffIntent());
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
        return new MoveState(SUPPRESS, Suppress, new HealIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBrokenState();
        var state2 = GetLeerState();
        var state3 = GetNailState();
        var state4 = GetHammerState();
        SuppressState = GetSuppressState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;
        SuppressState.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(SuppressState);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (Creature.IsDead)
        {
            return SUPPRESS;
        } else if (Phase == 2)
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
            if (LastMove(stateMachine, NAIL))
            {
                return HAMMER;
            }
            else
            {
                return NAIL;
            }
        }
    }
    
    private async Task Nail(IReadOnlyList<Creature> targets)
    {
        await NailAnimation(targets);
        await DamageCmd.Attack(NailDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Powers.Act3.Nail>(new ThrowingPlayerChoiceContext(), targets, NailAmt, Creature,  null);
        await CardPileCmd.AddToCombatAndPreview<Wound>(targets, PileType.Discard, StatusAmt, null);
        await WaitAnimation();
        await ResetNailHammerIdle();
    }
    
    private async Task Hammer(IReadOnlyList<Creature> targets)
    {
        await HammerAnimation(targets);
        await DamageCmd.Attack(HammerDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await WaitAnimation();
        await ResetNailHammerIdle();
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
        await CreatureCmd.Heal(Creature, Creature.MaxHp);
        await PhaseChangeAnimation();
        Phase = 2;
        await PowerCmd.Remove<Silence>(Creature);
        await ResetIdle(1.0f);
    }
    
    public async Task TriggerDeadState()
    {
        if (SuppressState != null)
        {
            SetMoveImmediate(SuppressState);
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
    
    private async Task PhaseChangeAnimation()
    {
        await AnimationAction("Ranged", Sfx.SilentPhaseChange);
        if (nailAnimator != null)
        {
            nailAnimator.SetTrigger("Dead");
        }
        if (hammerAnimator != null)
        {
            hammerAnimator.SetTrigger("Dead");
        }
    }
    
    private async Task NailAnimation(IReadOnlyList<Creature> targets)
    {
        if (nailAnimator != null)
        {
            nailAnimator.SetTrigger("Pierce");
        }
        await SoundAnimation(Sfx.SilentNail, targets);
    }
    
    private async Task ResetNailHammerIdle()
    {
        if (nailAnimator != null)
        {
            nailAnimator.SetTrigger("Idle");
        }
        if (hammerAnimator != null)
        {
            hammerAnimator.SetTrigger("Idle");
        }
    }
    
    private async Task HammerAnimation(IReadOnlyList<Creature> targets)
    {
        if (hammerAnimator != null)
        {
            hammerAnimator.SetTrigger("Blunt");
        }
        await SoundAnimation(Sfx.SilentHammer, targets);
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