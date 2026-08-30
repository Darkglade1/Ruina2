using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class GrinderPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override int DisplayAmount => DynamicVars["DamageCounter"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("DamageCounter",0), new("DamageThreshold",0)];

    private bool triggered = false;
    
    public void SetDamageThreshold(Decimal damageThreshold)
    {
        AssertMutable();
        DynamicVars["DamageThreshold"].BaseValue = damageThreshold;
    }
    
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            DynamicVars["DamageCounter"].BaseValue = 0;
            triggered = false;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }

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
            if (!triggered)
            {
                DynamicVars["DamageCounter"].BaseValue += result.UnblockedDamage + result.OverkillDamage;
                if (DynamicVars["DamageCounter"].BaseValue >= DynamicVars["DamageThreshold"].BaseValue)
                {
                    Flash();
                    DynamicVars["DamageCounter"].BaseValue = DynamicVars["DamageThreshold"].BaseValue;
                    triggered = true;
                    await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, Amount, Owner, null);
                }
                InvokeDisplayAmountChanged();
            }
        }
    }
}