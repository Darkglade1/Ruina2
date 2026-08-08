using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.Act1.RedShoes;

public sealed class LeftShoe : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 24, 22);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 26, 24);
    
    private int DesireDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int BleedAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

    protected override string VisualsPath => "LeftShoe/left_shoe.tscn".MonsterImagePath();

    private const string SANGUINE_DESIRE = "SANGUINE_DESIRE";

    private MoveState GetSanguineDesireState()
    {
        return new MoveState(SANGUINE_DESIRE, SanguineDesire, new SingleAttackIntent(DesireDamage), new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetSanguineDesireState();

        state1.FollowUpState = state1;

        states.Add(state1);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task SanguineDesire(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(DesireDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Bleed>(new ThrowingPlayerChoiceContext(), targets, BleedAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.ShoesAtk, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash"], controller);
    }
}