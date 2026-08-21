using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class Enchanted() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars["DamageCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("DamageCounter",0)];
    
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        DynamicVars["DamageCounter"].BaseValue = Amount;
        InvokeDisplayAmountChanged();
        if (Owner.Monster != null && Owner.Monster is AbstractAllyMonster ally)
        {
            ally.IsTargetableByPlayers = true;
            ally.SetToSide(CombatSide.Enemy);
        }
        return Task.CompletedTask;
    }
    
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == Owner)
        {
            DynamicVars["DamageCounter"].BaseValue -= result.UnblockedDamage;
            if (DynamicVars["DamageCounter"].BaseValue <= 0)
            {
                await PowerCmd.Remove<Enchanted>(Owner);
            }
            else
            {
                InvokeDisplayAmountChanged();
            }
        }
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (oldOwner.Monster != null && oldOwner.Monster is AbstractAllyMonster ally)
        {
            ally.IsTargetableByPlayers = false;
            ally.SetToSide(CombatSide.Player);
        }
        return Task.CompletedTask;
    }
    
    public override Decimal ModifyDamageCap(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != null && dealer == Owner)
        {
            return 1M;
        }
        return Decimal.MaxValue;
    }
}