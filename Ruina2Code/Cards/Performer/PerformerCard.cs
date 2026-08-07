using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Ruina2.Ruina2Code.Cards.Performer;

[Pool(typeof(StatusCardPool))]
public abstract class PerformerCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    Ruina2Card(cost, type, rarity, target), ICustomTypeTextCard
{
    public IEnumerable<LocString> GetTypeModifiers()
    {
        var type = new LocString("static_hover_tips", "RUINA2-PERFORMER_TYPE.description");
        return [type];
    }
}