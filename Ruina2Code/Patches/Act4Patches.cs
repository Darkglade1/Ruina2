using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Encounters.UninvitedGuests;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(MultiplayerScalingModel), "GetMultiplayerScaling")]
internal static class Act4MultiplayerScalingPatch
{
    private static bool Prefix(EncounterModel? encounter, int actIndex, ref decimal __result)
    {
        if (actIndex < 3 || !NeedsActFourScaling(encounter))
        {
            return true;
        }
        __result = 1.4M;
        return false;
    }

    private static bool NeedsActFourScaling(EncounterModel? encounter)
    {
        if (encounter is PhilipEncounter || encounter is EileenEncounter || encounter is GretaEncounter || 
            encounter is BremenEncounter || encounter is OswaldEncounter || encounter is TanyaEncounter || 
            encounter is PuppeteerEncounter || encounter is ElenaEncounter || encounter is PlutoEncounter || 
            encounter is ArgaliaBoss ||
            encounter is TheArchitectEventEncounter)
        {
            return true;
        }
        return false;
    }
}

[HarmonyPatch(typeof(ProgressSaveManager), "ObtainCharUnlockEpoch")]
internal static class Act4FinalEpochPatch
{
    [HarmonyPrefix]
    private static bool SkipMissingFourthActEpoch(Player localPlayer, int act)
    {
        if (act >= 3)
        {
            return !(localPlayer.RunState.Act is UninvitedGuests);
        }
        return true;
    }
}

[HarmonyPatch(typeof(TreasureRoom))]
[HarmonyPatch(MethodType.Constructor, new[] { typeof(int) })]
public static class TreasureRoomCtorPatch
{
    static bool Prefix(int actIndex)
    {
        if (actIndex >= 3)
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterMapCoordInternal))]
public static class RunManagerEnterMapCoordPatch
{
    public static void Prefix(RunManager __instance, MapCoord coord)
    {
        UninvitedGuestsActMap.CurrentMapCoord = coord;
    }
}

[HarmonyPatch(typeof(ActModel), nameof(ActModel.PullNextEncounter))]
public static class ActNextEncounterPatch
{
    public static void Postfix(ActModel __instance, RoomType roomType, ref EncounterModel __result)
    {
        if (RunManager.Instance.State != null && __instance is UninvitedGuests && roomType == RoomType.Elite)
        {
            var mapPoint = RunManager.Instance.State.Map.GetPoint(UninvitedGuestsActMap.CurrentMapCoord.col,
                UninvitedGuestsActMap.CurrentMapCoord.row);
            if (mapPoint != null)
            {
                var encounter = UninvitedGuestsActMap.MapPointSpecificEncounter.Get(mapPoint);
                if (encounter != null)
                {
                    MainFile.Logger.Info("Setting room's encounter to specific encounter: " + encounter.Id);
                    __result = encounter;
                }
            }
        }
    }
}

[HarmonyPatch(typeof(RewardsSet), nameof(RewardsSet.GenerateRewardsFor))]
public static class RewardsSetPatch
{
    public static void Postfix(RewardsSet __instance, Player player, AbstractRoom room, ref List<Reward> __result)
    {
        if (RunManager.Instance.State != null && RunManager.Instance.State.Act is UninvitedGuests)
        {
            var relicRewards = new List<Reward>();
            foreach (var reward in __result)
            {
                if (reward is RelicReward)
                {
                    relicRewards.Add(reward);
                }
            }
            foreach (var relicReward in relicRewards)
            {
                __result.Remove(relicReward);
            }
        }

        if (player.RunState.CurrentActIndex == 2 && room.RoomType == RoomType.Boss)
        {
            __result.Clear();   
        }
    }
}

