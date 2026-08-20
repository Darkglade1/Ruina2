using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act3.SilentGirl;

public sealed class Nail : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 165, 150);
    public override int MaxInitialHp => MinInitialHp;
    
    private int DiggingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);
    private int DebuffAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StatusAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    protected override string VisualsPath => "Nail/nail.tscn".MonsterImagePath();

    private const string DIGGING_NAIL = "DIGGING_NAIL";
    private const string CRACKED_HEART = "CRACKED_HEART";

    private MoveState GetDiggingNailState()
    {
        return new MoveState(DIGGING_NAIL, DiggingNail, new SingleAttackIntent(DiggingDamage), new DebuffIntent());
    }

    private MoveState GetCrackedHeartState()
    {
        return new MoveState(CRACKED_HEART, CrackedHeart, new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetDiggingNailState();
        var state2 = GetCrackedHeartState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task DiggingNail(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(DiggingDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Powers.Act3.Nail>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task CrackedHeart(IReadOnlyList<Creature> targets)
    {
        await CardPileCmd.AddToCombatAndPreview<Wound>(targets, PileType.Discard, StatusAmt, null);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.SilentNail, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Dead"], controller);
    }
}