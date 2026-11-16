using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Dominions.Act1;

public class FairyDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
  
  protected override Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
  {
    return PowerCmd.Apply<RavenousHunger>(new ThrowingPlayerChoiceContext(), creatures, 3, null, null);
  }
}
