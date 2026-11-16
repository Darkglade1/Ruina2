using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Dominions.Act3;

public class SilentGirlDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Nail>(), HoverTipFactory.FromPower<PainfulStabsPower>()];
  
  protected override async Task AfterPlayersEnteredCombat(IReadOnlyList<Player> players)
  {
    AssertMutable();
    foreach (Player player in players)
    {
      await PowerCmd.Apply<Nail>(new ThrowingPlayerChoiceContext(),player.Creature, 3, null, null);
    }
  }
  
  protected override async Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
  {
    foreach (Creature creature in creatures)
    {
      if (creature.Monster is not AbstractAllyMonster)
      {
        await PowerCmd.Apply<PainfulStabsPower>(new ThrowingPlayerChoiceContext(), creature, 1, null, null);
      }
    }
  }
}
