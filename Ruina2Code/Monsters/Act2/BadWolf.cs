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
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2;

public sealed class BadWolf : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 88, 80);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 92, 84);
    
    private int ClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private int HuntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);
    private int BleedAmt => 2;

    public int phase = 1;
    public bool powerTriggered = false;

    protected override string VisualsPath => "BadWolf/bad_wolf.tscn".MonsterImagePath();

    private const string CLAW = "CLAW";
    private const string BITE = "BITE";
    private const string HUNT = "HUNT";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Hunter>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
    }

    private MoveState GetClawState()
    {
        return new MoveState(CLAW, Claw, new SingleAttackIntent(ClawDamage), new DebuffIntent());
    }

    private MoveState GetBiteState()
    {
        return new MoveState(BITE, Bite, new SingleAttackIntent(BiteDamage), new HealIntent());
    }

    private MoveState GetHuntState()
    {
        return new MoveState(HUNT, Hunt, new SingleAttackIntent(HuntDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetClawState();
        var state2 = GetBiteState();
        var state3 = GetHuntState();
        
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
        if (powerTriggered)
        {
            return HUNT;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, CLAW)) {
                possibilities.Add(CLAW);
            }
            if (!LastMove(stateMachine, BITE)) {
                possibilities.Add(BITE);
            }
            if (!LastMove(stateMachine, HUNT)) {
                possibilities.Add(HUNT);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task Claw(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(ClawDamage)
            .FromMonster(this)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTick<Bleed>(targets, BleedAmt);
        await ResetIdle();
    }
    
    private async Task Bite(IReadOnlyList<Creature> targets)
    {
        await BiteAnimation(targets);
        var attackCommand = await DamageCmd.Attack(BiteDamage)
            .FromMonster(this)
            .Execute(null);
        await VampireHeal(attackCommand);
        await ResetIdle();
    }
    
    private async Task Hunt(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(HuntDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash" + phase, Sfx.WOLF_SLASH, targets);
    }
    
    private async Task BiteAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Bite" + phase, Sfx.WOLF_BITE, targets);
    }
    
    protected override async Task ResetIdle()
    {
        await WaitAnimation();
        await CreatureCmd.TriggerAnim(Creature, "Idle" + phase, 0);
    }
    
    public async Task SetPhase(int newPhase)
    {
        phase = newPhase;
        if (phase == 2)
        {
            powerTriggered = true;
            Sfx.WOLF_FOG.Play(0, 0.7f);
            var state = GetHuntState();
            state.FollowUpState = state;
            SetMoveImmediate(state);
        }
        await CreatureCmd.TriggerAnim(Creature, "Idle" + phase, 0);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle1", "Idle2", "Slash1", "Slash2", "Bite1", "Bite2"], controller);
    }
}