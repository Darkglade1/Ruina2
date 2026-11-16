using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Dominions.Act4;

public class ArgaliaDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Vibration>(), HoverTipFactory.FromCard<ControlledVibration>()];
  
  protected override async Task AfterPlayersEnteredCombat(IReadOnlyList<Player> players)
  {
    AssertMutable();
    foreach (Player player in players)
    {
      await PowerCmd.Apply<LifePlucked>(new ThrowingPlayerChoiceContext(),player.Creature, 2, null, null);
    }
  }
}
