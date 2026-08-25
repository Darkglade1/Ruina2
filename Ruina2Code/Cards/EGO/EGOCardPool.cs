using BaseLib.Abstracts;
using Godot;
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
    public override float H => 0.93f; //Hue; changes the color.
    public override float S => 0.97f; //Saturation
    public override float V => 0.27f; //Brightness

    //Alternatively, leave these values at 1 and provide a custom frame image.
    // public override Texture2D CustomFrame(CustomCardModel card)
    // {
    //     return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    // }

    //Color of small card icons
    public override Color DeckEntryCardColor => Color.Color8(69, 2, 30);
    
    public override bool IsShared => true;

    public override bool IsColorless => true;
}