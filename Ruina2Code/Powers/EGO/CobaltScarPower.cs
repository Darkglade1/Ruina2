using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class CobaltScarPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars["AttackCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(0, ValueProp.Unpowered), new("AttackCounter",0)];
    
    public void SetDamage(Decimal damage)
    {
        AssertMutable();
        DynamicVars.Damage.BaseValue = damage;
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.Owner.Creature == Owner)
        {
            DynamicVars["AttackCounter"].BaseValue += 1;
            if (DynamicVars["AttackCounter"].BaseValue >= Amount)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, DynamicVars.Damage.IntValue, ValueProp.Unpowered, Owner);
                DynamicVars["AttackCounter"].BaseValue = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
}