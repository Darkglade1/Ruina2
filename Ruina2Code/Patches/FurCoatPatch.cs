using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Relics;
using Ruina2.Ruina2Code.Monsters.Act3.BlueStar;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(FurCoat), nameof(FurCoat.BeforeCombatStart))]
public static class FurCoatPatch
{
    public static void Postfix(FurCoat __instance)
    {
        List<MapCoord>? markedCoords = __instance.GetMarkedCoords();
        if (markedCoords != null && __instance.Owner.RunState.CurrentMapPoint != null &&
            markedCoords.Contains(__instance.Owner.RunState.CurrentMapPoint.coord) && __instance.Owner.Creature.CombatState != null)
        {
            foreach (var creature in __instance.Owner.Creature.CombatState.HittableEnemies)
            {
                if (creature.Monster is Worshipper worshipper)
                {
                    worshipper.TriggerMartyr = false;
                }
            }
        }
    }
}