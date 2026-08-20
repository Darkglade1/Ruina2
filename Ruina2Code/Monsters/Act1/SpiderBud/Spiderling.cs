using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act1.SpiderBud;

public sealed class Spiderling : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 11, 10);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 13, 12);
    
    private int WebbingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int FangsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int StatusAmt => 1;
    
    protected override string VisualsPath => "Spiderling/spiderling.tscn".MonsterImagePath();

    private const string THIN_WEBBING = "THIN_WEBBING";
    private const string STINGY_FANGS = "STINGY_FANGS";

    private MoveState GetThinWebbingState()
    {
        return new MoveState(THIN_WEBBING, ThinWebbing, new SingleAttackIntent(WebbingDamage), new StatusIntent(StatusAmt));
    }

    private MoveState GetStingyFangsState()
    {
        return new MoveState(STINGY_FANGS, StingyFangs, new SingleAttackIntent(FangsDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetThinWebbingState();
        var state2 = GetStingyFangsState();
        
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
        if (!LastTwoMoves(stateMachine, THIN_WEBBING)) {
            possibilities.Add(THIN_WEBBING);
        }
        if (!LastTwoMoves(stateMachine, STINGY_FANGS)) {
            possibilities.Add(STINGY_FANGS);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task ThinWebbing(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(WebbingDamage)
            .FromMonster(this)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task StingyFangs(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(FangsDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SpiderBabyAtk, targets, 0.4f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash"], controller);
    }
}