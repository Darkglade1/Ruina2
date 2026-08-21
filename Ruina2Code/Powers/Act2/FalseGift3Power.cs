using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Powers.Act2;
public class FalseGift3Power() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(0, ValueProp.Unpowered)];
    
    public override string CustomPackedIconPath => "false_gift.png".PowerImagePath();
    public override string CustomBigIconPath => "false_gift.png".BigPowerImagePath();
    
    public void SetDamage(Decimal damage)
    {
        AssertMutable();
        DynamicVars.Damage.BaseValue = damage;
    }
    
    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return;
        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
        }
        else
        {
            Flash();
            await CreatureCmd.Damage(choiceContext, Owner, DynamicVars.Damage.IntValue, ValueProp.Unpowered, Owner, null);
            await PowerCmd.Remove(this);
        }
    }
}