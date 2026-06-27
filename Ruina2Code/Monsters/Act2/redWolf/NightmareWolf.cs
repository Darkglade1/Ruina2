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
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Monsters.Act2.redWolf;

public sealed class NightmareWolf : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 480, 450);
    public override int MaxInitialHp => MinInitialHp;

    private int ClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int FangDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int FangHits => 2;
    private int HuntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int HuntHits => 3;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int BlockAmount => 20;
    private int BleedAmount => 2;

    protected override string VisualsPath => "res://ActsFromThePast/monsters/fungi_beast/fungi_beast.tscn";

    private const string CRUEL_CLAWS = "CRUEL_CLAWS";
    private const string FEROCIOUS_FANGS = "FEROCIOUS_FANGS";
    private const string BLOODSTAINED_HUNT  = "BLOODSTAINED_HUNT";
    private const string HOWL = "HOWL";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        NumExtraIntents = 1;
        //await PowerCmd.Apply<SporeCloudPower>(new ThrowingPlayerChoiceContext(), Creature, VulnerableAmount, Creature, null);
    }

    public override async Task BeforeDeath(Creature creature)
    {
        await base.BeforeDeath(creature);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var clawState = new MoveState(
            CRUEL_CLAWS,
            Claws, new SingleAttackIntent(ClawDamage), new DefendIntent());
        var fangState = new MoveState(
            FEROCIOUS_FANGS,
            Fangs, new MultiAttackIntent(FangDamage, FangHits), new DebuffIntent());
        var huntState = new MoveState(
            BLOODSTAINED_HUNT,
            Hunt, new MultiAttackIntent(HuntDamage, HuntHits));

        clawState.FollowUpState = huntState;
        huntState.FollowUpState = fangState;
        fangState.FollowUpState = clawState;

        states.Add(clawState);
        states.Add(huntState);
        states.Add(fangState);

        return new MonsterMoveStateMachine(states, fangState);
    }

    public override List<MonsterMoveStateMachine> GenerateExtraIntentMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var howlState = new MoveState(
            HOWL,
            Howl, new BuffIntent());
        var fangState = new MoveState(
            FEROCIOUS_FANGS,
            Fangs, new SingleAttackIntent(FangDamage), new DebuffIntent());
        var huntState = new MoveState(
            BLOODSTAINED_HUNT,
            Hunt, new MultiAttackIntent(HuntDamage, HuntHits));

        fangState.FollowUpState = huntState;
        huntState.FollowUpState = howlState;
        howlState.FollowUpState = fangState;

        states.Add(howlState);
        states.Add(huntState);
        states.Add(fangState);
        
        return [new MonsterMoveStateMachine(states, huntState)];
    }

    private async Task Fangs(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < FangHits; i++)
        {
            await DamageCmd.Attack(FangDamage)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.5f)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                .Execute(null);
        }
    }
    
    private async Task Hunt(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < HuntHits; i++)
        {
            await DamageCmd.Attack(HuntDamage)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.5f)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                .Execute(null);
        }
    }
    
    private async Task Claws(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BlockAmount, ValueProp.Move, null);
        await DamageCmd.Attack(ClawDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(null);
    }

    private async Task Howl(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        var idle = new AnimState("Idle", true);
        var attack = new AnimState("Attack");
        var hit = new AnimState("Hit");
    
        attack.NextState = idle;
        hit.NextState = idle;
    
        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Hit", hit);
        
        controller.GetAnimationState().SetTimeScale(Rng.Chaotic.NextFloat(0.7f, 1.0f));
    
        return animator;
    }
}