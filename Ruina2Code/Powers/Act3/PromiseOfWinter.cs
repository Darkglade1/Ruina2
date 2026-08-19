using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class PromiseOfWinter() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;
    public override PowerStackType StackType =>
        PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Frozen>();
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
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
            await CardCmd.Afflict<Frozen>(card, 1);
        }
    }
}