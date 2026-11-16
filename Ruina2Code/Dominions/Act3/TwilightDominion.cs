using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Dominions.Act3;

public class TwilightDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Dazzled>()];
  
  protected override async Task AfterPlayersEnteredCombat(IReadOnlyList<Player> players)
  {
    AssertMutable();
    foreach (Player player in players)
    {
      await PowerCmd.Apply<BeastSeeking>(new ThrowingPlayerChoiceContext(),player.Creature, 1, null, null);
    }
  }
}
