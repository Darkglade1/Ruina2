using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards.EGO.Act1;
using Ruina2.Ruina2Code.Cards.EGO.Act2;
using Ruina2.Ruina2Code.Cards.EGO.Act3;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Cards.EGO;

public class EGOCardPool : CustomCardPoolModel
{
    public override string Title => "EGO";

    public override string BigEnergyIconPath => "cards/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "cards/text_energy.png".ImagePath();


    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    public override float H => 1.0f; //Hue; changes the color.
    public override float S => 1.0f; //Saturation
    public override float V => 1.0f; //Brightness

    //Color of small card icons
    public override Color DeckEntryCardColor => Color.Color8(69, 2, 30);
    
    public override bool IsShared => true;
    public override bool IsColorless => true;
    
    public static List<CardModel> GetAct1EgoCards()
    {
        List<CardModel> cards = new List<CardModel>();
        cards.Add(ModelDb.Card<BlackSwan>());
        cards.Add(ModelDb.Card<DaCapo>());
        cards.Add(ModelDb.Card<FaintAroma>());
        cards.Add(ModelDb.Card<FourthMatchFlame>());
        cards.Add(ModelDb.Card<FragmentsFromSomewhere>());
        cards.Add(ModelDb.Card<GreenStem>());
        cards.Add(ModelDb.Card<Grinder>());
        cards.Add(ModelDb.Card<Harmony>());
        cards.Add(ModelDb.Card<Hornet>());
        cards.Add(ModelDb.Card<Laetitia>());
        cards.Add(ModelDb.Card<MagicBullet>());
        cards.Add(ModelDb.Card<OurGalaxy>());
        cards.Add(ModelDb.Card<Pleasure>());
        cards.Add(ModelDb.Card<RedEyes>());
        cards.Add(ModelDb.Card<Regret>());
        cards.Add(ModelDb.Card<SanguineDesire>());
        cards.Add(ModelDb.Card<SolemnLament>());
        cards.Add(ModelDb.Card<TheForgotten>());
        cards.Add(ModelDb.Card<TodaysExpression>());
        cards.Add(ModelDb.Card<Wingbeat>());
        return cards;
    }

    public static List<CardModel> GetAct2EgoCards()
    {
        List<CardModel> cards = new List<CardModel>();
        cards.Add(ModelDb.Card<BlindRage>());
        cards.Add(ModelDb.Card<CobaltScar>());
        cards.Add(ModelDb.Card<CrimsonScar>());
        cards.Add(ModelDb.Card<GoldRush>());
        cards.Add(ModelDb.Card<LoveAndHate>());
        cards.Add(ModelDb.Card<FadedMemories>());
        cards.Add(ModelDb.Card<FalseThrone>());
        cards.Add(ModelDb.Card<Harvest>());
        cards.Add(ModelDb.Card<HomingInstinct>());
        cards.Add(ModelDb.Card<Lumber>());
        cards.Add(ModelDb.Card<Mimicry>());
        cards.Add(ModelDb.Card<Nihil>());
        cards.Add(ModelDb.Card<Smile>());
        cards.Add(ModelDb.Card<SwordSharpened>());
        cards.Add(ModelDb.Card<Thirst>());
        return cards;
    }
    
    public static List<CardModel> GetAct3EgoCards()
    {
        List<CardModel> cards = new List<CardModel>();
        cards.Add(ModelDb.Card<Apocalypse>());
        cards.Add(ModelDb.Card<Aspiration>());
        cards.Add(ModelDb.Card<Beak>());
        cards.Add(ModelDb.Card<DeadSilence>());
        cards.Add(ModelDb.Card<FrostSplinter>());
        cards.Add(ModelDb.Card<Heaven>());
        cards.Add(ModelDb.Card<Justitia>());
        cards.Add(ModelDb.Card<Lamp>());
        cards.Add(ModelDb.Card<Marionette>());
        cards.Add(ModelDb.Card<ParadiseLost>());
        cards.Add(ModelDb.Card<Penitence>());
        cards.Add(ModelDb.Card<Remorse>());
        cards.Add(ModelDb.Card<SoundOfAStar>());
        cards.Add(ModelDb.Card<Twilight>());
        cards.Add(ModelDb.Card<WristCutter>());
        return cards;
    }
}