using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Argalia;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Argalia;

public sealed class Argalia : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1650, 1500);
    public override int MaxInitialHp => MinInitialHp;

    public override int NumIntents => 3;

    private int LargoDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int AllegroDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int AllegroHits => 2;
    private int ScytheDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 35, 32);
    private int TrailsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 31, 28);
    private int DanzaDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int DanzaHits => 5;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StrengthLossAmt => 3;
    private int VibrationAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int BlockAmt => 40;

    public decimal ScytheDamageCalc
    {
        get
        {
            if (NextMoves.Count > 1 && NextMoves[1].Id == SCYTHE)
            {
                return ScytheDamage * 3; 
            }
            else
            {
                return ScytheDamage;
            }
        }
    }
    private List<String> movePool = new List<string>();

    protected override string VisualsPath => "Argalia/argalia.tscn".MonsterImagePath();

    private const string LARGO = "LARGO";
    private const string ALLEGRO = "ALLEGRO";
    private const string SCYTHE  = "SCYTHE";
    private const string TRAILS = "TRAILS";
    private const string DANZA = "DANZA";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Roland>();
        await PowerCmd.Apply<BlueReverb>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
        await PowerCmd.Apply<Resonance>(new ThrowingPlayerChoiceContext(), Creature, CombatState.PlayerCreatures.Count, Creature, null);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-ARGALIA.talk"), Creature, VfxColor.Blue);
    }

    private MoveState GetLargoState()
    {
        return new MoveState(LARGO, Largo, new RuinaSingleAttackIntent(LargoDamage), new DefendIntent());
    }
    
    private MoveState GetAllegroState()
    {
        return new MoveState(ALLEGRO, Allegro, new RuinaMultiAttackIntent(AllegroDamage, AllegroHits), new RuinaDebuffIntent());
    }
    
    private MoveState GetTrailsState()
    {
        return new MoveState(TRAILS, Trails, new RuinaSingleAttackIntent(TrailsDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetScytheState()
    {
        return new MoveState(SCYTHE, Scythe, new RuinaSingleAttackIntent((Func<decimal>) (() => ScytheDamageCalc)));
    }
    
    private MoveState GetDanzaState()
    {
        return new MoveState(DANZA, Danza, new RuinaMultiMassAttackIntent(DanzaDamage, DanzaHits));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLargoState();
        var state2 = GetAllegroState();
        var state3 = GetScytheState();
        var state4 = GetTrailsState(); 
        var state5 = GetDanzaState();
        
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
    
    private MonsterMoveStateMachine GenerateIntentStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLargoState();
        var state2 = GetAllegroState();
        var state3 = GetScytheState();
        var state4 = GetTrailsState(); 
        
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

    private void PopulateMovePool(Rng rng)
    {
        movePool.Add(LARGO);
        movePool.Add(LARGO);
        movePool.Add(ALLEGRO);
        movePool.Add(ALLEGRO);
        movePool.Add(TRAILS);
        movePool.Add(SCYTHE);
        movePool.StableShuffle(rng);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (movePool.Count == 0)
        {
            PopulateMovePool(rng);
        }
        
        var nextMove = movePool[rng.NextInt(movePool.Count)];
        movePool.Remove(nextMove);

        if (intentNum == 0 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive &&
            OtherSideTargetMonster.Monster is Roland roland && roland.NextMoves.Count > 0 &&
            roland.NextMoves[0].Id == Roland.FURIOSO)
        {
            return DANZA;
        }
        
        return nextMove;
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntentStateMachine(), GenerateIntentStateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            return CombatState.PlayerCreatures[0];
        }
        if ((intentNum == 1 || intentNum == 2) && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Allegro>();
        card1.SetDamage(AllegroDamage);
        card1.SetRepeat(AllegroHits);
        card1.DynamicVars["Vibration"].BaseValue = VibrationAmt;
        var card2 = CreateCardForIntent<Trails>();
        card2.SetDamage(TrailsDamage);
        card2.SetStrength(StrengthLossAmt);
        var card3 = CreateCardForIntent<Largo>();
        card3.SetDamage(LargoDamage);
        card3.SetBlock(BlockAmt);
        var card4 = CreateCardForIntent<Danza>();
        card4.SetDamage(DanzaDamage);
        card4.SetRepeat(DanzaHits);
        var card5 = CreateCardForIntent<Scythe>();
        card5.SetDamage(ScytheDamage);
        card5.DamageCalc = (Func<decimal>) (() => ScytheDamageCalc);
        return new Dictionary<string, CardModel>()
        {
            {ALLEGRO, card1},
            {TRAILS, card2},
            {LARGO, card3},
            {DANZA, card4},
            {SCYTHE, card5}
        };
    }

    private async Task Largo(IReadOnlyList<Creature> targets)
    {
        await SlashLeftAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await DamageCmd.Attack(LargoDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Allegro(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < AllegroHits; i++)
        {
            if (i % 2 == 0)
            {
                await SlashUpAnimation(targets);
            }
            else
            {
                await SlashDownAnimation(targets);
            }
            await DamageCmd.Attack(AllegroDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        await PowerCmd.Apply<Vibration>(new ThrowingPlayerChoiceContext(), targets, VibrationAmt, Creature,  null);
    }
    
    private async Task Trails(IReadOnlyList<Creature> targets)
    {
        await SlashUpAnimation2(targets);
        await DamageCmd.Attack(TrailsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), targets, -StrengthLossAmt, Creature,  null);
        if (NextMoves.Count > 0 && NextMoves[0].Id == TRAILS)
        {
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -StrengthLossAmt, Creature,  null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task Scythe(IReadOnlyList<Creature> targets)
    {
        await SpecialAttackAnimation(targets);
        await DamageCmd.Attack(ScytheDamageCalc)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task Danza(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < DanzaHits; i++) {
            IsMassAttacking = true;
            if (i == 0) 
            {
                await SlashLeftAnimation(targets);
            } else if (i == 1)
            {
                await SlashRightAnimation(targets);
            } else if (i == 2)
            {
                await SlashDownAnimation(targets);
            } else if (i == 3) {
               await SlashUpAnimation(targets);
            } else {
                await SpecialAttackAnimation(targets);
            }
            await DamageCmd.Attack(DanzaDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(1.0f);
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

    public async Task ShiftIntents()
    {
        if (NextMoves.Count > 0 && NextMoves[0].Id == DANZA)
        {
            return;
        }
        List<MoveState> tempList = new List<MoveState>();
        foreach (var moveState in NextMoves)
        {
            tempList.Add(moveState);
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            if (i == 0)
            {
                NextMoves[i] = tempList[tempList.Count - 1];
            }
            else
            {
                NextMoves[i] = tempList[i - 1];
            }
        }
        
        NCreature? creatureNode = Creature.GetCreatureNode();
        if (creatureNode == null || !CombatState.IsLiveCombat())
            return;
        await TaskHelper.RunSafely(creatureNode.RefreshIntents());
        UpdateCardIntentVisuals();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Roland roland)
        {
            if (roland.Creature.IsAlive)
            {
                await roland.OnBossDeath();
            }
        }
    }
    
    private async Task SlashLeftAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SlashLeft", Sfx.ArgaliaAtk, targets);
    }

    private async Task SlashRightAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SlashRight", Sfx.ArgaliaAtk, targets);
    }
    private async Task SlashUpAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SlashUp", Sfx.ArgaliaStrongAtk1, targets);
    }
    private async Task SlashDownAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SlashDown", Sfx.ArgaliaStrongAtk2, targets);
    }
    private async Task SpecialAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.ArgaliaFarAtk1, targets);
    }
    private async Task SlashUpAnimation2(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SlashUp", Sfx.ArgaliaFarAtk2, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "SlashLeft", "SlashRight", "SlashUp", "SlashDown", "Special"], controller);
    }
}