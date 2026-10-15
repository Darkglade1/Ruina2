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
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

public class Mermaid : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 330, 300);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 345, 315);
    public override int NumIntents => 1;

    private int SlapDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int DebuffAmt => 2;
    private int DazedAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int SlimedAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

    protected override string VisualsPath => "Mermaid/mermaid.tscn".MonsterImagePath();

    private const string HORRENDOUS_PITCH = "HORRENDOUS_PITCH";
    private const string RESONATING_TERROR = "RESONATING_TERROR";
    private const string FISH_SLAP = "FISH_SLAP";
    
    public Creature? knife;
    public Creature? oswald;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Tiph>();
        knife = FindHittableTarget<Knife>();
        oswald = FindHittableTarget<Oswald>();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
    }

    private MoveState GetFishSlapState()
    {
        return new MoveState(FISH_SLAP, FishSlap, new RuinaSingleAttackIntent(SlapDamage), new StatusIntent(SlimedAmt));
    }
    
    private MoveState GetHorrendousPitchState()
    {
        return new MoveState(HORRENDOUS_PITCH, HorrendousPitch, new StatusIntent(DazedAmt));
    }
    
    private MoveState GetResonatingTerrorState()
    {
        return new MoveState(RESONATING_TERROR, ResonatingTerror, new DebuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetResonatingTerrorState();
        var state2 = GetHorrendousPitchState();
        var state3 = GetFishSlapState();

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
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        return GetPlayerTarget();
    }

    private async Task FishSlap(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(SlapDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Slimed>(CombatState.PlayerCreatures, PileType.Discard, SlimedAmt, null);
        await ResetIdle();
    }
    
    private async Task HorrendousPitch(IReadOnlyList<Creature> targets)
    {
        await RangedAnimation(targets);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(CombatState.PlayerCreatures, PileType.Draw, DazedAmt, null, CardPilePosition.Random);
        await ResetIdle(1.0f);
    }
    
    private async Task ResonatingTerror(IReadOnlyList<Creature> targets)
    {
        await RangedAnimation(targets);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle(1.5f);
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.BluntVert, targets);
    }
    
    private async Task RangedAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.MermaidFar, targets, 0.6f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Ranged"], controller);
    }
}