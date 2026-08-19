using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
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
using Ruina2.Ruina2Code.Cards.Gifts;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Nodes;

namespace Ruina2.Ruina2Code.Monsters.Act2.Oz;

public sealed class Oz : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 310, 280);
    public override int MaxInitialHp => MinInitialHp;
    
    private int WelcomeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);
    private int BehaveDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int BehaveHits => 2;
    private int NoisyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private int AwayDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 28, 25);
    
    private int StrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int DebuffAmt => 2;

    protected override string VisualsPath => "Oz/oz.tscn".MonsterImagePath();

    private const string WELCOME = "WELCOME";
    private const string BEHAVE = "BEHAVE";
    private const string NOISY = "NOISY";
    private const string AWAY = "AWAY";
    private const string GIFT = "GIFT";
    
    public static readonly IReadOnlyList<IReadOnlyList<IChoosable>> GiftSets =
    [
        [
            ModelDb.Card<FalseGift1>(),
            ModelDb.Card<RefuseGift>()
        ],
        [
            ModelDb.Card<FalseGift2>(),
            ModelDb.Card<RefuseGift>()
        ],
        [
            ModelDb.Card<FalseGift3>(),
            ModelDb.Card<RefuseGift>()
        ]
    ];

    public int GiftCounter;

    private MoveState GetWelcomeState()
    {
        return new MoveState(WELCOME, Welcome, new SingleAttackIntent(WelcomeDamage), new SummonIntent());
    }

    private MoveState GetBehaveState()
    {
        return new MoveState(BEHAVE, Behave, new MultiAttackIntent(BehaveDamage, BehaveHits));
    }

    private MoveState GetNoisyState()
    {
        return new MoveState(NOISY, Noisy, new SingleAttackIntent(NoisyDamage), new DebuffIntent());
    }
    
    private MoveState GetAwayState()
    {
        return new MoveState(AWAY, Away, new SingleAttackIntent(AwayDamage), new BuffIntent());
    }
    
    private MoveState GetGiftState()
    {
        return new MoveState(GIFT, Gift, new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetWelcomeState();
        var state2 = GetBehaveState();
        var state3 = GetNoisyState();
        var state4 = GetAwayState();
        var state5 = GetGiftState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;
        state5.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(state5);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (CombatState.RoundNumber == 1)
        {
            return WELCOME;
        }

        if (LastMove(stateMachine, WELCOME) || LastMove(stateMachine, AWAY))
        {
            return GIFT;
        }

        if (ThreeTurnCooldownHasPassedForMove(stateMachine, AWAY) && stateMachine.StateLog.Count >= 4)
        {
            return AWAY;
        }

        List<string> possibilities = new List<string>();
        if (!LastMove(stateMachine, BEHAVE)) {
            possibilities.Add(BEHAVE);
        }
        if (!LastMove(stateMachine, NOISY)) {
            possibilities.Add(NOISY);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Welcome(IReadOnlyList<Creature> targets)
    {
        await SpecialStartAnimation(targets);
        await CrystalFallAnimation(targets);
        await DamageCmd.Attack(WelcomeDamage)
            .FromMonster(this)
            .Execute(null);
        await CrystalShardsAnimation(targets);
        var creature1 = await CreatureCmd.Add<ScowlingFace>(CombatState, "slot1");
        if (creature1.Monster is ScowlingFace scowlingFace)
        {
            scowlingFace.MinionNum = 1;
        }
        var creature2 = await CreatureCmd.Add<ScowlingFace>(CombatState, "slot2");
        if (creature2.Monster is ScowlingFace scowlingFace2)
        {
            scowlingFace2.MinionNum = 2;
        }
        await ResetIdle();
    }
    
    private async Task Behave(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < BehaveHits; i++)
        {
            await AttackAnimation2(targets);
            await DamageCmd.Attack(BehaveDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task Noisy(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(NoisyDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Away(IReadOnlyList<Creature> targets)
    {
        await SpecialStartAnimation(targets);
        await CrystalFallAnimation(targets);
        await DamageCmd.Attack(AwayDamage)
            .FromMonster(this)
            .Execute(null);
        await CrystalShardsAnimation(targets);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Gift(IReadOnlyList<Creature> targets)
    {
        await DebuffAnimation(targets);
        List<Task> taskList = new List<Task>();
        foreach (Creature target in targets)
        {
            taskList.Add(ChooseGift(target));
        }
        await Task.WhenAll(taskList);
        if (!CombatState.IsLiveCombat())
            return;
        GiftCounter++;
        if (GiftCounter >= GiftSets.Count)
        {
            GiftCounter = 0;
        }
        await ResetIdle();
    }
    
    public async Task ChooseGift(Creature target)
    {
        if (target.IsDead)
        {
            return;
        }
        if (target.Player != null)
        {
            CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), GiftSets[GiftCounter].Select((Func<IChoosable, CardModel>) (c =>
            {
                if (target.Player != null)
                {
                    return CombatState.CreateCard((CardModel) c, target.Player);
                }
                return null;
            })).ToList(), target.Player);
            if (cardModel == null)
            {
                return;
            }
            await ((IChoosable) cardModel).OnChosen();
        }
    }

    private async Task CrystalFallAnimation(IReadOnlyList<Creature> targets)
    {
        var target = targets.FirstOrDefault(LocalContext.IsMe);
        if (target != null)
        {
            var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
            if (targetNode != null)
            {
                var effect = OzCrystalEffect.Create(targetNode.VfxSpawnPosition);
                Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
                vfxContainer?.AddChildSafely(effect);
                await WaitAnimation(2.0f);
            }
        }
    }
    
    private async Task CrystalShardsAnimation(IReadOnlyList<Creature> targets)
    {
        var target = targets.FirstOrDefault(LocalContext.IsMe);
        if (target != null)
        {
            var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
            if (targetNode != null)
            {
                var effect = OzShardsEffect.Create(targetNode.VfxSpawnPosition);
                Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
                Sfx.OzStrongAtkFinish.Play();
                vfxContainer?.AddChildSafely(effect);
                await WaitAnimation(1.5f);
            }
        }
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.OzAtkBoom, targets);
    }
    
    private async Task AttackAnimation2(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.OzAtkUp, targets);
    }
    
    private async Task SpecialStartAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.OzStrongAtkStart, targets);
    }
    
    private async Task DebuffAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.OzMagic, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Ranged", "Special"], controller);
    }
    
    public interface IChoosable
    {
        Task OnChosen();
    }
}