using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Puppeteer;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Puppeteer;

public sealed class Puppeteer : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1100, 1000);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int PullingStringsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 44, 40);
    private int TuggingStringsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);
    private int TuggingStringsHits => 2;
    private int AssailingPullsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 3);
    private int DebuffAmt => 2;
    private int VulnerableAmt => 1;
    private int BlockAmt => 30;
    private int DamageReduction => 50;
    public Creature? puppet;

    protected override string VisualsPath => "Puppeteer/puppeteer.tscn".MonsterImagePath();

    private const string PULLING_STRINGS_TAUT = "PULLING_STRINGS_TAUT";
    private const string TUGGING_STRINGS = "TUGGING_STRINGS";
    private const string ASSAILING_PULLS  = "ASSAILING_PULLS";
    private const string THIN_STRINGS = "THIN_STRINGS";
    private const string PUPPETRY = "PUPPETRY";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Chesed>();
        puppet = FindTarget<Puppet>();
        await PowerCmd.Apply<Mastermind>(new ThrowingPlayerChoiceContext(), Creature, DamageReduction, Creature, null);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-PUPPETEER.talk"), Creature, VfxColor.Blue);
    }

    private MoveState GetPullingStringsState()
    {
        return new MoveState(PULLING_STRINGS_TAUT, PullingStringsTaut, new RuinaSingleMassAttackIntent(PullingStringsDamage));
    }

    private MoveState GetTuggingStringsState()
    {
        return new MoveState(TUGGING_STRINGS, TuggingStrings, new RuinaMultiAttackIntent(TuggingStringsDamage, TuggingStringsHits));
    }

    private MoveState GetAssailingPullsState()
    {
        return new MoveState(ASSAILING_PULLS, AssailingPulls, new RuinaSingleAttackIntent(AssailingPullsDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetThinStringsState()
    {
        return new MoveState(THIN_STRINGS, ThinStrings, new DefendIntent(), new RuinaDebuffIntent());
    }
    
    private MoveState GetPuppetryState()
    {
        return new MoveState(PUPPETRY, Puppetry, new BuffIntent());
    }

     private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPullingStringsState();
        var state2 = GetAssailingPullsState();
        var state3 = GetTuggingStringsState();
        var state4 = GetThinStringsState(); 
        
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

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetAssailingPullsState(); 
        var state2 = GetTuggingStringsState();
        var state3 = GetPuppetryState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 1);

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
        if (intentNum == 0)
        {
            if (ThreeTurnCooldownHasPassedForMove(stateMachine, PULLING_STRINGS_TAUT))
            {
                return PULLING_STRINGS_TAUT;
            } else if (LastMove(stateMachine, ASSAILING_PULLS))
            {
                return TUGGING_STRINGS;
            } else if (LastMove(stateMachine, TUGGING_STRINGS))
            {
                return THIN_STRINGS;
            }
            else
            {
                return ASSAILING_PULLS;
            }
        } else
        {
            if (LastMove(stateMachine, TUGGING_STRINGS) && LastMoveBefore(stateMachine, ASSAILING_PULLS))
            {
                return PUPPETRY;
            } else if (LastMove(stateMachine, TUGGING_STRINGS))
            {
                return ASSAILING_PULLS;
            }
            else
            {
                return TUGGING_STRINGS;
            }
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            return GetPlayerTarget();
        }
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return GetPlayerTarget();
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<PullingStrings>();
        card1.SetDamage(PullingStringsDamage);
        var card2 = CreateCardForIntent<AssailingPulls>();
        card2.SetDamage(AssailingPullsDamage);
        card2.SetVulnerable(VulnerableAmt);
        var card3 = CreateCardForIntent<Puppetry>();
        card3.SetStrength(StrengthAmount);
        var card4 = CreateCardForIntent<TuggingStrings>();
        card4.SetDamage(TuggingStringsDamage);
        card4.SetRepeat(TuggingStringsHits);
        var card5 = CreateCardForIntent<ThinStrings>();
        card5.SetBlock(BlockAmt);
        card5.SetWeak(DebuffAmt);
        return new Dictionary<string, CardModel>()
        {
            {PULLING_STRINGS_TAUT, card1},
            {ASSAILING_PULLS, card2},
            {PUPPETRY, card3},
            {TUGGING_STRINGS, card4},
            {THIN_STRINGS, card5},
        };
    }

    private async Task PullingStringsTaut(IReadOnlyList<Creature> targets)
    {
        IsMassAttacking = true;
        await MassAttackStartAnimation(targets);
        await MassAttackAnimation();
        await WaitAnimation(1.5f);
        await MassAttackFinishAnimation(targets);
        await DamageCmd.Attack(PullingStringsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task TuggingStrings(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < TuggingStringsHits; i++)
        {
            if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            }
            else
            {
                await BluntAnimation(targets);
            }
            await DamageCmd.Attack(TuggingStringsDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Puppetry(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (!(enemy.Monster is AbstractAllyMonster))
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, StrengthAmount, Creature,  null);
            }
        }
        await ResetIdle(1.0f);
    }
    
    private async Task ThinStrings(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (!(enemy.Monster is AbstractAllyMonster))
            {
                await CreatureCmd.GainBlock(enemy, BlockAmt, ValueProp.Move, null);
            }
        }
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task AssailingPulls(IReadOnlyList<Creature> targets)
    {
        await RangedAnimation(targets);
        await DamageCmd.Attack(AssailingPullsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTick<VulnerablePower>(targets, VulnerableAmt);
        if (puppet != null && puppet.Monster is Puppet puppetMonster)
        {
            if (targets[0].IsPlayer)
            {
                puppetMonster.attackingAlly = false;
            }
            else
            {
                puppetMonster.attackingAlly = true;
            }
        }
        await ResetIdle();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Chesed Chesed)
        {
            if (Chesed.Creature.IsAlive)
            {
                await Chesed.OnBossDeath();
            }
        }
    }
    
    public override IReadOnlyList<Creature> AdditionalMassAttackTargets()
    {
        var newList = new List<Creature>();
        foreach (var hittableEnemy in CombatState.HittableEnemies)
        {
            if (hittableEnemy.Monster is AbstractAllyMonster)
            {
                newList.Add(hittableEnemy);
            }
        }
        return newList;
    }
    
    private async Task MassAttackAnimation()
    {
        var effect = RedLinesEffect.Create();
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(effect);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.BluntBlow, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BluntHori, targets);
    }
    
    private async Task RangedAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.PuppetBreak, targets);
    }
    
    private async Task MassAttackStartAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.PuppetStart, targets);
    }
    
    private async Task MassAttackFinishAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.PuppetStrongAtk, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", null);
    }
    
    private async Task BuffAnimation()
    {
        await AnimationAction("Special", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Block", "Special", "Ranged"], controller);
    }
}