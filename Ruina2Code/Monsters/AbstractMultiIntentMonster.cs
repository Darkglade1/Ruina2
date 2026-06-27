using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractMultiIntentMonster : AbstractRuinaMonster
{
    public List<MonsterMoveStateMachine>? MultiIntentMoveStateMachines;
    public abstract List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine();
    public List<MoveState> NextMoves = new();
    public List<Creature> Targets = new();
    public virtual int NumIntents { get; set; }
    public Creature? OtherSideTargetMonster { get; set; }
    
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
            await Cmd.CustomScaledWait(0.1f, 0.4f);
        }
    }

    public abstract Creature DetermineTargetForIntent(int intentNum);

    public override bool ShouldClearBlock(Creature creature)
    {
        if (creature == Creature)
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.SetUpForCombat))]
public static class GenerateExtraIntentStateMachinesPatch
{
    public static void Postfix(MonsterModel __instance)
    {
        if (__instance is AbstractMultiIntentMonster monster)
        {
            monster.MultiIntentMoveStateMachines = monster.GenerateMultiIntentMoveStateMachine();
        }
    }
}

[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.PerformMove))]
public static class PatchPerformMove
{
    public static bool Prefix(MonsterModel __instance, ref Task __result)
    {
        if (__instance is AbstractMultiIntentMonster monster)
        {
            __result = Task.CompletedTask;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Creature), nameof(Creature.TakeTurn))]
public static class PatchTakeTurn
{
    public static void Postfix(Creature __instance, ref Task __result)
    {
        __result = Wrap(__instance, __result);
    }
    private static async Task Wrap(Creature __instance, Task original)
    {
        await original;
        if (__instance.Monster is AbstractMultiIntentMonster monster)
        {
            __instance.Block = 0;
            await monster.PerformMultiIntentMove();
        }
    }
}

[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.RollMove))]
public static class PatchRollMove
{
    public static void Postfix(MonsterModel __instance, IEnumerable<Creature> targets)
    {
        if (__instance is AbstractMultiIntentMonster monster)
        {
            if (monster.MultiIntentMoveStateMachines != null)
            {
                monster.NextMoves.Clear();
                monster.Targets.Clear();
                for (int i = 0; i < monster.NumIntents; i++)
                {
                    var stateMachine = monster.MultiIntentMoveStateMachines[i];
                    monster.NextMoves.Add( stateMachine.RollMove(targets, monster.Creature, monster.RunRng.MonsterAi));
                    monster.Targets.Add(monster.DetermineTargetForIntent(i));
                }
            }
        }
    }
}

[HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
public static class PatchUpdateIntent
{
    public static void Prefix(NCreature __instance)
    {
        if (__instance.Entity.Monster is AbstractMultiIntentMonster monster && monster.MultiIntentMoveStateMachines != null)
        {
            List<AbstractIntent> totalIntents = new List<AbstractIntent>();
            foreach (var move in monster.NextMoves)
            {
                totalIntents.AddRange(move.Intents);
            }
            monster.NextMove.Intents = totalIntents;
        }
    }
}