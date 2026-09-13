using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
internal static class RestSiteCharacterPatch
{
    private sealed class SavedActIndex
    {
        public RunState RunState { get; }

        public int Value { get; }

        public SavedActIndex(RunState runState, int value)
        {
            RunState = runState;
            Value = value;
        }
    }

    private static readonly FieldInfo CurrentActIndexField = AccessTools.Field(typeof(RunState), "_currentActIndex");

    [HarmonyPrefix]
    private static void UseGloryAnimation(NRestSiteCharacter __instance, out SavedActIndex? __state)
    {
        __state = null;
        RunState? runState = __instance.Player?.RunState as RunState;
        if (runState?.Act is UninvitedGuests)
        {
            __state = new SavedActIndex(runState, runState.CurrentActIndex);
            CurrentActIndexField.SetValue(runState, 2);
        }
    }

    [HarmonyFinalizer]
    private static Exception? RestoreActIndex(Exception? __exception, SavedActIndex? __state)
    {
        if (__state != null)
        {
            CurrentActIndexField.SetValue(__state.RunState, __state.Value);
        }
        return __exception;
    }
}