using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class SenselessWrath() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public const int THRESHOLD = 2;
    private bool ignoreNextStrDown = false;

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
        
        if (canonicalPower.GetTypeForAmount(amount) == PowerType.Debuff) {
            //Handle temp Strength down effects
            if (canonicalPower is StrengthPower) {
                if (Amount >= THRESHOLD && ignoreNextStrDown)
                {
                    ignoreNextStrDown = false;
                    modifiedAmount = amount;
                    return false;
                }
            }
            if (canonicalPower is TemporaryStrengthPower) {
                if (Amount < THRESHOLD)
                {
                    ignoreNextStrDown = true;
                }
            }
            //Actual code
            if (Amount >= THRESHOLD) {
                Flash();
                Amount = 1;
                modifiedAmount = 0M;
                return true;
            } else {
                Amount++;
                modifiedAmount = amount;
                return false;
            }
        }
        modifiedAmount = 0M;
        return true;
    }
}