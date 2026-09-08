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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class JudgementBird : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 310, 280);
    public override int MaxInitialHp => MinInitialHp;
    
    private int JudgementDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int GuiltDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int StrAmt => 3;
    private int DebuffAmt => 1;
    private int ParalysisAmt => 2;

    protected override string VisualsPath => "JudgementBird/judgement_bird.tscn".MonsterImagePath();

    private const string STARE = "STARE";
    private const string JUDGEMENT = "JUDGEMENT";
    private const string HEAVY_GUILT = "HEAVY_GUILT";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Sin>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetStareState()
    {
        return new MoveState(STARE, Stare, new DebuffIntent(), new BuffIntent());
    }

    private MoveState GetJudgementState()
    {
        return new MoveState(JUDGEMENT, Judgement, new SingleAttackIntent(JudgementDamage));
    }

    private MoveState GetHeavyGuiltState()
    {
        return new MoveState(HEAVY_GUILT, HeavyGuilt, new SingleAttackIntent(GuiltDamage), new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetStareState();
        var state2 = GetJudgementState();
        var state3 = GetHeavyGuiltState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state3;
        state3.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        
        return new MonsterMoveStateMachine(states, state3);
    }
    
    private async Task Stare(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Judgement(IReadOnlyList<Creature> targets)
    {
        await JudgementAnimation(targets);
        await WaitAnimation(0.25f);
        await JudgementFullScreenAnimation();
        await DamageCmd.Attack(JudgementDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task HeavyGuilt(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(GuiltDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, ParalysisAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task JudgementFullScreenAnimation()
    {
        Sfx.JudgementHang.Play();
        var fullScreenEffect = FullScreenAnimationEffect.Create("Hang/Hang".VfxImagePath(), 1.4f, 14);
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(fullScreenEffect);
        await WaitAnimation(1.4f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.JudgementAttack, targets);
    }
    
    private async Task JudgementAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Judgement", Sfx.JudgementDing, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.JudgementGong, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Special", "Judgement"], controller);
    }
}