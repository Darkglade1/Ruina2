using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
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
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Vermilion;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Shockwave = Ruina2.Ruina2Code.Cards.EnemyCards.Vermilion.Shockwave;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Elena;

public sealed class Vermilion : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 650, 600);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;
    public override string TargetTexturePath => "VermilionIcon.png".UIImagePath();
    private int RampageDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 55, 50);
    private int HeatedDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int HeatedHits => 2;
    private int ShockwavehDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 5);
    private int StatusAmt => 1;
    private int ObstructBlockAmt => 50;
    private int HeatUpBlockAmt => 10;
    private int IntangibleAmt => 1;
    public bool HadBlockAtTurnStart;

    protected override string VisualsPath => "Vermilion/vermilion.tscn".MonsterImagePath();

    private const string OBSTRUCT = "OBSTRUCT";
    private const string SHOCKWAVE = "SHOCKWAVE";
    private const string HEATED_WEAPON  = "HEATED_WEAPON";
    private const string RAMPAGE = "RAMPAGE";
    private const string HEAT_UP = "HEAT_UP";

    private Creature? elena;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Binah>();
        elena = FindTarget<Elena>();
    }

    private MoveState GetObstructState()
    {
        return new MoveState(OBSTRUCT, Obstruct, new DefendIntent());
    }

    private MoveState GetHeatedWeaponState()
    {
        return new MoveState(HEATED_WEAPON, HeatedWeapon, new RuinaMultiAttackIntent(HeatedDamage, HeatedHits), new StatusIntent(StatusAmt));
    }

    private MoveState GetHeatUpState()
    {
        return new MoveState(HEAT_UP, HeatUp, new DefendIntent(), new BuffIntent());
    }
    
    private MoveState GetShockwaveState()
    {
        return new MoveState(SHOCKWAVE, Shockwave, new RuinaSingleAttackIntent(ShockwavehDamage), new BuffIntent());
    }
    
    private MoveState GetRampageState()
    {
        return new MoveState(RAMPAGE, Rampage, new RuinaSingleAttackIntent(RampageDamage));
    }

     private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetObstructState();
        var state2 = GetShockwaveState();
        var state3 = GetHeatedWeaponState();
        
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
        var state1 = GetRampageState(); 
        var state2 = GetHeatUpState();
        var state3 = GetHeatedWeaponState();
        
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
            if (LastMove(stateMachine, OBSTRUCT))
            {
                return SHOCKWAVE;
            }
            if (stateMachine.StateLog.Count >= 3)
            {
                stateMachine.StateLog.Clear();
            }
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, OBSTRUCT) && !LastMoveBefore(stateMachine, OBSTRUCT)) {
                possibilities.Add(OBSTRUCT);
            }
            if (!LastMove(stateMachine, HEATED_WEAPON) && !LastMoveBefore(stateMachine, HEATED_WEAPON)) {
                possibilities.Add(HEATED_WEAPON);
            }
            if (possibilities.Count == 0)
            {
                possibilities.Add(HEATED_WEAPON);
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
            if (!LastMove(stateMachine, RAMPAGE) && !LastMoveBefore(stateMachine, RAMPAGE)) {
                possibilities.Add(RAMPAGE);
            }
            if (!LastMove(stateMachine, HEATED_WEAPON) && !LastMoveBefore(stateMachine, HEATED_WEAPON)) {
                possibilities.Add(HEATED_WEAPON);
            }
            if (!LastMove(stateMachine, HEAT_UP) && !LastMoveBefore(stateMachine, HEAT_UP)) {
                possibilities.Add(HEAT_UP);
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
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Shockwave>();
        card1.SetDamage(ShockwavehDamage);
        card1.DynamicVars["IntangiblePower"].BaseValue = IntangibleAmt;
        var card2 = CreateCardForIntent<Obstruct>();
        card2.SetBlock(ObstructBlockAmt);
        var card3 = CreateCardForIntent<HeatUp>();
        card3.SetBlock(HeatUpBlockAmt);
        card3.SetStrength(StrengthAmount);
        var card4 = CreateCardForIntent<HeatedWeapon>();
        card4.SetDamage(HeatedDamage);
        card4.SetRepeat(HeatedHits);
        card4.SetCards(StatusAmt);
        var card5 = CreateCardForIntent<RampageousStrike>();
        card5.SetDamage(RampageDamage);
        return new Dictionary<string, CardModel>()
        {
            {SHOCKWAVE, card1},
            {OBSTRUCT, card2},
            {HEAT_UP, card3},
            {HEATED_WEAPON, card4},
            {RAMPAGE, card5},
        };
    }
    
    private async Task HeatedWeapon(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < HeatedHits; i++)
        {
            if (i % 2 == 0)
            {
                await SlashAnimation(targets);
            }
            else
            {
                await BluntAnimation(targets);
            }
            await DamageCmd.Attack(HeatedDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        await CardPileCmd.AddToCombatAndPreview<Burn>(CombatState.PlayerCreatures, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
    }
    
    private async Task Obstruct(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, ObstructBlockAmt, ValueProp.Move, null);
        await ResetIdle(1.0f);
    }
    
    private async Task HeatUp(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, HeatUpBlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Rampage(IReadOnlyList<Creature> targets)
    {
        await StrongAttackAnimation(targets);
        await DamageCmd.Attack(RampageDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task Shockwave(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(ShockwavehDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        if (HadBlockAtTurnStart)
        {
            Creature? buffTarget = elena;
            if (elena == null || elena.IsDead)
            {
                buffTarget = Creature;
            }
            if (buffTarget != null)
            {
                await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), buffTarget, IntangibleAmt + 1, Creature,  null);
            }
        }
        await ResetIdle(1.0f);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Binah binah)
        {
            if (binah.Creature.IsAlive && elena != null)
            {
                binah.OtherSideTargetMonster = elena;
                binah.Targets[0] = elena;
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
        await AnimationAction("Slash", Sfx.FireVert, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.FireHori, targets);
    }
    
    private async Task StrongAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.FireStrong, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", Sfx.FireGuard);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Slash", "Block"], controller);
    }
}