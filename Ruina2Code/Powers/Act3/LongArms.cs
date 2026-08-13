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
        if (!canonicalPower.IsVisible)
        {
            modifiedAmount = amount;
            return false;
        }
        if ((target == Owner || target.Monster is Twilight) && canonicalPower.GetTypeForAmount(amount) == PowerType.Debuff)
        {
            Flash();
            modifiedAmount = 0M;
            return true;
        }
        modifiedAmount = amount;
        return false;
    }
}