using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act1.QueenBee;

public sealed class WorkerBee : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 46, 42);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 46);
    
    private int CarryDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int GarduDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int GarduHits => 2;
    private int BlockAmt => 5;

    protected override string VisualsPath => "WorkerBee/worker_bee.tscn".MonsterImagePath();

    private const string CARRY_LARVAE = "CARRY_LARVAE";
    private const string GARDU_DU_CORPS = "GARDU_DU_CORPS";

    private MoveState GetCarryLarvaeState()
    {
        return new MoveState(CARRY_LARVAE, CarryLarvae, new SingleAttackIntent(CarryDamage), new DefendIntent());
    }

    private MoveState GetGarduState()
    {
        return new MoveState(GARDU_DU_CORPS, Gardu, new MultiAttackIntent(GarduDamage, GarduHits));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetCarryLarvaeState();
        var state2 = GetGarduState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task CarryLarvae(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await DamageCmd.Attack(CarryDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Gardu(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < GarduHits; i++)
        {
            await AttackAnimation(targets);
            await DamageCmd.Attack(GarduDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Bite", Sfx.QueenBeeLegAtk, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Bite"], controller);
    }
}