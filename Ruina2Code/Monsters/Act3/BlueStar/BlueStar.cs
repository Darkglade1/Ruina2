using Godot;
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
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act3.BlueStar;

public sealed class BlueStar : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 220, 200);
    public override int MaxInitialHp => MinInitialHp;
    
    private int SoundDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 28, 25);
    private int StrAmt => 4;
    private int VulnAmt => 1;
    private int BlockAmt => 13;
    public override bool CanChangeScale => false;
    protected override string VisualsPath => "BlueStar/Shrine/shrine.tscn".MonsterImagePath();

    private const string RISING_STAR = "RISING_STAR";
    private const string STARRY_SKY = "STARRY_SKY";
    private const string SOUND_OF_STAR = "SOUND_OF_STAR";
    private const string WORSHIPPERS = "WORSHIPPERS";
    
    private CreatureAnimator? starAnimator;
    
    public override async Task AfterAddedToRoom()
    {
        if (NCombatRoom.Instance != null)
        {
            NCreature? creatureNode = NCombatRoom.Instance.GetCreatureNode(Creature);
            if (creatureNode != null)
            {
                var otherVisuals = creatureNode.Visuals.GetNodeOrNull<Node2D>((NodePath) "%OtherVisuals");
                if (otherVisuals != null)
                {
                    var otherSpineBody = new MegaSprite((Variant) (GodotObject) otherVisuals);
                    if (otherSpineBody?.GetSkeleton()?.GetData() == null)
                    {
                        otherSpineBody = null;
                    }
                    if (otherSpineBody != null)
                    {
                        var idle = new AnimState("Idle", true);
                        var special = new AnimState("Special");
                        var animator = new CreatureAnimator(idle, otherSpineBody);
                        animator.AddAnyState("Idle", idle);
                        animator.AddAnyState("Special", special);
                        starAnimator = animator;
                        starAnimator.SetTrigger("Idle");
                    }
                }
            }
        }
        await base.AfterAddedToRoom();
    }

    private MoveState GetRisingStarState()
    {
        return new MoveState(RISING_STAR, RisingStar, new DefendIntent(), new DebuffIntent());
    }

    private MoveState GetStarrySkyState()
    {
        return new MoveState(STARRY_SKY, StarrySky, new BuffIntent());
    }

    private MoveState GetSoundOfStarState()
    {
        return new MoveState(SOUND_OF_STAR, SoundOfStar, new SingleAttackIntent(SoundDamage));
    }
    
    private MoveState GetWorshippersState()
    {
        return new MoveState(WORSHIPPERS, Worshippers, new SummonIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetRisingStarState();
        var state2 = GetStarrySkyState();
        var state3 = GetSoundOfStarState();
        var state4 = GetWorshippersState();
        
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
        bool minionsAlive = false;
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.Monster is Worshipper && enemy.IsAlive)
            {
                minionsAlive = true;
            }
        }
        if (!minionsAlive)
        {
            return WORSHIPPERS;
        }
        if (LastMoveIgnoringMove(stateMachine, RISING_STAR, WORSHIPPERS))
        {
            return STARRY_SKY;
        } else if (LastMoveIgnoringMove(stateMachine, STARRY_SKY, WORSHIPPERS))
        {
            return SOUND_OF_STAR;
        }
        else
        {
            return RISING_STAR;
        }
    }
    
    private async Task RisingStar(IReadOnlyList<Creature> targets)
    {
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await CreatureCmd.GainBlock(enemy, BlockAmt, ValueProp.Move, null);
        }
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, VulnAmt, Creature,  null);
    }
    
    private async Task StarrySky(IReadOnlyList<Creature> targets)
    {
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, StrAmt, Creature,  null);
        }
    }
    
    private async Task SoundOfStar(IReadOnlyList<Creature> targets)
    {
        await SoundAnimation(Sfx.BlueStarCharge, targets);
        await WaitAnimation(1.0f);
        await StarAttackAnimation(targets);
        await DamageCmd.Attack(SoundDamage)
            .FromMonster(this)
            .Execute(null);
        await WaitAnimation(1.0f);
        await ResetStarAnimation();
    }
    
    private async Task Worshippers(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.Add<Worshipper>(CombatState, "minion1");
        await CreatureCmd.Add<Worshipper>(CombatState, "minion2");
    }
    
    public override async Task BeforeDeath(Creature creature)
    {
        if (creature == Creature)
        {
            foreach (var enemy in CombatState.Enemies)
            {
                if (enemy.Monster is Worshipper worshipper)
                {
                    worshipper.TriggerMartyr = false;
                }
            }
        }
    }

    private async Task StarAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await SoundAnimation(Sfx.BlueStarAtk, targets);
        if (starAnimator != null)
        {
            starAnimator.SetTrigger("Special");
        }
    }
    
    private async Task ResetStarAnimation()
    {
        if (starAnimator != null)
        {
            starAnimator.SetTrigger("Idle");
        }
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle"], controller);
    }
}