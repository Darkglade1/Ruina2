using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers.Act1;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace Ruina2.Ruina2Code.Monsters.Act1;

public sealed class Orchestra : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 250, 230);
    public override int MaxInitialHp => MinInitialHp;
    
    private int FirstDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int SecondDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int ThirdDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int FinaleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 28, 25);
    private int WeakAmt => 1;
    private int FerventDmg => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    private int PlayerDrawAmt => 2;
    private int StatusAmt => 1;
    private int BlockAmt => 8;
    private int HealAmt => 10;

    protected override string VisualsPath => "Orchestra/orchestra.tscn".MonsterImagePath();

    private const string FIRST = "FIRST";
    private const string SECOND = "SECOND";
    private const string THIRD = "THIRD";
    private const string FOURTH = "FOURTH";
    private const string FINALE = "FINALE";
    private const string CURTAINS = "CURTAINS";
    
    private OrchestraMusicEffect? movement1;
    private OrchestraMusicEffect? movement2;
    private OrchestraMusicEffect? movement3;
    private OrchestraMusicEffect? movement4;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<EndlessPerformance>(new ThrowingPlayerChoiceContext(), Creature, 4, Creature, null);
    }

    private MoveState GetFirstState()
    {
        return new MoveState(FIRST, First, new SingleAttackIntent(FirstDamage), new DebuffIntent());
    }

    private MoveState GetSecondState()
    {
        return new MoveState(SECOND, Second, new SingleAttackIntent(SecondDamage), new DebuffIntent());
    }

    private MoveState GetThirdStateState()
    {
        return new MoveState(THIRD, Third, new SingleAttackIntent(ThirdDamage), new BuffIntent());
    }
    
    private MoveState GetFourthState()
    {
        return new MoveState(FOURTH, Fourth, new StatusIntent(StatusAmt));
    }
    
    private MoveState GetFinaleState()
    {
        return new MoveState(FINALE, Finale, new SingleAttackIntent(FinaleDamage), new DebuffIntent());
    }
    
    private MoveState GetCurtainsState()
    {
        return new MoveState(CURTAINS, Curtains, new DefendIntent(), new HealIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetFirstState();
        var state2 = GetSecondState();
        var state3 = GetThirdStateState();
        var state4 = GetFourthState();
        var state5 = GetFinaleState();
        var state6 = GetCurtainsState();
        
        state1.FollowUpState = state2;
        state2.FollowUpState = state3;
        state3.FollowUpState = state4;
        state4.FollowUpState = state5;
        state5.FollowUpState = state6;
        state6.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(state5);
        states.Add(state6);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task First(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (targetNode != null)
        {
            movement1 = OrchestraMusicEffect.Create(targetNode.VfxSpawnPosition, "1st.png".VfxImagePath(), false, 1.15f);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(movement1);
        }
        await DamageCmd.Attack(FirstDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, WeakAmt, Creature, null);
        await ResetIdle(1.0f);
    }
    
    private async Task Second(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation2(targets);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (targetNode != null)
        {
            movement2 = OrchestraMusicEffect.Create(targetNode.VfxSpawnPosition, "2nd.png".VfxImagePath(), true, 1.55f);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(movement2);
        }
        await DamageCmd.Attack(SecondDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<FerventAdoration>(new ThrowingPlayerChoiceContext(), targets, FerventDmg, Creature, null);
        await ResetIdle(1.0f);
    }
    
    private async Task Third(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation(targets);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (targetNode != null)
        {
            movement3 = OrchestraMusicEffect.Create(targetNode.VfxSpawnPosition, "3rd.png".VfxImagePath(), false, 2.05f);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(movement3);
        }
        await DamageCmd.Attack(ThirdDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(new ThrowingPlayerChoiceContext(), targets, PlayerDrawAmt, Creature, null);
        await ResetIdle(1.0f);
    }
    
    private async Task Fourth(IReadOnlyList<Creature> targets)
    {
        await AttackAnimation2(targets);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (targetNode != null)
        {
            movement4 = OrchestraMusicEffect.Create(targetNode.VfxSpawnPosition, "3rd.png".VfxImagePath(), true, 2.55f);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(movement4);
        }
        await CardPileCmd.AddToCombatAndPreview<Void>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task Finale(IReadOnlyList<Creature> targets)
    {
        await FinaleAnimation(targets);
        if (movement1 != null)
        {
            movement1.End();
        }
        if (movement2 != null)
        {
            movement2.End();
        }
        if (movement3 != null)
        {
            movement3.End();
        }
        if (movement4 != null)
        {
            movement4.End();
        }
        await DamageCmd.Attack(FinaleDamage)
            .FromMonster(this)
            .Execute(null);
        await PowerCmd.Apply<FerventAdoration>(new ThrowingPlayerChoiceContext(), targets, FerventDmg, Creature, null);
        await ResetIdle(1.5f);
    }
    
    private async Task Curtains(IReadOnlyList<Creature> targets)
    {
        await CurtainAnimation(targets);
        var curtainEffect = OrchestraCurtainEffect.Create();
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(curtainEffect);
        await WaitAnimation(6.0f);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);   
        await CreatureCmd.Heal(Creature, Creature.ScaleHpForMultiplayer(HealAmt, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex));
        await ResetIdle();
    }
    
    private async Task AttackAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.OrchestraMovement1, targets);
    }
    
    private async Task AttackAnimation2(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.OrchestraMovement2, targets);
    }
    
    private async Task FinaleAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.OrchestraFinale, targets);
    }
    
    private async Task CurtainAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.OrchestraClap, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Special"], controller);
    }
}