using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.GenerateRooms))]
public static class GenerateRoomsPatch
{
    public static void Postfix(RunManager __instance)
    {
        RunState? runState = __instance.DebugOnlyGetState();
        if (runState != null)
        {
            UninvitedGuests? uninvitedGuests = runState.Acts.OfType<UninvitedGuests>().FirstOrDefault();
            if (uninvitedGuests != null)
            {
                uninvitedGuests.ConfigureFixedProgression();
            }
        }
    }
}