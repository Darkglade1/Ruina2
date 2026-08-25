using BaseLib.Config;

namespace Ruina2.Ruina2Code;

public class Config: SimpleModConfig
{
    [ConfigHideInUI]
    [ConfigIgnoreRestoreDefaults]
    public static bool ViewAllyFtue { get; set; } = true;
    
    [ConfigSection("Misc")]
    public static bool NeowAngelaAppears { get; set; } = true;
}