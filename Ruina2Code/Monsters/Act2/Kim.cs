using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Kim;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Patches;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2;

public sealed class Kim : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 220, 200);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    private int YieldDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int ClaimDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);
    private int AcupunctureDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int AcupunctureHits => 2;
    private int TakeOnesLifeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 30, 27);
    private int StrAmt => 2;
    private int ParalysisAmt => 2;

    private bool CounterSufficientlyTriggered => CountersThisTurn >= CombatState.PlayerCreatures.Count;
    private int CountersThisTurn = 0;

    protected override string VisualsPath => "Kim/kim.tscn".MonsterImagePath();

    private const string YIELD = "YIELD";
    private const string CLAIM = "CLAIM";
    private const string ACUPUNCTURE = "ACUPUNCTURE";
    public static string TAKE_ONES_LIFE = "TAKE_ONES_LIFE";
    
    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        // Timing jank
        if (side == CombatSide.Player && CombatState.RoundNumber == 1)
        {
            MusicPatches.RuinaActMusicPatches.KimFightStart();
        }
    }

    private MoveState GetYieldState()
    {
        return new MoveState(YIELD, Yield, new RuinaSingleAttackIntent(YieldDamage), new BuffIntent());
    }

    private MoveState GetClaimState()
    {
        return new MoveState(CLAIM, _ => Task.CompletedTask, new RuinaSingleCounterAttackIntent(ClaimDamage));
    }
    
    private MoveState GetAcupunctureState()
    {
        return new MoveState(ACUPUNCTURE, Acupuncture, new RuinaMultiAttackIntent(AcupunctureDamage, AcupunctureHits), new RuinaDebuffIntent());
    }
    
    private MoveState GetTakeOnesLifeState()
    {
        return new MoveState(TAKE_ONES_LIFE, TakeOnesLife, new RuinaSingleAttackIntent(TakeOnesLifeDamage), new RuinaDebuffIntent());
    }
    
    private MonsterMoveStateMachine GenerateIntentStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetYieldState();
        var state2 = GetClaimState();
        var state3 = GetAcupunctureState();
        var state4 = GetTakeOnesLifeState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (LastMove(stateMachine, YIELD))
        {
            return CLAIM;
        } else if (LastMove(stateMachine, CLAIM))
        {
            if (CounterSufficientlyTriggered)
            {
                CountersThisTurn = 0;
                return ACUPUNCTURE;
            }
            else
            {
                CountersThisTurn = 0;
                return TAKE_ONES_LIFE;
            }
        }
        else
        {
            return YIELD;
        }
    }
    
    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntentStateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        return GetPlayerTarget();
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Yield>();
        card1.SetDamage(YieldDamage);
        card1.SetStrength(StrAmt);
        var card2 = CreateCardForIntent<Claim>();
        card2.SetDamage(ClaimDamage);
        var card3 = CreateCardForIntent<Acupuncture>();
        card3.SetDamage(AcupunctureDamage);
        card3.SetRepeat(AcupunctureHits);
        card3.DynamicVars["Paralysis"].BaseValue = ParalysisAmt;
        var card4 = CreateCardForIntent<TakeOnesLife>();
        card4.SetDamage(TakeOnesLifeDamage);
        return new Dictionary<string, CardModel>()
        {
            {YIELD, card1},
            {CLAIM, card2},
            {ACUPUNCTURE, card3},
            {TAKE_ONES_LIFE, card4}
        };
    }
    
   private async Task Yield(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(YieldDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature, null);
        await ResetIdle();
        await ApplyPowerAndSkipNextDurationTick<CounterAttack>([Creature], CombatState.PlayerCreatures.Count);
    }
    
    public async Task Claim(Creature target)
    {
        await SpecialAnimation();
        await WaitAnimation(0.75f);
        await BluntAnimation([target]);
        await DamageCmd.Attack(ClaimDamage)
            .FromMonsterCreature(this)
            .Targeting(target)
            .Execute(null);
        await ResetIdle();
        CountersThisTurn++;
    }
    
    private async Task Acupuncture(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < AcupunctureHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            } 
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(AcupunctureDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, ParalysisAmt, Creature, null);
    }
    
    private async Task TakeOnesLife(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await WaitAnimation();
        await TakeOnesLifeFullScreenAnimation();
        await SpecialFinAnimation(targets);
        var attackCommand = await DamageCmd.Attack(TakeOnesLifeDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        foreach (var result in attackCommand.Results.SelectMany(r => r))
        {
            if (result.UnblockedDamage > 0 && result.Receiver.IsPlayer)
            {
                await PowerCmd.Apply<Rupture>(new ThrowingPlayerChoiceContext(), result.Receiver, result.UnblockedDamage, Creature, null);
            }
        }
        await ResetIdle(1.0f);
    }
    
    private async Task TakeOnesLifeFullScreenAnimation()
    {
        Sfx.RedMistHoriEye.Play();
        var fullScreenEffect = FullScreenAnimationEffect.Create("Kim/frame".VfxImagePath(), 2.4f, 24);
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(fullScreenEffect);
        await WaitAnimation(2.4f);
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SwordVert, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.SwordHori, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", Sfx.Parry, null);
    }
    
    private async Task SpecialFinAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.RedMistVertFin, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Slash", "Special"], controller);
    }
}