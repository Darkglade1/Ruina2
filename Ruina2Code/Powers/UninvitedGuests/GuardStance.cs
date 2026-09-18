using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class GuardStance() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        Decimal amount,
        Creature? _,
        out Decimal modifiedAmount)
    {
        if (target != Owner)
        {
            modifiedAmount = amount;
            return false;
        }
        if (canonicalPower.GetTypeForAmount(amount) != PowerType.Debuff)
        {
            modifiedAmount = amount;
            return false;
        }
        if (!canonicalPower.IsVisible)
        {
            modifiedAmount = amount;
            return false;
        }
        
        Flash();
        modifiedAmount = 0M;
        return true;
    }
}