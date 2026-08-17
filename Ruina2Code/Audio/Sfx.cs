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
    
    public static readonly ModSound CollectorCurse = new("collector_debuff.ogg".SfxPath());
    
    public static readonly ModSound HouseBoom = new("House_HouseBoom.ogg".SfxPath());
    public static readonly ModSound LionPoison = new("House_Lion_Poison.ogg".SfxPath());
    public static readonly ModSound MakeRoad = new("House_MakeRoad.ogg".SfxPath());
    public static readonly ModSound HouseAttack = new("House_NormalAtk.ogg".SfxPath());
    public static readonly ModSound LionChange = new("House_Lion_Change.ogg".SfxPath());
    
    public static readonly ModSound FragmentStab = new("Cosmos_Stab_Down.ogg".SfxPath());
    public static readonly ModSound FragmentSing = new("Cosmos_Sing.ogg".SfxPath());
    
    public static readonly ModSound ButterflyAtk = new("ButterFlyMan_ButterflyAtk.ogg".SfxPath());
    public static readonly ModSound FuneralReady = new("ButterFlyMan_StrongReady_Boss.ogg".SfxPath());
    public static readonly ModSound FuneralAtkBlack = new("ButterFlyMan_StongAtk_Black.ogg".SfxPath());
    public static readonly ModSound FuneralAtkWhite = new("ButterFlyMan_StongAtk_White.ogg".SfxPath());
    
    public static readonly ModSound HelperOn = new("Helper_On.ogg".SfxPath());
    public static readonly ModSound HelperCharge = new("Helper_FullCharge.ogg".SfxPath());
    public static readonly ModSound AlriuneHori = new("Ali_Boss_Hori.ogg".SfxPath());
    public static readonly ModSound AlriuneGuard = new("Ali_Guard.ogg".SfxPath());
    public static readonly ModSound LaetitiaAtk = new("Laetitia_Atk.ogg".SfxPath());
    public static readonly ModSound LaetitiaFriendAtk = new("Laetitia_Friend_Stab.ogg".SfxPath());
    
    public static readonly ModSound SwordStab = new("Sword_Stab.ogg".SfxPath());
    public static readonly ModSound SwordVert = new("Sword_Vert.ogg".SfxPath());
    public static readonly ModSound SwordHori = new("Sword_Hori.ogg".SfxPath());
    
    public static readonly ModSound FairySpecial = new("Fairy_Special.ogg".SfxPath());
    public static readonly ModSound FairyMinionAtk = new("Fairy_MiniAtk.ogg".SfxPath());
    public static readonly ModSound FairyQueenAtk = new("Fairy_QueenAtk.ogg".SfxPath());
    public static readonly ModSound FairyQueenChange = new("Fairy_QueenChange.ogg".SfxPath());
    public static readonly ModSound FairyQueenEat = new("Fairy_QueenEat.ogg".SfxPath());
    
    public static readonly ModSound PorccuStrongStab2 = new("Porccu_Strong_Stab2.ogg".SfxPath());
    public static readonly ModSound PorccuPenetrate = new("Porccu_Penetrate.ogg".SfxPath());
    
    public static readonly ModSound MatchExplode = new("MatchGirl_Explosion.ogg".SfxPath());
    public static readonly ModSound MatchSizzle = new("MatchGirl_Barrier.ogg".SfxPath());
    
    public static readonly ModSound TeddyOn = new("Teddy_On.ogg".SfxPath());
    public static readonly ModSound TeddyBlock = new("Teddy_Guard.ogg".SfxPath());
    public static readonly ModSound TeddyAtk = new("Teddy_NormalAtk.ogg".SfxPath());
    
    public static readonly ModSound OrchestraFinale = new("Sym_movement_5_finale.ogg".SfxPath());
    public static readonly ModSound OrchestraMovement1 = new("Sym_Chor_Atk.ogg".SfxPath());
    public static readonly ModSound OrchestraMovement2 = new("Sym_movement_5.ogg".SfxPath());
    public static readonly ModSound OrchestraClap = new("Sym_movement_0_clap.ogg".SfxPath());
    
    public static readonly ModSound ShoesOn = new("RedShoes_On3.ogg".SfxPath());
    public static readonly ModSound ShoesAtk = new("RedShoes_Atk.ogg".SfxPath());
    
    public static readonly ModSound GalaxyDef = new("GalaxyBoy_FriendDef.ogg".SfxPath());
    public static readonly ModSound GalaxyAtk = new("GalaxyBoy_FriendAtk.ogg".SfxPath());
    
    public static readonly ModSound QueenBeeStab = new("QueenBee_Queen_Stab.ogg".SfxPath());
    public static readonly ModSound QueenBeeBuff = new("QueenBee_AtkBuff.ogg".SfxPath());
    public static readonly ModSound QueenBeeLegAtk = new("QueenBee_BeeAtk_leg.ogg".SfxPath());
    
    public static readonly ModSound NothingStrong = new("NothingThere_Strong_Flesh.ogg".SfxPath());
    public static readonly ModSound NothingChange = new("NothingThere_Change.ogg".SfxPath());
    public static readonly ModSound NothingHello = new("NothingThere_Hello.ogg".SfxPath());
    public static readonly ModSound NothingNormal = new("NothingThere_Normal_Flesh.ogg".SfxPath());
    public static readonly ModSound NothingGoodbye = new("NothingThere_Goodbye.ogg".SfxPath());
    
    public static readonly ModSound BulletShot = new("Matan_NormalShot.ogg".SfxPath());
    public static readonly ModSound BulletFlame = new("Matan_Flame.ogg".SfxPath());
    public static readonly ModSound BulletFinalShot = new("Matan_FinalShot.ogg".SfxPath());
    
    public static readonly ModSound ShyAtk = new("Shy_Atk.ogg".SfxPath());
    
    public static readonly ModSound FingerSnap = new("Finger_Snapping.ogg".SfxPath());
    
    public static readonly ModSound ProphetBless = new("WhiteNight_Bless.ogg".SfxPath());
    public static readonly ModSound WhiteNightAppear = new("WhiteNight_Appear.ogg".SfxPath());
    public static readonly ModSound WhiteNightCall = new("WhiteNight_Call.ogg".SfxPath());
    public static readonly ModSound WhiteNightCharge = new("WhiteNight_Strong_Charge.ogg".SfxPath());
    public static readonly ModSound WhiteNightFire = new("WhiteNight_Strong_Fire.ogg".SfxPath());
    public static readonly ModSound WhiteNightSummon = new("WhiteNight_Apostle_Grogy.ogg".SfxPath());
    
    public static readonly ModSound BigBirdLamp = new("Bigbird_Attract.ogg".SfxPath());
    public static readonly ModSound BigBirdEyes = new("Bigbird_Eyes.ogg".SfxPath());
    public static readonly ModSound BigBirdCrunch = new("Bigbird_HeadCut.ogg".SfxPath());
    public static readonly ModSound BigBirdOpen = new("Bigbird_MouseOpen.ogg".SfxPath());
    
    public static readonly ModSound BlueStarAtk = new("BlueStar_Atk.ogg".SfxPath());
    public static readonly ModSound BlueStarCharge = new("BlueStar_Cast.ogg".SfxPath());
    public static readonly ModSound WorshipperSuicide = new("BlueStar_In.ogg".SfxPath());
    public static readonly ModSound WorshipperAttack = new("BlueStar_SubAtk.ogg".SfxPath());
    public static readonly ModSound WorshipperExplode = new("BlueStar_Suicide.ogg".SfxPath());
    
    public static readonly ModSound SnowAttack = new("SnowQueen_Atk.ogg".SfxPath());
    public static readonly ModSound SnowAttackFar = new("SnowQueen_Atk_Far.ogg".SfxPath());
    public static readonly ModSound SnowBlizzard = new("SnowQueen_Freeze.ogg".SfxPath());
    public static readonly ModSound SnowPrisonBreak = new("SnowQueen_IceCrash.ogg".SfxPath());
    
    public static readonly ModSound BossBirdSpecial = new("Bossbird_Longbird_On.ogg".SfxPath());
    public static readonly ModSound BossBirdLamp = new("Bossbird_Bigbird_FarAtk.ogg".SfxPath());
    public static readonly ModSound BossBirdBirth = new("BossBird_Birth.ogg".SfxPath());
    public static readonly ModSound BossBirdCrush = new("Bossbird_Bossbird_Stab.ogg".SfxPath());
    public static readonly ModSound BossBirdStrong = new("Bossbird_Bossbird_StrongAtk.ogg".SfxPath());
    public static readonly ModSound BossBirdSlam = new("Bossbird_Bossbird_VertDown.ogg".SfxPath());
    public static readonly ModSound BossBirdPunish = new("Bossbird_Longbird_StrongAtk.ogg".SfxPath());
    
    public static readonly ModSound BloodAttack = new("Bloodbath_Atk.ogg".SfxPath());
    public static readonly ModSound BloodSpecial = new("Bloodbath_EyeOn.ogg".SfxPath());
    
    public static readonly ModSound BirdSweep = new("LongBird_SubAtk.ogg".SfxPath());
    public static readonly ModSound BirdShout = new("LongBird_SubShout.ogg".SfxPath());
    public static readonly ModSound JudgementAttack = new("LongBird_Down.ogg".SfxPath());
    public static readonly ModSound JudgementHang = new("LongBird_Hang.ogg".SfxPath());
    public static readonly ModSound JudgementGong = new("LongBird_On.ogg".SfxPath());
    public static readonly ModSound JudgementDing = new("LongBird_Stun.ogg".SfxPath());
    
    public static readonly ModSound HeavenWakeStrong = new("MustSee_Wake_Strong.ogg".SfxPath());
    public static readonly ModSound HeavenNosee1 = new("MustSee_Nosee1.ogg".SfxPath());
    
    public static readonly ModSound PinoLie = new("Pino_Lie.ogg".SfxPath());
    public static readonly ModSound PinoFail = new("Pino_Fail.ogg".SfxPath());
    public static readonly ModSound PinoOn = new("Pino_On.ogg".SfxPath());
    
    public static readonly ModSound SmallBirdPeck = new("SmallBird_Atk.ogg".SfxPath());
    public static readonly ModSound SmallBirdPunish = new("SmallBird_StrongAtk.ogg".SfxPath());
    
    public static readonly ModSound SilenceEffect = new("Clock_NoCreate.ogg".SfxPath());
    public static readonly ModSound SilenceStop = new("Clock_StopCard.ogg".SfxPath());
    
    public static readonly ModSound SwanVertDown = new("BlackSwan_VertDown.ogg".SfxPath());
    public static readonly ModSound SwanGuard = new("BlackSwan_Guard.ogg".SfxPath());
    public static readonly ModSound SwanPierce = new("BlackSwan_Pierce.ogg".SfxPath());
    public static readonly ModSound SwanRevive = new("BlackSwan_Revive.ogg".SfxPath());
    public static readonly ModSound SwanShout = new("BlackSwan_Shout.ogg".SfxPath());
    
    public static readonly ModSound OzStrongAtkStart = new("Oz_StongAtk_Start.ogg".SfxPath());
    public static readonly ModSound OzStrongAtkDown = new("Oz_StongAtk_Down.ogg".SfxPath());
    public static readonly ModSound OzStrongAtkFinish = new("Oz_StongAtk_Finish.ogg".SfxPath());
    public static readonly ModSound OzAtkBoom = new("Oz_Atk_Boom.ogg".SfxPath());
    public static readonly ModSound OzAtkUp = new("Oz_Atk_Up.ogg".SfxPath());
    public static readonly ModSound OzMagic = new("Oz_ChangeMagic.ogg".SfxPath());
    
    public static readonly ModSound SmokeAtk = new("Cor_S1.ogg".SfxPath());
}