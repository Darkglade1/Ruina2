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

public sealed class QueenOfHate : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 111, 101);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 119, 108);
    
    private int ArcanaDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);
    private int HateDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 21, 19);
    private int StatusAmt => 2;
    private int VulnAmt => 3;

    protected override string VisualsPath => "QueenOfHatred/hatred.tscn".MonsterImagePath();

    private const string ARCANA_BEATS = "ARCANA_BEATS";
    private const string NAME_OF_HATE = "NAME_OF_HATE";
    private const string LIGHT_OF_HATRED = "LIGHT_OF_HATRED";
    
    public bool hysteriaTriggered = false;
    public bool hysteriaJustTriggered = false;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Hysteria>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
    }

    private MoveState GetArcanaState()
    {
        return new MoveState(ARCANA_BEATS, ArcanaBeats, new SingleAttackIntent(ArcanaDamage), new DebuffIntent());
    }

    private MoveState GetNameOfHateState()
    {
        return new MoveState(NAME_OF_HATE, NameOfHate, new SingleAttackIntent(HateDamage));
    }

    private MoveState GeLightOfHatredState()
    {
        return new MoveState(LIGHT_OF_HATRED, LightOfHatred, new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetArcanaState();
        var state2 = GetNameOfHateState();
        var state3 = GeLightOfHatredState();
        
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
        if (hysteriaJustTriggered)
        {
            return LIGHT_OF_HATRED;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!LastTwoMoves(stateMachine, ARCANA_BEATS)) {
                possibilities.Add(ARCANA_BEATS);
            }
            if (!LastTwoMoves(stateMachine, NAME_OF_HATE)) {
                possibilities.Add(NAME_OF_HATE);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }
    
    private async Task ArcanaBeats(IReadOnlyList<Creature> targets)
    {
        await BiteAnimation(targets);
        await DamageCmd.Attack(ArcanaDamage)
            .FromMonster(this)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Wound>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task NameOfHate(IReadOnlyList<Creature> targets)
    {
        await ShootAnimation(targets);
        await DamageCmd.Attack(HateDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task LightOfHatred(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, VulnAmt, Creature,  null);
        hysteriaJustTriggered = false;
    }
    
    private async Task BiteAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Bite", Sfx.MagicSnakeAtk, targets);
    }
    
    private async Task ShootAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Shoot", Sfx.MagicSnakeGun, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Bite", "Shoot"], controller);
    }
}