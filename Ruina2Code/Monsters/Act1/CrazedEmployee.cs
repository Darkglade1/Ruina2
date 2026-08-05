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

public sealed class CrazedEmployee : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 26, 24);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 30, 27);
    
    private int ShakingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int debuffAmt => 1;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    protected override string VisualsPath => "CrazedEmployee/employee.tscn".MonsterImagePath();

    private const string TREMBLING_MOTION = "TREMBLING_MOTION";
    private const string SHAKING_BLOW = "SHAKING_BLOW";

    private int debuffNum;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Song>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        for (int i = 0; i < CombatState.HittableEnemies.Count; i++)
        {
            if (Creature == CombatState.HittableEnemies[i])
            {
                debuffNum = i;
                break;
            }
        }
    }

    private MoveState GetTremblingMotionState()
    {
        return new MoveState(TREMBLING_MOTION, TremblingMotion, new DebuffIntent());
    }

    private MoveState GetShakingBlowState()
    {
        return new MoveState(SHAKING_BLOW, ShakingBlow, new SingleAttackIntent(ShakingDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetTremblingMotionState();
        var state2 = GetShakingBlowState();
        
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
        if (CombatState.RoundNumber == 1 && debuffNum == 0)
        {
            return TREMBLING_MOTION;
        } else if (CombatState.RoundNumber == 2 && debuffNum == 1)
        {
            return TREMBLING_MOTION;
        }
        else
        {
            return SHAKING_BLOW;
        }
    }
    
    private async Task TremblingMotion(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation(targets);
        if (debuffNum == 0)
        {
            await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, debuffAmt, Creature,  null);
        }
        else
        {
            await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, debuffAmt, Creature,  null);
        }
        await ResetIdle();
    }
    
    private async Task ShakingBlow(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(ShakingDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BluntVert, targets);
    }
    
    private async Task BlockAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Block", null, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Block"], controller);
    }
}