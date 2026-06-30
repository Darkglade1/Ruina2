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
    
    protected async Task ResetIdle()
    {
        await WaitAnimation();
        await CreatureCmd.TriggerAnim(Creature, "Idle", 0);
    }
    
    protected async Task ResetIdle(float waitTime)
    {
        await WaitAnimation(waitTime);
        await CreatureCmd.TriggerAnim(Creature, "Idle", 0);
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
        if (__instance is AbstractMultiIntentMonster && __instance.Creature.IsAlive)
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
    public static bool Prefix(Creature __instance, ref Task __result)
    {
        if (__instance.Monster is AbstractMultiIntentMonster && __instance.IsAlive)
        {
            __result = Wrap(__instance);
            return false;
        }
        return true;
    }
    private static async Task Wrap(Creature __instance)
    {
        if (__instance.Monster is AbstractMultiIntentMonster monster && __instance.IsAlive)
        {
            if (monster.ShouldClearBlockAtStartOfOwnTurn)
            {
                __instance.Block = 0;   
            }
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
                    monster.NextMoves.Add(stateMachine.RollMove(targets, monster.Creature, monster.RunRng.MonsterAi));
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

[HarmonyPatch(typeof(NIntent), nameof(NIntent.UpdateVisuals))]
public static class PatchUpdateVisuals
{
    public static void Postfix(NIntent __instance)
    {
        if (__instance._owner.Monster is AbstractAllyMonster ally && ally.IsAlly)
        {
            if (__instance._intent is RuinaAttackIntent || __instance._intent is RuinaDebuffIntent)
            {
                __instance.Modulate = Color.Color8(0, 255, 0);
            }
            else
            {
                __instance.Modulate = Color.Color8(255, 255, 255);
            }
        }
    }
}

[HarmonyPatch(typeof(NIntent), nameof(NIntent.UpdateVisuals))]
public static class PatchTargetTextureIcon
{
    public static void Postfix(NIntent __instance)
    {
        string nodeName = "TargetTextureNode";
        Creature? targetCreature = RuinaAttackIntent.GetIntentTargetedCreature(__instance._intent, __instance._owner);
        if (targetCreature?.Monster is AbstractMultiIntentMonster monster)
        {
            var texturePath = monster.TargetTexturePath;
            if (texturePath != null && (__instance._intent is RuinaAttackIntent || __instance._intent is RuinaDebuffIntent))
            {
                if (!__instance._intentHolder.HasNode(nodeName))
                {
                    Sprite2D textureSprite = new Sprite2D();
                    textureSprite.Scale = new Vector2(0.75f, 0.75f);
                    textureSprite.Position = new Vector2(10, 10);
                    textureSprite.Name = nodeName;
                    textureSprite.Texture = PreloadManager.Cache.GetTexture2D(texturePath);
                    __instance._intentHolder.AddChildSafely(textureSprite);
                }
            }
            else
            {
                var targetTextureNode = __instance._intentHolder.GetNodeOrNull<Sprite2D>(nodeName);
                if (targetTextureNode != null)
                {
                    __instance._intentHolder.RemoveChildSafely(targetTextureNode);
                }
            }
        }
        else
        {
            var targetTextureNode = __instance._intentHolder.GetNodeOrNull<Sprite2D>(nodeName);
            if (targetTextureNode != null)
            {
                __instance._intentHolder.RemoveChildSafely(targetTextureNode);
            }
        }
    }
}

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.GetPossibleTargets))]
public static class RemoveAlliesFromPossibleTargets
{
    public static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
    {
        var newResult = new List<Creature>(__result.ToList());
        bool removedAlly = false;
        foreach (var creature in __result)
        {
            if (creature.Monster is AbstractAllyMonster ally)
            {
                if (ally.IsAlly && !ally.IsTargetableByPlayers && creature.CombatState?.CurrentSide == CombatSide.Player)
                {
                    newResult.Remove(creature);
                    removedAlly = true;
                }
            }
        }
        if (removedAlly)
        {
            __result = newResult;
        }
    }
}

