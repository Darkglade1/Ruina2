using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers;

public class MultiplayerAlly() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer == Owner && props.IsPoweredAttack() && target != null && target.Monster is AbstractRuinaMonster)
        {
            return 1M + (decimal)Amount / 100;
        }
        if (target == Owner && props.IsPoweredAttack() && dealer != null && dealer.Monster is AbstractRuinaMonster)
        {
            return 1M + (decimal)Amount / 100;
        }
        return 1M;
    }
}