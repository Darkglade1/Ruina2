using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class Funeral : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 63, 57);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 68, 62);
    
    private int LamentDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);
    private int LamentHits => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int GuidingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int BlockAmt => 5;
    private int DebuffAmt => 2;

    protected override string VisualsPath => "Funeral/funeral.tscn".MonsterImagePath();

    private const string LAMENT = "LAMENT";
    private const string GUIDING_HAND = "GUIDING_HAND";
    private const string THE_QUIET = "THE_QUIET";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        foreach (Creature target in CombatState.PlayerCreatures)
        {
            Lamentation mutable = (Lamentation) ModelDb.Power<Lamentation>().ToMutable();
            mutable.Target = target;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), mutable, Creature, 1, Creature, null);
        }
    }

    private MoveState GetLamentState()
    {
        return new MoveState(LAMENT, Lament, new MultiAttackIntent(LamentDamage, LamentHits));
    }

    private MoveState GetGuidingHandState()
    {
        return new MoveState(GUIDING_HAND, GuidingHand, new SingleAttackIntent(GuidingDamage), new DebuffIntent());
    }
    
    private MoveState GetTheQuietState()
    {
        return new MoveState(THE_QUIET, TheQuiet, new DefendIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLamentState();
        var state2 = GetGuidingHandState();
        var state3 = GetTheQuietState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state3;
        state3.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        
        return new MonsterMoveStateMachine(states, state3);
    }
    
    private async Task GuidingHand(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(GuidingDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Lament(IReadOnlyList<Creature> targets)
    {
        await LamentAnimation(targets);
        for (int i = 0; i < LamentHits; i++)
        {
            if (i % 2 == 0)
            {
                await SoundAnimation(Sfx.FuneralAtkBlack, targets);
            }
            else
            {
                await SoundAnimation(Sfx.FuneralAtkWhite, targets);
            }
            await DamageCmd.Attack(LamentDamage)
                .FromMonster(this)
                .Execute(null);
            await WaitAnimation(0.25f);
        }
        await ResetIdle();
    }
    
    private async Task TheQuiet(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await ResetIdle(1.0f);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.FuneralAtkBlack, targets);
    }
    
    private async Task LamentAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", null, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", Sfx.FuneralReady);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Special", "Block"], controller);
    }
}