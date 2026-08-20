using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act3.Apostles;

public sealed class ScytheApostle : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 58, 53);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 63, 57);
    
    private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int AttackHits => 2;
    private int BlockAmt => 14;

    protected override string VisualsPath => "ScytheApostle/scythe_apostle.tscn".MonsterImagePath();

    private const string ATTACK = "ATTACK";
    private const string DEFEND = "DEFEND";

    private MoveState GetAttackState()
    {
        return new MoveState(ATTACK, Attack, new MultiAttackIntent(AttackDamage, AttackHits));
    }

    private MoveState GetDefendState()
    {
        return new MoveState(DEFEND, Defend, new DefendIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetAttackState();
        var state2 = GetDefendState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state2);
    }
    
    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < AttackHits; i++)
        {
            if (i % 2 == 0)
            {
                await AttackAnimation1(targets);
            }
            else
            {
                await AttackAnimation2(targets);
            }
            await DamageCmd.Attack(AttackDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Defend(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await ResetIdle();
    }
    
    private async Task AttackAnimation1(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SlashUp", Sfx.ApostleScytheUp, targets);
    }
    
    private async Task AttackAnimation2(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("SlashDown", Sfx.ApostleScytheDown, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "SlashUp", "SlashDown", "Block"], controller);
    }
}