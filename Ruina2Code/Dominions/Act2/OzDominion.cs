using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards.Gifts;
using Ruina2.Ruina2Code.Monsters.Act2.Oz;

namespace Ruina2.Ruina2Code.Dominions.Act2;

public class OzDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<FalseGift2>(), HoverTipFactory.FromCard<FalseGift3>()];
  
  public override async Task BeforeHandDraw(
    Player player,
    PlayerChoiceContext choiceContext,
    ICombatState combatState)
  {
    if (player.PlayerCombatState?.TurnNumber > 1 || player.Creature.IsDead)
      return;
    CardModel? cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, new List<CardModel>(2)
    {
      combatState.CreateCard(ModelDb.Card<FalseGift2>(), player),
      combatState.CreateCard(ModelDb.Card<FalseGift3>(), player)
    }, player);
    if (cardModel == null)
      return;
    await ((Oz.IChoosable) cardModel).OnChosen();
  }
}
