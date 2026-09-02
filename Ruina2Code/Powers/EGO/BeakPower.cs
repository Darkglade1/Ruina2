using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class BeakPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            Flash();
            var cards = PileType.Hand.GetPile(player).Cards.ToList();
            cards.Sort((card1, card2) => card1.EnergyCost.GetResolved() < card2.EnergyCost.GetResolved() ? 1 : -1);
            for (int i = 0; i < Amount; i++)
            {
                if (i >= cards.Count)
                {
                    break;
                }
                var card = cards[i];
                card.EnergyCost.SetThisCombat(0);
                if (card.Type != CardType.Power && !card.Keywords.Contains(CardKeyword.Exhaust))
                {
                    card.AddKeyword(CardKeyword.Exhaust);
                }
            }
        }
    }
}