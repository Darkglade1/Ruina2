using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Patches.Features;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(ModelDb), "Init")]
internal static class ModelDbTargetTypeInitPatch
{
    [HarmonyPostfix]
    private static void RegisterTargetTypes()
    {
        CustomTargetType.RegisterSingleTargetType(Brainwash.Self,
            (target, player) => target == player.Creature);
    }
}

    [HarmonyPatch]
    public static class MonsterNullSafeAttackPatch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            MethodInfo method = AccessTools.Method(typeof(GoForTheEyes), "OnPlay");
            return AccessTools.AsyncMoveNext(method) ?? (MethodBase)method;
        }
        
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo original = AccessTools.PropertyGetter(typeof(MonsterModel), nameof(MonsterModel.IntendsToAttack));
            MethodInfo replacement = AccessTools.Method(typeof(MonsterNullSafeAttackPatch), nameof(SafeIntendsToAttack));

            bool patched = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(original))
                {
                    yield return new CodeInstruction(OpCodes.Call, replacement).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
                    patched = true;
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        
        public static bool SafeIntendsToAttack(MonsterModel? monster)
        {
            return monster != null && monster.IntendsToAttack;
        }
    }
