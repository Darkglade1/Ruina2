using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Ruina2.Ruina2Code.Nodes;

public partial class RedLinesEffect : VfxEffect
{
    private const string TexturePath = "res://Ruina2/images/vfx/Line.png";

    private List<Sprite2D> lines = new List<Sprite2D>();
    private Sprite2D _sprite;
    private float startingScale = 0.0f;
    private float endScale = 1.5f;
    private float StartFadeDuration = 1.0f;

    public static RedLinesEffect Create()
    {
        var effect = new RedLinesEffect();
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
        EndDuration = 1.5f;
        int numStrings = 20;
        var textureRegion = ResolveTexture();
        
        Random rng = new Random();
        for (int i = 0; i < numStrings; i++)
        {
            Sprite2D sprite = new Sprite2D();
            sprite.Texture = textureRegion;
            sprite.Position = new Vector2(1920.0f / numStrings * i, 1100.0f);
            sprite.RotationDegrees = rng.Next(45, 135);
            sprite.Scale = new Vector2(startingScale, startingScale);
            lines.Add(sprite);
            AddChild(sprite);
        }
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        for (int i = 0; i < lines.Count; i++)
        {
            Sprite2D line = lines[i];
            var currScale = Lerp(startingScale, endScale, Duration / EndDuration);
            line.Scale = new Vector2(currScale, currScale);
            if (Duration >= StartFadeDuration)
            {
                var currFade = Lerp(1.0f, 0.0f, (Duration - StartFadeDuration) / (EndDuration - StartFadeDuration));
                line.Modulate = new Color(1, 1, 1, currFade);
            }
        }
      
        if (Duration >= EndDuration)
        {
            IsDone = true;
        }
    }
}