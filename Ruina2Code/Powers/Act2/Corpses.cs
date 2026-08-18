using MegaCrit.Sts2.Core.Entities.Powers;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class Corpses() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;
    
    public override bool ShouldOwnerDeathTriggerFatal() => false;
}