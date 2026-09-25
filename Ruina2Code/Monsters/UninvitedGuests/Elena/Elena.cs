using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Elena;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;
using Bleed = Ruina2.Ruina2Code.Powers.Bleed;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Elena;

public sealed class Elena : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 550, 500);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;
    public override string TargetTexturePath => "ElenaIcon.png".UIImagePath();
    private int SanguineDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int SanguineHits => 2;
    private int SiphonDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int BloodspreadingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 42, 38);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int InjectStrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int BleedAmt => 10;
    private int FrailAmt => 2;
    private int PlatingAmt => 10;

    protected override string VisualsPath => "Elena/elena.tscn".MonsterImagePath();

    private const string SANGUINE_NAILS = "SANGUINE_NAILS";
    private const string SIPHON = "SIPHON";
    private const string BLOODSPREADING  = "BLOODSPREADING";
    private const string CIRCULATION = "CIRCULATION";
    private const string INJECT = "INJECT";

    private Creature? vermilion;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Binah>();
        vermilion = FindTarget<Vermilion>();
        await PowerCmd.Apply<BloodRed>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-ELENA.talk"), Creature, VfxColor.Red);
    }

    private MoveState GetSiphonState()
    {
        return new MoveState(SIPHON, Siphon, new RuinaSingleAttackIntent(SiphonDamage), new RuinaDebuffIntent());
    }

    private MoveState GetSanguineNailsState()
    {
        return new MoveState(SANGUINE_NAILS, SanguineNails, new RuinaMultiAttackIntent(SanguineDamage, SanguineHits));
    }

    private MoveState GetInjectState()
    {
        return new MoveState(INJECT, Inject, new RuinaDebuffIntent(), new RuinaBuffIntent());
    }
    
    private MoveState GetCirculationState()
    {
        return new MoveState(CIRCULATION, Circulation, new RuinaBuffIntent());
    }
    
    private MoveState GetBloodspreadingState()
    {
        return new MoveState(BLOODSPREADING, Bloodspreading, new RuinaSingleAttackIntent(BloodspreadingDamage));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetSanguineNailsState();
        var state2 = GetSiphonState();
        var state3 = GetCirculationState();
        
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

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBloodspreadingState(); 
        var state2 = GetInjectState();
        var state3 = GetSanguineNailsState();
        
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
            if (stateMachine.StateLog.Count >= 3)
            {
                stateMachine.StateLog.Clear();
            }
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, SANGUINE_NAILS) && !LastMoveBefore(stateMachine, SANGUINE_NAILS)) {
                possibilities.Add(SANGUINE_NAILS);
            }
            if (!LastMove(stateMachine, SIPHON) && !LastMoveBefore(stateMachine, SIPHON)) {
                possibilities.Add(SIPHON);
            }
            if (!LastMove(stateMachine, CIRCULATION) && !LastMoveBefore(stateMachine, CIRCULATION)) {
                possibilities.Add(CIRCULATION);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
        else
        {
            if (stateMachine.StateLog.Count >= 3)
            {
                stateMachine.StateLog.Clear();
            }
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, BLOODSPREADING) && !LastMoveBefore(stateMachine, BLOODSPREADING)) {
                possibilities.Add(BLOODSPREADING);
            }
            if (!LastMove(stateMachine, INJECT) && !LastMoveBefore(stateMachine, INJECT)) {
                possibilities.Add(INJECT);
            }
            if (!LastMove(stateMachine, SANGUINE_NAILS) && !LastMoveBefore(stateMachine, SANGUINE_NAILS)) {
                possibilities.Add(SANGUINE_NAILS);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
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
        if (intentNum == 1 && NextMoves.Count >= 2)
        {
            if (NextMoves[intentNum].Id == BLOODSPREADING || NextMoves[intentNum].Id == INJECT)
            {
                return CombatState.PlayerCreatures[0];
            }
        }
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Inject>();
        card1.SetStrength(InjectStrAmt);
        card1.DynamicVars["Bleed"].BaseValue = BleedAmt;
        var card2 = CreateCardForIntent<Siphon>();
        card2.SetDamage(SiphonDamage);
        card2.SetFrail(FrailAmt);
        var card3 = CreateCardForIntent<Circulation>();
        card3.DynamicVars["PlatingPower"].BaseValue = PlatingAmt;
        card3.SetStrength(StrengthAmount);
        var card4 = CreateCardForIntent<SanguineNails>();
        card4.SetDamage(SanguineDamage);
        card4.SetRepeat(SanguineHits);
        var card5 = CreateCardForIntent<Bloodspreading>();
        card5.SetDamage(BloodspreadingDamage);
        return new Dictionary<string, CardModel>()
        {
            {INJECT, card1},
            {SIPHON, card2},
            {CIRCULATION, card3},
            {SANGUINE_NAILS, card4},
            {BLOODSPREADING, card5},
        };
    }

    private async Task Bloodspreading(IReadOnlyList<Creature> targets)
    {
        await SpecialAttackAnimation(targets);
        await DamageCmd.Attack(BloodspreadingDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task SanguineNails(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < SanguineHits; i++)
        {
            if (i % 2 == 0)
            {
                await SlashAnimation(targets);
            }
            else
            {
                await BluntAnimation(targets);
            }
            await DamageCmd.Attack(SanguineDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Circulation(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        Creature? buffTarget = vermilion;
        if (vermilion == null || vermilion.IsDead)
        {
            buffTarget = Creature;
        }
        if (buffTarget != null)
        {
            await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), buffTarget, PlatingAmt, Creature,  null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), buffTarget, StrengthAmount, Creature,  null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task Inject(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await PowerCmd.Apply<Bleed>(new ThrowingPlayerChoiceContext(), targets, BleedAmt, Creature, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, InjectStrAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Siphon(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(SiphonDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, FrailAmt, Creature,  null);
        await ResetIdle();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Binah binah)
        {
            if (binah.Creature.IsAlive)
            {
                binah.OtherSideTargetMonster = vermilion;
                NCreature? creatureNode = binah.Creature.GetCreatureNode();
                if (creatureNode == null || !CombatState.IsLiveCombat())
                    return;
                TaskHelper.RunSafely(creatureNode.RefreshIntents());
                await binah.OnBossDeath();
            }
        }
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SwordVert, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.SwordHori, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.ElenaStrongUp, targets);
    }
    
    private async Task SpecialAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.ElenaStrongAtk, targets);
    }
    
    private async Task BuffAnimation()
    {
        await AnimationAction("Block", Sfx.ElenaStrongStart);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Slash", "Block", "Special"], controller);
    }
}