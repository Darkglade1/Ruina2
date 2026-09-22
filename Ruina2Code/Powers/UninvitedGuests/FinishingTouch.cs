using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class FinishingTouch() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<Mark>();
}