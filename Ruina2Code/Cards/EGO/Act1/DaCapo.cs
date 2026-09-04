using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

public class DaCapo() : EGOCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardAttack(this, play).Execute(choiceContext);
        var lastCard = GetLastPlayedCard();
        if (lastCard != null)
        {
            var dupe = lastCard.CreateDupe(Owner);
            await CardCmd.AutoPlay(choiceContext, dupe, null);
        }
    }

    private CardModel? GetLastPlayedCard()
    {
        List<CardPlayFinishedEntry> cardHistoryList = CombatManager.Instance.History.CardPlaysFinished.ToList();
        for (int i = cardHistoryList.Count - 1; i >= 0; i--)
        {
            CardModel card = cardHistoryList[i].CardPlay.Card;
            if (card.Owner == Owner && !(card is DaCapo))
            {
                return card;
            }
        }
        return null;
    }

    protected override void OnUpgrade()
    {
       RemoveKeyword(CardKeyword.Exhaust);
    }
}