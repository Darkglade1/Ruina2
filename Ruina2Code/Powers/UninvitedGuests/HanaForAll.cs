using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class HanaForAll() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
    
    public override string CustomPackedIconPath => "gon.png".PowerImagePath();
    public override string CustomBigIconPath => "gon.png".BigPowerImagePath();

    public override Creature ModifyUnblockedDamageTarget(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer)
    {
        if (Owner.IsAlive && target.IsPlayer && props.IsPoweredAttack() && dealer != null && dealer.IsPlayer)
        {
            return Owner;
        }
        return target;
    }
}