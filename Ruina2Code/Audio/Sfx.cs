using BaseLib.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Audio;

public static class Sfx
{
    public static readonly ModSound WOLF_BITE = new("Wolf_Bite.ogg".SfxPath());
    public static readonly ModSound WOLF_HOWL = new("Wolf_Howl.ogg".SfxPath());
    public static readonly ModSound WOLF_SLASH = new("Wolf_Hori.ogg".SfxPath());
    public static readonly ModSound WOLF_PHASE = new("Wolf_Phase2.ogg".SfxPath());
    public static readonly ModSound WOLF_FOG = new("Wolf_FogChange.ogg".SfxPath());
}