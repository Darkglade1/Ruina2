using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class TeddyBear : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 60, 55);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 64, 59);
    
    private int AffectionDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private int BlockAmt => 11;
    private int DebuffAmt => 1;

    protected override string VisualsPath => "TeddyBear/teddy.tscn".MonsterImagePath();

    private const string TIMID_ENDEARMENT = "TIMID_ENDEARMENT";
    private const string DISPLAY_AFFECTION = "DISPLAY_AFFECTION";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        Sfx.TeddyOn.Play(0, 2.0f);
        await PowerCmd.Apply<Affection>(new ThrowingPlayerChoiceContext(), Creature, DebuffAmt, Creature,  null);
    }

    private MoveState GetTimidEndearmentState()
    {
        return new MoveState(TIMID_ENDEARMENT, TimidEndearment, new DefendIntent());
    }

    private MoveState GetDisplayAffectionState()
    {
        return new MoveState(DISPLAY_AFFECTION, DisplayAffection, new SingleAttackIntent(AffectionDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetTimidEndearmentState();
        var state2 = GetDisplayAffectionState();
        
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
        if (LastMove(stateMachine, TIMID_ENDEARMENT))
        {
            return DISPLAY_AFFECTION;
        }
        return TIMID_ENDEARMENT;
    }
    
    private async Task TimidEndearment(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await ResetIdle(1.0f);
    }
    
    private async Task DisplayAffection(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(AffectionDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.TeddyAtk, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", Sfx.TeddyBlock);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Block"], controller);
    }
}