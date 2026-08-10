using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Ruina2.Ruina2Code.Nodes;

public partial class FullScreenImageEffect : VfxEffect
{
    private string TexturePath = "";
    private TextureRect _texture;
    private float fullScreenDuration;
    private float fadeOutDuration;

    public static FullScreenImageEffect Create(string texturePath, float fullScreenDuration, float fadeOutDuration)
    {
        var effect = new FullScreenImageEffect();
        
        effect.TexturePath = texturePath;
        effect.fullScreenDuration = fullScreenDuration;
        effect.fadeOutDuration = fadeOutDuration;
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
        
        _texture = new TextureRect();
        _texture.Texture = textureRegion;
        _texture.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        AddChild(_texture);
        
        Position = new Vector2(0, 0);
    }

    protected override void Update(float delta)
    {
        Duration += delta;
        if (Duration > fullScreenDuration)
        {
            var progress = ((fullScreenDuration + fadeOutDuration) - Duration) / fadeOutDuration;
            _texture.Modulate = new Color(1, 1, 1, progress);
        }
        if (Duration >= fullScreenDuration + fadeOutDuration)
        {
            IsDone = true;
        }
    }
}