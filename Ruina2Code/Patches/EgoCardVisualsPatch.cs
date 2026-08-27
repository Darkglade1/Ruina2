using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdatePortrait))]
public static class EgoCardVisualsPatch
{
    public static void Postfix(NCard __instance)
    {
        if (__instance.IsNodeReady())
        {
            if (__instance.Model is EGOCard egoCard)
            {
                __instance._typePlaque.Texture = ResourceLoader.Load<Texture2D>(egoCard.CustomPlaqueTexturePath);
                __instance._portraitBorder.Material = null;
                __instance._typePlaque.Material = null;
                __instance._banner.Material = null;
            }
            else
            {
                __instance._typePlaque.Texture =
                    ResourceLoader.Load<Texture2D>("res://images/ui/cards/card_portrait_border_plaque2.png");
            }
        }
    }
}

[HarmonyPatch(typeof (CardModel), "BannerTexture", MethodType.Getter)]
public static class EgoBannerTexturePatch
{
    private static bool Prefix(CardModel __instance, ref Texture2D? __result)
    {
        if (!(__instance is EGOCard egoCard))
            return true;
        __result = ResourceLoader.Load<Texture2D>(egoCard.CustomBannerTexturePath);
        return __result == null;
    }
}

[HarmonyPatch(typeof (CardModel), "PortraitBorder", MethodType.Getter)]
public static class EgoCardPortraitBorderPatch
{
    private static bool Prefix(CardModel __instance, ref Texture2D? __result)
    {
        if (!(__instance is EGOCard egoCard))
            return true;
        __result = ResourceLoader.Load<Texture2D>(egoCard.CustomPortraitBorderPath);
        return __result == null;
    }
}