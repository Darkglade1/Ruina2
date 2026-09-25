using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Philip;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Philip;

public sealed class Philip : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 770, 700);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2 + numExtraIntents;

    private int SearingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int StigmatizeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int StigmatizeHits => 2;
    private int SorrowDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int SorrowHits => 3;
    private int EventideBurns => 3;
    private int SearingBurns => 1;
    private int DamageBonus => 34;
    private int StrengthAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int powerBurns => 1;
    private int BlockAmt => 10;
    private int FirstChangeTurn = 3;
    private int SecondChangeTurn = 6;
    private bool attackingAlly;
    private int phase = 1;
    private int numExtraIntents = 0;
    private bool gotBonusIntent;
    private bool gotBonusDamage;

    private Creature? minion1;
    private Creature? minion2;

    protected override string VisualsPath => "Philip/philip.tscn".MonsterImagePath();

    private const string EVENTIDE = "EVENTIDE";
    private const string EMOTIONS = "EMOTIONS";
    private const string STIGMATIZE  = "STIGMATIZE";
    private const string SEARING = "SEARING";
    private const string SORROW = "SORROW";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Malkuth>();
        var flameShield = await PowerCmd.Apply<FlameShield>(new ThrowingPlayerChoiceContext(), Creature, powerBurns, Creature, null);
        if (flameShield != null)
        {
            flameShield.DynamicVars["Turns"].BaseValue = SecondChangeTurn;
        }
        TalkCmd.Play(L10NMonsterLookup("RUINA2-PHILIP.talk"), Creature, VfxColor.Gold);
    }

    private MoveState GetEventideState()
    {
        return new MoveState(EVENTIDE, Eventide, new StatusIntent(EventideBurns));
    }

    private MoveState GetEmotionsState()
    {
        return new MoveState(EMOTIONS, Emotions, new DefendIntent(), new BuffIntent());
    }
    
    private MoveState GetStigmatizeState()
    {
        return new MoveState(STIGMATIZE, Stigmatize, new RuinaMultiAttackIntent(StigmatizeDamage, StigmatizeHits));
    }
    
    private MoveState GetSorrowState()
    {
        return new MoveState(SORROW, Sorrow, new RuinaMultiAttackIntent(SorrowDamage, SorrowHits));
    }

    private MoveState GetSearingState()
    {
        return new MoveState(SEARING, Searing, new RuinaSingleAttackIntent(SearingDamage), new StatusIntent(SearingBurns));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetEventideState();
        var state2 = GetSearingState();
        var state3 = GetStigmatizeState();
        var state4 = GetSorrowState(); 
        
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
        var state1 = GetSearingState(); 
        var state2 = GetStigmatizeState();
        var state3 = GetEmotionsState();
        var state4 = GetSorrowState();
        
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
        var state1 = GetSearingState(); 
        var state2 = GetStigmatizeState();
        var state3 = GetEmotionsState();
        var state4 = GetSorrowState();
        
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
        if (intentNum == 0)
        {
            List<string> possibilities = new List<string>();
            if (!gotBonusDamage && !gotBonusIntent)
            {
                if (!LastMove(stateMachine, EVENTIDE)) {
                    possibilities.Add(EVENTIDE);
                }
            }

            if (!gotBonusDamage)
            {
                if (!LastMove(stateMachine, SEARING)) {
                    possibilities.Add(SEARING);
                }
            }
            if (!LastMove(stateMachine, STIGMATIZE)) {
                possibilities.Add(STIGMATIZE);
            }

            if (gotBonusIntent)
            {
                if (!LastMove(stateMachine, SORROW)) {
                    possibilities.Add(SORROW);
                }
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        } else if (intentNum == 1)
        {
            List<string> possibilities = new List<string>();
            if (!gotBonusDamage)
            {
                if (!LastMove(stateMachine, SEARING)) {
                    possibilities.Add(SEARING);
                }
            }
            if (!LastMove(stateMachine, STIGMATIZE)) {
                possibilities.Add(STIGMATIZE);
            }
            if (gotBonusIntent)
            {
                if (!LastMove(stateMachine, SORROW)) {
                    possibilities.Add(SORROW);
                }
            }
            if (!gotBonusIntent && !gotBonusDamage)
            {
                if (!LastMove(stateMachine, EMOTIONS)) {
                    possibilities.Add(EMOTIONS);
                }
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!gotBonusDamage)
            {
                if (!LastMove(stateMachine, SEARING)) {
                    possibilities.Add(SEARING);
                }
            }
            if (!LastMove(stateMachine, STIGMATIZE)) {
                possibilities.Add(STIGMATIZE);
            }
            if (gotBonusIntent)
            {
                if (!LastMove(stateMachine, SORROW)) {
                    possibilities.Add(SORROW);
                }
            }
            if (gotBonusIntent && !gotBonusDamage)
            {
                if (!LastMove(stateMachine, EMOTIONS)) {
                    possibilities.Add(EMOTIONS);
                }
            }
            return possibilities[rng.NextInt(possibilities.Count)];
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
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        if (intentNum == 2 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive && attackingAlly)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Sorrow>();
        card1.SetDamage(SorrowDamage);
        card1.SetRepeat(SorrowHits);
        var card2 = CreateCardForIntent<Searing>();
        card2.SetDamage(SearingDamage);
        card2.SetCards(SearingBurns);
        var card3 = CreateCardForIntent<Emotions>();
        card3.SetBlock(BlockAmt);
        card3.SetStrength(StrengthAmt);
        var card4 = CreateCardForIntent<Stigmatize>();
        card4.SetDamage(StigmatizeDamage);
        card4.SetRepeat(StigmatizeHits);
        var card5 = CreateCardForIntent<Eventide>();
        card5.SetCards(EventideBurns);
        return new Dictionary<string, CardModel>()
        {
            {SORROW, card1},
            {SEARING, card2},
            {EMOTIONS, card3},
            {STIGMATIZE, card4},
            {EVENTIDE, card5}
        };
    }

    private async Task Sorrow(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < SorrowHits; i++)
        {
            if (i == SorrowHits - 1)
            {
                await RangedAnimation(targets);
            } else if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(SorrowDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.5f, phase);
        }
    }
    
    private async Task Stigmatize(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < StigmatizeHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            }
            else
            {
                await PierceAnimation(targets);
            }
            await DamageCmd.Attack(StigmatizeDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.5f, phase);
        }
    }
    
    private async Task Searing(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(SearingDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Burn>(CombatState.PlayerCreatures, PileType.Draw, SearingBurns, null, CardPilePosition.Random);
        await ResetIdle(0.5f, phase);
    }
    
    private async Task Eventide(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        await CardPileCmd.AddToCombatAndPreview<Burn>(CombatState.PlayerCreatures, PileType.Discard, EventideBurns, null);
        await ResetIdle(1.0f, phase);
    }
    
    private async Task Emotions(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmt, Creature,  null);
        await ResetIdle(1.0f, phase);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Malkuth malkuth)
        {
            if (malkuth.Creature.IsAlive)
            {
                await malkuth.OnBossDeath();
            }
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Creature))
        {
            attackingAlly = Rng.NextBool();
            if (CombatState.RoundNumber >= FirstChangeTurn && !gotBonusIntent)
            {
                gotBonusIntent = true;
                Sfx.PhilipTransform.Play(0.0f, 2.0f);
                numExtraIntents++;
            }
            if (CombatState.RoundNumber >= SecondChangeTurn && !gotBonusDamage)
            {
                gotBonusDamage = true;
                Sfx.PhilipTransform.Play(0.0f, 2.0f);
                phase++;
                await ResetIdle(0.0f, phase);
                await PowerCmd.Remove<FlameShield>(Creature);
                await PowerCmd.Apply<SwordOfEmbers>(new ThrowingPlayerChoiceContext(), Creature, DamageBonus, Creature, null);
            }
        }
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash" + phase, Sfx.PhilipVert, targets);
    }

    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce" + phase, Sfx.PhilipStab, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt" + phase, Sfx.PhilipHori, targets);
    }
    
    private async Task RangedAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged" + phase, Sfx.PhilipExplosion, targets);
    }
    
    private async Task BuffAnimation()
    {
        await AnimationAction("Guard" + phase, Sfx.FireGuard);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle1", "Idle2", "Blunt1", "Blunt2", "Pierce1", "Pierce2", "Slash1", "Slash2", "Guard1", "Guard2", "Ranged1", "Ranged2", "Special1", "Special2"], controller);
    }
}