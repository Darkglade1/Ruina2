using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.Act2.Wrath;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class BlindFury() : Ruina2Power, IHasSecondAmount
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("HPLossCounter",0)];

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
            DynamicVars["HPLossCounter"].BaseValue += result.UnblockedDamage;
            if (DynamicVars["HPLossCounter"].BaseValue > Amount)
            {
                DynamicVars["HPLossCounter"].BaseValue = Amount;
            }
            this.InvokeSecondAmountChanged();
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            if (DynamicVars["HPLossCounter"].BaseValue >= Amount && Owner.Monster is ServantOfWrath wrath)
            {
                DynamicVars["HPLossCounter"].BaseValue = 0;
                this.InvokeSecondAmountChanged();
                wrath.Enrage();
            }
        }
    }
    
    public string GetSecondAmount()
    {
        return DynamicVars["HPLossCounter"].IntValue.ToString();
    }
}