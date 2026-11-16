using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
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
  
  protected override Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
  {
    return PowerCmd.Apply<PainfulStabsPower>(new ThrowingPlayerChoiceContext(), creatures, 1, null, null);
  }
}
