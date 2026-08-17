using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Encounters.Act3;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Patches;

public class MusicPatches
{
    [HarmonyPatch(typeof(NRunMusicController))]
    public static class RuinaActMusicPatches
    {
        private static readonly PropertyInfo? StateProperty =
            typeof(RunManager).GetProperty("State", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo? CombatStateField =
            typeof(CombatManager).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);

        private enum TrackType
        {
            None,
            Exploration,
            Elite,
            Boss
        }

        private enum RuinaAct
        {
            None,
            Asiyah,
            Briah,
            Atziluth
        }

        private static TrackType _currentTrackType = TrackType.None;
        public static bool _isPlayingRuinaMusic = false;
        public static bool _isPlayingBaseGameMusic = false;

        private const string BaseGameBankPath = "res://banks/desktop/act1_a1.bank";
        private const string BaseGameTrack = "event:/music/act1_a1_v1";

        private static bool _whiteNightAwake = false;

        private static RuinaAct GetCurrentRuinaAct()
        {
            var runState = StateProperty?.GetValue(RunManager.Instance) as RunState;
            return runState?.Act switch
            {
                Asiyah => RuinaAct.Asiyah,
                Briah => RuinaAct.Briah,
                Atziluth => RuinaAct.Atziluth,
                _ => RuinaAct.None
            };
        }

        private static bool IsRuinaAct() => GetCurrentRuinaAct() != RuinaAct.None;

        private static Node? GetProxy(NRunMusicController controller)
        {
            return (Node?)controller.Get("_proxy");
        }

        private static void StartBaseGameMusic(NRunMusicController controller, int progress, float fadeDelay = 1f)
        {
            var proxy = GetProxy(controller);
            if (proxy == null) return;

            var tree = Engine.GetMainLoop() as SceneTree;
            tree?.CreateTimer(fadeDelay).Connect("timeout", Callable.From(() =>
            {
                proxy.Call("load_act_banks", new Godot.Collections.Array { BaseGameBankPath });
                proxy.Call("update_music", BaseGameTrack);
                proxy.Call("update_global_parameter", "Progress", progress);
                _isPlayingBaseGameMusic = true;
            }));
        }

        private static void StopBaseGameMusic(NRunMusicController controller)
        {
            if (!_isPlayingBaseGameMusic) return;

            var proxy = GetProxy(controller);
            if (proxy == null) return;

            proxy.Call("stop_music");
            proxy.Call("unload_act_banks");
            _isPlayingBaseGameMusic = false;
        }

        private static int? GetSpecialRoomProgress()
        {
            var runState = StateProperty?.GetValue(RunManager.Instance) as RunState;
            return runState?.CurrentRoom?.RoomType switch
            {
                RoomType.Shop => 2,
                RoomType.RestSite => 3,
                _ => null
            };
        }
        
        public static void OnWhiteNightAwakened()
        {
            _whiteNightAwake = true;
            RuinaAudio.FadeIn("WhiteNightBGM.ogg".MusicPath(), 1f);
            _isPlayingRuinaMusic = true;
            _currentTrackType = TrackType.Boss;
        }

        private static bool IsBossRoom()
        {
            var runState = StateProperty?.GetValue(RunManager.Instance) as RunState;
            return runState?.CurrentRoom?.RoomType == RoomType.Boss;
        }

        private static bool IsEliteRoom()
        {
            var runState = StateProperty?.GetValue(RunManager.Instance) as RunState;
            return runState?.CurrentRoom?.RoomType == RoomType.Elite;
        }
        
