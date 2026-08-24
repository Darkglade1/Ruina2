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
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act2;

public sealed class AWolf : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 40);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 47, 43);
    
    private int SwipeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int CripplingStrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int VulnAmt => 1;
    private int BlockAmt => 7;

    protected override string VisualsPath => "AWolf/wolf.tscn".MonsterImagePath();

    private const string SWIPE = "SWIPE";
    private const string PACK_TACTICS = "PACK_TACTICS";
    private const string CRIPPLING_STRIKE = "CRIPPLING_STRIKE";

    private MoveState GetSwipeState()
    {
        return new MoveState(SWIPE, Swipe, new SingleAttackIntent(SwipeDamage), new DefendIntent());
    }

    private MoveState GetCripplingStrikeState()
    {
        return new MoveState(CRIPPLING_STRIKE, CripplingStrike, new SingleAttackIntent(CripplingStrikeDamage), new DebuffIntent());
    }

    private MoveState GetPackTacticsState()
    {
        return new MoveState(PACK_TACTICS, PackTactics, new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetSwipeState();
        var state2 = GetCripplingStrikeState();
        var state3 = GetPackTacticsState();

        state1.FollowUpState = state3;
        state2.FollowUpState = state1;
        state3.FollowUpState = state2;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task Swipe(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await DamageCmd.Attack(SwipeDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task CripplingStrike(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(CripplingStrikeDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, VulnAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task PackTactics(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        foreach (var creature in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, StrengthAmount, Creature,  null);
        }
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack", Sfx.WOLF_SLASH, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack", "Block"], controller);
    }
}