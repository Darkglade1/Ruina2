using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards.Quests;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.RestSite;
public class PrescriptRestSiteOption(Player owner) : CustomRestSiteOption(owner)
{
  public override string OptionId => "RUINA2-BURN-PRESCRIPT";

  public override string CustomIconPath => "BurnPrescriptIcon.png".UIImagePath();

  public override async Task<bool> OnSelect()
  {
    List<CardModel> list = PileType.Deck.GetPile(Owner).Cards.Where(c => c is Prescript).ToList();
    foreach (CardModel original in list)
    {
      await CardCmd.TransformTo<PrescriptAshes>(original);
    }
    return true;
  }
}
