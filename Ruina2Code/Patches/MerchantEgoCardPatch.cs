using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(MerchantInventory), nameof(MerchantInventory.PopulateColorlessCardEntries))]
public static class MerchantEgoCardPatch
{
    public static void Postfix(MerchantInventory __instance)
    {
        if (RunManager.Instance.State?.Act is AbstractRuinaAct)
        {
            __instance._colorlessCardEntries.Clear();
            List<CardModel> list = EGOCardPool.GetAct2EgoCards();
            for (int i = 0; i < 2; i++)
            {
                MerchantCardEntry merchantCardEntry = new MerchantCardEntry(__instance.Player, __instance, list, CardRarity.Rare);
                merchantCardEntry.Populate();
                __instance._colorlessCardEntries.Add(merchantCardEntry);
            }
        }
    }
}

[HarmonyPatch(typeof(MerchantCardEntry), nameof(MerchantCardEntry.GetCost))]
public static class MerchantEgoCardPricePatch
{
    public static void Postfix(CardModel card, ref int __result)
    {
        if (card.Pool is EGOCardPool)
        { 
            __result = Mathf.RoundToInt(__result * 1.15f);
        }
    }
}