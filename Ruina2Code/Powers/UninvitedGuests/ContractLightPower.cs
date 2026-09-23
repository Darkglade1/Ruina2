using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class ContractLightPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Decimal ModifyMaxEnergy(Player player, Decimal amount)
    {
        return player != this.Owner.Player ? amount : amount + (Decimal) this.Amount;
    }
    
    public override bool ShouldDraw(Player player, bool fromHandDraw)
    {
        return fromHandDraw || player != this.Owner.Player || player.Creature.Side != player.Creature.CombatState.CurrentSide;
    }

    public override Task AfterPreventingDraw()
    {
        this.Flash();
        return Task.CompletedTask;
    }
}