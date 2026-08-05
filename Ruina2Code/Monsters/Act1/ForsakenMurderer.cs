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
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class ForsakenMurderer : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 47, 43);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 52, 47);
    
    private int ChainedDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int RingingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int RingingHits => 2;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

    protected override string VisualsPath => "ForsakenMurderer/murderer.tscn".MonsterImagePath();

    private const string CHAINED_WRATH = "CHAINED_WRATH";
    private const string METALLIC_RINGING = "METALLIC_RINGING";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Fear>(new ThrowingPlayerChoiceContext(), Creature, CombatState.Players.Count, Creature,  null);
    }

    private MoveState GetChainedWrathState()
    {
        return new MoveState(CHAINED_WRATH, ChainedWrath, new SingleAttackIntent(ChainedDamage), new BuffIntent());
    }

    private MoveState GetMetallicRingingState()
    {
        return new MoveState(METALLIC_RINGING, MetallicRinging, new MultiAttackIntent(RingingDamage, RingingHits));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetChainedWrathState();
        var state2 = GetMetallicRingingState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (CombatState.RoundNumber == 1)
        {
            return METALLIC_RINGING;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, CHAINED_WRATH)) {
                possibilities.Add(CHAINED_WRATH);
            }
            if (!LastTwoMoves(stateMachine, METALLIC_RINGING)) {
                possibilities.Add(METALLIC_RINGING);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task ChainedWrath(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(ChainedDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle();
    }
    
    private async Task MetallicRinging(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < RingingHits; i++)
        {
            await AttackAnimation(targets);
            await DamageCmd.Attack(RingingDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BluntHori, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt"], controller);
    }
}