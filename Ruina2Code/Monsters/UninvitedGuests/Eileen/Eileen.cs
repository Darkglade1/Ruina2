using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Eileen;
using Ruina2.Ruina2Code.Cards.EnemyCards.Oswald;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;
using Brainwash = Ruina2.Ruina2Code.Cards.EnemyCards.Eileen.Brainwash;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Eileen;

public sealed class Eileen : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1100, 1000);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int PropagateDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int BrainwashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);
    private int BlockAmt => 22;
    private int DebuffAmt => 1;
    public Creature? minion1;
    public Creature? minion2;

    protected override string VisualsPath => "Eileen/eileen.tscn".MonsterImagePath();

    private const string PREACH = "PREACH";
    private const string ACCELERATE = "ACCELERATE";
    private const string PROPAGATE  = "PROPAGATE";
    private const string BRAINWASH = "BRAINWASH";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Yesod>();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.Monster is GearWorshipper)
            {
                if (minion1 == null)
                {
                    minion1 = enemy;
                } else if (minion2 == null)
                {
                    minion2 = enemy;
                }
            }
        }
        await PowerCmd.Apply<Church>(new ThrowingPlayerChoiceContext(), Creature, Creature.ScaleHpForMultiplayer(100, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex), Creature, null);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-EILEEN.talk"), Creature, VfxColor.Blue);
    }

    private MoveState GetPreachState()
    {
        return new MoveState(PREACH, Preach, new BuffIntent());
    }

    private MoveState GetAccelerateState()
    {
        return new MoveState(ACCELERATE, Accelerate, new DefendIntent());
    }

    private MoveState GetPropagateState()
    {
        return new MoveState(PROPAGATE, Propagate, new RuinaSingleAttackIntent(PropagateDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetBrainwashState()
    {
        return new MoveState(BRAINWASH, Brainwash, new RuinaSingleAttackIntent(BrainwashDamage));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPropagateState();
        var state2 = GetBrainwashState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPreachState();
        var state2 = GetAccelerateState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Accelerate>();
        card1.SetBlock(BlockAmt);
        var card2 = CreateCardForIntent<Propagate>();
        card2.SetDamage(PropagateDamage);
        card2.SetVulnerable(DebuffAmt);
        var card3 = CreateCardForIntent<Preach>();
        card3.SetStrength(StrengthAmount);
        var card4 = CreateCardForIntent<Brainwash>();
        card4.SetDamage(BrainwashDamage);
        return new Dictionary<string, CardModel>()
        {
            {ACCELERATE, card1},
            {PROPAGATE, card2},
            {PREACH, card3},
            {BRAINWASH, card4}
        };
    }

    private async Task Preach(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        foreach (var creature in CombatState.HittableEnemies)
        {
            if (!(creature.Monster is AbstractAllyMonster))
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, StrengthAmount, Creature,  null);
            }
        }
        await ResetIdle(1.0f);
    }
    
    private async Task Accelerate(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        foreach (var creature in CombatState.HittableEnemies)
        {
            if (!(creature.Monster is AbstractAllyMonster))
            {
                await CreatureCmd.GainBlock(creature, BlockAmt, ValueProp.Move, null);
            }
        }
        await ResetIdle(1.0f);
    }
    
    private async Task Propagate(IReadOnlyList<Creature> targets)
    {
        await RangeAnimation(targets);
        await DamageCmd.Attack(PropagateDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTick<VulnerablePower>(targets, DebuffAmt);
        await ResetIdle();
    }
    
    private async Task Brainwash(IReadOnlyList<Creature> targets)
    {
        await StrongAttackAnimation(targets);
        await DamageCmd.Attack(BrainwashDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }
    
    public override async Task BeforeDeath(Creature creature)
    {
        await base.BeforeDeath(creature);
        if (creature != Creature)
            return;

        var livingMinions = CombatState.GetTeammatesOf(Creature)
            .Where(t => t != Creature && t.IsAlive && t.Monster is GearWorshipper)
            .ToList();

        foreach (var minion in livingMinions)
        {
            await CreatureCmd.Kill(minion);
        }
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Yesod yesod)
        {
            if (yesod.Creature.IsAlive)
            {
                await yesod.OnBossDeath();
            }
        }
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", Sfx.GearStrongStart);
    }

    private async Task BuffAnimation()
    {
        await AnimationAction("Ranged", Sfx.GearStrongStart);
    }
    
    private async Task RangeAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special1", Sfx.GearFar, targets);
    }
    
    private async Task StrongAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special2", Sfx.GearStrongAtk, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Block", "Ranged", "Special1", "Special2"], controller);
    }
}