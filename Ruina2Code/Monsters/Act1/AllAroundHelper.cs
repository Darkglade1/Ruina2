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

public sealed class AllAroundHelper : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 55, 50);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 59, 54);
    
    private int CleanDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int CleanHits => 2;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    private int DamageThreshold => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

    protected override string VisualsPath => "Helper/helper.tscn".MonsterImagePath();

    private const string CHARGE = "CHARGE";
    private const string CLEAN = "CLEAN";

    private int attackCounter;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Pattern>(new ThrowingPlayerChoiceContext(), Creature, DamageThreshold * CombatState.Players.Count, Creature,  null);
        for (int i = 0; i < CombatState.HittableEnemies.Count; i++)
        {
            if (Creature == CombatState.HittableEnemies[i])
            {
                attackCounter = i;
                if (attackCounter == 0)
                {
                    Sfx.HelperOn.Play(0, 5.0f);
                }
                break;
            }
        }
    }

    private MoveState GetCleanState()
    {
        return new MoveState(CLEAN, Clean, new MultiAttackIntent(CleanDamage, CleanHits));
    }

    private MoveState GetChargeState()
    {
        return new MoveState(CHARGE, Charge, new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetCleanState();
        var state2 = GetChargeState();
        
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
            return CLEAN;
        }

        if (LastMove(stateMachine, CHARGE))
        {
            return CLEAN;
        }
        return CHARGE;
    }
    
    private async Task Charge(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        await ApplyPowerAndSkipNextDurationTick<HelperTempStr>([Creature], StrengthAmount - 1);
        await ResetIdle();
    }
    
    private async Task Clean(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < CleanHits; i++)
        {
            await AttackAnimation(targets);
            await DamageCmd.Attack(CleanDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SwordVert, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", Sfx.HelperCharge);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Special"], controller);
    }
}