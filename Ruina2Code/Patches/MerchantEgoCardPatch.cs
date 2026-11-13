using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(MerchantInventory), nameof(MerchantInventory.GetDefaultColorlessCardEntries))]
public static class MerchantEgoCardPatch
{
    public static void Postfix(Player player, ref List<MerchantCardEntry> __result)
    {
        if (RunManager.Instance.State?.Act is AbstractRuinaAct)
        {
            List<MerchantCardEntry> egoCardEntries = new List<MerchantCardEntry>();
            List<CardModel> list;
            if (RunManager.Instance.State?.Act is Asiyah)
            {
                list = EGOCardPool.GetAct1EgoCards();
            } else if (RunManager.Instance.State?.Act is Briah)
            {
                list = EGOCardPool.GetAct2EgoCards();
            } else if (RunManager.Instance.State?.Act is Atziluth)
            {
                list = EGOCardPool.GetAct3EgoCards();
            }
            else
            {
                list = EGOCardPool.GetAct1EgoCards();
                list.AddRange(EGOCardPool.GetAct2EgoCards());
                list.AddRange(EGOCardPool.GetAct3EgoCards());
            }
            for (int i = 0; i < 2; i++)
            {
                MerchantCardEntry merchantCardEntry = new MerchantCardEntry(player, list, CardRarity.Rare);
                egoCardEntries.Add(merchantCardEntry);
                __result = egoCardEntries;
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