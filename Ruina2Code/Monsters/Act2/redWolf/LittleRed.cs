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
using Ruina2.Ruina2Code.Intents;

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
    private int HealAmount => 10;
    private int DebuffAmt = 1;
    public bool enraged;

    protected override string VisualsPath => "LittleRed/little_red.tscn".MonsterImagePath();

    private const string BEAST_HUNT = "BEAST_HUNT";
    private const string CATCH_BREATH = "CATCH_BREATH";
    private const string HOLLOW_POINT_SHELL  = "HOLLOW_POINT_SHELL ";
    private const string BULLET_SHOWER = "BULLET_SHOWER";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        SetToSide(CombatSide.Player);
        FindAndSetTarget<NightmareWolf>();
        FlipHorizontal();
        var node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (node != null)
        {
            node.Position = new Vector2(0, 200);
        }
        MainFile.Logger.Info("Little Red position: "+ node?.Position);
        //await PowerCmd.Apply<SporeCloudPower>(new ThrowingPlayerChoiceContext(), Creature, VulnerableAmount, Creature, null);
    }

    private MoveState GetBeastHuntState()
    {
        return new MoveState(BEAST_HUNT, BeastHunt, new RuinaSingleAttackIntent(BeastHuntDamage), new DebuffIntent());
    }

    private MoveState GetCatchBreathState()
    {
        return new MoveState(CATCH_BREATH, CatchBreath, new BuffIntent());
    }

    private MoveState GetHollowPointShellState()
    {
        return new MoveState(HOLLOW_POINT_SHELL, HollowPointShell, new RuinaMultiAttackIntent(HollowPointDamage, HollowPointHits));
    }

    private MoveState GetBulletShowerState()
    {
        return new MoveState(BULLET_SHOWER, BulletShower, new RuinaMultiAttackIntent(BulletShowerDamage, BulletShowerHits));
    }

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
        await CreatureCmd.Heal(Creature, HealAmount, true);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
    }
    
    private async Task HollowPointShell(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < HollowPointHits; i++)
        {
            if (i % 2 == 0)
            {
                await Shoot1Animation(targets);
            } else {
                await Shoot2Animation(targets);
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
        await SlashAnimation(targets);
        await DamageCmd.Attack(BeastHuntDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTick<VulnerablePower>(targets, DebuffAmt);
        await ResetIdle();
    }
    
    private async Task BulletShower(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < BulletShowerHits; i++)
        {
            if (i == 0)
            {
                await Shoot1Animation(targets);
            } else if (i == 1)
            {
                await Shoot2Animation(targets);
            } else {
                await Shoot3Animation(targets);
            }
            await DamageCmd.Attack(BulletShowerDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.LITTLE_RED_SLASH, targets);
    }
    
    private async Task Shoot1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Shoot1", Sfx.LITTLE_RED_GUN, targets);
    }
    
    private async Task Shoot2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Shoot2", Sfx.LITTLE_RED_GUN, targets);
    }
    
    private async Task Shoot3Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Shoot3", Sfx.LITTLE_RED_GUN, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Shoot1", "Shoot2", "Shoot3", "Slash"], controller);
    }
}