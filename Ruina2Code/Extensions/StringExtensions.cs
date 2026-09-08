using Godot;

namespace Ruina2.Ruina2Code.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", path);
    }

    public static string CardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find card image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big card image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find power image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "powers", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big power image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "big", "relic.png");
    }

    public static string CharacterUiPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "charui", path);
    }
    
    public static string MonsterImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "monsters", path);
    }
    
    public static string UIImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "ui", path);
    }
    
    public static string BackgroundImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "background", path);
    }
    
    public static string EncounterImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "encounters", path);
    }
    
    public static string EventImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "events", path);
    }
    
    public static string PotionImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "potions", path);
    }
    
    public static string VfxImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "vfx", path);
    }
    
    public static string AfflictionImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "afflictions", path);
    }
    
    public static string EnchantmentImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "enchantments", path);
    }
    
    public static string SfxPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "audio", "sfx", path);
    }
    
    public static string MusicPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "audio", "music", path);
    }
}