        private static bool IsArchitectEvent()
        {
            var runState = StateProperty?.GetValue(RunManager.Instance) as RunState;
            return runState?.CurrentRoom is EventRoom eventRoom && eventRoom.CanonicalEvent is TheArchitect;
        }

[HarmonyPatch(nameof(NRunMusicController.UpdateMusic))]
[HarmonyPrefix]
public static bool UpdateMusic_Prefix(NRunMusicController __instance)
{
    
    if (IsArchitectEvent())
    {
        if (_isPlayingRuinaMusic)
        {
            RuinaAudio.StopMusic();
            RuinaAudio.StopAmbience();
            _isPlayingRuinaMusic = false;
            _currentTrackType = TrackType.None;
        }
        return true;
    }
    
    if (!IsRuinaAct())
    {
        if (_isPlayingRuinaMusic)
        {
            RuinaAudio.StopMusic();
            RuinaAudio.StopAmbience();
            _isPlayingRuinaMusic = false;
            _currentTrackType = TrackType.None;
        }
        return true;
    }
    __instance.StopMusic();
    RuinaAudio.FadeIn(AbstractRuinaAct.GetBGMBasedOnFloor(AbstractRuinaAct.GetFloorBasedOnBoss()), 1.0f);
    _isPlayingRuinaMusic = true;
    _currentTrackType = TrackType.Exploration;
    __instance.UpdateAmbience();
    return false;
}

[HarmonyPatch(nameof(NRunMusicController.UpdateTrack), new Type[0])]
[HarmonyPrefix]
public static bool UpdateTrack_Prefix(NRunMusicController __instance)
{
    
    if (IsArchitectEvent())
    {
        if (_isPlayingRuinaMusic)
        {
            RuinaAudio.StopMusic();
            RuinaAudio.StopAmbience();
            _isPlayingRuinaMusic = false;
            _currentTrackType = TrackType.None;
        }
        return true;
    }
    
    if (!IsRuinaAct())
    {
        if (_isPlayingRuinaMusic)
        {
            RuinaAudio.StopMusic();
            RuinaAudio.StopAmbience();
            _isPlayingRuinaMusic = false;
            _currentTrackType = TrackType.None;
        }
        return true;
    }

    var combatManager = CombatManager.Instance;
    var combatInProgress2 = combatManager?.IsInProgress ?? false;

    // Handle Shop / Rest Site (must be before stinger guard so stinger fades on room transition)
    var specialProgress = GetSpecialRoomProgress();
    if (specialProgress == 3)
    {
        if (_isPlayingRuinaMusic)
        {
            RuinaAudio.FadeOut(1f);
            _isPlayingRuinaMusic = false;
            _currentTrackType = TrackType.None;
            StartBaseGameMusic(__instance, specialProgress.Value, 1f);
        }
        else if (_isPlayingBaseGameMusic)
        {
            var proxy = GetProxy(__instance);
            proxy?.Call("update_global_parameter", "Progress", specialProgress.Value);
        }
        return false;
    }

    // Handle Boss rooms
    if (IsBossRoom() && combatInProgress2)
    {
        if (!_isPlayingRuinaMusic || _currentTrackType != TrackType.Boss)
        {
            StopBaseGameMusic(__instance);
            RuinaAudio.FadeIn(AbstractRuinaAct.GetBGMBasedOnBoss(), 1f);
            _isPlayingRuinaMusic = true;
            _currentTrackType = TrackType.Boss;
        }
        return false;
    }

    // Handle Elite rooms
    if (IsEliteRoom() && combatInProgress2)
    {
        if (!_isPlayingRuinaMusic || _currentTrackType != TrackType.Elite)
        {
            StopBaseGameMusic(__instance);
            RuinaAudio.FadeIn(AbstractRuinaAct.GetBGMBasedOnElite(), 1f);
            _isPlayingRuinaMusic = true;
            _currentTrackType = TrackType.Elite;
        }
        return false;
    }

    // Default: exploration music
    if (!_isPlayingRuinaMusic || _currentTrackType != TrackType.Exploration)
    {
        StopBaseGameMusic(__instance);
        RuinaAudio.FadeIn(AbstractRuinaAct.GetBGMBasedOnFloor(AbstractRuinaAct.GetFloorBasedOnBoss()), 1f);
        _isPlayingRuinaMusic = true;
        _currentTrackType = TrackType.Exploration;
    }
    return false;
}

        [HarmonyPatch(nameof(NRunMusicController.ToggleMerchantTrack))]
        [HarmonyPrefix]
        public static bool ToggleMerchantTrack_Prefix(NRunMusicController __instance)
        {
            if (!IsRuinaAct()) return true;
            if (_isPlayingRuinaMusic) return false;

            var proxy = GetProxy(__instance);
            if (proxy == null) return false;

            var mapVisible = NMapScreen.Instance?.IsVisible() ?? false;
            proxy.Call("update_global_parameter", "Progress", mapVisible ? 9 : 2);
            return false;
        }

        [HarmonyPatch(nameof(NRunMusicController.TriggerEliteSecondPhase))]
        [HarmonyPrefix]
        public static bool TriggerEliteSecondPhase_Prefix(NRunMusicController __instance)
        {
            if (!IsRuinaAct()) return true;
            if (_isPlayingRuinaMusic) return false;

            var proxy = GetProxy(__instance);
            if (proxy == null) return false;

            proxy.Call("update_global_parameter", "Progress", 8);
            return false;
        }

        [HarmonyPatch(nameof(NRunMusicController.TriggerCampfireGoingOut))]
        [HarmonyPrefix]
        public static bool TriggerCampfireGoingOut_Prefix() => !IsRuinaAct();

        [HarmonyPatch(nameof(NRunMusicController.StopMusic))]
        [HarmonyPostfix]
        public static void StopMusic_Postfix(NRunMusicController __instance)
        {
            RuinaAudio.StopMusic();
            RuinaAudio.StopAmbience();
            StopBaseGameMusic(__instance);
            _isPlayingRuinaMusic = false;
            _currentTrackType = TrackType.None;
        }

        [HarmonyPatch(nameof(NRunMusicController.UpdateAmbience))]
        [HarmonyPrefix]
        public static bool UpdateAmbience_Prefix()
        {
            if (!IsRuinaAct())
            {
                if (_isPlayingRuinaMusic)
                {
                    RuinaAudio.StopAmbience();
                }
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetBgmVol))]
    public static class SetBgmVolPatch
    {
        public static void Postfix(float volume)
        {
            RuinaAudio.SetMusicVolume(volume);
        }
    }

    [HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.SetAmbienceVol))]
    public static class SetAmbienceVolPatch
    {
        public static void Postfix(float volume)
        {
            RuinaAudio.SetAmbienceVolume(volume);
        }
    }
}