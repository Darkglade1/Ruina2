using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;

namespace Ruina2.Ruina2Code.Monsters.Act2.Wrath;

public sealed class HermitStaff : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 48, 44);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 48);
    public override int NumIntents => 1;

    private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

    protected override string VisualsPath => "Staff/staff.tscn".MonsterImagePath();

    private const string ATTACK = "ATTACK";
    private Creature? hermit;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<ServantOfWrath>();
        hermit = FindTarget<Hermit>();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
    }

    private MoveState GetAttackState()
    {
        return new MoveState(ATTACK, Attack, new RuinaSingleAttackIntent(AttackDamage));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetAttackState();

        state1.FollowUpState = state1;

        states.Add(state1);
        
        return new MonsterMoveStateMachine(states, state1);
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(AttackDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }
    
    public override async Task BeforeDeath(Creature creature)
    {
        await base.BeforeDeath(creature);
        if (creature != Creature)
            return;
    
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.Monster is ServantOfWrath wrath && hermit != null && !wrath.enraged)
        {
            wrath.Targets = [hermit];
        }
    }

    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Attack", Sfx.BluntBlow, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Attack"], controller);
    }
}