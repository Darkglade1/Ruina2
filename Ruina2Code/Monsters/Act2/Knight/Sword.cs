using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.Knight;

public sealed class Sword : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 40);
    public override int MaxInitialHp => MinInitialHp;
    
    private int TearDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int BlockAmt => 8;

    protected override string VisualsPath => "Sword/sword.tscn".MonsterImagePath();

    private const string TEAR_HEART = "TEAR_HEART";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        if (!CombatManager.Instance.IsEnemyTurnStarted)
        {
            await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);   
        }
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, BlockAmt, Creature,  null);
        await PowerCmd.Apply<Worthless>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
    }

    private MoveState GetTearHeartState()
    {
        return new MoveState(TEAR_HEART, TearHeart, new SingleAttackIntent(TearDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetTearHeartState();

        state1.FollowUpState = state1;

        states.Add(state1);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task TearHeart(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(TearDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack", Sfx.KnightVertGaho, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack"], controller);
    }
}