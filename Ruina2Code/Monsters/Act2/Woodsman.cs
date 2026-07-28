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
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2;

public sealed class Woodsman : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 154, 140);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 163, 148);
    
    private int StrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int StrikeHits => 2;
    private int LumberDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);
    private int LumberHits => 4;
    private int PulseDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    
    private int EnergyGain => 2;
    private int DebuffAmt => 2;
    private int StatusAmt => 1;

    protected override string VisualsPath => "Woodsman/woodsman.tscn".MonsterImagePath();

    private const string STRIKE = "STRIKE";
    private const string LUMBER = "LUMBER";
    private const string PULSE = "PULSE";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<WarmHeart>(new ThrowingPlayerChoiceContext(), Creature, EnergyGain, Creature, null);
    }

    private MoveState GetStrikeState()
    {
        return new MoveState(STRIKE, Strike, new MultiAttackIntent(StrikeDamage, StrikeHits), new StatusIntent(StatusAmt));
    }

    private MoveState GetLumberState()
    {
        return new MoveState(LUMBER, Lumber, new MultiAttackIntent(LumberDamage, LumberHits));
    }

    private MoveState GetPulseState()
    {
        return new MoveState(PULSE, Pulse, new SingleAttackIntent(PulseDamage), new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetStrikeState();
        var state2 = GetLumberState();
        var state3 = GetPulseState();
        
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
        List<string> possibilities = new List<string>();
        if (!LastMove(stateMachine, STRIKE)) {
            possibilities.Add(STRIKE);
        }
        if (!LastMove(stateMachine, LUMBER)) {
            possibilities.Add(LUMBER);
        }
        if (!LastMove(stateMachine, PULSE) && !LastMoveBefore(stateMachine, PULSE)) {
            possibilities.Add(PULSE);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }
    
    private async Task Strike(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < StrikeHits; i++)
        {
            if (i % 2 == 0) {
                await StrikeAnimation(targets);
            } else {
                await SlashAnimation(targets);
            } 
            await DamageCmd.Attack(StrikeDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
        await CardPileCmd.AddToCombatAndPreview<Wound>(CombatState.PlayerCreatures, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
    }
    
    private async Task Lumber(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < LumberHits; i++)
        {
            if (i == LumberHits - 1) {
                await FinishAnimation(targets);
            } else if (i % 2 == 0) {
                await SlashAnimation(targets);
            } else {
                await StrikeAnimation(targets);
            }
            await DamageCmd.Attack(LumberDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Pulse(IReadOnlyList<Creature> targets)
    {
        await StrikeAnimation(targets);
        await DamageCmd.Attack(PulseDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.WoodStrike, targets);
    }
    
    private async Task StrikeAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Strike", Sfx.WoodFinish, targets);
    }
    
    private async Task FinishAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Finish", Sfx.WoodFinish, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Strike", "Finish"], controller);
    }
}