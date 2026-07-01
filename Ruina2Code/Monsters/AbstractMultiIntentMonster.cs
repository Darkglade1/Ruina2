using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Ruina2.Ruina2Code.Intents;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractMultiIntentMonster : AbstractRuinaMonster
{
    public List<MonsterMoveStateMachine>? MultiIntentMoveStateMachines;
    public abstract List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine();
    public List<MoveState> NextMoves = new();
    public List<Creature> Targets = new();
    public bool ShouldClearBlockAtStartOfOwnTurn = true;
    public virtual int NumIntents { get; set; }
    public Creature? OtherSideTargetMonster { get; set; }
    public virtual string? TargetTexturePath { get; set; }
    
    // MultiIntent monsters use their own state machines so we can properly do targeting of other monsters with any intent
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState emptyState = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        emptyState.FollowUpState = emptyState;
        MoveState initialState = emptyState;
        initialState.FollowUpState = initialState;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { initialState },
            initialState
        );
    }
    
    public async Task PerformMultiIntentMove()
    {
        for (int i = 0; i < NumIntents; i++)
        {
            await Cmd.CustomScaledWait(0.1f, 0.2f);
            IsPerformingMove = true;
            MoveState move = NextMoves[i];
            IReadOnlyList<Creature> targets;
            Creature target = Targets[i];
            if (target.IsPlayer)
            {
                targets = CombatState.PlayerCreatures;
            }
            else
            {
                targets = [target];
            }
            Log.Info($"Monster {Id.Entry} performing move {move.Id}");
            await move.PerformMove(targets);
            MultiIntentMoveStateMachines?[i].OnMovePerformed(move);
            CombatManager.Instance.History.MonsterPerformedMove(CombatState, this, move, targets);
            IsPerformingMove = false;
            if (Creature.IsDead && Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(CombatState, Creature))
            {
                CombatState.RemoveCreature(Creature);
            }
            await Cmd.CustomScaledWait(0.25f, 0.4f);
        }
    }

    public abstract Creature DetermineTargetForIntent(int intentNum);

    protected Creature? FindTarget<T>() where T : MonsterModel
    {
        foreach (var enemy in CombatState.Enemies)
        {
            if (enemy.Monster is T)
            {
                return enemy;
            }
        }
        return null;
    }

    public override bool ShouldClearBlock(Creature creature)
    {
        if (creature == Creature)
        {
            return false;
        }
        return true;
    }

    protected async Task WaitAnimation()
    {
        await Cmd.Wait(0.5f);
    }
    
    protected async Task WaitAnimation(float waitTime)
    {
        await Cmd.Wait(waitTime);
    }
    
    protected virtual async Task ResetIdle()
    {
        await WaitAnimation();
        await CreatureCmd.TriggerAnim(Creature, "Idle", 0);
    }
    
    protected virtual async Task ResetIdle(float waitTime)
    {
        await WaitAnimation(waitTime);
        await CreatureCmd.TriggerAnim(Creature, "Idle", 0);
    }
}

