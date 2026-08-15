using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
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
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.Twilight;

public sealed class Twilight : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 530, 480);
    public override int MaxInitialHp => MinInitialHp;
    
    private int PeaceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 44, 40);
    private int SurveillanceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);
    private int TornDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 22);
    private int TalonsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);
    private int TalonsHits => 2;
    private int FrailAmt => 2;
    private int BleedAmt => 3;
    private int VulnerableAmt => 1;
    private int BlockAmt => 24;
    private int HPLossPercent => 25;

    protected override string VisualsPath => "Twilight/twilight.tscn".MonsterImagePath();

    private const string PEACE_FOR_ALL = "PEACE_FOR_ALL";
    private const string SURVEILLANCE = "SURVEILLANCE";
    private const string TORN_MOUTH = "TORN_MOUTH";
    private const string TILTED_SCALE = "TILTED_SCALE";
    private const string TALONS = "TALONS";

    private static int BIG_BIRD_PHASE = 1;
    private static int SMALL_BIRD_PHASE = 2;
    private static int LONG_BIRD_PHASE = 3;
    private int phase = BIG_BIRD_PHASE;
    
    private CreatureAnimator? birdAnimator;
    
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
                        var smash = new AnimState("Smash");
                        smash.NextState = idle;
                        var animator = new CreatureAnimator(idle, otherSpineBody);
                        animator.AddAnyState("Idle", idle);
                        animator.AddAnyState("Smash", smash);
                        birdAnimator = animator;
                        birdAnimator.SetTrigger("Idle");
                    }
                }
            }
        }
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<FadingTwilight>(new ThrowingPlayerChoiceContext(), Creature, HPLossPercent, Creature, null);
        Sfx.BossBirdBirth.Play(0, 0.5f);
    }

    private MoveState GetPeaceForAllState()
    {
        return new MoveState(PEACE_FOR_ALL, PeaceForAll, new SingleAttackIntent(PeaceDamage));
    }

    private MoveState GetSurveillanceState()
    {
        return new MoveState(SURVEILLANCE, Surveillance, new SingleAttackIntent(SurveillanceDamage), new DebuffIntent());
    }

    private MoveState GetTornMouthState()
    {
        return new MoveState(TORN_MOUTH, TornMouth, new SingleAttackIntent(TornDamage), new DebuffIntent());
    }
    
    private MoveState GetTiltedScaleState()
    {
        return new MoveState(TILTED_SCALE, TiltedScale, new DefendIntent(), new DebuffIntent());
    }
    
    private MoveState GetTalonsState()
    {
        return new MoveState(TALONS, Talons, new MultiAttackIntent(TalonsDamage, TalonsHits));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPeaceForAllState();
        var state2 = GetSurveillanceState();
        var state3 = GetTornMouthState();
        var state4 = GetTiltedScaleState();
        var state5 = GetTalonsState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;
        state5.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(state5);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (phase == BIG_BIRD_PHASE)
        {
            if (!LastMove(stateMachine, SURVEILLANCE))
            {
                return SURVEILLANCE;
            }
            else
            {
                return SetAttack(stateMachine);
            }
        } else if (phase == SMALL_BIRD_PHASE)
        {
            if (!LastMove(stateMachine, TORN_MOUTH))
            {
                return TORN_MOUTH;
            }
            else
            {
                return SetAttack(stateMachine);
            }
        }
        else
        {
            if (!LastMove(stateMachine, TILTED_SCALE))
            {
                return TILTED_SCALE;
            }
            else
            {
                return SetAttack(stateMachine);
            }
        }
    }
    
    private string SetAttack(MonsterMoveStateMachine stateMachine) {
        if (UsedTalonsLast(stateMachine))
        {
            return PEACE_FOR_ALL;
        } else
        {
            return TALONS;
        }
    }
    
    private bool UsedTalonsLast(MonsterMoveStateMachine stateMachine) {
        for (int i = stateMachine.StateLog.Count - 1; i >= 0; i--)
        {
            var move = stateMachine.StateLog[i];
            if (move.Id == PEACE_FOR_ALL) {
                return false;
            }
            if (move.Id == TALONS) {
                return true;
            }
        }
        return false;
    }
    
    private async Task PeaceForAll(IReadOnlyList<Creature> targets)
    {
        await CommandAnimation(targets);
        if (birdAnimator != null)
        {
            birdAnimator.SetTrigger("Smash");
        }
        await WaitAnimation(1.3f);
        await ShockwaveAnimation(targets);
        await SoundAnimation(Sfx.BossBirdStrong, targets);
        await DamageCmd.Attack(PeaceDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
        IncrementPhase();
    }
    
    private async Task Surveillance(IReadOnlyList<Creature> targets)
    {
        await LampAnimation(targets);
        await DamageCmd.Attack(SurveillanceDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, FrailAmt, Creature, null);
        await ResetIdle();
    }
    
    private async Task TornMouth(IReadOnlyList<Creature> targets)
    {
        await PunishAnimation(targets);
        await DamageCmd.Attack(TornDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Bleed>(new ThrowingPlayerChoiceContext(), targets, BleedAmt, Creature, null);
        await ResetIdle();
    }
    private async Task TiltedScale(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, VulnerableAmt, Creature, null);
        await ResetIdle(1.0f);
    }
    
    private async Task Talons(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < TalonsHits; i++)
        {
            if (i % 2 == 0)
            {
                await CrushAnimation(targets);
            }
            else
            {
                await SlamAnimation(targets);
            }
            await DamageCmd.Attack(TalonsDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
        IncrementPhase();
    }

    private void IncrementPhase()
    {
        phase++;
        if (phase > LONG_BIRD_PHASE)
        {
            phase = BIG_BIRD_PHASE;
        }
    }

    private async Task ShockwaveAnimation(IReadOnlyList<Creature> targets)
    {
        var target = targets.FirstOrDefault(LocalContext.IsMe);
        if (target != null)
        {
            var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
            if (targetNode != null)
            {
                var shockwaveEffect = BirdShockwaveEffect.Create(targetNode.VfxSpawnPosition);
                Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
                vfxContainer?.AddChildSafely(shockwaveEffect);
            }
        }
    }
    
    private async Task CrushAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Crush", Sfx.BossBirdCrush, targets);
    }
    
    private async Task SlamAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slam", Sfx.BossBirdSlam, targets);
    }
    
    private async Task LampAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Lamp", Sfx.BossBirdLamp, targets);
    }
    
    private async Task PunishAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Punish", Sfx.BossBirdPunish, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.BossBirdSpecial, targets);
    }
    
    private async Task CommandAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Command", null, targets);
    }
    
    private async Task ShockwaveAnimation()
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (node != null)
        {
            var shockwaveEffect = ShockwaveEffect.Create(node.VfxSpawnPosition);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(shockwaveEffect);
            await WaitAnimation(2.0f);
        }
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Command", "Crush", "Lamp", "Punish", "Slam", "Special"], controller);
    }
}