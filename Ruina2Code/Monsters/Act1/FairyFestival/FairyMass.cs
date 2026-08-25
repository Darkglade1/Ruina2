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
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1.FairyFestival;

public sealed class FairyMass : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 20, 18);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 22, 20);
    
    private int WingbeatsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int GluttonyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);
    private int DebuffAmt => 1;

    protected override string VisualsPath => "FairyMass/fairy_mass.tscn".MonsterImagePath();

    private const string WINGBEATS = "WINGBEATS";
    private const string GLUTTONY = "GLUTTONY";

    private float ConsumeThreshold = 0.33f;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        await PowerCmd.Apply<Meal>(new ThrowingPlayerChoiceContext(), Creature, (int)Math.Round(Creature.MaxHp * ConsumeThreshold), Creature,  null);
    }

    private MoveState GetWingbeatsState()
    {
        return new MoveState(WINGBEATS, Wingbeats, new SingleAttackIntent(WingbeatsDamage), new DebuffIntent());
    }

    private MoveState GetGluttonyState()
    {
        return new MoveState(GLUTTONY, Gluttony, new SingleAttackIntent(GluttonyDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetWingbeatsState();
        var state2 = GetGluttonyState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        List<string> possibilities = new List<string>();
        if (!LastTwoMoves(stateMachine, WINGBEATS)) {
            possibilities.Add(WINGBEATS);
        }
        if (!LastTwoMoves(stateMachine, GLUTTONY)) {
            possibilities.Add(GLUTTONY);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Wingbeats(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(WingbeatsDamage)
            .FromMonster(this)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTick<Bleed>(targets, DebuffAmt);
        await ResetIdle();
    }
    
    private async Task Gluttony(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(GluttonyDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Bite", Sfx.FairyMinionAtk, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Bite"], controller);
    }
}