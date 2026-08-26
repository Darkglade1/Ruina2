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
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.PunshingBird;

public sealed class PunishingBird : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 165, 150);
    public override int MaxInitialHp => MinInitialHp;
    
    private int PeckDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int PeckHits => 3;
    private int PunishmentDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 50, 45);
    private int StrengthAmount => 1;

    protected override string VisualsPath => "PunishingBird/punishing_bird.tscn".MonsterImagePath();

    private const string PECK = "PECK";
    private const string PUNISHMENT = "PUNISHMENT";

    public bool Enraged = false;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Punishment>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
    }

    private MoveState GetPeckState()
    {
        return new MoveState(PECK, Peck, new MultiAttackIntent(PeckDamage, PeckHits), new BuffIntent());
    }

    private MoveState GetPunishmentState()
    {
        return new MoveState(PUNISHMENT, Punishment, new SingleAttackIntent(PunishmentDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPeckState();
        var state2 = GetPunishmentState();
        
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
        if (Enraged)
        {
            Enraged = false;
            return PUNISHMENT;
        }
        else
        {
            return PECK;
        }
    }
    
    private async Task Peck(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < PeckHits; i++)
        {
            await PeckAnimation(targets);
            await DamageCmd.Attack(PeckDamage)
                .FromMonster(this)
                .Execute(null);
            await RecoilAnimation(targets);
            await WaitAnimation(0.25f);
        }
        await ResetIdle(0.0f);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
    }
    
    private async Task Punishment(IReadOnlyList<Creature> targets)
    {
        await PunishAnimation(targets);
        await DamageCmd.Attack(PunishmentDamage)
            .FromMonster(this)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task PeckAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Peck", Sfx.SmallBirdPeck, targets);
    }
    
    private async Task RecoilAnimation(IReadOnlyList<Creature> targets)
    {
        await WaitAnimation(0.25f);
        await AnimationAction("Recoil", null, targets);
    }
    
    private async Task PunishAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Punish", Sfx.SmallBirdPunish, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Peck", "Punish", "Recoil"], controller);
    }
}