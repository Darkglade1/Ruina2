using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Merchant;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(MerchantInventory), nameof(MerchantInventory.GetBlitzEntries))]
public static class BlitzMerchantEgoPatch
{
    public static void Postfix(MerchantInventory __instance, Player player, ref List<BlitzMerchantEntry> __result)
    {
        if (RunManager.Instance.State != null && RunManager.Instance.State.Act is AbstractRuinaAct)
        {
            var list = __result.ToList();
            List<BlitzMerchantEntry> newList = new List<BlitzMerchantEntry>();
            bool changed = false;
            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                if (entry.MerchantEntryType == BlitzMerchantEntryType.ColorlessCardReward)
                {
                    newList.Add(new BlitzMerchantEgoCardRewardEntry(player));
                    changed = true;
                }
                else
                {
                    newList.Add(entry);
                }
            }

            if (changed)
            {
                __result = newList;
            }
        }
    }
}

// [HarmonyPatch(typeof(BlitzMerchantEntry), nameof(BlitzMerchantEntry.GetTitleForHistory))]
// internal static class TitleForHistoryPatch
// {
//     [HarmonyPostfix]
//     private static void PatchTitle(BlitzMerchantHistoryEntry entry, ref LocString __result)
//     {
//         if (entry. is BlitzMerchantEgoCardRewardEntry)
//         {
//             
//         }
//     }
// }