using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Pattern() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer == Owner && props.IsPoweredAttack())
        {
            if (result.UnblockedDamage > 0 && !result.Receiver.IsPet)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner, result.UnblockedDamage, ValueProp.Unpowered, null);
            }
        }
    }
}