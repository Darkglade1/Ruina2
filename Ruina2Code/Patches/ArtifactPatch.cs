using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters.Act2.Jester;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(ArtifactPower), nameof(ArtifactPower.GetScaledAmountForMultiplayer))]
public static class ArtifactPatch
{
    public static void Postfix(ArtifactPower __instance, ICombatState combatState,
        Creature? applier,
        Decimal amount,
        Creature target,
        CardModel? cardSource,
        ref Decimal __result)
    {
        if (target.Monster is JesterOfNihil && applier == target)
        {
            __result = amount;
        }
    }
}