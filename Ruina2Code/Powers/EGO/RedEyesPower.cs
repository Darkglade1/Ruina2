using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class RedEyesPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Decimal ModifyPowerAmountGivenMultiplicative(
        PowerModel power,
        Creature giver,
        Decimal amount,
        Creature? target,
        CardModel? cardSource)
    {
        if (power.Type == PowerType.Debuff && !(power is RedEyesPower) && target == Owner)
        {
            return 1 + (Amount / 100M);
        }
        return 1;
    }

    public override async Task AfterModifyingPowerAmountGiven(PowerModel power)
    {
        if (power.Type == PowerType.Debuff && !(power is RedEyesPower) && power.Owner == Owner)
        {
            await PowerCmd.Remove(this);
        }
    }
}