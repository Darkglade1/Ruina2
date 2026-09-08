using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.EGO;


public class HornetPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker != Owner || !command.DamageProps.IsPoweredAttack())
            return Task.CompletedTask;
        Data internalData = GetInternalData<Data>();
        if (internalData.commandToModify != null || command.ModelSource != null && !(command.ModelSource is CardModel) || !command.DamageProps.IsPoweredAttack())
            return Task.CompletedTask;
        internalData.commandToModify = command;
        internalData.amountWhenAttackStarted = Amount;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative
    (
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner != dealer || !props.IsPoweredAttack())
            return 1M;
        Data internalData = GetInternalData<Data>();
        return internalData.commandToModify != null && cardSource != null && cardSource != internalData.commandToModify.ModelSource || internalData.commandToModify != null && internalData.commandToModify.Attacker != dealer ? 1M : 2M;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        Data internalData = GetInternalData<Data>();
        if (command != internalData.commandToModify)
            return;
        await PowerCmd.ModifyAmount(choiceContext, this, -internalData.amountWhenAttackStarted, null, null);
    }
    
    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        if (card.Owner.Creature == Owner && card.Type == CardType.Attack)
        {
            modifiedCost = 0M;
            return true; 
        }
        modifiedCost = originalCost;
        return false;
    }

    public class Data
    {
        public AttackCommand? commandToModify;
        public int amountWhenAttackStarted;
    }
}