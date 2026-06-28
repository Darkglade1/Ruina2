using System.Reflection;
using Godot;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act2.redWolf;

public sealed class LittleRed : AbstractAllyMonster
{
    public override int MinInitialHp => 200;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;

    private int BeastHuntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int HollowPointDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int HollowPointHits => 2;
    private int BulletShowerDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int BulletShowerHits => 3;
    private int StrengthAmount => 3;
    private int BlockAmount => 10;
    private int DebuffAmt = 1;

    protected override string VisualsPath => "NightmareWolf/nightmare_wolf.tscn".MonsterImagePath();

    private const string BEAST_HUNT = "BEAST_HUNT";
    private const string CATCH_BREATH = "CATCH_BREATH";
    private const string HOLLOW_POINT_SHELL  = "HOLLOW_POINT_SHELL ";
    private const string BULLET_SHOWER = "BULLET_SHOWER";

    public override async Task AfterAddedToRoom()
    {
        IsAlly = true;
        FieldInfo? backingField = typeof(Creature).GetField("<Side>k__BackingField", 
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField != null)
        {
            backingField.SetValue(Creature, CombatSide.Player); 
        }
        MainFile.Logger.Info("LITTLE RED'S SIDE IS: " + Creature.Side);
        await base.AfterAddedToRoom();
        foreach (var enemy in CombatState.Enemies)
        {
            if (enemy.Monster is NightmareWolf)
            {
                OtherSideTargetMonster = enemy;
            }
        }
        FlipHorizontal();
        //await PowerCmd.Apply<SporeCloudPower>(new ThrowingPlayerChoiceContext(), Creature, VulnerableAmount, Creature, null);
    }

    private MoveState GetBeastHuntState()
    {
        return new MoveState(BEAST_HUNT, BeastHunt, new SingleAttackIntent(BeastHuntDamage), new DebuffIntent());
    }

    private MoveState GetCatchBreathState()
    {
        return new MoveState(CATCH_BREATH, CatchBreath, new DefendIntent(), new BuffIntent());
    }

    private MoveState GetHollowPointShellState()
    {
        return new MoveState(HOLLOW_POINT_SHELL, HollowPointShell, new MultiAttackIntent(HollowPointDamage, HollowPointHits));
    }

    // private MoveState GetBulletShowerState()
    // {
    //     return new MoveState(BULLET_SHOWER, Hunt, new MultiAttackIntent(BulletShowerDamage, BulletShowerHits));
    // }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var beastHuntState = GetBeastHuntState();
        var catchBreathState = GetCatchBreathState();
        var hollowPointShellState = GetHollowPointShellState();

        hollowPointShellState.FollowUpState = catchBreathState;
        catchBreathState.FollowUpState = beastHuntState;
        beastHuntState.FollowUpState = hollowPointShellState;

        states.Add(beastHuntState);
        states.Add(catchBreathState);
        states.Add(hollowPointShellState);
        
        return new MonsterMoveStateMachine(states, hollowPointShellState);
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (IsAlly && OtherSideTargetMonster != null)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task CatchBreath(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BlockAmount, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
    }
    
    private async Task HollowPointShell(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < HollowPointHits; i++)
        {
            if (i % 2 == 0)
            {
                await ClawAnimation();
            } else {
                await BiteAnimation();
            }
            await DamageCmd.Attack(HollowPointDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task BeastHunt(IReadOnlyList<Creature> targets)
    {
        await ClawAnimation();
        await DamageCmd.Attack(BeastHuntDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
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