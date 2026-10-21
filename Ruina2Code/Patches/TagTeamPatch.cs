using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(TagTeamPower), nameof(TagTeamPower.ModifyCardPlayCount))]
public static class TagTeamPatch
{
    public static void Postfix(TagTeamPower __instance, CardModel card, Creature? target, int playCount, ref int __result)
    {
        if (card.Type == CardType.Attack && card.Owner.Creature != __instance.Applier && card.TargetType == Captivated.SpotlightOnly && target == __instance.Owner)
        {
            __result = playCount + __instance.Amount;
        }
    }
}