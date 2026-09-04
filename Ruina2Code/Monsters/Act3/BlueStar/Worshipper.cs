using Godot;
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
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.BlueStar;

public sealed class Worshipper : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 42, 38);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 48, 44);
    
    private int ForDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int FaithDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int HearDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int MartyrDamage => 15;
    private int DebuffAmt => 1;
    private const float STRONG_ATTACK_HP_THRESHOLD = 0.65f;
    private const float MEET_AGAIN_HP_THRESHOLD = 0.35f;

    protected override string VisualsPath => "Worshipper/worshipper.tscn".MonsterImagePath();

    private const string FOR_THE_STAR = "FOR_THE_STAR";
    private const string EVERLASTING_FAITH = "EVERLASTING_FAITH";
    private const string HEAR_STAR = "HEAR_STAR";
    private const string MEET_AGAIN = "MEET_AGAIN";
    
    public MoveState? _meetState;
    public MoveState? MeetState
    {
        get => _meetState;
        set
        {
            AssertMutable();
            _meetState = value;
        }
    }

    private int meetAgainThreshold;
    public bool TriggerMartyr = true;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        meetAgainThreshold = (int)Math.Round(Creature.MaxHp * MEET_AGAIN_HP_THRESHOLD);
        await PowerCmd.Apply<Martyr>(new ThrowingPlayerChoiceContext(), Creature, MartyrDamage, Creature, null);
        await PowerCmd.Apply<MeetAgain>(new ThrowingPlayerChoiceContext(), Creature, meetAgainThreshold, Creature, null);
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetForTheStarState()
    {
        return new MoveState(FOR_THE_STAR, ForTheStar, new SingleAttackIntent(ForDamage));
    }

    private MoveState GetEverlastingFaithState()
    {
        return new MoveState(EVERLASTING_FAITH, EverlastingFaith, new SingleAttackIntent(FaithDamage), new DebuffIntent());
    }

    private MoveState GetHearStarState()
    {
        return new MoveState(HEAR_STAR, HearStar, new SingleAttackIntent(HearDamage));
    }
    
    private MoveState GetMeetAgainState()
    {
        return new MoveState(MEET_AGAIN, MeetAgain, new UnknownIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetForTheStarState();
        var state2 = GetEverlastingFaithState();
        var state3 = GetHearStarState();
        MeetState = GetMeetAgainState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        MeetState.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(MeetState);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (Creature.CurrentHp < meetAgainThreshold)
        {
            return MEET_AGAIN;
        } else if (Creature.CurrentHp < Creature.MaxHp * STRONG_ATTACK_HP_THRESHOLD)
        {
            return HEAR_STAR;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastTwoMoves(stateMachine, FOR_THE_STAR)) {
                possibilities.Add(FOR_THE_STAR);
            }
            if (!LastTwoMoves(stateMachine, EVERLASTING_FAITH)) {
                possibilities.Add(EVERLASTING_FAITH);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task ForTheStar(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(ForDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task EverlastingFaith(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(FaithDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task HearStar(IReadOnlyList<Creature> targets)
    {
        await BigAttackAnimation(targets);
        await DamageCmd.Attack(HearDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task MeetAgain(IReadOnlyList<Creature> targets)
    {
        TriggerMartyr = false;
        await SuicideAnimation();
        await CreatureCmd.Kill(Creature);
    }
    
    private async Task SuicideAnimation()
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (node != null)
        {
            var suicideEffect = WorshipperSuicideEffect.Create(node.VfxSpawnPosition);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(suicideEffect);
            node.Visuals.Visible = false;
            await WaitAnimation(2.0f);
        }
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.WorshipperAttack, targets);
    }
    
    private async Task BigAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.WorshipperAttack, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Suicide"], controller);
    }
}