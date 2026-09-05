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

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0), new("AttackCounter",0)];
    
    public void SetCards(Decimal cards)
    {
        AssertMutable();
        DynamicVars.Cards.BaseValue = cards;
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.Owner.Creature == Owner)
        {
            DynamicVars["AttackCounter"].BaseValue += 1;
            if (DynamicVars["AttackCounter"].BaseValue >= DynamicVars.Cards.IntValue)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
                DynamicVars["AttackCounter"].BaseValue = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
}