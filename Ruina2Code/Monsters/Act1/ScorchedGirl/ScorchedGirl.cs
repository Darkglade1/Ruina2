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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act1.ScorchedGirl;

public sealed class ScorchedGirl : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 39, 35);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 43, 39);
    
    private int ExtinguishDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 22);
    private int StatusAmt => 1;

    protected override string VisualsPath => "ScorchedGirl/scorched_girl.tscn".MonsterImagePath();

    private const string EMBER = "EMBER";
    private const string EXTINGUISH = "EXTINGUISH";

    private MoveState GetEmberState()
    {
        return new MoveState(EMBER, Ember, new StatusIntent(StatusAmt));
    }

    private MoveState GetExtinguishState()
    {
        return new MoveState(EXTINGUISH, Extinguish, new DeathBlowIntent(() => ExtinguishDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetEmberState();
        var state2 = GetExtinguishState();
        
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
        if (!LastTwoMoves(stateMachine, EMBER))
        {
            return EMBER;
        }
        return EXTINGUISH;
    }
    
    private async Task Ember(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
        await ResetIdle();
    }
    
    private async Task Extinguish(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        NCombatRoom? instance = NCombatRoom.Instance;
        if (instance != null)
        {
            instance.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(Creature)); 
        }
        await Cmd.CustomScaledWait(0.2f, 0.3f);
        await DamageCmd.Attack(ExtinguishDamage)
            .FromMonster(this)
            .Execute(null);
        await CreatureCmd.Kill(Creature);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.MatchExplode, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Move", Sfx.MatchSizzle, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Move", "Special"], controller);
    }
}