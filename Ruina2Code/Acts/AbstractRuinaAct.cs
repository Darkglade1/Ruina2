using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Encounters.Act2;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Acts;

public abstract class AbstractRuinaAct(int actNumber) : CustomActModel(actNumber)
{
    public enum RuinaFloor
    {
        Malkuth,
        Yesod,
        Hod,
        Netzach,
        Tiphereth,
        Gebura,
        Chesed,
        Binah,
        Hokma,
        Keter,
        Guests,
        BlackSilence
    }

    public static RuinaFloor GetFloorBasedOnBoss()
    {
        if (RunManager.Instance.State?.Act.BossEncounter is RedWolfEncounterBoss)
        {
            return RuinaFloor.Gebura;
        }
        return RuinaFloor.Gebura;
    }
    
    public static string GetBGMBasedOnBoss()
    {
        string path = "";
        var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        if (localPlayer?.Creature.CombatState?.Encounter is RedWolfEncounterBoss)
        {
            path = "Roland1.ogg";
        }
        return path.MusicPath().SimplifyPath();
    }
    
    public static string GetBGMBasedOnElite()
    {
        // var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        // if (localPlayer?.Creature.CombatState?.Encounter is RedWolfEncounter)
        // {
        //     
        // }
        return "Warning2.ogg".MusicPath().SimplifyPath();
    }
    
    public static string GetBGMBasedOnFloor(RuinaFloor floor)
    {
        string path = "";
        switch (floor)
        {
            case RuinaFloor.Malkuth:
                path = "Malkuth2.ogg";
                break;
            case RuinaFloor.Yesod:
                path = "Yesod2.ogg";
                break;
            case RuinaFloor.Hod:
                path = "Hod1.ogg";
                break;
            case RuinaFloor.Netzach:
                path = "Netzach2.ogg";
                break;
            case RuinaFloor.Tiphereth:
                path = "Tiphereth2.ogg";
                break;
            case RuinaFloor.Gebura:
                path = "Gebura2.ogg";
                break;
            case RuinaFloor.Chesed:
                path = "Chesed2.ogg";
                break;
            case RuinaFloor.Binah:
                path = "Binah2.ogg";
                break;
            case RuinaFloor.Hokma:
                path = "Hokma2.ogg";
                break;
            case RuinaFloor.Keter:
                path = "Keter1.ogg";
                break;
            default:
                path = "Gebura2.ogg";
                break;
        }
        return path.MusicPath().SimplifyPath();
    }
}