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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.Act1.NothingDer;

public sealed class DerFreischutz : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 180);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int RuthlessDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int InevitableDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int MagicDamage => 13;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int BlockAmount => 10;
    private int WeakAmt => 1;
    private int VulnerableAmt => 1;

    protected override string VisualsPath => "DerFreischutz/der_frei.tscn".MonsterImagePath();
    public override string TargetTexturePath => "GunIcon.png".UIImagePath();

    private const string RUTHLESS_BULLETS = "RUTHLESS_BULLETS";
    private const string INEVITABLE_BULLET = "INEVITABLE_BULLET";
    private const string MAGIC_BULLET  = "MAGIC_BULLET";
    private const string SILENT_SCOPE = "SILENT_SCOPE";
    private const string DEATH_MARK = "DEATH_MARK";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<NothingThere>();
    }

    private MoveState GetRuthlessBulletsState()
    {
        return new MoveState(RUTHLESS_BULLETS, RuthlessBullets, new RuinaSingleMassAttackIntent(RuthlessDamage));
    }

    private MoveState GetInevitableBulletState()
    {
        return new MoveState(INEVITABLE_BULLET, InevitableBullet, new RuinaSingleAttackIntent(InevitableDamage));
    }

    private MoveState GetMagicBulletState()
    {
        return new MoveState(MAGIC_BULLET, MagicBullet, new RuinaSingleAttackIntent(MagicDamage));
    }

    private MoveState GetSilentScopeState()
    {
        return new MoveState(SILENT_SCOPE, SilentScope, new DefendIntent(), new RuinaDebuffIntent());
    }
    
    private MoveState GetDeathMarkState()
    {
        return new MoveState(DEATH_MARK, DeathMark, new RuinaDebuffIntent(), new BuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetRuthlessBulletsState();
        var state2 = GetInevitableBulletState();
        var state3 = GetSilentScopeState();
        
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

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetMagicBulletState();
        var state2 = GetDeathMarkState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 1);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (intentNum == 0)
        {
            if (stateMachine.StateLog.Count >= 2 && !LastMove(stateMachine, RUTHLESS_BULLETS) && !LastMoveBefore(stateMachine, RUTHLESS_BULLETS))
            {
                return RUTHLESS_BULLETS;
            }
            else
            {
                List<string> possibilities = new List<string>();
                if (!LastTwoMoves(stateMachine, INEVITABLE_BULLET)) {
                    possibilities.Add(INEVITABLE_BULLET);
                }
                if (!LastMove(stateMachine, SILENT_SCOPE)) {
                    possibilities.Add(SILENT_SCOPE);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
            }
        }
        else
        {
            if (stateMachine.StateLog.Count == 1 ||
                (LastMove(stateMachine, MAGIC_BULLET) && LastMoveBefore(stateMachine, MAGIC_BULLET)))
            {
                return DEATH_MARK;
            }
            else
            {
                return MAGIC_BULLET;
            }
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            return CombatState.PlayerCreatures[0];
        }
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task RuthlessBullets(IReadOnlyList<Creature> targets)
    {
        IsMassAttacking = true;
        await MassAttackAnimation(targets);
        await WaitAnimation(1.0f);
        await DamageCmd.Attack(RuthlessDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task InevitableBullet(IReadOnlyList<Creature> targets)
    {
        if (CombatState.RoundNumber == 1)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-DER_FREISCHUTZ.greeting"), Creature, VfxColor.Blue);
        }
        await AttackAnimation(targets);
        await DamageCmd.Attack(InevitableDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(1.0f);
    }
    
    private async Task MagicBullet(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(MagicDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(1.0f);
    }

    private async Task SilentScope(IReadOnlyList<Creature> targets)
    {
        if (CombatState.RoundNumber == 1)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-DER_FREISCHUTZ.greeting"), Creature, VfxColor.Blue);
        }
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmount, ValueProp.Move, null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, WeakAmt, Creature, null);
        await ResetIdle(1.0f);
    }
    
    private async Task DeathMark(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<VulnerablePower>(targets, VulnerableAmt);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
        await ResetIdle(1.0f);
    }

    public async Task OnNothingThereDeath()
    {
        TalkCmd.Play(L10NMonsterLookup("RUINA2-DER_FREISCHUTZ.goodbye"), Creature, VfxColor.Blue);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is NothingThere nothingThere)
        {
            if (nothingThere.Creature.IsAlive)
            {
                await nothingThere.OnDerFreiDeath();
            }
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

    private async Task MassAttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.BulletFinalShot, targets);
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.BulletShot, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Dodge", null);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Block", Sfx.BulletFlame);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Dodge", "Block", "Special"], controller);
    }
}