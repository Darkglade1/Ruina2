using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.Greed;

public sealed class KingOfGreed : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 127, 115);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 134, 122);
    
    private int KingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 29, 26);
    private int FixationDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int EdacityDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int VulnAmt => 1;
    private int FrailAmt => 2;

    protected override string VisualsPath => "Greed/greed.tscn".MonsterImagePath();

    private const string ROAD_OF_THE_KING = "ROAD_OF_THE_KING";
    private const string FIXATION = "FIXATION";
    private const string EDACITY = "EDACITY";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Road>(new ThrowingPlayerChoiceContext(), Creature, 3, Creature,  null);
        Sfx.GreedDiamond.Play();
    }

    private MoveState GetRoadOfTheKingState()
    {
        return new MoveState(ROAD_OF_THE_KING, RoadOfTheKing, new SingleAttackIntent(KingDamage));
    }

    private MoveState GetFixationState()
    {
        return new MoveState(FIXATION, Fixation, new SingleAttackIntent(FixationDamage), new DebuffIntent());
    }

    private MoveState GetEdacityState()
    {
        return new MoveState(EDACITY, Edacity, new SingleAttackIntent(EdacityDamage), new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetRoadOfTheKingState();
        var state2 = GetFixationState();
        var state3 = GetEdacityState();

        state1.FollowUpState = state3;
        state2.FollowUpState = state1;
        state3.FollowUpState = state2;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        
        return new MonsterMoveStateMachine(states, state3);
    }
    
    private async Task RoadOfTheKing(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await DamageCmd.Attack(KingDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task Fixation(IReadOnlyList<Creature> targets)
    {
        await SlashAnimation(targets);
        await DamageCmd.Attack(FixationDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, VulnAmt, Creature,  null);
        await ResetIdle();
        await WaitAnimation(0.25f);
        await SpecialReadyAnimation();
    }
    
    private async Task Edacity(IReadOnlyList<Creature> targets)
    {
        await StabAnimation(targets);
        await DamageCmd.Attack(EdacityDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, FrailAmt, Creature,  null);
        await ResetIdle();
    }
    
    public override async Task BeforeDeath(Creature creature)
    {
        await base.BeforeDeath(creature);
        if (creature != Creature)
            return;

        var livingMinions = CombatState.GetTeammatesOf(Creature)
            .Where(t => t != Creature && t.IsAlive && t.Monster is BrilliantBliss)
            .ToList();

        foreach (var minion in livingMinions)
        {
            await CreatureCmd.Kill(minion);
        }
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.GreedVertChange, targets);
    }
    
    private async Task StabAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Stab", Sfx.GreedStabChange, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.GreedStrAtkChange, targets);
    }
    
    private async Task SpecialReadyAnimation()
    {
        await AnimationAction("SpecialIdle", Sfx.GreedStrAtkReady);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Stab", "Special", "SpecialIdle"], controller);
    }
}