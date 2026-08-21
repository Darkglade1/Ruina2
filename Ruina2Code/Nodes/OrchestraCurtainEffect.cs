using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Ruina2.Ruina2Code.Nodes;

public partial class OrchestraCurtainEffect : VfxEffect
{
    private const string CurtainMid = "res://Ruina2/images/vfx/Curtain.png";
    private const string CurtainLeft = "res://Ruina2/images/vfx/CurtainUpperLeft.png";
    private const string CurtainRight = "res://Ruina2/images/vfx/CurtainUpperRight.png";

    private Sprite2D curtainMid1;
    private Sprite2D curtainMid2;
    private Sprite2D curtainLeft;
    private Sprite2D curtainRight;
    
    private float screenWidth;
    private float outCurtainXLeft;
    private float outCurtainXRight;
    private float outCurtainX2Left;
    private float outCurtainX2Right;

    public static OrchestraCurtainEffect Create()
    {
        var effect = new OrchestraCurtainEffect();
        effect.Setup();
        return effect;
    }
    
    private Texture2D ResolveTexture(string path)
    {
        return PreloadManager.Cache.GetAsset<Texture2D>(path);
    }

    protected override void Initialize()
    {
        Duration = 0f;
        EndDuration = 6.0f;
        screenWidth = 1920;
        outCurtainXLeft = 0;
        outCurtainXRight = screenWidth;
        outCurtainX2Left = screenWidth - (screenWidth * 0.7f);
        outCurtainX2Right = screenWidth * 0.7f;

        curtainMid1 = new Sprite2D();
        curtainMid1.Texture = ResolveTexture(CurtainMid);
        AddChild(curtainMid1);
        
        curtainMid2 = new Sprite2D();
        curtainMid2.Texture = ResolveTexture(CurtainMid);
        curtainMid2.Scale = new Vector2(-1, 1);
        AddChild(curtainMid2);
        
        curtainLeft = new Sprite2D();
        curtainLeft.Texture = ResolveTexture(CurtainLeft);
        AddChild(curtainLeft);
        
        curtainRight = new Sprite2D();
        curtainRight.Texture = ResolveTexture(CurtainRight);
        AddChild(curtainRight);
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        if (Duration < 2.1)
        {
            outCurtainXLeft += screenWidth * 0.25f * delta;
            outCurtainXRight -= screenWidth * 0.25f * delta;
            outCurtainX2Left += screenWidth * 0.25f * 0.7f * delta;
            outCurtainX2Right -= screenWidth * 0.25f * 0.7f * delta;
        } else if (Duration > 3.1 && Duration < EndDuration)
        {
            outCurtainXLeft -= screenWidth * 0.25f * delta;
            outCurtainXRight += screenWidth * 0.25f * delta;
            outCurtainX2Left -= screenWidth * 0.25f * 0.7f * delta;
            outCurtainX2Right += screenWidth * 0.25f * 0.7f * delta;
        }
        curtainMid1.Position = new Vector2(outCurtainXLeft, 540);
        curtainMid2.Position = new Vector2(outCurtainXRight, 540);
        curtainLeft.Position = new Vector2(outCurtainX2Left, 540);
        curtainRight.Position = new Vector2(outCurtainX2Right, 540);
        if (Duration >= EndDuration)
        {
            IsDone = true;
        }
    }
}