using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Monsters.Act3.Twilight;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class LongArms() : Ruina2Power
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
        if (target.Monster is not Twilight)
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
        modifiedAmount = 0M;
        return true;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        Flash();
    }
}