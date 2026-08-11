using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.BigBird;

public sealed class BigBird : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 440, 400);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 3;

    private int SalvationDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int IlluminateDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int Illuminate2Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StatusAmt => 2;
    private int DebuffAmt => 1;
    
    private int EnchantedHPAmt = 20;
    private int EnchantedHPIncrease = 5;

    protected override string VisualsPath => "BigBird/big_bird.tscn".MonsterImagePath();

    private const string SALVATION = "SALVATION";
    private const string DAZZLE_PLAYER = "DAZZLE_PLAYER";
    private const string DAZZLE_PLAYER2 = "DAZZLE_PLAYER2";
    private const string DAZZLE_ALLY  = "DAZZLE_ALLY";
    private const string ILLUMINATE = "ILLUMINATE";
    private const string ILLUMINATE2 = "ILLUMINATE2";

    private bool firstDazzleAllyDone;

    public Creature? sage1;
    public Creature? sage2;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        foreach (var enemy in CombatState.Enemies)
        {
            if (enemy.Monster is Sage)
            {
                if (sage1 == null)
                {
                    sage1 = enemy;
                } else if (sage2 == null)
                {
                    sage2 = enemy;
                }
            }
        }
        await PowerCmd.Apply<Salvation>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetSalvationState()
    {
        return new MoveState(SALVATION, Salvation, new RuinaSingleAttackIntent(SalvationDamage));
    }

    private MoveState GetDazzlePlayerState()
    {
        return new MoveState(DAZZLE_PLAYER, DazzlePlayer, new StatusIntent(StatusAmt));
    }
    
    private MoveState GetDazzlePlayer2State()
    {
        return new MoveState(DAZZLE_PLAYER2, DazzlePlayer2, new StatusIntent(StatusAmt));
    }

    private MoveState GetDazzleAllyState()
    {
        return new MoveState(DAZZLE_ALLY, DazzleAlly,new RuinaDebuffIntent());
    }

    private MoveState GetIlluminateState()
    {
        return new MoveState(ILLUMINATE, Illuminate, new RuinaSingleAttackIntent(IlluminateDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetIlluminate2State()
    {
        return new MoveState(ILLUMINATE2, Illuminate2, new RuinaSingleAttackIntent(Illuminate2Damage), new BuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetSalvationState();
        var state2 = GetDazzlePlayerState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetDazzlePlayer2State();
        var state2 = GetDazzleAllyState();
        var state3 = GetSalvationState();
        var state4 = GetIlluminateState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 1);

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
    
    private MonsterMoveStateMachine GenerateIntent3StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetDazzlePlayer2State();
        var state2 = GetDazzleAllyState();
        var state3 = GetSalvationState();
        var state4 = GetIlluminate2State();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 2);

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
        if (intentNum == 1)
        {
            if (CombatState.RoundNumber == 1)
            {
                return DAZZLE_PLAYER2;
            }
            else
            {
                if (sage1 == null || sage1.IsDead)
                {
                    if (NextMoves[0].Id == SALVATION)
                    {
                        return SALVATION;
                    }
                    else
                    {
                        return ILLUMINATE;
                    }
                }
                else
                {
                    if (LastMove(stateMachine, DAZZLE_ALLY))
                    {
                        return DAZZLE_PLAYER2;
                    } else if (LastMove(stateMachine, DAZZLE_PLAYER2) && firstDazzleAllyDone)
                    {
                        return SALVATION;
                    }
                    else
                    {
                        firstDazzleAllyDone = true;
                        return DAZZLE_ALLY;
                    }
                }
            }
        } else
        {
            if (sage2 == null || sage2.IsDead)
            {
                if (NextMoves[0].Id == SALVATION)
                {
                    return SALVATION;
                }
                else
                {
                    return ILLUMINATE2;
                }
            }
            else
            {
                if (LastMove(stateMachine, DAZZLE_ALLY))
                {
                    return DAZZLE_PLAYER2;
                } else if (LastMove(stateMachine, DAZZLE_PLAYER2))
                {
                    return SALVATION;
                }
                else
                {
                    return DAZZLE_ALLY;
                }
            }
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine(), GenerateIntent3StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            return CombatState.PlayerCreatures[0];
        }

        if (intentNum == 1)
        {
            if (NextMoves[intentNum].Id == DAZZLE_PLAYER2)
            {
                return CombatState.PlayerCreatures[0];
            }
            if (sage1 != null && sage1.IsAlive)
            {
                return sage1;
            }
            return CombatState.PlayerCreatures[0];
        }
        if (intentNum == 2)
        {
            if (NextMoves[intentNum].Id == DAZZLE_PLAYER2)
            {
                return CombatState.PlayerCreatures[0];
            }
            if (sage2 != null && sage2.IsAlive)
            {
                return sage2;
            }
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task Salvation(IReadOnlyList<Creature> targets)
    {
        if (targets[0].Monster is Sage && targets[0].HasPower<Enchanted>())
        {
            await Salvation1Animation(targets);
            await WaitAnimation(0.25f);
            await SalvationFullScreenEffect();
            await Salvation2Animation(targets);
            Node? vfxContainer = targets[0].GetVfxContainer();
            NDamageNumVfx? child = NDamageNumVfx.Create(targets[0], 999);
            if (child != null)
            {
                if (vfxContainer != null)
                {
                    vfxContainer.AddChildSafely(child);
                }
                else
                {
                    NRun.Instance?.GlobalUi.AddChildSafely(child);
                }
            }
            NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
            await CreatureCmd.Kill(targets);
        }
        else
        {
            await DazzleAnimation(targets);
            await DamageCmd.Attack(SalvationDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task DazzleAlly(IReadOnlyList<Creature> targets)
    {
        await DazzleAnimation(targets);
        if (targets[0].Monster is Sage)
        {
            await PowerCmd.Apply<Enchanted>(new ThrowingPlayerChoiceContext(), targets, Creature.ScaleHpForMultiplayer(EnchantedHPAmt, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex), Creature, null);
            EnchantedHPAmt += EnchantedHPIncrease;
        }
        else
        { 
            await CardPileCmd.AddToCombatAndPreview<Dazzled>(targets, PileType.Discard, StatusAmt, null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task DazzlePlayer(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await CardPileCmd.AddToCombatAndPreview<Dazzled>(targets, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
        await ResetIdle(1.0f);
    }
    
    private async Task DazzlePlayer2(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await CardPileCmd.AddToCombatAndPreview<Dazzled>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle(1.0f);
    }

    private async Task Illuminate(IReadOnlyList<Creature> targets)
    {
        await DazzleAnimation(targets);
        await DamageCmd.Attack(IlluminateDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature, null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature, null);
        await ResetIdle(1.0f);
    }
    
    private async Task Illuminate2(IReadOnlyList<Creature> targets)
    {
        await DazzleAnimation(targets);
        await DamageCmd.Attack(Illuminate2Damage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
        await ResetIdle(1.0f);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature)
        {
            if (sage1 != null && sage1.IsAlive && sage1.Monster is Sage sage)
            {
                await sage.OnBigBirdDeath();
            }
            if (sage2 != null && sage2.IsAlive && sage2.Monster is Sage sageTwo)
            {
                await sageTwo.OnBigBirdDeath();
            }
        }
    }
    
    private async Task SalvationFullScreenEffect()
    {
        var fullScreenEffect = FullScreenImageEffect.Create("Salvation.png".VfxImagePath(), 1.0f, 1.0f);
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(fullScreenEffect);
        await WaitAnimation(2.0f);
    }

    private async Task Salvation1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Salvation1", Sfx.BigBirdOpen, targets);
    }
    
    private async Task Salvation2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Salvation2", Sfx.BigBirdCrunch, targets);
    }
    
    private async Task DazzleAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Lamp", Sfx.BigBirdLamp, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Lamp", Sfx.BigBirdEyes, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Lamp", "Salvation1", "Salvation2"], controller);
    }
}