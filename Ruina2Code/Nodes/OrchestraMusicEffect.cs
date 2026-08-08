using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Ruina2.Ruina2Code.Nodes;

public partial class OrchestraMusicEffect : VfxEffect
{
    private string TexturePath = "";
    private Sprite2D _sprite;
    private float _startingX;
    private float _startingY;
    private bool clockwise;
    private float targetScale;
    private float endingScale = 10.0f;
    private bool ending = false;

    public static OrchestraMusicEffect Create(Vector2 target, string texturePath, bool clockwise, float targetScale)
    {
        var effect = new OrchestraMusicEffect();

        float distY = -800.0f;
        effect._startingX = target.X;
        effect._startingY = target.Y;
        effect.TexturePath = texturePath;
        effect.clockwise = clockwise;
        effect.targetScale = targetScale;
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
        _sprite.Scale = new Vector2(0.0f, 0.0f);
        AddChild(_sprite);
        
        Position = new Vector2(_startingX, _startingY);
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        if (ending)
        {
            if (_sprite.Scale.X < endingScale)
            {
                _sprite.Scale = new Vector2(Lerp(targetScale, endingScale, Duration * 1.5f), Lerp(targetScale, endingScale, Duration * 1.5f));
            }
            if (_sprite.Scale.X > endingScale)
            {
                _sprite.Scale = new Vector2(endingScale, endingScale);
                IsDone = true;
            }
        }
        else
        {
            if (_sprite.Scale.X < targetScale)
            {
                _sprite.Scale = new Vector2(Lerp(0.0f, targetScale, Duration), Lerp(0.0f, targetScale, Duration));
            }

            if (_sprite.Scale.X > targetScale)
            {
                _sprite.Scale = new Vector2(targetScale, targetScale);
            }
        }

        if (clockwise)
        {
            _sprite.Rotation += delta * 0.52f;
        }
        else
        {
            _sprite.Rotation -= delta * 0.52f;
        }
    }

    public void End()
    {
        Duration = 0;
        ending = true;
    }
}