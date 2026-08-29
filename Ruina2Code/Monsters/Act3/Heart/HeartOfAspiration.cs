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

namespace Ruina2.Ruina2Code.Monsters.Act3.Heart;

public sealed class HeartOfAspiration : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 48);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 61, 55);
    
    private int BeatsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private int StrAmt => 2;

    protected override string VisualsPath => "Heart/heart.tscn".MonsterImagePath();

    private const string PULSATION = "PULSATION";
    private const string BEATS_OF_ASPIRATION = "BEATS_OF_ASPIRATION";

    private MoveState GetPulsationState()
    {
        return new MoveState(PULSATION, Pulsation, new BuffIntent());
    }

    private MoveState GetBeatsOfAspirationState()
    {
        return new MoveState(BEATS_OF_ASPIRATION, BeatsOfAspiration, new SingleAttackIntent(BeatsDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPulsationState();
        var state2 = GetBeatsOfAspirationState();
        
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
        if (CombatState.HittableEnemies.Count > 1)
        {
            return PULSATION;
        }
        else
        {
            return BEATS_OF_ASPIRATION;
        }
    }
    
    private async Task Pulsation(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, StrAmt, Creature,  null);
        }
        await ResetIdle();
    }
    
    private async Task BeatsOfAspiration(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(BeatsDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.BluntBlow, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Special"], controller);
    }
}