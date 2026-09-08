using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class Alriune : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 90, 82);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 95, 86);
    
    private int BloomDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int EndDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int StatusAmt => 1;
    private int DexDown => 1;
    private int StrengthAmount => 2;
    private int DAMAGE_REDUCTION => 50;

    protected override string VisualsPath => "Alriune/alriune.tscn".MonsterImagePath();

    private const string SPRINGS_GENESIS = "SPRINGS_GENESIS";
    private const string FULL_BLOOM = "FULL_BLOOM";
    private const string MAGNIFICENT_END = "MAGNIFICENT_END";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        foreach (Creature target in CombatState.PlayerCreatures)
        {
            WintersInception mutable = (WintersInception) ModelDb.Power<WintersInception>().ToMutable();
            mutable.Target = target;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), mutable, Creature, DAMAGE_REDUCTION, Creature, null);
        }
    }

    private MoveState GetSpringsGenesisState()
    {
        return new MoveState(SPRINGS_GENESIS, SpringsGenesis, new DebuffIntent(), new BuffIntent());
    }

    private MoveState GetFullBloomState()
    {
        return new MoveState(FULL_BLOOM, FullBloom, new SingleAttackIntent(BloomDamage), new StatusIntent(StatusAmt));
    }
    
    private MoveState GetMagnificentEndState()
    {
        return new MoveState(MAGNIFICENT_END, MagnificentEnd, new SingleAttackIntent(EndDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetSpringsGenesisState();
        var state2 = GetFullBloomState();
        var state3 = GetMagnificentEndState();
        
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
        if (stateMachine.StateLog.Count >= 2 && !LastMove(stateMachine, SPRINGS_GENESIS) && !LastMoveBefore(stateMachine, SPRINGS_GENESIS))
        {
            return SPRINGS_GENESIS;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, FULL_BLOOM)) {
                possibilities.Add(FULL_BLOOM);
            }
            if (!LastMove(stateMachine, MAGNIFICENT_END)) {
                possibilities.Add(MAGNIFICENT_END);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task SpringsGenesis(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -DexDown, Creature,  null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task FullBloom(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(BloomDamage)
            .FromMonster(this)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task MagnificentEnd(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(EndDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.AlriuneHori, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.AlriuneGuard, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Special"], controller);
    }
}