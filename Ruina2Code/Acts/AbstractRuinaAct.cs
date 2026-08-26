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
        if (RunManager.Instance.State?.Act.BossEncounter is BlackSwanBoss)
        {
            return RuinaFloor.Hod;
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
        if (RunManager.Instance.State?.Act.BossEncounter is OzBoss)
        {
            return RuinaFloor.Chesed;
        }
        if (RunManager.Instance.State?.Act.BossEncounter is TwilightBoss)
        {
            return RuinaFloor.Binah;
        }
        if (RunManager.Instance.State?.Act.BossEncounter is WhiteNightBoss)
        {
            return RuinaFloor.Hokma;
        }
        if (RunManager.Instance.State?.Act.BossEncounter is SilentGirlBoss)
        {
            return RuinaFloor.Keter;
        }
        return RuinaFloor.Gebura;
    }
    
    public static string GetBGMBasedOnBoss()
    {
        string path = "Roland2.ogg";
        var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        var encounter = localPlayer?.Creature.CombatState?.Encounter;
        if (encounter is BlackSwanBoss)
        {
            path = "Angela1.ogg";
        }
        if (encounter is FairyBoss)
        {
            path = "Angela2.ogg";
        }
        if (encounter is OrchestraBoss || encounter is WhiteNightBoss)
        {
            path = "Angela3.ogg";
        }
        if (encounter is NothingDerBoss)
        {
            path = "Warning3.ogg";
        }
        if (encounter is RedWolfBoss)
        {
            path = "Roland1.ogg";
        }
        if (encounter is JesterBoss || encounter is OzBoss || encounter is TwilightBoss)
        {
            path = "Roland3.ogg";
        }
        if (encounter is SilentGirlBoss)
        {
            path = "Story2.ogg";
        }
        return path.MusicPath().SimplifyPath();
    }
    
    public static string GetBGMBasedOnElite()
    {
        string path = "Warning2.ogg";
        var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        var encounter = localPlayer?.Creature.CombatState?.Encounter;
        if (encounter is HelpersElite || encounter is LaetitiaElite || encounter is RoadHomeElite || encounter is SnowQueenElite)
        {
            path = "Warning1.ogg";
        }
        if (encounter is AlriuneElite || encounter is WrathElite || encounter is BigBirdElite)
        {
            path = "Warning2.ogg";
        }
        if (encounter is MountainElite || encounter is BlueStarElite)
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
     
    public Color BossIconBgColor
    {
        get {
            
            switch (GetFloorBasedOnBoss())
            {
                case RuinaFloor.Malkuth:
                    return Color.Color8(119, 79, 61);
                case RuinaFloor.Yesod:
                    return Color.Color8(82, 66, 109);
                case RuinaFloor.Hod:
                    return Color.Color8(84, 54, 43);
                case RuinaFloor.Netzach:
                    return Color.Color8(38, 79, 49);
                case RuinaFloor.Tiphereth:
                    return Color.Color8(192, 161, 124);
                case RuinaFloor.Gebura:
                    return new Color(0.5f, 0.2f, 0.2f);
                case RuinaFloor.Chesed:
                    return Color.Color8(50, 89, 134);
                case RuinaFloor.Binah:
                    return Color.Color8(22, 22, 22);
                case RuinaFloor.Hokma:
                    return Color.Color8(84, 84, 84);
                case RuinaFloor.Keter:
                    return Color.Color8(123, 123, 123);
            }
            return new Color("9B9562");
        }
    }

    public override Color MapTraveledColor
    {
        get {
            
            switch (GetFloorBasedOnBoss())
            {
                case RuinaFloor.Tiphereth:
                    return new Color("1D1E2F");
            }
            return Color.Color8(217, 218, 219);
        }
    }
    
    public override Color MapBgColor
    {
        get {
            
            switch (GetFloorBasedOnBoss())
            {
                case RuinaFloor.Tiphereth:
                    return Color.Color8(192, 161, 124);
            }
            return Color.Color8(229, 198, 130);
        }
    }
    
    public override Color MapUntraveledColor
    {
        get {
            
            switch (GetFloorBasedOnBoss())
            {
                case RuinaFloor.Tiphereth:
                    return new Color("36454F");
            }
            return Color.Color8(229, 198, 130);
        }
    }
    
    protected override string CustomMapTopBgPath => $"map/map_top_{GetFloorBasedOnBoss().ToString().ToLowerInvariant()}.png".UIImagePath();
    protected override string CustomMapMidBgPath => $"map/map_middle_{GetFloorBasedOnBoss().ToString().ToLowerInvariant()}.png".UIImagePath();
    protected override string CustomMapBotBgPath => $"map/map_bottom_{GetFloorBasedOnBoss().ToString().ToLowerInvariant()}.png".UIImagePath();
    protected override string CustomRestSiteBackgroundPath => "rest/ruina_rest_site.tscn".BackgroundImagePath();
}