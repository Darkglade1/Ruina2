using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Dominions.Act2;

public class JesterDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [];
  
  protected override async Task AfterPlayersEnteredCombat(IReadOnlyList<Player> players)
  {
    AssertMutable();
    foreach (Player player in players)
    {
      await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(),player.Creature, -2, null, null);
    }
  }
}
