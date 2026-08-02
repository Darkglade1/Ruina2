using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters;
using Ruina2.Ruina2Code.Vfx;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class SenselessWrath() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public const int THRESHOLD = 2;
    private bool ignoreNextStrDown = false;
    
    public override int DisplayAmount => DynamicVars["Counter"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Counter",1)];

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
                if (DynamicVars["Counter"].BaseValue >= THRESHOLD && ignoreNextStrDown)
                {
                    ignoreNextStrDown = false;
                    modifiedAmount = amount;
                    return false;
                }
            }
            if (canonicalPower is TemporaryStrengthPower) {
                if (DynamicVars["Counter"].BaseValue < THRESHOLD)
                {
                    ignoreNextStrDown = true;
                }
            }
            //Actual code
            if (DynamicVars["Counter"].BaseValue >= THRESHOLD) {
                if (Owner.Monster is AbstractRuinaMonster monster)
                {
                    if (monster.StanceVfx != null)
                    {
                        monster.StanceVfx.OnExit(Owner);
                        monster.StanceVfx = null;
                    }
                }
                Flash();
                DynamicVars["Counter"].BaseValue = 1;
                InvokeDisplayAmountChanged();
                modifiedAmount = 0M;
                return true;
            } else {
                if (Owner.Monster is AbstractRuinaMonster monster)
                {
                    monster.StanceVfx = new StanceVfxController(AbstractRuinaMonster.WrathVfxConfig);
                    monster.StanceVfx.OnEnter(Owner);
                }
                DynamicVars["Counter"].BaseValue++;
                InvokeDisplayAmountChanged();
                modifiedAmount = amount;
                return false;
            }
        }
        modifiedAmount = 0M;
        return true;
    }
}