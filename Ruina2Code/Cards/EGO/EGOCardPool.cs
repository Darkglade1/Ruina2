using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards.EGO.Act2;
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

    //Alternatively, leave these values at 1 and provide a custom frame image.
    // public override Texture2D CustomFrame(CustomCardModel card)
    // {
    //     return PreloadManager.Cache.GetTexture2D($"cards/frame_{card.Type.ToString().ToLowerInvariant()}_ego.png".ImagePath());
    // }

    //Color of small card icons
    public override Color DeckEntryCardColor => Color.Color8(69, 2, 30);
    
    public override bool IsShared => true;
    public override bool IsColorless => true;

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
}