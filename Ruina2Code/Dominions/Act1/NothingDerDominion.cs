using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Dominions.Act1;

public class NothingDerDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [];
  
  public override async Task AfterRoomEntered(AbstractRoom room)
  {
    if (!(room is CombatRoom combatRoom))
      return;
    await PowerCmd.Apply<InevitableBullet>(new ThrowingPlayerChoiceContext(),combatRoom.CombatState.Players.Select<Player, Creature>(p => p.Creature).ToArray(), 7, null, null);
  }
}
