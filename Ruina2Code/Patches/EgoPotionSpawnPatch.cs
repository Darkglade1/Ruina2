using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Potions;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(PotionFactory), nameof(PotionFactory.GetPotionOptions))]
public static class EgoPotionSpawnPatch
{
    public static IEnumerable<PotionModel> Postfix(IEnumerable<PotionModel> __result, Player player)
    {
        var potionModels = __result.ToList();
        if (!(player.RunState.Act is AbstractRuinaAct))
        {
            List<PotionModel> potionListWithoutEgoPotion = new List<PotionModel>();
            foreach (var potion in potionModels)
            {
                if (!(potion is EgoPotion))
                {
                    potionListWithoutEgoPotion.Add(potion);
                }
            }
            return potionListWithoutEgoPotion;
        }
        return potionModels;
    }
}