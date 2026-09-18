using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Hod;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.EGO;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;
using Barrier = Ruina2.Ruina2Code.Cards.EnemyCards.Hod.Barrier;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Greta;

public sealed class Hod : AbstractAllyCardMonster
{
    public override int MinInitialHp => 130;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "HodIcon.png".UIImagePath();
    private bool talked = false;

    private int SnakeSlitDamage => 7;
    private int SnakeSlitHits => 2;
    private int ViolateBladeDamage => 8;
    private int VioletBladeHits => 3;
    private int LacerationDamage => 12;
    private int FangsDamage => 14;
    private int DuelDamage => 17;
    private int StrengthAmt => 3;
    private int BarrierBlockAmt => 18;
    private int DuelBlockAmt => 10;
    private int WeakAmt => 2;
    private int VulnerableAmt => 2;
    
    public static int SLASH = 1;
    public static int PIERCE = 2;
    public static int GUARD = 3;
    private int currentStance = SLASH;

    private MoveState? currentSlashMove;
    private MoveState? currentPierceMove;
    private MoveState? currentGuardMove;
    
    public MoveState? SnakeSlitState;
    public MoveState? VioletBladeState;
    public MoveState? LacerationState;
    public MoveState? VenomousFangsState;
    public MoveState? BarrierState;
    public MoveState? DuelState;

    private int SlashStanceBonus = 50;
    protected override string VisualsPath => "Hod/hod.tscn".MonsterImagePath();

