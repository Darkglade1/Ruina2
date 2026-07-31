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
    
    public static readonly ModSound GreedGetPower = new("Greed_GetPower.ogg".SfxPath());
    public static readonly ModSound GreedBlunt = new("Greed_Stab.ogg".SfxPath());
    public static readonly ModSound GreedSlam = new("Greed_StrongAtk.ogg".SfxPath());
    public static readonly ModSound GreedDiamond = new("Greed_MakeDiamond.ogg".SfxPath());
    public static readonly ModSound GreedStabChange = new("Greed_Stab_Change.ogg".SfxPath());
    public static readonly ModSound GreedStrAtkChange = new("Greed_StrongAtk_Change.ogg".SfxPath());
    public static readonly ModSound GreedVertChange = new("Greed_Vert_Change.ogg".SfxPath());
    public static readonly ModSound GreedStrAtkReady = new("Greed_StrongAtk_Ready.ogg".SfxPath());
    
    public static readonly ModSound KnightAttack = new("KnightOfDespair_Atk_Strong.ogg".SfxPath());
    public static readonly ModSound KnightChange = new("KnightOfDespair_Change.ogg".SfxPath());
    public static readonly ModSound KnightGaho = new("KnightOfDespair_Gaho.ogg".SfxPath());
    public static readonly ModSound KnightVertGaho = new("KnightOfDespair_Vert_gaho.ogg".SfxPath());
    
    public static readonly ModSound MagicAttack = new("MagicalGirl_Atk.ogg".SfxPath());
    public static readonly ModSound MagicKiss = new("MagicalGirl_kiss.ogg".SfxPath());
    public static readonly ModSound MagicGun = new("MagicalGirl_Gun.ogg".SfxPath());
    public static readonly ModSound MagicSnakeAtk = new("MagicalGirl_SnakeAtk.ogg".SfxPath());
    public static readonly ModSound MagicSnakeGun = new("MagicalGirl_SnakeAtk_gun.ogg".SfxPath());
    
    public static readonly ModSound Rake = new("Scarecrow_Atk2.ogg".SfxPath());
    public static readonly ModSound Harvest = new("Scarecrow_Drink.ogg".SfxPath());
    public static readonly ModSound ScarecrowDeath = new("Scarecrow_Dead.ogg".SfxPath());
    
    public static readonly ModSound WrathMeet = new("Angry_Meet.ogg".SfxPath());
    public static readonly ModSound HermitAtk = new("Angry_R_Atk.ogg".SfxPath());
    public static readonly ModSound HermitStrongAtk = new("Angry_R_StrongAtk.ogg".SfxPath());
    public static readonly ModSound HermitWand = new("Angry_R_WandHit.ogg".SfxPath());
    public static readonly ModSound WrathStrong1 = new("Angry_StrongAtk1.ogg".SfxPath());
    public static readonly ModSound WrathStrong2 = new("Angry_StrongAtk2.ogg".SfxPath());
    public static readonly ModSound WrathStrong3 = new("Angry_StrongFinish.ogg".SfxPath());
    public static readonly ModSound WrathAtk1 = new("Angry_Vert1.ogg".SfxPath());
    public static readonly ModSound WrathAtk2 = new("Angry_Vert2.ogg".SfxPath());
    
    public static readonly ModSound BluntBlow = new("Blow_Stab.ogg".SfxPath());
    public static readonly ModSound BluntHori = new("Blow_Hori.ogg".SfxPath());
    public static readonly ModSound BluntVert = new("Blow_Vert.ogg".SfxPath());
}