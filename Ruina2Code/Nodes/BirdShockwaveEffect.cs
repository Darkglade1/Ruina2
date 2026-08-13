using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Ruina2.Ruina2Code.Nodes;

public partial class BirdShockwaveEffect : VfxEffect
{
    private const string TexturePath = "res://Ruina2/images/vfx/Shockwave.png";

    private Sprite2D _sprite;
    private float _startingX;
    private float _startingY;
    private float startingScale = 0.8f;
    private float endScale = 2.2f;

    public static BirdShockwaveEffect Create(Vector2 target)
    {
        var effect = new BirdShockwaveEffect();

        float distY = 100.0f;
        effect._startingX = target.X;
        effect._startingY = distY + target.Y;

        effect.Setup();
        return effect;
    }
    
    private Texture2D ResolveTexture()
    {
        return PreloadManager.Cache.GetAsset<Texture2D>(TexturePath);
    }

    protected override void Initialize()
    {
        Duration = 0f;
        EndDuration = 0.7f;
        
        var textureRegion = ResolveTexture();

        _sprite = new Sprite2D();
        _sprite.Texture = textureRegion;
        AddChild(_sprite);
        
        Position = new Vector2(_startingX, _startingY);
        Scale = new Vector2(startingScale, startingScale);
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        var currScale = Lerp(startingScale, endScale, Duration / EndDuration);
        var currFade = Lerp(1.0f, 0.0f, Duration / EndDuration);
        Scale = new Vector2(currScale, currScale);
        _sprite.Modulate = new Color(1, 1, 1, currFade);
        if (Duration >= EndDuration)
        {
            IsDone = true;
        }
    }
}