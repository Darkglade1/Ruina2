using BaseLib.Patches.Features;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Afflictions;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(ModelDb), "Init")]
internal static class ModelDbTargetTypeInitPatch
{
    [HarmonyPostfix]
    private static void RegisterTargetTypes()
    {
        CustomTargetType.RegisterSingleTargetType(Captivated.SpotlightOnly,
            (target, _) => target.HasPower<Spotlight>());
    }
}
