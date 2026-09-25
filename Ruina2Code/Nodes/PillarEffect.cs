using Godot;
using MegaCrit.Sts2.Core.Assets;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Nodes;

public partial class PillarEffect : VfxEffect
{
    private const string TexturePath = "res://Ruina2/images/vfx/stigma.png";

    private Sprite2D _sprite;
    private float _startingX;
    private float _startingY;
    private float targetX;
    private bool playedSound;

    public static PillarEffect Create(Vector2 start, float targetX)
    {
        var effect = new PillarEffect();
        
        effect._startingX = start.X;
        effect._startingY = start.Y;
        effect.targetX = targetX + 250.0f;

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
        EndDuration = 0.8f;
        
        var textureRegion = ResolveTexture();

        _sprite = new Sprite2D();
        _sprite.Texture = textureRegion;
        AddChild(_sprite);
        
        GlobalPosition = new Vector2(_startingX, _startingY);
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        if (Duration >= 0.1f && !playedSound)
        {
            Sfx.BinahStoneFire.Play();
            playedSound = true;
        }
        var currX = Lerp(_startingX, targetX, Duration / EndDuration);
        GlobalPosition = new Vector2(currX, _startingY);
        if (Duration >= EndDuration)
        {
            IsDone = true;
        }
    }
}