using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Dominions.Act1;

public class BlackSwanDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [];
  
  protected override async Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
  {
    var enumerable = creatures.ToList();
    foreach (var creature in enumerable)
    {
      await CreatureCmd.GainBlock(creature, 15, ValueProp.Move, null);
    }
    await PowerCmd.Apply<BlurPower>(new ThrowingPlayerChoiceContext(), enumerable, 1, null, null);
  }
}
