using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class ContractLibertyPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(0)];

    public override Decimal ModifyMaxEnergy(Player player, Decimal amount)
    {
        if (player == Owner.Player)
        {
            return amount - DynamicVars.Energy.IntValue;
        }
        return amount;
    }
    
    public override Decimal ModifyHandDraw(Player player, Decimal count)
    {
        return player != this.Owner.Player ? count : count + (Decimal) this.Amount;
    }
}