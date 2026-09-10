using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;

namespace Ruina2.Ruina2Code.Extensions;

public static class RelicRarityExtensions
{
    public static LocString ToLocString(this RelicRarity relicRarity)
    {
        LocString locString;
        switch (relicRarity)
        {
            case RelicRarity.Common:
                locString = new LocString("gameplay_ui", "RELIC_RARITY.COMMON");
                break;
            case RelicRarity.Uncommon:
                locString = new LocString("gameplay_ui", "RELIC_RARITY.UNCOMMON");
                break;
            case RelicRarity.Rare:
                locString = new LocString("gameplay_ui", "RELIC_RARITY.RARE");
                break;
            case RelicRarity.Event:
                locString = new LocString("gameplay_ui", "RELIC_RARITY.EVENT");
                break;
            default:
                locString = new LocString("gameplay_ui", "RELIC_RARITY.UNCOMMON");
                break;
        }
        return locString;
    }
}