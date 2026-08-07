using Godot;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.RoadHome;

public sealed class ScaredyCat : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 110, 100);
    public override int MaxInitialHp => MinInitialHp;
    
    private int RawrDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int RawrHits => 2;
    private int GrowlDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int StatusAmt => 1;
    private bool coward = false;

    protected override string VisualsPath => "ScaredyCat/cat.tscn".MonsterImagePath();

    private const string RAWR = "RAWR";
    private const string GROWL = "GROWL";
    private const string FLEE = "FLEE";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Courage>(new ThrowingPlayerChoiceContext(), Creature, CombatState.Players.Count, Creature,  null);
    }

    private MoveState GetRawrState()
    {
        return new MoveState(RAWR, Rawr, new MultiAttackIntent(RawrDamage, RawrHits));
    }

    private MoveState GetGrowlState()
    {
        return new MoveState(GROWL, Growl, new SingleAttackIntent(GrowlDamage), new StatusIntent(StatusAmt));
    }

    private MoveState GetFleeState()
    {
        var state = new MoveState(FLEE, Flee, new EscapeIntent());
        state.FollowUpState = state;
        return state;
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetRawrState();
        var state2 = GetGrowlState();
        var state3 = GetFleeState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (coward)
        {
            return FLEE;
        }
        else
        {
            if (LastMove(stateMachine, RAWR))
            {
                return GROWL;
            }
            else
            {
                return RAWR;
            }
        }
    }
    
    private async Task Rawr(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < RawrHits; i++)
        {
            if (i % 2 == 0)
            {
                await Attack1Animation(targets);
            }
            else
            {
                await Attack2Animation(targets);
            }
            await DamageCmd.Attack(RawrDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Growl(IReadOnlyList<Creature> targets)
    {
        await PoisonAnimation(targets);
        await DamageCmd.Attack(GrowlDamage)
            .FromMonster(this)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Wound>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task Flee(IReadOnlyList<Creature> targets)
    {
        NCombatRoom.Instance?.GetCreatureNode(Creature)?.ToggleIsInteractable(false);
        var catRunAwayEffect = CatRunAway.Create(Creature);
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(catRunAwayEffect);
        await WaitAnimation(catRunAwayEffect.EndDuration);
        await CreatureCmd.Escape(Creature);
    }

    public async Task OnRoadDeath()
    {
        coward = true;
        await ScaredAnimation();
        Creature.MaxHp = 1;
        Creature.CurrentHp = 1;
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, -9999, Creature, null);
        await PowerCmd.Remove<Courage>(Creature);
        SetMoveImmediate(GetFleeState());
    }
    
    private async Task Attack1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack1", Sfx.WOLF_SLASH, targets);
    }
    
    private async Task Attack2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack2", Sfx.WoodStrike, targets);
    }
    
    private async Task PoisonAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Poison", Sfx.LionPoison, targets);
    }
    
    private async Task ScaredAnimation()
    {
        await AnimationAction("Scared", Sfx.LionChange);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack1", "Attack2", "Poison", "Scared"], controller);
    }
}