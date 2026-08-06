using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act1;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act1.FairyFestival;

public sealed class FairyQueen : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 180);
    public override int MaxInitialHp => MinInitialHp;
    
    private int PredationDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int PredationHits => 2;
    private int SummonStr => 1;
    private int BuffStrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);

    protected override string VisualsPath => "FairyQueen/fairy_queen.tscn".MonsterImagePath();

    private const string QUEENS_DECREE = "QUEENS_DECREE";
    private const string PREDATION = "PREDATION";
    private const string RAVENOUSNESS = "RAVENOUSNESS";

    private bool enraged;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Satiation>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetQueensDecreeState()
    {
        return new MoveState(QUEENS_DECREE, QueensDecree, new SummonIntent(), new BuffIntent());
    }

    private MoveState GetPredationState()
    {
        return new MoveState(PREDATION, Predation, new MultiAttackIntent(PredationDamage, PredationHits));
    }

    private MoveState GetRavenousnessState()
    {
        return new MoveState(RAVENOUSNESS, Ravenousness, new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetQueensDecreeState();
        var state2 = GetPredationState();
        var state3 = GetRavenousnessState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        int numMinions = 0;
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.Monster is FairyMass)
            {
                numMinions++;
            }
        }

        if (enraged)
        {
            return PREDATION;
        } else if (Creature.CurrentHp < Creature.MaxHp * 0.5f)
        {
            return RAVENOUSNESS;
        } else if (numMinions == 0)
        {
            return QUEENS_DECREE;
        }
        else
        {
            return PREDATION;
        }
    }
    
    private async Task QueensDecree(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await CreatureCmd.Add<FairyMass>(CombatState, "minion1");
        await CreatureCmd.Add<FairyMass>(CombatState, "minion2");
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, SummonStr, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Predation(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < PredationHits; i++)
        {
            await SlashAnimation(targets);
            await DamageCmd.Attack(PredationDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task Ravenousness(IReadOnlyList<Creature> targets)
    {
        await EnrageAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, BuffStrAmt, Creature,  null);
        await ResetIdle(1.0f);
        enraged = true;
    }

    public async Task ConsumeMinions(IReadOnlyList<Creature> minions, int weakVulnAmt, int strLossAmt)
    {
        await ConsumeAnimation(minions);
        foreach (var minion in minions)
        {
            await CreatureCmd.Kill(minion);
            await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), Creature, weakVulnAmt, Creature, null);
            await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), Creature, weakVulnAmt, Creature, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, -strLossAmt, Creature, null);
        }
        await ResetIdle();
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.FairyQueenAtk, targets);
    }
    
    private async Task ConsumeAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.FairyQueenEat, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", Sfx.FairySpecial);
    }
    
    private async Task EnrageAnimation()
    {
        await AnimationAction("Special", Sfx.FairyQueenChange);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Special"], controller);
    }
}