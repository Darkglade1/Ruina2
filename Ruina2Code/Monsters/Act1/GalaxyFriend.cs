using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class GalaxyFriend : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 31, 28);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 34, 31);
    
    private int StarDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int GlimmerDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int BlockAmt => 7;
    private int RegenAmt => 2;
    private int StatusAmt => 1;

    protected override string VisualsPath => "GalaxyFriend/galaxy_friend.tscn".MonsterImagePath();

    private const string WAITING = "WAITING";
    private const string STAR_SHOWER = "STAR_SHOWER";
    private const string GLIMMER = "GLIMMER";
    private const string REVIVE = "REVIVE";
    
    public MoveState? _reviveState;
    public MoveState? ReviveState
    {
        get => _reviveState;
        set
        {
            AssertMutable();
            _reviveState = value;
        }
    }

    public override bool ShouldDisappearFromDoom
    {
        get
        {
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy.Monster is GalaxyFriend && enemy.IsAlive)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<DontLeave>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        await PowerCmd.Apply<MonsterRegen>(new ThrowingPlayerChoiceContext(), Creature, Creature.ScaleHpForMultiplayer(RegenAmt, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex), Creature, null);
    }

    private MoveState GetWaitingState()
    {
        return new MoveState(WAITING, Waiting, new DefendIntent());
    }

    private MoveState GetStarShowerState()
    {
        return new MoveState(STAR_SHOWER, StarShower, new SingleAttackIntent(StarDamage));
    }
    
    private MoveState GetGlimmerState()
    {
        return new MoveState(GLIMMER, Glimmer, new SingleAttackIntent(GlimmerDamage), new StatusIntent(StatusAmt));
    }
    
    private MoveState GetReviveState()
    {
        return new MoveState(REVIVE, Revive, new HealIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetWaitingState();
        var state2 = GetStarShowerState();
        var state3 = GetGlimmerState();
        ReviveState = GetReviveState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        ReviveState.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(ReviveState);
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
            if (!LastMove(stateMachine, WAITING)) {
                possibilities.Add(WAITING);
            }
            if (!LastMove(stateMachine, STAR_SHOWER)) {
                possibilities.Add(STAR_SHOWER);
            }
            if (!LastMove(stateMachine, GLIMMER)) {
                possibilities.Add(GLIMMER);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task Waiting(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await ResetIdle(1.0f);
    }
    
    private async Task StarShower(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(StarDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Glimmer(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(GlimmerDamage)
            .FromMonster(this)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task Revive(IReadOnlyList<Creature> targets)
    {
        int healAmount = 1;
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.IsAlive && enemy.Monster is GalaxyFriend)
            {
                healAmount = enemy.CurrentHp;
            }
        }
        await CreatureCmd.Heal(Creature, healAmount);
        Creature.GetPower<DontLeave>()?.DoRevive();
    }
    
    public async Task TriggerDeadState()
    {
        if (ReviveState != null)
        {
            SetMoveImmediate(ReviveState);
        }
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.GalaxyAtk, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", Sfx.GalaxyDef);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Block"], controller);
    }
}