using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Dominions.Act1;

public class BlackSwanDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [];
  
  protected override async Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
  {
    var enumerable = creatures.ToList();
    await PowerCmd.Apply<BlackSwanDominionBlock>(new ThrowingPlayerChoiceContext(), enumerable, 15, null, null);
    await PowerCmd.Apply<BlurPower>(new ThrowingPlayerChoiceContext(), enumerable, 1, null, null);
  }
}
