using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Hokma;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.EGO;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;
using Time = Ruina2.Ruina2Code.Cards.EnemyCards.Hokma.Time;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Pluto;

public sealed class Hokma : AbstractAllyCardMonster
{
    public override int MinInitialHp => 160;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "HokmaIcon.png".UIImagePath();
    private bool talked = false;

    private int SilenceDamage => 10;
    private int SilenceDamageIncrease => 10;
    private int CurrentDamageIncrease = 0;
    public decimal SilenceTotalDamage => SilenceDamage + CurrentDamageIncrease;
    private int CardsPerTurn => 6;
    protected override string VisualsPath => "Hokma/hokma.tscn".MonsterImagePath();

    private const string SILENCE = "SILENCE";
    private const string TIME = "TIME";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Pluto>();
        if (NCombatRoom.Instance != null)
        {
            NCreature? creatureNode = NCombatRoom.Instance.GetCreatureNode(Creature);
            creatureNode?.MoveChildSafely(creatureNode?.Visuals, 0);
            Marker2D? specialNode = creatureNode?.GetSpecialNode<Marker2D>("%IntentPos");
            if (specialNode != null)
            {
                specialNode.Position += new Vector2(0.0f, -75.0f);
            }
        }
        await PowerCmd.Apply<PriceOfTime>(new ThrowingPlayerChoiceContext(), Creature, CardsPerTurn * CombatState.PlayerCreatures.Count, Creature,  null);
    }

    private MoveState GetSilenceState()
    {
        return new MoveState(SILENCE, Silence, new RuinaSingleAttackIntent((Func<decimal>) (() => SilenceTotalDamage)));
    }

    private MoveState GetTimeState()
    {
        return new MoveState(TIME, Time, new RuinaBuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetSilenceState();
        var state2 = GetTimeState();
        
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
        if (LastMove(stateMachine, SILENCE) && LastMoveBefore(stateMachine, SILENCE))
        {
            return TIME;
        }
        else
        {
            return SILENCE;
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
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Silence>();
        card1.SetDamage(SilenceDamage);
        card1.DynamicVars["Increase"].BaseValue = SilenceDamageIncrease;
        var card2 = CreateCardForIntent<Time>();
        return new Dictionary<string, CardModel>()
        {
            {SILENCE, card1},
            {TIME, card2}
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-HOKMA.response"), Creature, VfxColor.DarkGray);
            talked = true;
        }
    }

    private async Task Silence(IReadOnlyList<Creature> targets)
    {
        Talk();
        await SlashAnimation(targets);
        await DamageCmd.Attack(SilenceTotalDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
        CurrentDamageIncrease += SilenceDamageIncrease;
    }
    
    private async Task Time(IReadOnlyList<Creature> targets)
    {
        Sfx.SilenceStop.Play(1.0f, 0.9f);
        await PowerCmd.Apply<DeadSilencePower>(new ThrowingPlayerChoiceContext(),  CombatState.PlayerCreatures,1, Creature, null);
        foreach (var player in CombatState.Players)
        {
            PlayerCmd.EndTurn(player, false);
        }
    }
    
    public async Task OnBossDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-HOKMA.victory"), Creature, VfxColor.DarkGray);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SilenceEffect, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash"], controller);
    }
}