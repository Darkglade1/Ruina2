using MegaCrit.Sts2.Core.Entities.Powers;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class SwordSharpenedPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.None;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
}