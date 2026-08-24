using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1.NothingDer;

public sealed class NothingThere : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 240, 220);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int EyeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int ReachingDamage => 3;
    private int ReachingHits => 2;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int BlockAmount => 12;
    private int StatusAmt => 1;

    protected override string VisualsPath => "NothingThere/nothing_there.tscn".MonsterImagePath();
    public override string TargetTexturePath => "NothingIcon.png".UIImagePath();

    private const string DENSE_FLESH = "DENSE_FLESH";
    private const string EYE_CONTACT = "EYE_CONTACT";
    private const string REACHING_HAND  = "REACHING_HAND";
    private const string EVOLVE = "EVOLVE";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<DerFreischutz>();
        FlipHorizontal();
        await PowerCmd.Apply<Mimicry>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        if (CombatState.Players.Count > 1)
        {
            await PowerCmd.Apply<MultiplayerAlly>(new ThrowingPlayerChoiceContext(), Creature, AbstractAllyMonster.GetAllyMultiplayerDamageModifier(CombatState), Creature,  null);
        }
    }

    private MoveState GetDenseFleshState()
    {
        return new MoveState(DENSE_FLESH, DenseFlesh, new DefendIntent());
    }

    private MoveState GetEyeContactState()
    {
        return new MoveState(EYE_CONTACT, EyeContact, new StatusIntent(StatusAmt), new RuinaSingleAttackIntent(EyeDamage));
    }

    private MoveState GetReachingHandState()
    {
        return new MoveState(REACHING_HAND, ReachingHand, new RuinaMultiAttackIntent(ReachingDamage, ReachingHits));
    }

    private MoveState GetEvolveState()
    {
        return new MoveState(EVOLVE, Evolve, new BuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetDenseFleshState();
        var state2 = GetEyeContactState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetReachingHandState();
        var state2 = GetEvolveState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
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
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task DenseFlesh(IReadOnlyList<Creature> targets)
    {
        if (CombatState.RoundNumber == 1)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-NOTHING_THERE.greeting"), Creature, VfxColor.Red);
            Sfx.NothingHello.Play();
        }
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmount, ValueProp.Move, null);
        await ResetIdle();
    }
    
    private async Task EyeContact(IReadOnlyList<Creature> targets)
    {
        await PierceAnimation(targets);
        await DamageCmd.Attack(EyeDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task ReachingHand(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < ReachingHits; i++)
        {
            if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(ReachingDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }

    private async Task Evolve(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
        await ResetIdle(1.0f);
    }

    public async Task OnDerFreiDeath()
    {
        TalkCmd.Play(L10NMonsterLookup("RUINA2-NOTHING_THERE.goodbye"), Creature, VfxColor.Red);
        Sfx.NothingGoodbye.Play();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is DerFreischutz der)
        {
            if (der.Creature.IsAlive)
            {
                await der.OnNothingThereDeath();
            }
        }
    }

    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.NothingNormal, targets);
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.NothingStrong, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Evade", null);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Evade", Sfx.NothingChange);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Slash", "Evade"], controller);
    }
}