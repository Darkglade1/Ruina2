using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Nodes;

public partial class HouseDropEffect : VfxEffect
{
    private const string TexturePath = "res://Ruina2/images/vfx/house.png";

    private Sprite2D _sprite;
    private float _startingX;
    private float _startingY;
    private float _targetY;
    private float LingerDuration;
    private float StartFadeOutDuration;
    private bool hasImpacted = false;

    public static HouseDropEffect Create(Vector2 target)
    {
        var effect = new HouseDropEffect();

        float distY = -800.0f;
        effect._startingX = target.X;
        effect._startingY = distY + target.Y;
        effect._targetY = target.Y;

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
        EndDuration = 1.0f;
        StartFadeOutDuration = 1.75f;
        LingerDuration = 2.5f;
        
        var textureRegion = ResolveTexture();

        _sprite = new Sprite2D();
        _sprite.Texture = textureRegion;
        AddChild(_sprite);
        
        Position = new Vector2(_startingX, _startingY);
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        var t = EaseIn(Duration / EndDuration);
        if (t > 1.0f)
        {
            t = 1.0f;
        }
        var currY = Lerp(_startingY, _targetY, t);
        Position = new Vector2(_startingX, currY);
        if (Duration >= EndDuration && !hasImpacted)
        {
            hasImpacted = true;
            Sfx.HouseBoom.Play();
            NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
        }
        if (Duration >= StartFadeOutDuration)
        {
            var diff = LingerDuration - StartFadeOutDuration;
            var progress = (LingerDuration - Duration) / diff;
            _sprite.Modulate = new Color(1, 1, 1, progress);
        }
        if (Duration >= LingerDuration)
        {
            IsDone = true;
        }
    }
}