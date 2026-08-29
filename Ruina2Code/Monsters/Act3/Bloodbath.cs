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
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class Bloodbath : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 97, 88);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 103, 94);
    
    private int PaleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int DebuffAmt => 1;
    private int BleedAmt => 3;
    private int BlockAmt => 7;

    protected override string VisualsPath => "Bloodbath/bloodbath.tscn".MonsterImagePath();

    private const string PALE_HANDS = "PALE_HANDS";
    private const string DEPRESSION = "DEPRESSION";

    private MoveState GetPaleHandsState()
    {
        return new MoveState(PALE_HANDS, PaleHands, new SingleAttackIntent(PaleDamage), new DebuffIntent());
    }

    private MoveState GetDepressionState()
    {
        return new MoveState(DEPRESSION, Depression, new DefendIntent(), new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPaleHandsState();
        var state2 = GetDepressionState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task PaleHands(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(PaleDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Bleed>(new ThrowingPlayerChoiceContext(), targets, BleedAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Depression(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), targets, -DebuffAmt, Creature,  null);
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -DebuffAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BloodAttack, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.BloodSpecial, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Special"], controller);
    }
}