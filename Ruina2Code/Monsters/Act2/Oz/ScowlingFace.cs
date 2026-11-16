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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act2.Oz;

public sealed class ScowlingFace : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 48, 44) - (AscensionHelper.IsDread() ? 10 : 0);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 48) - (AscensionHelper.IsDread() ? 10 : 0);
    
    private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int BlowUpDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private int BlockAmt => 9;
    private int StatusAmt => 1;

    protected override string VisualsPath => "ScowlingFace/scowling_face.tscn".MonsterImagePath();

    private const string ATTACK = "ATTACK";
    private const string DEBUFF = "DEBUFF";
    private const string BLOW_UP = "BLOW_UP";

    public int MinionNum = 0;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
    }

    private MoveState GetAttackState()
    {
        return new MoveState(ATTACK, Attack, new SingleAttackIntent(AttackDamage));
    }
    
    private MoveState GetDebuffState()
    {
        return new MoveState(DEBUFF, Debuff, new DefendIntent(), new StatusIntent(StatusAmt));
    }
    
    private MoveState GetBlowUpState()
    {
        return new MoveState(BLOW_UP, BlowUp, new DeathBlowIntent(() => BlowUpDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetAttackState();
        var state2 = GetDebuffState();
        var state3 = GetBlowUpState();
        
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
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (stateMachine.StateLog.Count >= 2 && AscensionHelper.IsDread())
        {
            return BLOW_UP;
        }
        if (MinionNum == 1)
        {
            if (LastMove(stateMachine, ATTACK))
            {
                return DEBUFF;
            }
            else
            {
                return ATTACK;
            }
        }
        else
        {
            if (LastMove(stateMachine, DEBUFF))
            {
                return ATTACK;
            }
            else
            {
                return DEBUFF;
            }
        }
    }
    
    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(AttackDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await DebuffAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await CardPileCmd.AddToCombatAndPreview<Burn>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task BlowUp(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        NCombatRoom? instance = NCombatRoom.Instance;
        if (instance != null)
        {
            instance.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(Creature)); 
        }
        await Cmd.CustomScaledWait(0.2f, 0.3f);
        await DamageCmd.Attack(BlowUpDamage)
            .FromMonster(this)
            .Execute(null);
        await CreatureCmd.Kill(Creature);
        await WaitAnimation();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.SmokeAtk, targets, 0.7f);
    }
    
    private async Task DebuffAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.MatchSizzle, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt"], controller);
    }
}