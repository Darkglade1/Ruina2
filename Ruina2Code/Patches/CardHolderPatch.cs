using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using Ruina2.Ruina2Code.Cards.EnemyCards;
using Ruina2.Ruina2Code.Cards.EnemyCards.Bremen;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof (NCardHolder), "SmallScale", MethodType.Getter)]
public static class CardHolderPatchSmallScale
{
    private static bool Prefix(NCardHolder __instance, ref Vector2 __result)
    {
        if (__instance.CardModel is Melody)
        {
            __result = Vector2.One * 0.7f;
            return false;
        }
        if (__instance.CardModel is EnemyCard)
        {
            __result = Vector2.One * 0.4f;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof (NCardHolder), "HoverScale", MethodType.Getter)]
public static class CardHolderPatchHoverScale
{
    private static bool Prefix(NCardHolder __instance, ref Vector2 __result)
    {
        if (__instance.CardModel is Melody)
        {
            __result = Vector2.One * 0.9f;
            return false;
        }
        if (__instance.CardModel is EnemyCard)
        {
            __result = Vector2.One * 0.7f;
            return false;
        }
        return true;
    }
}