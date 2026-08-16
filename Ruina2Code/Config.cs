using BaseLib.Config;

namespace Ruina2.Ruina2Code;

public class Config: SimpleModConfig
{
    [ConfigSection("Tutorial")]
    public static bool ViewAllyFtue { get; set; } = true;
}