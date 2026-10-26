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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards;
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

    private int LargoDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 22);
    private int AllegroDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);
    private int AllegroHits => 2;
    private int ScytheDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 44, 40);
    private int TrailsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 33, 30);
    private int DanzaDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int DanzaHits => 5;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 3);
    private int VibrationAmount =>  AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int BlockAmt => 45;
    private int StatusAmt => 1;
    private bool attackingAlly = true;

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
        await PowerCmd.Apply<BlueReverb>(new ThrowingPlayerChoiceContext(), Creature, VibrationAmount, Creature,  null);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-ARGALIA.talk"), Creature, VfxColor.Blue);
    }

    private MoveState GetLargoState()
    {
        return new MoveState(LARGO, Largo, new RuinaSingleAttackIntent(LargoDamage), new DefendIntent());
    }
    
    private MoveState GetAllegroState()
    {
        return new MoveState(ALLEGRO, Allegro, new RuinaMultiAttackIntent(AllegroDamage, AllegroHits), new StatusIntent(StatusAmt));
    }
    
    private MoveState GetTrailsState()
    {
        return new MoveState(TRAILS, Trails, new RuinaSingleAttackIntent(TrailsDamage), new BuffIntent());
    }
    
    private MoveState GetScytheState()
    {
        return new MoveState(SCYTHE, Scythe, new RuinaSingleAttackIntent(ScytheDamage));
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
    
    private MonsterMoveStateMachine GenerateIntentState2Machine()
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
    
    private MonsterMoveStateMachine GenerateIntentState3Machine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLargoState();
        var state2 = GetAllegroState();
        var state3 = GetScytheState();
        var state4 = GetTrailsState(); 
        
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
        if (intentNum == 0 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive &&
            OtherSideTargetMonster.Monster is Roland roland && roland.NextMoves.Count > 0 &&
            roland.NextMoves[0].Id == Roland.FURIOSO)
        {
            return DANZA;
        }

        if (intentNum == 0)
        {
            if (LastMove(stateMachine, LARGO))
            {
                return ALLEGRO;
            } else if (LastMove(stateMachine, ALLEGRO))
            {
                return SCYTHE;
            }
            else
            {
                return LARGO;
            }
        } else if (intentNum == 1)
        {
            if (LastMove(stateMachine, ALLEGRO))
            {
                return SCYTHE;
            } else if (LastMove(stateMachine, SCYTHE))
            {
                return LARGO;
            }
            else
            {
                return ALLEGRO;
            }
        }
        else
        {
            if (LastMove(stateMachine, ALLEGRO))
            {
                return TRAILS;
            } else if (LastMove(stateMachine, TRAILS))
            {
                return LARGO;
            }
            else
            {
                return ALLEGRO;
            }
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntentState2Machine(), GenerateIntentState3Machine()];
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
        if (intentNum == 2 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive && attackingAlly)
        {
            return OtherSideTargetMonster;
        }
        return GetPlayerTarget();
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Allegro>();
        card1.SetDamage(AllegroDamage);
        card1.SetRepeat(AllegroHits);
        card1.SetCards(StatusAmt);
        var card2 = CreateCardForIntent<Trails>();
        card2.SetDamage(TrailsDamage);
        card2.SetStrength(StrengthAmount);
        var card3 = CreateCardForIntent<Largo>();
        card3.SetDamage(LargoDamage);
        card3.SetBlock(BlockAmt);
        var card4 = CreateCardForIntent<Danza>();
        card4.SetDamage(DanzaDamage);
        card4.SetRepeat(DanzaHits);
        var card5 = CreateCardForIntent<Scythe>();
        card5.SetDamage(ScytheDamage);
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
        await CardPileCmd.AddToCombatAndPreview<ControlledVibration>(CombatState.PlayerCreatures, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
    }
    
    private async Task Trails(IReadOnlyList<Creature> targets)
    {
        await SlashUpAnimation2(targets);
        await DamageCmd.Attack(TrailsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Scythe(IReadOnlyList<Creature> targets)
    {
        await SpecialAttackAnimation(targets);
        await DamageCmd.Attack(ScytheDamage)
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