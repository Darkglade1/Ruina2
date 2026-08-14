using Godot;
using MegaCrit.Sts2.Core.Assets;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Nodes;

public partial class WorshipperSuicideEffect : VfxEffect
{
    private const string TexturePath = "res://Ruina2/images/vfx/Suicide.png";

    private Sprite2D _sprite;
    private float _startingX;
    private float _startingY;
    private float targetX = 960.0f;
    private float targetY = 250.0f;
    private float startingScale = 1.0f;
    private float endScale = 0f;

    public static WorshipperSuicideEffect Create(Vector2 target)
    {
        var effect = new WorshipperSuicideEffect();
        
        effect._startingX = target.X;
        effect._startingY = target.Y;

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
        EndDuration = 2.0f;
        
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
        var currX = Lerp(_startingX, targetX, Duration / EndDuration);
        float currY = Lerp(_startingY, targetY, Duration / EndDuration);
        var currScale = Lerp(startingScale, endScale, Duration / EndDuration);
        Scale = new Vector2(currScale, currScale);
        Position = new Vector2(currX, currY);
        _sprite.Rotation -= delta * 6.98f;
        if (Duration >= EndDuration)
        {
            Sfx.WorshipperSuicide.Play();
            IsDone = true;
        }
    }
}