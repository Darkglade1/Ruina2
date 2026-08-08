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

namespace Ruina2.Ruina2Code.Monsters.Act1.RedShoes;

public sealed class RightShoe : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 29, 26);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 31, 28);
    
    private int DesireDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 3);

    protected override string VisualsPath => "RightShoe/right_shoe.tscn".MonsterImagePath();

    private const string OBSESSION = "OBSESSION";
    private const string BURSTING_DESIRE = "BURSTING_DESIRE";

    private MoveState GetObsessionState()
    {
        return new MoveState(OBSESSION, Obsession, new BuffIntent());
    }

    private MoveState GetBurstingDesireState()
    {
        return new MoveState(BURSTING_DESIRE, BurstingDesire, new SingleAttackIntent(DesireDamage), new HealIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetObsessionState();
        var state2 = GetBurstingDesireState();
        
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
        List<string> possibilities = new List<string>();
        if (!LastMove(stateMachine, OBSESSION)) {
            possibilities.Add(OBSESSION);
        }
        if (!LastTwoMoves(stateMachine, BURSTING_DESIRE)) {
            possibilities.Add(BURSTING_DESIRE);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Obsession(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task BurstingDesire(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        var attackCommand = await DamageCmd.Attack(DesireDamage)
            .FromMonster(this)
            .Execute(null);
        await VampireHeal(attackCommand);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.ShoesAtk, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", Sfx.ShoesOn);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Special"], controller);
    }
}