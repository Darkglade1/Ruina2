using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Ruina2.Ruina2Code.Nodes;

public partial class OzShardsEffect : VfxEffect
{
    private const string TexturePath1 = "res://Ruina2/images/vfx/OzShard1.png";
    private const string TexturePath2 = "res://Ruina2/images/vfx/OzShard2.png";
    
    private List<Sprite2D> shards = new List<Sprite2D>();
    private List<Vector2> velocities = new List<Vector2>();
    private float _startingX;
    private float _startingY;

    private float shardSpeed = 300;

    public static OzShardsEffect Create(Vector2 target)
    {
        var effect = new OzShardsEffect();
        
        effect._startingX = target.X;
        effect._startingY = target.Y;

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
        EndDuration = 1.5f;
        
        var textureRegion1 = ResolveTexture(TexturePath1);
        var textureRegion2 = ResolveTexture(TexturePath2);

        Random rng = new Random();
        for (int i = 0; i < rng.Next(16, 24); i++)
        {
            Sprite2D sprite = new Sprite2D();
            if (rng.Next(0, 2) == 0)
            {
                sprite.Texture = textureRegion1;
            }
            else
            {
                sprite.Texture = textureRegion2;
            }
            sprite.Position = new Vector2(_startingX, _startingY);
            shards.Add(sprite);
            AddChild(sprite);

            Vector2 velocity = new Vector2((float)(rng.NextDouble() * 2 - 1), (float)(rng.NextDouble() * 2 - 1));
            velocities.Add(velocity);
        }
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        for (int i = 0; i < shards.Count; i++)
        {
            Sprite2D shard = shards[i];
            Vector2 velocity = velocities[i];
            shard.Position += velocity * shardSpeed * delta;
            shard.Rotation += delta * 1.74f;
            var currFade = Lerp(1.0f, 0.0f, Duration / EndDuration);
            shard.Modulate = new Color(1, 1, 1, currFade);
        }
        if (Duration >= EndDuration)
        {
            IsDone = true;
        }
    }
}