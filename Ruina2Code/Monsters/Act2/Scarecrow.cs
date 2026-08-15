using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2;

public sealed class Scarecrow : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 48);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 59, 54);
    
    private int RakeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int HarvestDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);
    private int HarvestHits => 3;
    private int StatusAmt => 2;

    protected override string VisualsPath => "Scarecrow/scarecrow.tscn".MonsterImagePath();

    private const string RAKE = "RAKE";
    private const string HARVEST = "HARVEST";
    private const string STRUGGLE = "STRUGGLE";

    private int struggleCounter = 0;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Search>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        for (int i = 0; i < CombatState.HittableEnemies.Count; i++)
        {
            if (Creature == CombatState.HittableEnemies[i])
            {
                struggleCounter = i;
                break;
            }
        }
    }

    private MoveState GetRakeState()
    {
        return new MoveState(RAKE, Rake, new SingleAttackIntent(RakeDamage));
    }

    private MoveState GetHarvestState()
    {
        return new MoveState(HARVEST, Harvest, new MultiAttackIntent(HarvestDamage, HarvestHits));
    }

    private MoveState GetStruggleState()
    {
        return new MoveState(STRUGGLE, Struggle, new StatusIntent(StatusAmt));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetRakeState();
        var state2 = GetHarvestState();
        var state3 = GetStruggleState();
        
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
        if (stateMachine.StateLog.Count >= struggleCounter && !LastMove(stateMachine, STRUGGLE) && !LastMoveBefore(stateMachine, STRUGGLE))
        {
            return STRUGGLE;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, RAKE)) {
                possibilities.Add(RAKE);
            }
            if (!LastMove(stateMachine, HARVEST)) {
                possibilities.Add(HARVEST);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task Rake(IReadOnlyList<Creature> targets)
    {
        await RakeAnimation(targets);
        await DamageCmd.Attack(RakeDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Harvest(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < HarvestHits; i++)
        {
            await HarvestAnimation(targets);
            await DamageCmd.Attack(HarvestDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task Struggle(IReadOnlyList<Creature> targets)
    {
        await CardPileCmd.AddToCombatAndPreview<Wisdom>(targets, PileType.Discard, StatusAmt, null);
    }
    
    public override async Task BeforeDeath(Creature creature)
    {
        await base.BeforeDeath(creature);
        if (creature != Creature)
            return;
        Sfx.ScarecrowDeath.Play(0, 0.4f);
    }
    
    private async Task RakeAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack1", Sfx.Rake, targets);
    }
    
    private async Task HarvestAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack2", Sfx.Harvest, targets, 0.3f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack1", "Attack2"], controller);
    }
}