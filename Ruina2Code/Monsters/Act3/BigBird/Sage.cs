using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;

namespace Ruina2.Ruina2Code.Monsters.Act3.BigBird;

public class Sage : AbstractAllyMonster
{
    public override int MinInitialHp => 500;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "SageIcon.png".UIImagePath();

    private int SmackDamage => 6;
    private int RitualAmt => 1;

    protected override string VisualsPath => "Keeper/keeper.tscn".MonsterImagePath();

    private const string RING = "RING";
    private const string SMACK = "SMACK";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<BigBird>();
    }
    
    private MoveState GetRingState()
    {
        return new MoveState(RING, Ring, new RuinaBuffIntent());
    }

    private MoveState GetSmackState()
    {
        return new MoveState(SMACK, Smack, new RuinaSingleAttackIntent(SmackDamage));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetRingState();
        var state2 = GetSmackState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state2;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
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

    private async Task Ring(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation();
        await PowerCmd.Apply<RitualPower>(new ThrowingPlayerChoiceContext(), Creature, RitualAmt, Creature, null);
        await ResetIdle();
    }
    
    private async Task Smack(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        await DamageCmd.Attack(SmackDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
        await ResetIdle();
    }
    
    public virtual async Task OnBigBirdDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-SAGE.birdDeath1"), Creature, VfxColor.Black);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }

    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Smack", Sfx.BluntBlow, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Ring", Sfx.BossBirdSpecial);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Ring", "Smack"], controller);
    }
}