using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using Ruina2.Ruina2Code.Cards.Performer;

namespace Ruina2.Ruina2Code.Dominions.Act1;

public class OrchestraDominion : Ruina2Dominion
{
  public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SecondChair>(), HoverTipFactory.FromCard<ThirdChair>()];

  protected override async Task AfterPlayersEnteredCombat(IReadOnlyList<Player> players)
  {
    AssertMutable();
    foreach (Player player in players)
    {
      await CardPileCmd.AddToCombatAndPreview<SecondChair>(player.Creature, PileType.Draw, 1, player, CardPilePosition.Random);
      await CardPileCmd.AddToCombatAndPreview<ThirdChair>(player.Creature, PileType.Draw, 1, player, CardPilePosition.Random);
    }
  }
}
