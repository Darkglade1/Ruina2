using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Cards.EGO;

[Pool(typeof(EGOCardPool))]
public abstract class EGOCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    Ruina2Card(cost, type, rarity, target)
{
    public string? CustomPortraitBorderPath => $"cards/border_{Type.ToString().ToLowerInvariant()}_ego.png".ImagePath();
    
    public string? CustomBannerTexturePath =>  "cards/banner_ego.png".ImagePath();
    
    public string? CustomPlaqueTexturePath =>  "cards/plaque_ego.png".ImagePath();
}