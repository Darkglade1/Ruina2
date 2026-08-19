using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Nodes;

public partial class OzCrystalEffect : VfxEffect
{
    private const string TexturePath = "res://Ruina2/images/vfx/OzCrystalFall.png";
    private const string TexturePathHit = "res://Ruina2/images/vfx/OzCrystalHit.png";
    private const string TexturePathGlow = "res://Ruina2/images/vfx/OzGlow.png";

    private Sprite2D _sprite;
    private Sprite2D glowSprite;
    private float _startingX;
    private float _startingY;
    private float _targetY;
    private float GlowDuration;
    private bool hasImpacted = false;
    private float startingScale = 1.0f;
    private float targetScale = 2.5f;

    public static OzCrystalEffect Create(Vector2 target)
    {
        var effect = new OzCrystalEffect();

        float distY = -800.0f;
        effect._startingX = target.X;
        effect._startingY = distY + target.Y;
        effect._targetY = target.Y;

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
        EndDuration = 1.0f;
        GlowDuration = 1.0f;
        
        var textureRegion = ResolveTexture(TexturePath);

        _sprite = new Sprite2D();
        _sprite.Texture = textureRegion;
        AddChild(_sprite);

        glowSprite = new Sprite2D();
        glowSprite.Texture = ResolveTexture(TexturePathGlow);
        
        Position = new Vector2(_startingX, _startingY);
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        if (!hasImpacted)
        {
            var t = EaseIn(Duration / EndDuration);
            if (t > 1.0f)
            {
                t = 1.0f;
            }
            var currY = Lerp(_startingY, _targetY, t);
            Position = new Vector2(_startingX, currY);
        }
        if (Duration >= EndDuration && !hasImpacted)
        {
            hasImpacted = true;
            _sprite.Texture = ResolveTexture(TexturePathHit);
            Sfx.OzStrongAtkDown.Play();
            NGame.Instance?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Short);
            AddChild(glowSprite);
            Duration = 0;
        }
        if (hasImpacted)
        {
            if (glowSprite.Scale.X < targetScale)
            {
                glowSprite.Scale = new Vector2(Lerp(startingScale, targetScale, Duration), Lerp(startingScale, targetScale, Duration));
            }

            if (glowSprite.Scale.X > targetScale)
            {
                glowSprite.Scale = new Vector2(targetScale, targetScale);
            }
            
            if (Duration >= GlowDuration)
            {
                IsDone = true;
            }
        }
    }
}