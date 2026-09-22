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
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Puppeteer;

public class Puppet : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 99, 90);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 108, 98);
    public override int NumIntents => 1;

    private int ForcefulGestureDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int RepressedFleshDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int RepressedFleshHits => 2;
    private int BlockAmt => 12;
    private int PlatingAmt => 10;
    public bool attackingAlly;

    protected override string VisualsPath => "Puppet/puppet.tscn".MonsterImagePath();

    private const string FORCEFUL_GESTURE = "FORCEFUL_GESTURE";
    private const string REPRESSED_FLESH = "REPRESSED_FLESH";
    public static string REVIVING = "REVIVING";
    public static string REVIVE = "REVIVE";
    private Creature? puppeteer;
    
    public MoveState? _revivingState;
    public MoveState? RevivingState
    {
        get => _revivingState;
        set
        {
            AssertMutable();
            _revivingState = value;
        }
    }

    public override bool ShouldDisappearFromDoom => false;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Chesed>();
        puppeteer = FindTarget<Puppeteer>();
        await CreatureCmd.GainBlock(Creature, PlatingAmt, ValueProp.Move, null); 
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, PlatingAmt, Creature,  null);
        await PowerCmd.Apply<PuppetStrings>(new ThrowingPlayerChoiceContext(), Creature, Creature.ScaleHpForMultiplayer(PlatingAmt, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex), Creature,  null);
        attackingAlly = Rng.NextBool();
    }

    private MoveState GetForcefulGestureState()
    {
        return new MoveState(FORCEFUL_GESTURE, ForcefulGesture, new RuinaSingleAttackIntent(ForcefulGestureDamage), new DefendIntent());
    }
    
    private MoveState GetRepressedFleshState()
    {
        return new MoveState(REPRESSED_FLESH, RepressedFlesh, new RuinaMultiAttackIntent(RepressedFleshDamage, RepressedFleshHits));
    }
    
    private MoveState GetRevivingState()
    {
        return new MoveState(REVIVING, Reviving, new UnknownIntent());
    }
    
    private MoveState GetReviveState()
    {
        return new MoveState(REVIVE, Revive, new HealIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetForcefulGestureState();
        var state2 = GetRepressedFleshState();
        var state3 = GetReviveState();
        RevivingState = GetRevivingState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        RevivingState.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(RevivingState);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
     private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (Creature.IsDead)
        {
            return REVIVE;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastTwoMoves(stateMachine, FORCEFUL_GESTURE)) {
                possibilities.Add(FORCEFUL_GESTURE);
            }
            if (!LastTwoMoves(stateMachine, REPRESSED_FLESH)) {
                possibilities.Add(REPRESSED_FLESH);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive && attackingAlly)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task ForcefulGesture(IReadOnlyList<Creature> targets)
    {
        await BluntAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await DamageCmd.Attack(ForcefulGestureDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
        attackingAlly = !attackingAlly;
    }
    
    private async Task RepressedFlesh(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < RepressedFleshHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(RepressedFleshDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        attackingAlly = !attackingAlly;
    }
    
    private async Task Reviving(IReadOnlyList<Creature> targets)
    {
    }
    
    private async Task Revive(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.Heal(Creature, Creature.MaxHp);
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, PlatingAmt, Creature,  null);
        IsReviving = false;
    }
    
    public async Task TriggerDeadState()
    {
        if (RevivingState != null)
        {
            SetMoveImmediateMultiIntentMonster(RevivingState, 0);
        }
    }
    
    public override async Task BeforeDeath(Creature creature)
    {
        await base.BeforeDeath(creature);
        if (creature != Creature)
            return;
    
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.Monster is Chesed chesed && puppeteer != null)
        {
            chesed.Targets = [puppeteer];
        }
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.BluntVert, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BluntHori, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Blunt"], controller);
    }
}