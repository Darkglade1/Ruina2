using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Monsters.Act2.Greed;
using Ruina2.Ruina2Code.Monsters.Act2.mountain;
using Ruina2.Ruina2Code.Monsters.Act2.RoadHome;
using Ruina2.Ruina2Code.Monsters.Act3.SnowQueen;
using Ruina2.Ruina2Code.Monsters.Act3.Twilight;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.PerformMove))]
public static class DoNothingMinionSkipTurnPatch
{
    public static bool Prefix(MonsterModel __instance, ref Task __result)
    {
        if (__instance is BigEgg || __instance is SmallEgg || __instance is LongEgg || __instance is Corpse || __instance is BrilliantBliss || __instance is Home || __instance is PrisonOfIce)
        {
            __result = Task.CompletedTask;
            return false;
        }
        return true;
    }
}