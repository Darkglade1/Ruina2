using Godot;
using MegaCrit.Sts2.Core.Assets;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Nodes;

public partial class ShockwaveEffect : VfxEffect
{
    private const string TexturePath = "res://Ruina2/images/vfx/GlowingCircle.png";

    private Sprite2D _sprite;
    private float _startingX;
    private float _startingY;
    private bool finishedCharging = false;
    private float chargeDuration = 1.2f;
    private float burstDuration = 0.3f;
    private float startingScale = 4.0f;
    private float endingScale = 16.0f;

    public static ShockwaveEffect Create(Vector2 center)
    {
        var effect = new ShockwaveEffect();
        
        effect._startingX = center.X;
        effect._startingY = center.Y;

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
        
        var textureRegion = ResolveTexture();

        _sprite = new Sprite2D();
        _sprite.Texture = textureRegion;
        _sprite.Scale = new Vector2(startingScale, startingScale);
        AddChild(_sprite);
        
        Position = new Vector2(_startingX, _startingY);
        
        Sfx.WhiteNightCharge.Play();
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        if (!finishedCharging)
        {
            var currScale = Lerp(startingScale, 0, Duration / chargeDuration);
            _sprite.Scale = new Vector2(currScale, currScale);
        }
        if (Duration >= chargeDuration && !finishedCharging)
        {
            finishedCharging = true;
            Duration = 0;
            Sfx.WhiteNightFire.Play();
        }

        if (finishedCharging)
        {
            var currScale = Lerp(0, endingScale, Duration / burstDuration);
            _sprite.Scale = new Vector2(currScale, currScale);
        }
        if (Duration >= burstDuration && finishedCharging)
        {
            IsDone = true;
        }
        _sprite.Rotation -= delta * 10.47f;
    }
}