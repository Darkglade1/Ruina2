using BaseLib.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Audio;

public static class Sfx
{
    public static readonly ModSound WOLF_BITE = new("Wolf_Bite.ogg".SfxPath());
    public static readonly ModSound WOLF_HOWL = new("Wolf_Howl.ogg".SfxPath());
    public static readonly ModSound WOLF_SLASH = new("Wolf_Hori.ogg".SfxPath());
    public static readonly ModSound WOLF_PHASE = new("Wolf_Phase2.ogg".SfxPath());
    public static readonly ModSound WOLF_FOG = new("Wolf_FogChange.ogg".SfxPath());
    
    public static readonly ModSound LITTLE_RED_SLASH = new("RedHood_Atk1.ogg".SfxPath());
    public static readonly ModSound LITTLE_RED_GUN = new("RedHood_Gun.ogg".SfxPath());
    public static readonly ModSound LITTLE_RED_RAGE = new("RedHood_Rage.ogg".SfxPath());
    
    public static readonly ModSound RAM = new("Danggo_Lv3_Atk.ogg".SfxPath());
    public static readonly ModSound SCREECH = new("Danggo_Lv2_Shout.ogg".SfxPath());
    public static readonly ModSound VOMIT = new("Danggo_Lv3_Special.ogg".SfxPath());
    public static readonly ModSound GROW = new("Danggo_LvUp.ogg".SfxPath());
    public static readonly ModSound SHRINK = new("Danggo_LvDown.ogg".SfxPath());
    public static readonly ModSound SPAWN = new("Danggo_Birth.ogg".SfxPath());
    
    public static readonly ModSound BAT_ATTACK = new("Nosferatu_Atk_Bat.ogg".SfxPath());
    public static readonly ModSound NOS_CHANGE = new("Nosferatu_Change.ogg".SfxPath());
    public static readonly ModSound NOS_BLOOD_EAT = new("Nosferatu_Changed_BloodEat.ogg".SfxPath());
    public static readonly ModSound NOS_GRAB = new("Nosferatu_Changed_Grab.ogg".SfxPath());
    public static readonly ModSound NOS_SPECIAL = new("Nosferatu_Changed_StrongAtk_Start.ogg".SfxPath());
    public static readonly ModSound NOS_SPECIAL_EYE = new("Nosferatu_Changed_StrongAtk_Eye.ogg".SfxPath());
    
    public static readonly ModSound WoodStrike = new("WoodMachine_AtkStrong.ogg".SfxPath());
    public static readonly ModSound WoodFinish = new("WoodMachine_Kill.ogg".SfxPath());
    public static readonly ModSound OzmaGuard = new("Ozma_Guard.ogg".SfxPath());
    public static readonly ModSound OzmaFin = new("Ozma_StrongAtk_Fin.ogg".SfxPath());
    public static readonly ModSound OzmaStart = new("Ozma_StrongAtk_Start.ogg".SfxPath());
}