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
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class Porccubus : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 48, 45);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 48);
    
    private int PleasureDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int BristleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int ThornsAmt => 1;
    private int PowerDmg => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    protected override string VisualsPath => "Porccubus/porccubus.tscn".MonsterImagePath();

    private const string UNBEARABLE_PLEASURE = "UNBEARABLE_PLEASURE";
    private const string BRISTLE = "BRISTLE";

    private MoveState GetUnbearablePleasureState()
    {
        return new MoveState(UNBEARABLE_PLEASURE, UnbearablePleasure, new SingleAttackIntent(PleasureDamage), new DebuffIntent());
    }

    private MoveState GetBristleState()
    {
        return new MoveState(BRISTLE, Bristle, new SingleAttackIntent(BristleDamage), new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetUnbearablePleasureState();
        var state2 = GetBristleState();
        
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
        if (LastMove(stateMachine, UNBEARABLE_PLEASURE))
        {
            return BRISTLE;
        }
        return UNBEARABLE_PLEASURE;
    }
    
    private async Task UnbearablePleasure(IReadOnlyList<Creature> targets)
    {
        await PierceAnimation(targets);
        await DamageCmd.Attack(PleasureDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Pleasure>(new ThrowingPlayerChoiceContext(), targets, PowerDmg, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Bristle(IReadOnlyList<Creature> targets)
    {
        await BluntAnimation(targets);
        await DamageCmd.Attack(BristleDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<ThornsPower>(new ThrowingPlayerChoiceContext(), Creature, ThornsAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.PorccuStrongStab2, targets);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.PorccuPenetrate, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce"], controller);
    }
}