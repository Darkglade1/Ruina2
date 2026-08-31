using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace Ruina2.Ruina2Code.Powers.EGO;
public class AspirationPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterCombatEnd(CombatRoom _)
    {
        if (Owner.IsDead)
            return;
        Flash();
        await CreatureCmd.Heal(Owner, Amount);
    }
}