using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(PainfulStabsPower), nameof(PainfulStabsPower.ShouldCreatureBeRemovedFromCombatAfterDeath))]
public static class PainfulStabsPowerPatch
{
    public static void Postfix(PainfulStabsPower __instance, Creature creature, ref bool __result)
    {
        if (__instance.Owner == creature && creature.Monster is AbstractRuinaMonster)
        {
            __result = true;
        }
    }
}