using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Encounters.Act1;
using Ruina2.Ruina2Code.Encounters.Act2;
using Ruina2.Ruina2Code.Encounters.Act3;
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
        if (RunManager.Instance.State?.Act.BossEncounter is FairyBoss)
        {
            return RuinaFloor.Malkuth;
        }
        if (RunManager.Instance.State?.Act.BossEncounter is NothingDerBoss)
        {
            return RuinaFloor.Yesod;
        }
        if (RunManager.Instance.State?.Act.BossEncounter is OrchestraBoss)
        {
            return RuinaFloor.Netzach;
        }
        if (RunManager.Instance.State?.Act.BossEncounter is RedWolfBoss)
        {
            return RuinaFloor.Gebura;
        }
        if (RunManager.Instance.State?.Act.BossEncounter is JesterBoss)
        {
            return RuinaFloor.Tiphereth;
        }
        if (RunManager.Instance.State?.Act.BossEncounter is WhiteNightBoss)
        {
            return RuinaFloor.Hokma;
        }
        return RuinaFloor.Gebura;
    }
    
    public static string GetBGMBasedOnBoss()
    {
        string path = "Roland2.ogg";
        var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        var encounter = localPlayer?.Creature.CombatState?.Encounter;
        if (encounter is FairyBoss)
        {
            path = "Angela2.ogg";
        }
        if (encounter is NothingDerBoss)
        {
            path = "Warning3.ogg";
        }
        if (encounter is OrchestraBoss || encounter is WhiteNightBoss)
        {
            path = "Angela3.ogg";
        }
        if (encounter is RedWolfBoss)
        {
            path = "Roland1.ogg";
        }
        if (encounter is JesterBoss)
        {
            path = "Roland3.ogg";
        }
        return path.MusicPath().SimplifyPath();
    }
    
    public static string GetBGMBasedOnElite()
    {
        string path = "Warning2.ogg";
        var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        var encounter = localPlayer?.Creature.CombatState?.Encounter;
        if (encounter is HelpersElite || encounter is LaetitiaElite || encounter is RoadHomeElite)
        {
            path = "Warning1.ogg";
        }
        if (encounter is AlriuneElite || encounter is WrathElite || encounter is BigBirdElite)
        {
            path = "Warning2.ogg";
        }
        if (encounter is MountainElite)
        {
            path = "Warning3.ogg";
        }
        return path.MusicPath().SimplifyPath();
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
    
    protected override string CustomMapTopBgPath
    {
        get {
            string path;
            switch (AbstractRuinaAct.GetFloorBasedOnBoss())
            {
                case AbstractRuinaAct.RuinaFloor.Malkuth:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Yesod:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Hod:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Netzach:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Tiphereth:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Gebura:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Chesed:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Binah:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Hokma:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                case AbstractRuinaAct.RuinaFloor.Keter:
                    path = ModelDb.Act<Hive>().MapTopBgPath;
                    break;
                default:
                    path = "map_top_test.png".UIImagePath();
                    break;
            }
            return path;
        }
    }
    
    protected override string CustomMapMidBgPath => ModelDb.Act<Hive>().MapMidBgPath;
    protected override string CustomMapBotBgPath => ModelDb.Act<Hive>().MapBotBgPath;
    protected override string CustomRestSiteBackgroundPath => ModelDb.Act<Hive>().RestSiteBackgroundPath;
}