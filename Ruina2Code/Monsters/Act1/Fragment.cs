using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Enlightenment = Ruina2.Ruina2Code.Cards.Enlightenment;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class Fragment : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 46);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 57, 52);

    private int PenetrateDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int EchoesDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int StatusAmt => 2;
    
    protected override string VisualsPath => "Fragment/fragment.tscn".MonsterImagePath();

    private const string PENETRATE = "PENETRATE";
    private const string ECHOES = "ECHOES";

    private MoveState GetPenetrateState()
    {
        return new MoveState(PENETRATE, Penetrate, new SingleAttackIntent(PenetrateDamage), new StatusIntent(StatusAmt));
    }

    private MoveState GetEchoesState()
    {
        return new MoveState(ECHOES, Echoes, new SingleAttackIntent(EchoesDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPenetrateState();
        var state2 = GetEchoesState();
        
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
        if (LastMove(stateMachine, PENETRATE))
        {
            return ECHOES;
        }
        else
        {
            return PENETRATE;
        }
    }
    
    private async Task Penetrate(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(PenetrateDamage)
            .FromMonster(this)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Enlightenment>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task Echoes(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await DamageCmd.Attack(EchoesDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.FragmentStab, targets, 0.7f);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.FragmentSing, targets, 0.5f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Special"], controller);
    }
}