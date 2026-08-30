using Godot;
using HarmonyLib;
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
                __instance._typePlaque.Material = null;
                __instance._portraitBorder.Texture = ResourceLoader.Load<Texture2D>(egoCard.CustomPortraitBorderPath);
                __instance._portraitBorder.Material = null;
                __instance._banner.Texture = ResourceLoader.Load<Texture2D>(egoCard.CustomBannerTexturePath);
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