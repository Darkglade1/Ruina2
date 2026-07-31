using Godot;
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
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.Wrath;

public sealed class ServantOfWrath : AbstractAllyMonster
{
    public override int MinInitialHp => 300;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "WrathIcon.png".UIImagePath();
    private bool talked = false;

    private int EvilDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int EvilHits = 3;
    private int RageDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 8);
    private int RageHits => 2;
    private int DebuffAmt => 3;
    private int BlindFuryThreshold => 20;
    private int DamageIncrease = 2;
    private int CurrentDamageIncrease = 0;
    public decimal EvilTotalDamage => EvilDamage + CurrentDamageIncrease;
    public bool enraged;
    
    public MoveState? _evilState;
    public MoveState? EvilState
    {
        get => _evilState;
        set
        {
            AssertMutable();
            _evilState = value;
        }
    }

    protected override string VisualsPath => "ServantOfWrath/wrath.tscn".MonsterImagePath();

    private const string RAGE = "RAGE";
    private const string EMBODIMENTS_OF_EVIL = "EMBODIMENTS_OF_EVIL";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        MassAttackHitsPlayer = true;
        Sfx.WrathMeet.Play();
        OtherSideTargetMonster = FindTarget<Hermit>();
        SetPosition(new Vector2(0, 200));
        await PowerCmd.Apply<BlindFury>(new ThrowingPlayerChoiceContext(), Creature, BlindFuryThreshold, Creature, null);
    }

    private MoveState GetRageState()
    {
        return new MoveState(RAGE, Rage, new RuinaMultiAttackIntent(RageDamage, RageHits), new RuinaDebuffIntent());
    }

    private MoveState GetEmbodimentsOfEvilState()
    {
        return new MoveState(EMBODIMENTS_OF_EVIL, EmbodimentsOfEvil, new RuinaMultiMassAttackIntent((Func<decimal>) (() => EvilTotalDamage), EvilHits));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetRageState();
        EvilState = GetEmbodimentsOfEvilState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        EvilState.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(EvilState);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (enraged)
        {
            return EMBODIMENTS_OF_EVIL;
        }
        else
        {
            return RAGE;
        }
    }


    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (enraged)
        {
            return CombatState.PlayerCreatures[0];
        }  
        if (OtherSideTargetMonster != null)
        {
            if (OtherSideTargetMonster.Monster is Hermit hermit && hermit.staff != null && hermit.staff.IsAlive)
            {
                return hermit.staff;
            }
            else
            {
                return OtherSideTargetMonster;
            }
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task Rage(IReadOnlyList<Creature> targets)
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-WRATH.combatStart"), Creature, VfxColor.Green);
            talked = true;
        }
        for (int i = 0; i < RageHits; i++)
        {
            if (i % 2 == 0)
            {
                await Attack1Animation(targets);
            } else {
                await Attack2Animation(targets);
            }
            await DamageCmd.Attack(RageDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        await ApplyPowerAndSkipNextDurationTick<Erosion>(targets, DebuffAmt);
    }
    
    private async Task EmbodimentsOfEvil(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < EvilHits; i++)
        {
            IsMassAttacking = true;
            if (i == 0) {
                await BigAttack1Animation(targets);
            } else if (i == 1){
                await BigAttack2Animation(targets);
            } else {
                await BigAttack3Animation(targets);
            }
            await DamageCmd.Attack(EvilTotalDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
            await ResetIdle(0.9f);
            await WaitAnimation(0.1f);
        }
        CurrentDamageIncrease += DamageIncrease;
        enraged = false;
    }

    public void Enrage()
    {
        Sfx.WrathMeet.Play();
        enraged = true;
        if (EvilState != null)
        {
            SetMoveImmediateMultiIntentMonster(EvilState, 0);
        }
    }
    
    public override IReadOnlyList<Creature> AdditionalMassAttackTargets()
    {
        var newList = new List<Creature>();
        foreach (var hittableEnemy in CombatState.HittableEnemies)
        {
            if (hittableEnemy != Creature)
            {
                newList.Add(hittableEnemy);
            }
        }
        return newList;
    }
    
    public async Task OnHermitDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-WRATH.hermitDeath"), Creature, VfxColor.Green);
        await WaitAnimation(3.0f);
        await CreatureCmd.Kill(Creature);
    }

    private async Task Attack1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack1", Sfx.WrathAtk1, targets);
    }
    
    private async Task Attack2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack2", Sfx.WrathAtk2, targets);
    }
    
    private async Task BigAttack1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("BigAttack1", Sfx.WrathStrong1, targets);
    }
    
    private async Task BigAttack2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("BigAttack2", Sfx.WrathStrong2, targets);
    }
    
    private async Task BigAttack3Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("BigAttack3", Sfx.WrathStrong3, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack1", "Attack2", "BigAttack1", "BigAttack2", "BigAttack3"], controller);
    }
}