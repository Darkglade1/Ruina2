using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class FrostSplinterPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Decimal ModifyHandDraw(Player player, Decimal count)
    {
        return player != Owner.Player ? count : count + Amount;
    }

    public override Decimal ModifyMaxEnergy(Player player, Decimal amount)
    {
        return player != Owner.Player ? amount : amount + Amount;
    }
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            var locstring = new LocString("card_selection", "RUINA2-TO_FREEZE");
            foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, player, new CardSelectorPrefs(locstring, Amount), (Func<CardModel, bool>) (c =>
                     {
                         if (c.Affliction is Frozen)
                         {
                             return false;
                         }
                         return true;
                     }), this))
            {
                if (card.Affliction != null)
                {
                    CardCmd.ClearAffliction(card);
                }
                await CardCmd.Afflict<Frozen>(card, 1);
            }
        }
    }

    public override bool ShouldAfflict(CardModel card, AfflictionModel affliction)
    {
        if (card.Affliction is Frozen)
        {
            return false;
        }
        return true;
    }
}