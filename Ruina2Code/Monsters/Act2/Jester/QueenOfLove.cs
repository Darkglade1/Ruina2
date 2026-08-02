using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.Jester;

public sealed class QueenOfLove : AbstractAllyMonster
{
    public override int MinInitialHp => 140;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "LoveIcon.png".UIImagePath();

    private int LoveDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);
    private int LoveHits = 3;
    private int StrAmt => 2;

    protected override string VisualsPath => "QueenOfLove/love.tscn".MonsterImagePath();

    private const string LOVE_AND_JUSTICE = "LOVE_AND_JUSTICE";
    private const string ARCANA_BEATS = "ARCANA_BEATS";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<JesterOfNihil>();
        await PowerCmd.Apply<Justice>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetLoveAndJustisceState()
    {
        return new MoveState(LOVE_AND_JUSTICE, LoveAndJustice, new RuinaMultiAttackIntent(LoveDamage, LoveHits));
    }

    private MoveState GetArcanaBeatsState()
    {
        return new MoveState(ARCANA_BEATS, ArcanaBeats, new RuinaBuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetLoveAndJustisceState();
        var state2 = GetArcanaBeatsState();
        
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
            return LOVE_AND_JUSTICE;
        }
        else
        {
            if (LastMove(stateMachine, LOVE_AND_JUSTICE))
            {
                return ARCANA_BEATS;
            }
            else
            {
                return LOVE_AND_JUSTICE;
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

    private async Task LoveAndJustice(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < LoveHits; i++)
        {
            await AttackAnimation(targets);
            await DamageCmd.Attack(LoveDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task ArcanaBeats(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature, null);
        await ResetIdle(1.0f);
    }
    
    public async Task OnJesterDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-WRATH.hermitDeath"), Creature, VfxColor.Green);
        await WaitAnimation(3.0f);
        await CreatureCmd.Kill(Creature);
    }

    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack", Sfx.MagicAttack, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", Sfx.MagicKiss);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack", "Special"], controller);
    }
}