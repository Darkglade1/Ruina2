using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.redWolf;

public sealed class LittleRed : AbstractAllyMonster
{
    public override int MinInitialHp => 200;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "RedIcon.png".UIImagePath();

    private int BeastHuntDamage => 9;
    private int HollowPointDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int HollowPointHits => 2;
    private int BulletShowerDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int BulletShowerHits => 3;
    private int StrengthAmount => 2;
    private int HealAmount => 10;
    private int DebuffAmt = 1;
    public bool enraged;
    public bool killedWolf;
    private bool attackingWolf;

    protected override string VisualsPath => "LittleRed/little_red.tscn".MonsterImagePath();

    private const string BEAST_HUNT = "BEAST_HUNT";
    private const string CATCH_BREATH = "CATCH_BREATH";
    private const string HOLLOW_POINT_SHELL  = "HOLLOW_POINT_SHELL ";
    private const string BULLET_SHOWER = "BULLET_SHOWER";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<NightmareWolf>();
        SetPosition(new Vector2(0, 200));
        await PowerCmd.Apply<Fury>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetBeastHuntState()
    {
        return new MoveState(BEAST_HUNT, BeastHunt, new RuinaSingleAttackIntent(BeastHuntDamage), new RuinaDebuffIntent());
    }

    private MoveState GetCatchBreathState()
    {
        return new MoveState(CATCH_BREATH, CatchBreath, new RuinaBuffIntent());
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
        var bulletShowerState = GetBulletShowerState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        hollowPointShellState.FollowUpState = moveBranch;
        catchBreathState.FollowUpState = moveBranch;
        beastHuntState.FollowUpState = moveBranch;
        bulletShowerState.FollowUpState = moveBranch;

        states.Add(beastHuntState);
        states.Add(catchBreathState);
        states.Add(hollowPointShellState);
        states.Add(bulletShowerState);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (enraged)
        {
            if (LastMove(stateMachine, BULLET_SHOWER))
            {
                return HOLLOW_POINT_SHELL;
            }
            else
            {
                return BULLET_SHOWER;
            }
        }
        else
        {
            if (LastMove(stateMachine, CATCH_BREATH) || (LastMove(stateMachine, HOLLOW_POINT_SHELL) && LocalContext.GetMe(CombatState)?.PlayerCombatState?.TurnNumber == 2))
            {
                return BEAST_HUNT;
            } else if (LastMove(stateMachine, HOLLOW_POINT_SHELL))
            {
                return CATCH_BREATH;
            }
            else
            {
                return HOLLOW_POINT_SHELL;
            }
        }
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
        await CreatureCmd.Heal(Creature, Creature.ScaleHpForMultiplayer(HealAmount, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex), true);
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
            if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
            {
                attackingWolf = true;
            }
            AttackCommand attackCommand = await DamageCmd.Attack(HollowPointDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            var targetKilled = attackCommand.Results.SelectMany(r => r)
                .Any((Func<DamageResult, bool>)(r => r.WasTargetKilled));
            if (targetKilled && attackingWolf)
            {
                await OnKillWolf();
            }
            await ResetIdle();
        }
    }
    
    private async Task BeastHunt(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            attackingWolf = true;
        }
        AttackCommand attackCommand = await DamageCmd.Attack(BeastHuntDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        var targetKilled = attackCommand.Results.SelectMany(r => r)
            .Any((Func<DamageResult, bool>)(r => r.WasTargetKilled));
        if (targetKilled && attackingWolf)
        {
            await OnKillWolf();
        }
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<VulnerablePower>(targets, DebuffAmt);
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

    private async Task OnKillWolf()
    {
        killedWolf = true;
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-LITTLE_RED.killWolf"), Creature, VfxColor.Red);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }

    public async Task Enrage()
    {
        if (attackingWolf || killedWolf)
        {
            return;
        }
        IsAlly = false;
        TalkCmd.Play(L10NMonsterLookup("RUINA2-LITTLE_RED.killStolen"), Creature, VfxColor.Red);
        Sfx.LITTLE_RED_RAGE.Play();
        if (NCombatRoom.Instance != null)
        {
            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(Creature));
        }
        SetToSide(CombatSide.Enemy);
        FlipHorizontal();
        RemoveAllyBlockButton();
        await CreatureCmd.Heal(Creature, Creature.MaxHp);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
        enraged = true;
        NCreature? creatureNode = Creature.GetCreatureNode();
        if (creatureNode != null)
        {
            await TaskHelper.RunSafely(creatureNode.RefreshIntents());
        }
        var player = LocalContext.GetMe(CombatState);
        if (player != null)
        {
            Targets[0] = player.Creature;
        }

        if (CombatState.Players.Count > 1)
        {
            await PowerCmd.Remove<MultiplayerAlly>(Creature);
        }
    }
    
    public override Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && !enraged && !killedWolf)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-LITTLE_RED.onAllyDeath"), Creature, VfxColor.Red);
            if (OtherSideTargetMonster?.Monster is NightmareWolf wolf && wolf.Creature.IsAlive)
            {
                wolf.OnRedDeath();
            }
        }
        return Task.CompletedTask;
    }
    
    protected override async Task ResetIdle()
    {
        await base.ResetIdle();
        attackingWolf = false;
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