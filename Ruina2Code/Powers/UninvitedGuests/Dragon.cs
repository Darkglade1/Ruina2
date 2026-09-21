using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Dragon() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    
    public override int DisplayAmount => DynamicVars["CardCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new("CardCounter",0), new("Exhaust",1)];
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Target && Target.Player != null)
        {
            DynamicVars["CardCounter"].BaseValue += 1;
            if (DynamicVars["CardCounter"].BaseValue >= Amount)
            {
                Flash();
                CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, DynamicVars["Exhaust"].IntValue);
                foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, Target.Player, prefs, null, this))
                {
                    await CardCmd.Exhaust(choiceContext, card);
                    if (card.Type == CardType.Status)
                    {
                        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Target.Player);
                    }
                }
                DynamicVars["CardCounter"].BaseValue = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
}