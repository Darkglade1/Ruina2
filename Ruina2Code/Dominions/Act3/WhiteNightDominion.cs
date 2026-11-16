using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Dominions.Act3;

public class WhiteNightDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [];
  
  protected override async Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
  {
    var enumerable = creatures.ToList();
    await PowerCmd.Apply<MonsterRegen>(new ThrowingPlayerChoiceContext(), enumerable, 10, null, null);
    await PowerCmd.Apply<RitualPower>(new ThrowingPlayerChoiceContext(), enumerable, 1, null, null);
  }
}