    private const string SNAKE_SLIT = "SNAKE_SLIT";
    private const string VIOLET_BLADE = "VIOLET_BLADE";
    private const string LACERATION = "LACERATION";
    private const string VENOMOUS_FANGS = "VENOMOUS_FANGS";
    private const string BARRIER = "BARRIER";
    private const string DUEL = "DUEL";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Greta>();
        Sfx.PurpleChange.Play();
        currentSlashMove = SnakeSlitState;
        currentPierceMove = LacerationState;
        currentGuardMove = BarrierState;
        var slashButton = SetUpAllyButton("SlashButton","res://Ruina2/images/ui/purple_tear_stance_button.tscn", "res://Ruina2/images/ui/SlashStance.png", 1);
        if (slashButton is NPurpleTearStanceButton slashStanceButton)
        {
            slashStanceButton.Stance = SLASH;
        }
        var pierceButton = SetUpAllyButton("PierceButton","res://Ruina2/images/ui/purple_tear_stance_button.tscn", "res://Ruina2/images/ui/PierceStance.png", 2);
        if (pierceButton is NPurpleTearStanceButton pierceStanceButton)
        {
            pierceStanceButton.Stance = PIERCE;
        }
        var guardButton = SetUpAllyButton("PierceButton","res://Ruina2/images/ui/purple_tear_stance_button.tscn", "res://Ruina2/images/ui/GuardStance.png", 3);
        if (guardButton is NPurpleTearStanceButton guardStanceButton)
        {
            guardStanceButton.Stance = GUARD;
        }
        await PowerCmd.Apply<SlashStance>(new ThrowingPlayerChoiceContext(), Creature, SlashStanceBonus, Creature,  null);
    }

    private MoveState GetSnakeSlitState()
    {
        return new MoveState(SNAKE_SLIT, SnakeSlit, new RuinaMultiAttackIntent(SnakeSlitDamage, SnakeSlitHits), new BuffIntent());
    }

    private MoveState GetVioletBladeState()
    {
        return new MoveState(VIOLET_BLADE, VioletBlade, new RuinaMultiAttackIntent(ViolateBladeDamage, VioletBladeHits));
    }
    
    private MoveState GetLacerationState()
    {
        return new MoveState(LACERATION, Laceration, new RuinaSingleAttackIntent(LacerationDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetVenomousFangsState()
    {
        return new MoveState(VENOMOUS_FANGS, VenomousFangs, new RuinaSingleAttackIntent(FangsDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetBarrierState()
    {
        return new MoveState(BARRIER, Barrier, new RuinaDefendIntent());
    }
    
    private MoveState GetDuelState()
    {
        return new MoveState(DUEL, Duel, new RuinaSingleAttackIntent(DuelDamage), new RuinaDefendIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        SnakeSlitState = GetSnakeSlitState();
        VioletBladeState = GetVioletBladeState();
        LacerationState = GetLacerationState();
        VenomousFangsState = GetVenomousFangsState();
        BarrierState = GetBarrierState();
        DuelState = GetDuelState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        SnakeSlitState.FollowUpState = moveBranch;
        VioletBladeState.FollowUpState = moveBranch;
        LacerationState.FollowUpState = moveBranch;
        VenomousFangsState.FollowUpState = moveBranch;
        BarrierState.FollowUpState = moveBranch;
        DuelState.FollowUpState = moveBranch;

        states.Add(SnakeSlitState);
        states.Add(VioletBladeState);
        states.Add(LacerationState);
        states.Add(VenomousFangsState);
        states.Add(BarrierState);
        states.Add(DuelState);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, SnakeSlitState);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (currentStance == SLASH)
        {
            if (currentSlashMove != null && currentSlashMove.Id == SNAKE_SLIT)
            {
                currentSlashMove = VioletBladeState;
                return VIOLET_BLADE;
            }
            else
            {
                currentSlashMove = SnakeSlitState;
                return SNAKE_SLIT;
            }
        } else if (currentStance == PIERCE)
        {
            if (currentPierceMove != null && currentPierceMove.Id == LACERATION)
            {
                currentPierceMove = VenomousFangsState;
                return VENOMOUS_FANGS;
            }
            else
            {
                currentPierceMove = LacerationState;
                return LACERATION;
            }
        }
        else
        {
            if (currentGuardMove != null && currentGuardMove.Id == BARRIER)
            {
                currentGuardMove = DuelState;
                return DUEL;
            }
            else
            {
                currentGuardMove = BarrierState;
                return BARRIER;
            }
        }
    }


    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<SnakeSlit>();
        card1.SetDamage(SnakeSlitDamage);
        card1.SetRepeat(SnakeSlitHits);
        card1.SetStrength(StrengthAmt);
        var card2 = CreateCardForIntent<VioletBlade>();
        card2.SetDamage(ViolateBladeDamage);
        card2.SetRepeat(VioletBladeHits);
        var card3 = CreateCardForIntent<Laceration>();
        card3.SetDamage(LacerationDamage);
        card3.SetVulnerable(VulnerableAmt);
        var card4 = CreateCardForIntent<VenomousFangs>();
        card4.SetDamage(FangsDamage);
        card4.SetWeak(WeakAmt);
        var card5 = CreateCardForIntent<Barrier>();
        card5.SetBlock(BarrierBlockAmt);
        var card6 = CreateCardForIntent<Duel>();
        card6.SetDamage(DuelDamage);
        card6.SetBlock(DuelBlockAmt);
        return new Dictionary<string, CardModel>()
        {
            {SNAKE_SLIT, card1},
            {VIOLET_BLADE, card2},
            {LACERATION, card3},
            {VENOMOUS_FANGS, card4},
            {BARRIER, card5},
            {DUEL, card6}
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-HOD.response"), Creature, VfxColor.Gold);
            talked = true;
        }
    }

    private async Task SnakeSlit(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < SnakeSlitHits; i++)
        {
            if (i % 2 == 0)
            {
                await Slash2Animation(targets);
            }
            else
            {
                await Slash1Animation(targets);
            }
            await DamageCmd.Attack(SnakeSlitDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.5f, currentStance);
        }
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmt, Creature,  null);
    }
    
    private async Task VioletBlade(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < VioletBladeHits; i++)
        {
            if (i % 2 == 0)
            {
                await Slash1Animation(targets);
            }
            else
            {
                await Slash2Animation(targets);
            }
            await DamageCmd.Attack(ViolateBladeDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.5f, currentStance);
        }
    }
    
    private async Task Laceration(IReadOnlyList<Creature> targets)
    {
        Talk();
        await Pierce1Animation(targets);
        await DamageCmd.Attack(LacerationDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<VulnerablePower>(targets, VulnerableAmt);
        await ResetIdle(0.5f, currentStance);
    }
    
    private async Task VenomousFangs(IReadOnlyList<Creature> targets)
    {
        Talk();
        await Pierce2Animation(targets);
        await DamageCmd.Attack(FangsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, WeakAmt, Creature, null);
        await PowerCmd.Apply<RedEyesPower>(new ThrowingPlayerChoiceContext(), targets, 100, Creature, null);
        await ResetIdle(0.5f, currentStance);
    }
    
    private async Task Barrier(IReadOnlyList<Creature> targets)
    {
        Talk();
        await BlockAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BarrierBlockAmt, ValueProp.Move, null);
        foreach (var player in CombatState.PlayerCreatures)
        {
            await CreatureCmd.GainBlock(player, BarrierBlockAmt, ValueProp.Move, null);
        }
        await ResetIdle(1.0f, currentStance);
    }
    
    private async Task Duel(IReadOnlyList<Creature> targets)
    {
        Talk();
        await BlockAnimation(targets);
        await CreatureCmd.GainBlock(Creature, DuelBlockAmt, ValueProp.Move, null);
        foreach (var player in CombatState.PlayerCreatures)
        {
            await CreatureCmd.GainBlock(player, DuelBlockAmt, ValueProp.Move, null);
        }
        await WaitAnimation();
        await BluntAnimation(targets);
        await DamageCmd.Attack(DuelDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(0.5f, currentStance);
    }
    
    public async Task OnBossDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-HOD.victory"), Creature, VfxColor.Gold);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }

    public async Task ChangeStance(int newStance)
    {
        if (currentStance != newStance)
        {
            Sfx.PurpleChange.Play();
            currentStance = newStance;
            await AllowApplyPowersToAllies();
            if (currentStance == SLASH && currentSlashMove != null)
            {
                SetMoveImmediateMultiIntentMonster(currentSlashMove, 0);
                await PowerCmd.Remove<PierceStance>(Creature);
                await PowerCmd.Remove<GuardStance>(Creature);
                await PowerCmd.Apply<SlashStance>(new ThrowingPlayerChoiceContext(), Creature, SlashStanceBonus, Creature,  null);
            }
            if (currentStance == PIERCE && currentPierceMove != null)
            {
                SetMoveImmediateMultiIntentMonster(currentPierceMove, 0);
                await PowerCmd.Remove<SlashStance>(Creature);
                await PowerCmd.Remove<GuardStance>(Creature);
                await PowerCmd.Apply<PierceStance>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
            }
            if (currentStance == GUARD && currentGuardMove != null)
            {
                SetMoveImmediateMultiIntentMonster(currentGuardMove, 0);
                await PowerCmd.Remove<PierceStance>(Creature);
                await PowerCmd.Remove<SlashStance>(Creature);
                await PowerCmd.Apply<GuardStance>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
            }
            await DisableApplyPowersToAllies();
            await ResetIdle(0.0f, currentStance);
        }
    }
    
    private async Task Slash1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash1", Sfx.PurpleSlashVert, targets);
    }
    
    private async Task Slash2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash2", Sfx.PurpleSlashHori, targets);
    }
    
    private async Task Pierce1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.PurpleStab1, targets);
    }
    
    private async Task Pierce2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.PurpleStab2, targets);
    }

    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.PurpleBlunt, targets);
    }
    
    private async Task BlockAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Block", Sfx.PurpleGuard, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle1", "Idle2", "Idle3", "Blunt", "Pierce", "Slash1", "Slash2", "Block"], controller);
    }
}