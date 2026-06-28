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
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act2.redWolf;

public sealed class NightmareWolf : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 480, 450);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int ClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int FangDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int FangHits => 2;
    private int HuntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int HuntHits => 3;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int BlockAmount => 20;
    private int BleedAmount => 2;

    protected override string VisualsPath => "NightmareWolf/nightmare_wolf.tscn".MonsterImagePath();

    private const string CRUEL_CLAWS = "CRUEL_CLAWS";
    private const string FEROCIOUS_FANGS = "FEROCIOUS_FANGS";
    private const string BLOODSTAINED_HUNT  = "BLOODSTAINED_HUNT";
    private const string HOWL = "HOWL";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        //await PowerCmd.Apply<SporeCloudPower>(new ThrowingPlayerChoiceContext(), Creature, VulnerableAmount, Creature, null);
    }

    private MoveState GetClawState()
    {
        return new MoveState(CRUEL_CLAWS, Claws, new SingleAttackIntent(ClawDamage), new DefendIntent());
    }

    private MoveState GetFangState()
    {
        return new MoveState(FEROCIOUS_FANGS, Fangs, new MultiAttackIntent(FangDamage, FangHits), new DebuffIntent());
    }

    private MoveState GetHuntState()
    {
        return new MoveState(BLOODSTAINED_HUNT, Hunt, new MultiAttackIntent(HuntDamage, HuntHits));
    }

    private MoveState GetHowlState()
    {
        return new MoveState(HOWL, Howl, new BuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var clawState = GetClawState();
        var fangState = GetFangState();
        var huntState = GetHuntState();

        clawState.FollowUpState = huntState;
        huntState.FollowUpState = fangState;
        fangState.FollowUpState = clawState;

        states.Add(clawState);
        states.Add(huntState);
        states.Add(fangState);
        
        return new MonsterMoveStateMachine(states, fangState);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var howlState = GetHowlState();
        var fangState = GetFangState();
        var huntState = GetHuntState();

        fangState.FollowUpState = huntState;
        huntState.FollowUpState = howlState;
        howlState.FollowUpState = fangState;

        states.Add(howlState);
        states.Add(huntState);
        states.Add(fangState);
        
        return new MonsterMoveStateMachine(states, huntState);
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            return CombatState.PlayerCreatures[0];
        }
        if (intentNum == 1 && OtherSideTargetMonster != null)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task Fangs(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < FangHits; i++)
        {
            await BiteAnimation();
            await DamageCmd.Attack(FangDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task Hunt(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < HuntHits; i++)
        {
            if (i % 2 == 0)
            {
                await ClawAnimation();
            } else {
                await BiteAnimation();
            }
            await DamageCmd.Attack(HuntDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Claws(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BlockAmount, ValueProp.Move, null);
        await ClawAnimation();
        await DamageCmd.Attack(ClawDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }

    private async Task Howl(IReadOnlyList<Creature> targets)
    {
        await HowlAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
        await ResetIdle(1.0f);
    }

    private async Task BiteAnimation()
    {
        await CreatureCmd.TriggerAnim(Creature, "Bite", 0);
        Sfx.WOLF_BITE.Play();
    }
    
    private async Task ClawAnimation()
    {
        await CreatureCmd.TriggerAnim(Creature, "Claw", 0);
        Sfx.WOLF_SLASH.Play();
    }
    
    private async Task HowlAnimation()
    {
        await CreatureCmd.TriggerAnim(Creature, "Howl", 0);
        Sfx.WOLF_HOWL.Play();
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        var idle = new AnimState("Idle", true);
        var bite = new AnimState("Bite");
        var claw = new AnimState("Claw");
        var howl = new AnimState("Howl");
    
        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Bite", bite);
        animator.AddAnyState("Claw", claw);
        animator.AddAnyState("Howl", howl);
    
        return animator;
    }
}