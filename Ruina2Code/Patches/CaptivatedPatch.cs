using BaseLib.Patches.Features;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
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
