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

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class EyeballChick : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 40);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 46);
    
    private int PierceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int StrAmt => 4;

    protected override string VisualsPath => "EyeballChick/eyeball_chick.tscn".MonsterImagePath();

    private const string STARE = "STARE";
    private const string PIERCE = "PIERCE";
    
    private int attackCounter;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        for (int i = 0; i < CombatState.HittableEnemies.Count; i++)
        {
            if (Creature == CombatState.HittableEnemies[i])
            {
                attackCounter = i;
                break;
            }
        }
    }

    private MoveState GetStareState()
    {
        return new MoveState(STARE, Stare, new BuffIntent());
    }

    private MoveState GetPierceState()
    {
        return new MoveState(PIERCE, Pierce, new SingleAttackIntent(PierceDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetStareState();
        var state2 = GetPierceState();
        
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
        if (attackCounter % 2 == 0 && CombatState.RoundNumber == 1)
        {
            return PIERCE;
        }

        if (LastMove(stateMachine, STARE) || LastMoveBefore(stateMachine, STARE))
        {
            return PIERCE;
        }
        return STARE;
    }
    
    private async Task Stare(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Pierce(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(PierceDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.WoodFinish, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce"], controller);
    }
}