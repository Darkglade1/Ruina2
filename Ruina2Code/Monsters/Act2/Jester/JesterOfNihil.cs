using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.Act2;
using CollectorCurseEffect = Ruina2.Ruina2Code.Nodes.CollectorCurseEffect;

namespace Ruina2.Ruina2Code.Monsters.Act2.Jester;

public sealed class JesterOfNihil : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 550, 500);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 3;

    private int NihilDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 24);
    private int DesireDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int DesireHits => 2;
    private int HateDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);
    private int TearsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int TearsHits => 2;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int AtkBlockAmount => 8;
    private int HateBlockAmount => 20;
    private int DebuffAmt => 1;
    private const int RampageCooldown = 1;
    private int rampageCooldown = RampageCooldown;
    private int numIntentThatCanRampage = 2; //0 is the second intent, 1 is the third intent, 2 is the first intent

    protected override string VisualsPath => "Jester/jester.tscn".MonsterImagePath();

    private const string WILL_OF_NIHIL = "WILL_OF_NIHIL";
    private const string CONSUMING_DESIRE = "CONSUMING_DESIRE";
    private const string LOVE_AND_HATE  = "LOVE_AND_HATE";
    private const string SWORD_OF_TEARS = "SWORD_OF_TEARS";
    private const string RAMPAGE = "RAMPAGE";

    public Creature? girl1;
    public Creature? girl2;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        girl1 = FindTarget<QueenOfLove>();
        girl2 = FindTarget<ServantOfCourage>();
        await PowerCmd.Apply<PointlessHate>(new ThrowingPlayerChoiceContext(), Creature, Creature.ScaleHpForMultiplayer(HateBlockAmount, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex), Creature, null);
        await PowerCmd.Apply<SenselessWrath>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetWillOfNihilState()
    {
        return new MoveState(WILL_OF_NIHIL, WillOfNihil, new RuinaSingleMassAttackIntent(NihilDamage));
    }

    private MoveState GetConsumingDesireState()
    {
        return new MoveState(CONSUMING_DESIRE, ConsumingDesire, new RuinaMultiAttackIntent(DesireDamage, DesireHits), new DefendIntent());
    }

    private MoveState GetLoveAndHateState()
    {
        return new MoveState(LOVE_AND_HATE, LoveAndHate, new RuinaSingleAttackIntent(HateDamage), new RuinaDebuffIntent());
    }

    private MoveState GetSwordOfTearsState()
    {
        return new MoveState(SWORD_OF_TEARS, SwordOfTears, new RuinaMultiAttackIntent(TearsDamage, TearsHits));
    }
    
    private MoveState GetRampageState()
    {
        return new MoveState(RAMPAGE, Rampage, new BuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetWillOfNihilState();
        var state2 = GetConsumingDesireState();
        var state3 = GetLoveAndHateState();
        var state4 = GetSwordOfTearsState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetConsumingDesireState();
        var state2 = GetLoveAndHateState();
        var state3 = GetSwordOfTearsState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 1);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private MonsterMoveStateMachine GenerateIntent3StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetConsumingDesireState();
        var state2 = GetLoveAndHateState();
        var state3 = GetSwordOfTearsState();
        var state4 = GetRampageState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 2);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (intentNum == 0)
        {
            if (ThreeTurnCooldownHasPassedForMove(stateMachine, WILL_OF_NIHIL) && ((girl1 != null && !girl1.IsDead) || (girl2 != null && !girl2.IsDead)))
            {
                return WILL_OF_NIHIL;
            }
            else
            {
                List<string> possibilities = new List<string>();
                if (!LastMove(stateMachine, CONSUMING_DESIRE)) {
                    possibilities.Add(CONSUMING_DESIRE);
                }
                if (!LastMove(stateMachine, LOVE_AND_HATE)) {
                    possibilities.Add(LOVE_AND_HATE);
                }
                if (!LastMove(stateMachine, SWORD_OF_TEARS)) {
                    possibilities.Add(SWORD_OF_TEARS);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
            }
        } else if (intentNum == 2)
        {
            if (rampageCooldown <= 0 && !(numIntentThatCanRampage == 2 && NextMoves[0].Id == WILL_OF_NIHIL))
            {
                return RAMPAGE;
            }
            else
            {
                List<string> possibilities = new List<string>();
                if (!LastMove(stateMachine, CONSUMING_DESIRE)) {
                    possibilities.Add(CONSUMING_DESIRE);
                }
                if (!LastMove(stateMachine, LOVE_AND_HATE)) {
                    possibilities.Add(LOVE_AND_HATE);
                }
                if (!LastMove(stateMachine, SWORD_OF_TEARS)) {
                    possibilities.Add(SWORD_OF_TEARS);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
            }
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, CONSUMING_DESIRE)) {
                possibilities.Add(CONSUMING_DESIRE);
            }
            if (!LastMove(stateMachine, LOVE_AND_HATE)) {
                possibilities.Add(LOVE_AND_HATE);
            }
            if (!LastMove(stateMachine, SWORD_OF_TEARS)) {
                possibilities.Add(SWORD_OF_TEARS);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine(), GenerateIntent3StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            if (NextMoves[0].Id == WILL_OF_NIHIL)
            {
                return CombatState.PlayerCreatures[0];
            }
            if (numIntentThatCanRampage == 2 && rampageCooldown <= 0)
            {
                if (girl2 != null && girl2.IsAlive)
                {
                    return girl2;
                }
            }
            return CombatState.PlayerCreatures[0];
        }

        if (intentNum == 1)
        {
            if (numIntentThatCanRampage == 0 && rampageCooldown <= 0)
            {
                if (girl2 != null && girl2.IsAlive)
                {
                    return girl2;
                }
            }

            if (girl1 != null && girl1.IsAlive)
            {
                return girl1;
            }
            return CombatState.PlayerCreatures[0];
        }
        if (intentNum == 2)
        {
            if (girl2 != null && girl2.IsAlive)
            {
                return girl2;
            }
            return CombatState.PlayerCreatures[0];
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task WillOfNihil(IReadOnlyList<Creature> targets)
    {
        IsMassAttacking = true;
        var target = targets.FirstOrDefault(LocalContext.IsMe);
        if (target != null)
        {
            SpawnCurseVfx(target);
        }
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy != Creature)
            {
                SpawnCurseVfx(enemy);
            }
        }
        await WaitAnimation(2.0f);
        await DamageCmd.Attack(NihilDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }

    private void SpawnCurseVfx(Creature target)
    {
        Sfx.CollectorCurse.Play();
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (targetNode != null)
        {
            var curse = CollectorCurseEffect.Create(targetNode.VfxSpawnPosition);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(curse);
        }
    }
    
    private async Task ConsumingDesire(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, AtkBlockAmount, ValueProp.Move, null);
        for (int i = 0; i < DesireHits; i++)
        {
            if (i % 2 == 0) {
                await BluntAnimation(targets);
            } else {
                await SlamAnimation(targets);
            }
            await DamageCmd.Attack(DesireDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task LoveAndHate(IReadOnlyList<Creature> targets)
    {
        await HateAnimation(targets);
        await DamageCmd.Attack(HateDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTick<WeakPower>(targets, DebuffAmt);
        await ResetIdle();
    }

    private async Task SwordOfTears(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < TearsHits; i++)
        {
            await PierceAnimation(targets);
            await DamageCmd.Attack(TearsDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task Rampage(IReadOnlyList<Creature> targets)
    {
        await SlamAnimation(targets);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
        await ResetIdle(1.0f);
        numIntentThatCanRampage = (numIntentThatCanRampage + 1) % 3;
        rampageCooldown = RampageCooldown + 1;
    }
    
    public override IReadOnlyList<Creature> AdditionalMassAttackTargets()
    {
        var newList = new List<Creature>();
        foreach (var hittableEnemy in CombatState.HittableEnemies)
        {
            if (hittableEnemy != Creature)
            {
                newList.Add(hittableEnemy);
            }
        }
        return newList;
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            rampageCooldown--;
        }
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature)
        {
            if (girl1 != null && girl1.IsAlive && girl1.Monster is QueenOfLove queen)
            {
                await queen.OnJesterDeath();
            }
            if (girl2 != null && girl2.IsAlive && girl2.Monster is ServantOfCourage servant)
            {
                await servant.OnJesterDeath();
            }
        }
    }

    private async Task HateAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Range", Sfx.MagicGun, targets);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.KnightAttack, targets);
    }
    
    private async Task SlamAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.GreedSlam, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.GreedBlunt, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Slash", "Range"], controller);
    }
}