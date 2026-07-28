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
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.Act2;

public sealed class Nosferatu : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 93, 85);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 99, 90);
    
    private int DroughtDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private int GestureDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 3);
    private int VulnAmt => 1;
    private int ParalysisAmt => 2;

    protected override string VisualsPath => "Nosferatu/nosferatu.tscn".MonsterImagePath();

    private const string UNBEARABLE_DROUGHT = "UNBEARABLE_DROUGHT";
    private const string MERCILESS_GESTURE = "MERCILESS_GESTURE";
    private const string LOOMING_PRESENCE = "LOOMING_PRESENCE";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        Sfx.NOS_CHANGE.Play();
    }

    private MoveState GetUnbearableDroughtState()
    {
        return new MoveState(UNBEARABLE_DROUGHT, UnbearableDrought, new SingleAttackIntent(DroughtDamage), new HealIntent());
    }

    private MoveState GetMercilessGestureState()
    {
        return new MoveState(MERCILESS_GESTURE, MercilessGesture, new SingleAttackIntent(GestureDamage), new DebuffIntent());
    }

    private MoveState GetLoomingPresenceState()
    {
        return new MoveState(LOOMING_PRESENCE, LoomingPresence, new DebuffIntent(), new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetUnbearableDroughtState();
        var state2 = GetMercilessGestureState();
        var state3 = GetLoomingPresenceState();
        
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
        if (LastMove(stateMachine, MERCILESS_GESTURE))
        {
            return UNBEARABLE_DROUGHT;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, UNBEARABLE_DROUGHT)) {
                possibilities.Add(UNBEARABLE_DROUGHT);
            }
            if (!LastMove(stateMachine, MERCILESS_GESTURE)) {
                possibilities.Add(MERCILESS_GESTURE);
            }
            if (!LastMove(stateMachine, LOOMING_PRESENCE) && !LastMoveBefore(stateMachine, LOOMING_PRESENCE)) {
                possibilities.Add(LOOMING_PRESENCE);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task UnbearableDrought(IReadOnlyList<Creature> targets)
    {
        await Attack1Animation(targets);
        var attackCommand = await DamageCmd.Attack(DroughtDamage)
            .FromMonster(this)
            .Execute(null);
        await CreatureCmd.Heal(Creature,
            attackCommand.Results.SelectMany(r => r)
                .Sum((Func<DamageResult, int>)(r => r.TotalDamage + r.OverkillDamage)));
        await ResetIdle();
    }
    
    private async Task MercilessGesture(IReadOnlyList<Creature> targets)
    {
        await Attack2Animation(targets);
        await DamageCmd.Attack(GestureDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, VulnAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task LoomingPresence(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, ParalysisAmt, Creature,  null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Attack1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack1", Sfx.NOS_BLOOD_EAT, targets);
    }
    
    private async Task Attack2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack2", Sfx.NOS_GRAB, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", Sfx.NOS_SPECIAL);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack1", "Attack2", "Special"], controller);
    }
}