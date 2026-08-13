using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Ruina2.Ruina2Code.Events;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch]
public static class NeowAngelaPatch
{
    private static readonly MethodInfo IsNeowLikeMethod =
        AccessTools.Method(
            typeof(NeowAngelaPatch),
            nameof(IsNeowLike))
        ?? throw new MissingMethodException(
            nameof(NeowAngelaPatch),
            nameof(IsNeowLike));

    /// <summary>
    ///     BeforeEventStarted is async, so its actual body is inside the
    ///     compiler-generated IAsyncStateMachine.MoveNext method.
    /// </summary>
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        MethodInfo beforeEventStarted =
            AccessTools.Method(
                typeof(AncientEventModel),
                "BeforeEventStarted",
                [typeof(bool)])
            ?? throw new MissingMethodException(
                typeof(AncientEventModel).FullName,
                "BeforeEventStarted");

        AsyncStateMachineAttribute stateMachineAttribute =
            beforeEventStarted.GetCustomAttribute<AsyncStateMachineAttribute>()
            ?? throw new InvalidOperationException(
                "BeforeEventStarted does not have an AsyncStateMachineAttribute.");

        return AccessTools.Method(
                   stateMachineAttribute.StateMachineType,
                   nameof(IAsyncStateMachine.MoveNext))
               ?? throw new MissingMethodException(
                   stateMachineAttribute.StateMachineType.FullName,
                   nameof(IAsyncStateMachine.MoveNext));
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        int replacements = 0;

        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Isinst &&
                instruction.operand is Type checkedType &&
                checkedType == typeof(Neow))
            {
                /*
                 * Original:
                 *
                 *     ancient
                 *     isinst Neow
                 *
                 * Replacement:
                 *
                 *     ancient
                 *     dup
                 *     isinst Neow
                 *     call IsNeowLike
                 *
                 * Stack:
                 *
                 *     ancient
                 *     ancient, ancient
                 *     ancient, originalResult
                 *     extendedResult
                 *
                 * The original `isinst Neow` instruction remains unchanged,
                 * so transpilers running after this one can still find it.
                 */
                var duplicateInstruction =
                    new CodeInstruction(OpCodes.Dup);

                var extendResultInstruction =
                    new CodeInstruction(
                        OpCodes.Call,
                        IsNeowLikeMethod);

                /*
                 * A branch targeting the original isinst must execute the
                 * inserted dup first.
                 */
                instruction.MoveLabelsTo(duplicateInstruction);

                /*
                 * Keep the complete inserted sequence inside the same
                 * exception region:
                 *
                 * - beginning boundaries go on the first instruction;
                 * - ending boundaries go on the final instruction.
                 */
                foreach (ExceptionBlock block in instruction.ExtractBlocks())
                {
                    if (block.blockType == ExceptionBlockType.EndExceptionBlock)
                    {
                        extendResultInstruction.blocks.Add(block);
                    }
                    else
                    {
                        duplicateInstruction.blocks.Add(block);
                    }
                }

                yield return duplicateInstruction;
                yield return instruction;
                yield return extendResultInstruction;

                replacements++;
                continue;
            }

            yield return instruction;
        }

        if (replacements != 2)
        {
            throw new InvalidOperationException(
                $"Expected to extend 2 Neow checks in " +
                $"{nameof(AncientEventModel)}.BeforeEventStarted, " +
                $"but extended {replacements}. The game method or another " +
                $"transpiler may have changed.");
        }
    }

    /// <summary>
    ///     Extends the result of the original `isinst Neow` instruction.
    ///     A non-null original result is retained. This also composes with
    ///     another transpiler that replaces the preserved isinst with a
    ///     compatible object-to-object check.
    /// </summary>
    private static object? IsNeowLike(
        object? ancient,
        object? originalResult)
    {
        return originalResult
               ?? (ancient is NeowAngela ? ancient : null);
    }
}