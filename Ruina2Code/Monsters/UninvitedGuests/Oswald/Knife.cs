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
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

public class Knife : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 220, 200);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 230, 210);
    public override int NumIntents => 1;

    private int KnifeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int KnifeHits => 2;
    private int StrAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    protected override string VisualsPath => "Knife/knife.tscn".MonsterImagePath();

    private const string KNIFE_DANCE = "KNIFE_DANCE";
    private const string KNIFE_TOSS = "KNIFE_TOSS";

    private bool attackingAlly;
    public Creature? mermaid;
    public Creature? oswald;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Tiph>();
        mermaid = FindHittableTarget<Mermaid>();
        oswald = FindHittableTarget<Oswald>();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        await PowerCmd.Apply<Spotlight>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        attackingAlly = true;
    }

    private MoveState GetKnifeTossState()
    {
        return new MoveState(KNIFE_TOSS, KnifeToss, new RuinaMultiAttackIntent(KnifeDamage, KnifeHits));
    }
    
    private MoveState GetKnifeDanceState()
    {
        return new MoveState(KNIFE_DANCE, KnifeDance, new BuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetKnifeTossState();
        var state2 = GetKnifeDanceState();
        
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
        if ((LastMove(stateMachine, KNIFE_TOSS) && LastMoveBefore(stateMachine, KNIFE_TOSS)) ||
            CombatState.RoundNumber == 1)
        {
            return KNIFE_DANCE;
        }
        else
        {
            return KNIFE_TOSS;
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive && attackingAlly)
        {
            return OtherSideTargetMonster;
        }
        return GetPlayerTarget();
    }

    private async Task KnifeToss(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < KnifeHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(KnifeDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        attackingAlly = !attackingAlly;
    }
    
    private async Task KnifeDance(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature,  null);
        await ResetIdle();
    }

    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.SwordHori, targets);
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SwordVert, targets);
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