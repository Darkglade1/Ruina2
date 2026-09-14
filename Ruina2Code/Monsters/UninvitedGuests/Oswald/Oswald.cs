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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

public sealed class Oswald : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 770, 700);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int ClimaxDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int ClimaxHitsIncrease = 1;
    private int CurrentClimaxHitsIncrease = 0;
    public int ClimaxTotalHits => 4 + CurrentClimaxHitsIncrease;
    private int FunDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int FunHits => 2;
    private int BrainwashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StatusAmt => 2;
    private int DebuffAmt => 2;
    private int AllyDebuffAmt => 1;

    protected override string VisualsPath => "Oswald/oswald.tscn".MonsterImagePath();

    private const string CLIMAX = "CLIMAX";
    private const string FUN = "FUN";
    private const string CATCH  = "CATCH";
    private const string POW = "POW";
    private const string BRAINWASH = "BRAINWASH";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Tiph>();
        foreach (Creature target in CombatState.PlayerCreatures)
        {
            Brainwash mutable = (Brainwash) ModelDb.Power<Brainwash>().ToMutable();
            mutable.Target = target;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), mutable, Creature, 1, Creature, null);
        }
        TalkCmd.Play(L10NMonsterLookup("RUINA2-OSWALD.talk"), Creature, VfxColor.Gold);
    }

    private MoveState GetClimaxState()
    {
        return new MoveState(CLIMAX, Climax, new RuinaMultiAttackIntent(ClimaxDamage, (Func<int>) (() => ClimaxTotalHits)));
    }

    private MoveState GetFunState()
    {
        return new MoveState(FUN, Fun, new RuinaMultiAttackIntent(FunDamage, FunHits));
    }

    private MoveState GetCatchState()
    {
        return new MoveState(CATCH, Catch, new StatusIntent(StatusAmt));
    }
    
    private MoveState GetPowState()
    {
        return new MoveState(POW, Pow, new RuinaDebuffIntent(), new BuffIntent());
    }
    
    private MoveState GetBrainwashState()
    {
        return new MoveState(BRAINWASH, Brainwash, new RuinaSingleAttackIntent(BrainwashDamage), new RuinaDebuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetClimaxState();
        var state2 = GetFunState();
        var state3 = GetCatchState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state3;
        state3.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        
        return new MonsterMoveStateMachine(states, state1);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBrainwashState();
        var state2 = GetFunState();
        var state3 = GetPowState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state3;
        state3.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        
        return new MonsterMoveStateMachine(states, state1);
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
            if (NextMoves[intentNum].Id == POW)
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

    private async Task Climax(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < ClimaxTotalHits; i++)
        {
            if (i == ClimaxTotalHits - 1)
            {
                await SpecialAttackAnimation(targets);
            } else if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(ClimaxDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        await WaitAnimation();
        CurrentClimaxHitsIncrease += ClimaxHitsIncrease;
    }
    
    private async Task Fun(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < FunHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(FunDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Catch(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        await CardPileCmd.AddToCombatAndPreview<Wound>(targets, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
        await ResetIdle(1.0f);
    }
    
    private async Task Pow(IReadOnlyList<Creature> targets)
    {
        await BuffAnimation();
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Brainwash(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await DamageCmd.Attack(BrainwashDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTick<WeakPower>(targets, AllyDebuffAmt);
        await ApplyPowerAndSkipNextDurationTick<VulnerablePower>(targets, AllyDebuffAmt);
        await ResetIdle();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Tiph tiph)
        {
            if (tiph.Creature.IsAlive)
            {
                await tiph.OnBossDeath();
            }
        }
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.OswaldVert, targets);
    }

    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.OswaldStab, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.OswaldHori, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.OswaldAttract, targets);
    }
    
    private async Task SpecialAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.OswaldFinish, targets);
    }
    
    private async Task BuffAnimation()
    {
        await AnimationAction("Block", Sfx.OswaldLaugh);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Slash", "Block", "Special"], controller);
    }
}