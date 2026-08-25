using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Ruina2.Ruina2Code.Cards.EGO;

[Pool(typeof(EGOCardPool))]
public abstract class EGOCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    Ruina2Card(cost, type, rarity, target)
{
   
}