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
using Ruina2.Ruina2Code.Cards.EnemyCards.Bremen;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;
using Melody = Ruina2.Ruina2Code.Powers.UninvitedGuests.Melody;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Bremen;

public sealed class Bremen : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 940, 850);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 3;

    private int ChorusDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int TendonDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int TendonHits => 2;
    private int TrioDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int TrioHits => 3;
    private int NeighDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StatusAmt => 3;
    private int DebuffAmt => 2;
    private int BlockAmt => 22;
    private int BaseMelodyLength => 3;
    private int MelodyLengthIncrease => 1;
    private int IncreasedMelodyLength = 0;
    public int MelodyLength => BaseMelodyLength + IncreasedMelodyLength;
    private int MelodyFragileAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    private bool attackingAlly;

    protected override string VisualsPath => "Bremen/bremen.tscn".MonsterImagePath();

    private const string CHORUS = "CHORUS";
    private const string NEIGH = "NEIGH";
    private const string BAWK  = "BAWK";
    private const string RARF = "RARF";
    private const string TENDON = "TENDON";
    private const string TRIO = "TRIO";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Netzach>();
        foreach (Creature target in CombatState.PlayerCreatures)
        {
            Melody mutable = (Melody) ModelDb.Power<Melody>().ToMutable();
            mutable.Target = target;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), mutable, Creature, MelodyFragileAmt, Creature, null);
        }
        TalkCmd.Play(L10NMonsterLookup("RUINA2-BREMEN.talk"), Creature, VfxColor.Green);
    }

    private MoveState GetChorusState()
    {
        return new MoveState(CHORUS, Chorus, new RuinaSingleAttackIntent(ChorusDamage), new BuffIntent());
    }

    private MoveState GetNeighState()
    {
        return new MoveState(NEIGH, Neigh, new RuinaSingleAttackIntent(NeighDamage));
    }
    
    private MoveState GetTendonState()
    {
        return new MoveState(TENDON, Tendon, new RuinaMultiAttackIntent(TendonDamage, TendonHits));
    }
    
    private MoveState GetTrioState()
    {
        return new MoveState(TRIO, Trio, new RuinaMultiAttackIntent(TrioDamage, TrioHits), new BuffIntent());
    }

    private MoveState GetBawkState()
    {
        return new MoveState(BAWK, Bawk, new StatusIntent(StatusAmt));
    }
    
    private MoveState GetRarfState()
    {
        return new MoveState(RARF, Rarf, new DefendIntent(), new RuinaDebuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetTrioState();
        var state2 = GetRarfState();
        var state3 = GetBawkState();
        var state4 = GetNeighState(); 
        var state5 = GetTendonState();
        
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

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetNeighState(); 
        var state2 = GetTendonState();
        var state3 = GetBawkState();
        
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
    
    private MonsterMoveStateMachine GenerateIntent3StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetChorusState();
        var state2 = GetRarfState();
        var state3 = GetNeighState(); 
        var state4 = GetTendonState();
        
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
            if (stateMachine.StateLog.Count >= 2 && !LastMove(stateMachine, TRIO) &&
                !LastMoveBefore(stateMachine, TRIO))
            {
                return TRIO;
            }
            else
            {
                List<string> possibilities = new List<string>();
                if (!LastMove(stateMachine, RARF) && !LastMoveBefore(stateMachine, RARF)) {
                    possibilities.Add(RARF);
                }
                if (!LastMove(stateMachine, BAWK) && !LastMoveBefore(stateMachine, BAWK)) {
                    possibilities.Add(BAWK);
                }
                if (!LastMove(stateMachine, NEIGH) && !LastMoveBefore(stateMachine, NEIGH)) {
                    possibilities.Add(NEIGH);
                }
                if (!LastMove(stateMachine, TENDON) && !LastMoveBefore(stateMachine, TENDON)) {
                    possibilities.Add(TENDON);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
            }
        } else if (intentNum == 1)
        {
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, NEIGH)) {
                possibilities.Add(NEIGH);
            }
            if (!LastMove(stateMachine, TENDON)) {
                possibilities.Add(TENDON);
            }
            if (!LastMove(stateMachine, BAWK) && !LastMoveBefore(stateMachine, BAWK) && NextMoves[0].Id != BAWK) {
                possibilities.Add(BAWK);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!attackingAlly)
            {
                if (!LastMove(stateMachine, RARF) && !LastMoveBefore(stateMachine, RARF) && NextMoves[0].Id != RARF) {
                    possibilities.Add(RARF);
                }
            }
            if (!LastMove(stateMachine, NEIGH) && !LastMoveBefore(stateMachine, NEIGH)) {
                possibilities.Add(NEIGH);
            }
            if (!LastMove(stateMachine, TENDON) && !LastMoveBefore(stateMachine, TENDON)) {
                possibilities.Add(TENDON);
            }
            if (!LastMove(stateMachine, CHORUS) && !LastMoveBefore(stateMachine, CHORUS)) {
                possibilities.Add(CHORUS);
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
        var card1 = CreateCardForIntent<Trio>();
        card1.SetDamage(TrioDamage);
        card1.SetRepeat(TrioHits);
        card1.DynamicVars["Increase"].BaseValue = MelodyLengthIncrease;
        var card2 = CreateCardForIntent<Neigh>();
        card2.SetDamage(NeighDamage);
        var card3 = CreateCardForIntent<Chorus>();
        card3.SetDamage(ChorusDamage);
        card3.SetStrength(StrengthAmount);
        var card4 = CreateCardForIntent<Tendon>();
        card4.SetDamage(TendonDamage);
        card4.SetRepeat(TendonHits);
        var card5 = CreateCardForIntent<Bawk>();
        card5.SetCards(StatusAmt);
        var card6 = CreateCardForIntent<Rarf>();
        card6.SetBlock(BlockAmt);
        card6.DynamicVars["Paralysis"].BaseValue = DebuffAmt;
        return new Dictionary<string, CardModel>()
        {
            {TRIO, card1},
            {NEIGH, card2},
            {CHORUS, card3},
            {TENDON, card4},
            {BAWK, card5},
            {RARF, card6}
        };
    }

    private async Task Trio(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < TrioHits; i++)
        {
            if (i % 2 == 0)
            {
                await SpecialAttackAnimation(targets);
            }
            else
            {
                await SpecialAttackAnimation2(targets);
            }
            await DamageCmd.Attack(TrioDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(1.0f);
        }
        IncreasedMelodyLength += MelodyLengthIncrease;
    }
    
    private async Task Tendon(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < TendonHits; i++)
        {
            if (i % 2 == 0)
            {
                await SlashAnimation(targets);
            }
            else
            {
                await PierceAnimation(targets);
            }
            await DamageCmd.Attack(TendonDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Neigh(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(NeighDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Bawk(IReadOnlyList<Creature> targets)
    {
        await DebuffAnimation(targets);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(CombatState.PlayerCreatures, PileType.Discard, StatusAmt, null);
        await ResetIdle(1.0f);
    }
    
    private async Task Rarf(IReadOnlyList<Creature> targets)
    {
        await PierceAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Chorus(IReadOnlyList<Creature> targets)
    {
        await BluntAnimation(targets);
        await DamageCmd.Attack(ChorusDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Netzach netzach)
        {
            if (netzach.Creature.IsAlive)
            {
                await netzach.OnBossDeath();
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
            attackingAlly = !attackingAlly;
        }
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.BremenHorse, targets);
    }

    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.BremenDog, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BluntHori, targets);
    }
    
    private async Task DebuffAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.BremenChicken, targets);
    }
    
    private async Task SpecialAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special1", Sfx.BremenStrong, targets);
    }
    
    private async Task SpecialAttackAnimation2(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special2", Sfx.BremenStrongFar, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Slash", "Block", "Special1", "Special2", "Ranged"], controller);
    }
}