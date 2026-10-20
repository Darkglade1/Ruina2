using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(TemporaryStrengthPower), nameof(TemporaryStrengthPower.AfterSideTurnEnd))]
public static class TempStrengthPatch
{
    public static bool Prefix(TemporaryStrengthPower __instance, PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants, ref Task __result)
    {
        if (participants.Contains(__instance.Owner) && __instance.Owner.IsDead)
        {
            __result = Task.CompletedTask;
            return false;
        }
        return true;
    }
}