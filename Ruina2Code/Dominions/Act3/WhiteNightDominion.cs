using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Dominions.Act3;

public class WhiteNightDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MonsterRegen>(), HoverTipFactory.FromPower<RitualPower>()];
  
  protected override async Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
  {
    foreach (Creature creature in creatures)
    {
      if (creature.Monster is not AbstractAllyMonster)
      {
        await PowerCmd.Apply<MonsterRegen>(new ThrowingPlayerChoiceContext(), creature, 10, null, null);
        await PowerCmd.Apply<RitualPower>(new ThrowingPlayerChoiceContext(), creature, 1, null, null);
      }
    }
  }
}
