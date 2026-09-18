using MegaCrit.Sts2.Core.Entities.Powers;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class PierceStance() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;
}