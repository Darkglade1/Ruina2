using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Ruina2.Ruina2Code.Nodes;

public partial class FullScreenAnimationEffect : VfxEffect
{
    private string TexturePath = "";
    private TextureRect _texture;
    private float fullScreenDuration;
    private float frameInterval = 0.1f;
    private float frameTimer = 0;
    private int frameCounter = 1;
    private int numFrames;

    public static FullScreenAnimationEffect Create(string texturePath, float fullScreenDuration, int numFrames)
    {
        var effect = new FullScreenAnimationEffect();
        
        effect.TexturePath = texturePath;
        effect.fullScreenDuration = fullScreenDuration;
        effect.numFrames = numFrames;
        effect.Setup();
        return effect;
    }
    
    private Texture2D ResolveTexture(int frame)
    {
        return PreloadManager.Cache.GetAsset<Texture2D>(TexturePath + frame + ".png");
    }

    protected override void Initialize()
    {
        Duration = 0f;
        var textureRegion = ResolveTexture(frameCounter);
        
        _texture = new TextureRect();
        _texture.Texture = textureRegion;
        _texture.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        AddChild(_texture);
        
        Position = new Vector2(0, 0);
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        frameTimer += delta;
        if (frameTimer >= frameInterval && frameCounter < numFrames)
        {
            frameTimer = 0;
            frameCounter++;
            var textureRegion = ResolveTexture(frameCounter);
            _texture.Texture = textureRegion;
        }
        if (Duration >= fullScreenDuration)
        {
            IsDone = true;
        }
    }
}