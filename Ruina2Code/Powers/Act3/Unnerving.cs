using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class Unnerving() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target == Owner && props.IsPoweredAttack() && dealer != null)
        {
            return 1M - (decimal)Amount / 100;
        }
        return 1M;
    }
    
    public override async Task AfterBlockGained(
        Creature creature,
        Decimal amount,
        ValueProp props,
        CardModel? cardSource)
    {
        if (amount > 0 && creature.Player != null)
        {
            Flash();
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, amount,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, null, null);
        }
    }
}