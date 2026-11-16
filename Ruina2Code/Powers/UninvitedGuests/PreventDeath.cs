using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Argalia;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

#pragma warning disable STS001
public class PreventDeath() : Ruina2Power
#pragma warning restore STS001
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;
    
    protected override bool IsVisibleInternal => false;

    public override Decimal ModifyHpLostAfterOstyLate(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == Owner && dealer != null && dealer.Monster is Roland)
        {
            if (amount >= Owner.CurrentHp)
            {
                if (Owner.CurrentHp - 1 == 0)
                {
                    Node? vfxContainer = target.GetVfxContainer();
                    NDamageNumVfx? child = NDamageNumVfx.Create(target, (int)amount);
                    if (child != null)
                    {
                        if (vfxContainer != null)
                        {
                            vfxContainer.AddChildSafely(child);
                        }
                        else
                        {
                            NRun.Instance?.GlobalUi.AddChildSafely(child);
                        }
                    }
                    NGame.Instance?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Short);
                }
                return Owner.CurrentHp - 1;
            }
        }
        return amount;
    }
}