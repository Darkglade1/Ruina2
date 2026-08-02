using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Ruina2.Ruina2Code.Nodes;

/// <summary>
///     Shared VFX utilities for stance visual effects.
/// </summary>
public static class StanceVfx
{
    /// <summary>
    ///     Scale factor for all stance VFX positions and sizes.
    ///     Adjust this if the watcher character gets resized.
    /// </summary>
    public const float VfxScale = 0.9f;

    /// <summary>
    ///     Fetches a texture from the preload cache, re-fetching if the cached
    ///     handle was disposed by a cache eviction ("Unloading N missed cache assets").
    ///     Returns false if a valid texture can't be obtained, so callers can stop
    ///     spawning instead of assigning a disposed object to a sprite.
    /// </summary>
    public static bool TryGetTexture(string path, ref Texture2D cached)
    {
        if (!GodotObject.IsInstanceValid(cached))
            cached = PreloadManager.Cache.GetAsset<Texture2D>(path);
        return GodotObject.IsInstanceValid(cached);
    }
}