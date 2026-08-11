using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.Act2.Jester;

public sealed class ServantOfCourage : AbstractAllyMonster
{
    public override int MinInitialHp => 140;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "CourageIcon.png".UIImagePath();

    private int HelpDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 7);
    private int HelpHits = 2;
    private int DebuffAmt => 4;

    protected override string VisualsPath => "ServantOfCourage/courage.tscn".MonsterImagePath();

    private const string HELP = "HELP";
    private const string PROTECT_FRIEND = "PROTECT_FRIEND";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<JesterOfNihil>();
    }

    private MoveState GetHelpState()
    {
        return new MoveState(HELP, Help, new RuinaMultiAttackIntent(HelpDamage, HelpHits));
    }

    private MoveState GetProtectFriendState()
    {
        return new MoveState(PROTECT_FRIEND, ProtectFriend, new RuinaDebuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetHelpState();
        var state2 = GetProtectFriendState();
        
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
        if (Creature.CurrentHp <= Creature.MaxHp * 0.3f)
        {
            return HELP;
        }
        else
        {
            if (LastMove(stateMachine, HELP))
            {
                return PROTECT_FRIEND;
            }
            else
            {
                return HELP;
            }
        }
    }


    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task Help(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < HelpHits; i++)
        {
            if (i % 2 == 0)
            {
                await Attack1Animation(targets);
            } else {
                await Attack2Animation(targets);
            }
            await DamageCmd.Attack(HelpDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task ProtectFriend(IReadOnlyList<Creature> targets)
    {
        await DebuffAnimation(targets);
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<Erosion>(targets, DebuffAmt);
        await ResetIdle();
    }
    
    public async Task OnJesterDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-COURAGE.victory"), Creature, VfxColor.Green);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }
    
    public override Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-COURAGE.death"), Creature, VfxColor.Green);
        }
        return Task.CompletedTask;
    }

    private async Task Attack1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.WoodStrike, targets);
    }
    
    private async Task Attack2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.WoodFinish, targets);
    }
    
    private async Task DebuffAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.GreedGetPower, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Slash", "Special"], controller);
    }
}