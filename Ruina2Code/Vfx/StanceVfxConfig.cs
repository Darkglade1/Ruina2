using Godot;

namespace Ruina2.Ruina2Code.Vfx;

public record StanceVfxConfig(
    string? AuraScenePath = null,
    Color? BodyTint = null
)
{
    public IEnumerable<string> AssetPaths
    {
        get
        {
            if (AuraScenePath != null) yield return AuraScenePath;
        }
    }
}