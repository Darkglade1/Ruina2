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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Tanya;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Tanya;

public sealed class Tanya : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1000, 900);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int OverspeedDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 33, 30);
    private int OverspeedHits => 2;
    private int KicksDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private int KicksHits => 2;
    private int LupineDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 24);
    private int FisticuffsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 28, 26);
    private int BeatdownDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 31, 28);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int DebuffAmt => 2;
    private int PlatingAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 30, 25);
    private int BlockAmt => 18;

    protected override string VisualsPath => "Tanya/tanya.tscn".MonsterImagePath();

    private const string OVERSPEED = "OVERSPEED";
    private const string KICKS_AND_STOMPS = "KICKS_AND_STOMPS";
    private const string BEATDOWN  = "BEATDOWN";
    private const string INTIMIDATE = "INTIMIDATE";
    private const string LUPINE_ASSAULT = "LUPINE_ASSAULT";
    private const string FISTICUFFS = "FISTICUFFS";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Gebura>();
        await PowerCmd.Apply<BarricadePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await CreatureCmd.GainBlock(Creature, PlatingAmt, ValueProp.Move, null);
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, PlatingAmt, Creature, null);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-TANYA.talk"), Creature, VfxColor.Red);
    }

    private MoveState GetOverspeedState()
    {
        return new MoveState(OVERSPEED, Overspeed, new RuinaMultiAttackIntent(OverspeedDamage, OverspeedHits));
    }

    private MoveState GetKicksAndStompsState()
    {
        return new MoveState(KICKS_AND_STOMPS, KicksAndStomps, new RuinaMultiAttackIntent(KicksDamage, KicksHits), new BuffIntent());
    }

    private MoveState GetFisticuffsState()
    {
        return new MoveState(FISTICUFFS, Fisticuffs, new RuinaSingleAttackIntent(FisticuffsDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetLupineAssaultState()
    {
        return new MoveState(LUPINE_ASSAULT, LupineAssault, new RuinaSingleAttackIntent(LupineDamage), new DefendIntent());
    }
    
    private MoveState GetBeatdownState()
    {
        return new MoveState(BEATDOWN, Beatdown, new RuinaSingleMassAttackIntent(BeatdownDamage));
    }
    
    private MoveState GetIntimidateState()
    {
        return new MoveState(INTIMIDATE, Intimidate, new BuffIntent());
    }

     private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetOverspeedState();
        var state2 = GetBeatdownState();
        var state3 = GetLupineAssaultState();
        var state4 = GetFisticuffsState(); 
        
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
        var state1 = GetOverspeedState(); 
        var state2 = GetLupineAssaultState();
        var state3 = GetIntimidateState();
        var state4 = GetKicksAndStompsState();
        
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
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive &&
            OtherSideTargetMonster.Monster is Gebura gebura && gebura.NextMoves.Count > 0 &&
            (gebura.NextMoves[0].Id == Gebura.GSV || gebura.NextMoves[0].Id == Gebura.GSH))
        {
            return OVERSPEED;
        }
        if (intentNum == 0)
        {
            if (stateMachine.StateLog.Count >= 2 && !LastMoveIgnoringMove(stateMachine, BEATDOWN, OVERSPEED) && !LastMoveBeforeIgnoringMove(stateMachine, BEATDOWN, OVERSPEED))
            {
                return BEATDOWN;
            }
            else
            {
                List<string> possibilities = new List<string>();
                if (!LastMove(stateMachine, LUPINE_ASSAULT)) {
                    possibilities.Add(LUPINE_ASSAULT);
                }
                if (!LastMove(stateMachine, FISTICUFFS)) {
                    possibilities.Add(FISTICUFFS);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
            }
        } else
        {
            if (stateMachine.StateLog.Count >= 2 && !LastMoveIgnoringMove(stateMachine, INTIMIDATE, OVERSPEED) && !LastMoveBeforeIgnoringMove(stateMachine, INTIMIDATE, OVERSPEED))
            {
                return INTIMIDATE;
            }
            else
            {
                List<string> possibilities = new List<string>();
                if (!LastMove(stateMachine, LUPINE_ASSAULT)) {
                    possibilities.Add(LUPINE_ASSAULT);
                }
                if (!LastMove(stateMachine, KICKS_AND_STOMPS)) {
                    possibilities.Add(KICKS_AND_STOMPS);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
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
            return CombatState.PlayerCreatures[0];
        }
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Beatdown>();
        card1.SetDamage(BeatdownDamage);
        var card2 = CreateCardForIntent<Fisticuffs>();
        card2.SetDamage(FisticuffsDamage);
        card2.SetWeak(DebuffAmt);
        var card3 = CreateCardForIntent<Intimidate>();
       card3.DynamicVars["PlatingPower"].BaseValue = (Creature.ScaleHpForMultiplayer(PlatingAmt, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex));
        var card4 = CreateCardForIntent<KicksAndStomps>();
        card4.SetDamage(KicksDamage);
        card4.SetRepeat(KicksHits);
        card4.SetStrength(StrengthAmount);
        var card5 = CreateCardForIntent<LupineAssault>();
        card5.SetBlock(BlockAmt);
        card5.SetDamage(LupineDamage);
        var card6 = CreateCardForIntent<Overspeed>();
        card6.SetDamage(OverspeedDamage);
        card6.SetRepeat(OverspeedHits);
        return new Dictionary<string, CardModel>()
        {
            {BEATDOWN, card1},
            {FISTICUFFS, card2},
            {INTIMIDATE, card3},
            {KICKS_AND_STOMPS, card4},
            {LUPINE_ASSAULT, card5},
            {OVERSPEED, card6},
        };
    }

    private async Task Beatdown(IReadOnlyList<Creature> targets)
    {
        IsMassAttacking = true;
        await MassAttackStartAnimation(targets);
        await WaitAnimation(0.75f);
        await MassAttackFinishAnimation(targets);
        await DamageCmd.Attack(BeatdownDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task KicksAndStomps(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < KicksHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(KicksDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
    }
    
    private async Task Overspeed(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < OverspeedHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            }
            else
            {
                await PierceAnimation(targets);
            }
            await DamageCmd.Attack(OverspeedDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Intimidate(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, PlatingAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Fisticuffs(IReadOnlyList<Creature> targets)
    {
        await PierceAnimation(targets);
        await DamageCmd.Attack(FisticuffsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task LupineAssault(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await DamageCmd.Attack(LupineDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Gebura gebura)
        {
            if (gebura.Creature.IsAlive)
            {
                await gebura.OnBossDeath();
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
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.BluntBlow, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BluntHori, targets);
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.BluntVert, targets);
    }
    
    private async Task MassAttackStartAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special1", null, targets);
    }
    
    private async Task MassAttackFinishAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special3", Sfx.GreedSlam, targets);
    }
    
    private async Task BuffAnimation()
    {
        await AnimationAction("Special5", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Slash", "Special1", "Special2", "Special3", "Special5"], controller);
    }
}