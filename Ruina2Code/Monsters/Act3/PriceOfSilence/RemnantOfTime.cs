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
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Monsters.Act3.PriceOfSilence;

public sealed class RemnantOfTime : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 88, 80);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 95, 86);
    
    private int BacklashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int BacklashHits => 2;
    private int StrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int BlockAmt => 9;

    protected override string VisualsPath => "Remnant/remnant.tscn".MonsterImagePath();

    private const string BACKLASH_OF_TIME = "BACKLASH_OF_TIME";
    private const string TORRENT_OF_HOURS = "TORRENT_OF_HOURS";

    private MoveState GetBacklashOfTimeState()
    {
        return new MoveState(BACKLASH_OF_TIME, BacklashOfTime, new MultiAttackIntent(BacklashDamage, BacklashHits));
    }

    private MoveState GetTorrentOfHoursState()
    {
        return new MoveState(TORRENT_OF_HOURS, TorrentOfHours, new DefendIntent(), new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBacklashOfTimeState();
        var state2 = GetTorrentOfHoursState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task BacklashOfTime(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < BacklashHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(BacklashDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task TorrentOfHours(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await CreatureCmd.GainBlock(enemy, BlockAmt, ValueProp.Move, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, StrAmt, Creature,  null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SwordHori, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.SwordStab, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Blunt", "Block"], controller);
    }
}