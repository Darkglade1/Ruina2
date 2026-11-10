using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class ContractLibertyPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (card.Owner != this.Owner.Player || card.EnergyCost.Canonical < 0)
            return Task.CompletedTask;
        int cost = this.NextEnergyCost();
        card.EnergyCost.SetThisCombat(cost);
        NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
        return Task.CompletedTask;
    }

    public int NextEnergyCost()
    {
        if (Owner.Player != null)
        {
            return Owner.Player.RunState.Rng.CombatEnergyCosts.NextInt(4);
        }
        return 1;
    }
}