[HarmonyPatch(typeof(RoomSet), "FromSave")]
internal static class RoomSetSaveCompatibilityPatch
{
    [HarmonyPrefix]
    private static void RestoreOmittedEmptyCollections(SerializableRoomSet save)
    {
        bool num = save.EventIds == null || save.NormalEncounterIds == null || save.EliteEncounterIds == null;
        SerializableRoomSet serializableRoomSet = save;
        if (serializableRoomSet.EventIds == null)
        {
            List<ModelId> list = (serializableRoomSet.EventIds = new List<ModelId>());
        }
        serializableRoomSet = save;
        if (serializableRoomSet.NormalEncounterIds == null)
        {
            List<ModelId> list = (serializableRoomSet.NormalEncounterIds = new List<ModelId>());
        }
        serializableRoomSet = save;
        if (serializableRoomSet.EliteEncounterIds == null)
        {
            List<ModelId> list = (serializableRoomSet.EliteEncounterIds = new List<ModelId>());
        }
    }
}

[HarmonyPatch(typeof (NNormalMapPoint), "UpdateIcon")]
public static class NNormalMapPointIconPatch
{
    private static void Postfix(NNormalMapPoint __instance)
    {
        var specificEncounter = UninvitedGuestsActMap.MapPointSpecificEncounter.Get(__instance.Point);
        if (specificEncounter != null && __instance.Point.PointType == MapPointType.Elite)
        {
            if (specificEncounter is PhilipEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("MalkuthMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("MalkuthMap.png".UIImagePath());
            }
            if (specificEncounter is EileenEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("YesodMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("YesodMap.png".UIImagePath());
            }
            if (specificEncounter is GretaEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("HodMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("HodMap.png".UIImagePath());
            }
            if (specificEncounter is BremenEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("NetzachMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("NetzachMap.png".UIImagePath());
            }
            if (specificEncounter is OswaldEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("TiphMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("TiphMap.png".UIImagePath());
            }
            if (specificEncounter is TanyaEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("GeburaMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("GeburaMap.png".UIImagePath());
            }
            if (specificEncounter is PuppeteerEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("ChesedMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("ChesedMap.png".UIImagePath());
            }
            if (specificEncounter is ElenaEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("BinahMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("BinahMap.png".UIImagePath());
            }
            if (specificEncounter is PlutoEncounter)
            {
                __instance._icon.Texture = PreloadManager.Cache.GetAsset<Texture2D>("HokmaMap.png".UIImagePath());
                __instance._outline.Texture = PreloadManager.Cache.GetAsset<Texture2D>("HokmaMap.png".UIImagePath());
            }
        }
    }
}

[HarmonyPatch(typeof(RunManager), "GenerateRooms")]
internal static class RoomGenerationPatch
{
	private static void Postfix(RunManager __instance)
	{
		if (RunManager.Instance.State != null)
		{
			var finalAct = RunManager.Instance.State.Acts.OfType<UninvitedGuests>().FirstOrDefault();
			if (finalAct != null)
			{
				RestoreVanillaFinalActDoubleBoss(RunManager.Instance.State, finalAct);
			}
		}
	}

	private static void RestoreVanillaFinalActDoubleBoss(RunState runState, ActModel finalAct)
	{
		if (runState.AscensionLevel < 10)
		{
			return;
		}
		int num = runState.Acts.FindIndex((ActModel act) => act == finalAct);
		if (num <= 0)
		{
			return;
		}
		ActModel precedingAct = runState.Acts[num - 1];
		if (!precedingAct.HasSecondBoss)
		{
			List<EncounterModel> list = precedingAct.AllBossEncounters.Where((EncounterModel encounter) => encounter.Id != precedingAct.BossEncounter.Id).OrderBy<EncounterModel, string>((EncounterModel encounter) => encounter.Id.Entry, StringComparer.Ordinal).ToList();
			if (list.Count == 0)
			{
				return;
			}
			EncounterModel encounterModel = runState.Rng.UpFront.NextItem(list);
			precedingAct.SetSecondBossEncounter(encounterModel);
		}
	}

	private static int FindIndex<T>(this IReadOnlyList<T> items, Func<T, bool> predicate)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (predicate(items[i]))
			{
				return i;
			}
		}
		return -1;
	}
}