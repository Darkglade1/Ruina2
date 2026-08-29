namespace Ruina2.Ruina2Code.Afflictions;

public class Punished : Ruina2Affliction
{
    public override string? CustomLocalizationKey => Id.Entry;
    
    public override bool IsStackable => true;
}