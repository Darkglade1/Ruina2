using HarmonyLib;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
public static class RemoveActSharedEventsPatch
{
    public static void Postfix(ActModel __instance, Rng rng, UnlockState unlockState, bool isMultiplayer)
    {
        if (__instance is Asiyah || __instance is Briah)
        {
            __instance._rooms.events.Clear();
            var list = __instance.AllEvents.ToList();
            __instance._rooms.events.AddRange(list.UnstableShuffle(rng));
        }
    }
}