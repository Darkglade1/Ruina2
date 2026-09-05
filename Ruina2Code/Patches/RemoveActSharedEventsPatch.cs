using HarmonyLib;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Events;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
public static class RemoveActSharedEventsPatch
{
    public static void Postfix(ActModel __instance, Rng rng, UnlockState unlockState, bool isMultiplayer)
    {
        if (__instance is Briah)
        {
            __instance._rooms.events.Clear();
            var list = __instance.AllEvents.ToList();
            __instance._rooms.events.AddRange(list.UnstableShuffle(rng));
        }

        // Hack to stop ancient config from overriding Neow Angela
        // This is probably load order dependent but Ritsulib sort seems to put the mods in the right order
        if (__instance is Asiyah)
        {
            if (RunManager.Instance.State != null && RunManager.Instance.State.Modifiers.Count == 0 && Config.NeowAngelaAppears)
            {
                __instance._rooms._ancient = ModelDb.AncientEvent<NeowAngela>();
            }
        }
    }
}