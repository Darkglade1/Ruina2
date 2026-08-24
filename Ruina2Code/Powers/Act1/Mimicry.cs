using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Mimicry() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars["MostRecentDamage"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("MostRecentDamage",0), new("DamageCap", 20)];

    public override Decimal ModifyDamageAdditive(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer == Owner && props.IsPoweredAttack())
        {
            return DynamicVars["MostRecentDamage"].IntValue;
        }
        return 0;
    }
    
    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == Owner && props.IsPoweredAttack() && cardSource != null && !result.WasFullyBlocked)
        {
            DynamicVars["MostRecentDamage"].BaseValue = result.UnblockedDamage;
            if (DynamicVars["MostRecentDamage"].BaseValue > DynamicVars["DamageCap"].BaseValue)
            {
                DynamicVars["MostRecentDamage"].BaseValue = DynamicVars["DamageCap"].BaseValue;
            }
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}