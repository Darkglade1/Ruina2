using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Intents;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Monsters;
using Ruina2.Ruina2Code.Monsters.Act2.mountain;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Patches;

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
        if (__instance is AbstractMultiIntentMonster monster && (__instance.Creature.IsAlive || monster.IsReviving))
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
        if (__instance.Monster is AbstractMultiIntentMonster)
        {
            __result = Wrap(__instance);
            return false;
        }
        return true;
    }
    private static async Task Wrap(Creature __instance)
    {
        if (__instance.Monster is AbstractMultiIntentMonster monster && (__instance.IsAlive || monster.IsReviving))
        {
            if (monster.ShouldClearBlockAtStartOfOwnTurn)
            {
                __instance.Block = 0;   
            }
            foreach (var intent in monster.NextMove.Intents)
            {
                if (intent is StunIntent)
                {
                    return;
                }
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
                monster.NextMove = new MoveState();
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
            foreach (var intent in monster.NextMove.Intents)
            {
                if (intent is StunIntent)
                {
                    return;
                }
            }
            List<AbstractIntent> totalIntents = new List<AbstractIntent>();
            if (monster.FlippedHorizontal)
            {
                for (int i = monster.NextMoves.Count - 1; i >= 0; i--)
                {
                    var move = monster.NextMoves[i];
                    totalIntents.AddRange(move.Intents);
                }
            }
            else
            {
                foreach (var move in monster.NextMoves)
                {
                    totalIntents.AddRange(move.Intents);
                }
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
        else
        {
            __instance.Modulate = Color.Color8(255, 255, 255);
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
            if (texturePath != null && ((__instance._intent is RuinaAttackIntent && !(__instance._intent is RuinaMassAttackIntent)) || __instance._intent is RuinaDebuffIntent))
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
                else
                {
                    var targetTextureNode = __instance._intentHolder.GetNodeOrNull<Sprite2D>(nodeName);
                    targetTextureNode.Texture = PreloadManager.Cache.GetTexture2D(texturePath);
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
public static class RemoveCreaturesFromPossibleTargets
{
    public static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
    {
        var newResult = new List<Creature>(__result.ToList());
        bool removedCreature = false;
        foreach (var creature in __result)
        {
            if (creature.Monster is AbstractAllyMonster ally)
            {
                if (ally.IsAlly && !ally.IsTargetableByPlayers && creature.CombatState?.CurrentSide == CombatSide.Player)
                {
                    newResult.Remove(creature);
                    removedCreature = true;
                }
            }
            if (creature.HasPower<Silence>())
            {
                newResult.Remove(creature);
                removedCreature = true;
            }
        }
        if (removedCreature)
        {
            __result = newResult;
        }
    }
}

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.GetPossibleTargets))]
public static class PatchAddCreaturesToPossibleTargetsForMassAttacks
{
    public static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
    {
        if (__instance.Attacker != null && __instance.Attacker.CombatState != null && __instance.Attacker?.Monster is AbstractMultiIntentMonster monster && monster.IsMassAttacking)
        {
            var newList = new List<Creature>(__result.ToList());
            foreach (var target in monster.AdditionalMassAttackTargets())
            {
                newList.Add(target);
            }
            __result = newList;
        }
    }
}

[HarmonyPatch(typeof(IntentAnimData), nameof(IntentAnimData.GetAnimationFrame))]
public static class PatchIntentAnimDataGetAnimationFrame
{
    public static bool Prefix(string animation, int frame, ref string __result)
    {
        if (animation == "mass_attack")
        {
            __result = "intent_mass_attack.png".UIImagePath();
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(IntentAnimData), nameof(IntentAnimData.GetAnimationFrameCount))]
public static class PatchIntentAnimDataGetAnimationFrameCount
{
    public static bool Prefix(string animation, ref int __result)
    {
        if (animation == "mass_attack")
        {
            __result = 0;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(NIntent), nameof(NIntent.UpdateVisuals))]
public static class PatchUpdateVisualsMassAttack
{
    public static void Postfix(NIntent __instance)
    {
        if (__instance._animationName == "mass_attack")
        {
            __instance._animationFrames.Clear();
            __instance._animationFrames.Add(GD.Load<Texture2D>("intent_mass_attack.png".UIImagePath()));
        }
    }
}