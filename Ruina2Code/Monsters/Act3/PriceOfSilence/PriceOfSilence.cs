using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.PriceOfSilence;

public sealed class PriceOfSilence : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 90, 82);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 97, 88);
    
    private int SilentDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int StatusAmt => 3;
    private int CardPlayThreshold => 6;

    protected override string VisualsPath => "PriceOfSilence/price_of_silence.tscn".MonsterImagePath();

    private const string STOLEN_TIME = "STOLEN_TIME";
    private const string SILENT_HOUR = "SILENT_HOUR";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<TickingTime>(new ThrowingPlayerChoiceContext(), Creature, CombatState.Players.Count * CardPlayThreshold, Creature,  null);
    }

    private MoveState GetStolenTimeState()
    {
        return new MoveState(STOLEN_TIME, StolenTime, new StatusIntent(StatusAmt));
    }

    private MoveState GetSilentHourState()
    {
        return new MoveState(SILENT_HOUR, SilentHour, new SingleAttackIntent(SilentDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetStolenTimeState();
        var state2 = GetSilentHourState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task StolenTime(IReadOnlyList<Creature> targets)
    {
        await EffectAnimation(targets);
        await CardPileCmd.AddToCombatAndPreview<StolenTime>(targets, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
        await ResetIdle(1.0f);
    }
    
    private async Task SilentHour(IReadOnlyList<Creature> targets)
    {
        await EffectAnimation(targets);
        await DamageCmd.Attack(SilentDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task EffectAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Flash", Sfx.SilenceEffect, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Flash"], controller);
    }
}