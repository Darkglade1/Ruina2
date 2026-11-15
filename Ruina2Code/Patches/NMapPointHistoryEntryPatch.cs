using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Runs.History;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(NMapPointHistoryEntry), nameof(NMapPointHistoryEntry._Ready))]
public static class NMapPointHistoryEntryPatch
{
    public static void Postfix(NMapPointHistoryEntry __instance)
    {
        var modelId = __instance._entry.Rooms.First<MapPointRoomHistoryEntry>().ModelId;
        if (modelId != null)
        {
            bool changed = false;
            if (modelId.Entry == "RUINA2-PHILIP_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("MalkuthMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("MalkuthMap.png".UIImagePath());
                changed = true;
            }
            if (modelId.Entry == "RUINA2-EILEEN_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("YesodMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("YesodMap.png".UIImagePath());
                changed = true;
            }
            if (modelId.Entry == "RUINA2-GRETA_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("HodMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("HodMap.png".UIImagePath());
                changed = true;
            }
            if (modelId.Entry == "RUINA2-BREMEN_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("NetzachMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("NetzachMap.png".UIImagePath());
                changed = true;
            }
            if (modelId.Entry == "RUINA2-OSWALD_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("TiphMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("TiphMap.png".UIImagePath());
                changed = true;
            }
            if (modelId.Entry == "RUINA2-TANYA_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("GeburaMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("GeburaMap.png".UIImagePath());
                changed = true;
            }
            if (modelId.Entry == "RUINA2-PUPPETEER_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("ChesedMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("ChesedMap.png".UIImagePath());
                changed = true;
            }
            if (modelId.Entry == "RUINA2-ELENA_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("BinahMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("BinahMap.png".UIImagePath());
                changed = true;
            }
            if (modelId.Entry == "RUINA2-PLUTO_ENCOUNTER")
            {
                __instance._texture.Texture = PreloadManager.Cache.GetAsset<Texture2D>("HokmaMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("HokmaMap.png".UIImagePath());
                changed = true;
            }

            if (changed)
            {
                __instance._baseAngle = 0f;
                __instance._texture.RotationDegrees = 0f;
                __instance._baseScale = Vector2.One;
                __instance._texture.Scale = Vector2.One;
            }
        }
       
    }
}