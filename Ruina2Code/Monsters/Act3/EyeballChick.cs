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

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class EyeballChick : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 40);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 46);

    private int StareDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int PierceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int StrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int DebuffAmt => 1;

    protected override string VisualsPath => "EyeballChick/eyeball_chick.tscn".MonsterImagePath();

    private const string STARE = "STARE";
    private const string PIERCE = "PIERCE";

    private MoveState GetStareState()
    {
        return new MoveState(STARE, Stare, new SingleAttackIntent(StareDamage), new DebuffIntent());
    }

    private MoveState GetPierceState()
    {
        return new MoveState(PIERCE, Pierce, new SingleAttackIntent(PierceDamage), new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetStareState();
        var state2 = GetPierceState();
        
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
        if (!LastTwoMoves(stateMachine, STARE)) {
            possibilities.Add(STARE);
        }
        if (!LastTwoMoves(stateMachine, PIERCE)) {
            possibilities.Add(PIERCE);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Stare(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(StareDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Pierce(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(PierceDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.WoodFinish, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce"], controller);
    }
}