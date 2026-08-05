using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class FearTempStrLoss : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Power<Fear>();
    
    public string CustomPackedIconPath => "flex.png".PowerImagePath();
    public string CustomBigIconPath => "flex.png".BigPowerImagePath();

    protected override bool IsPositive => false;
    
    public override LocString Title
    {
        get
        {
            switch (OriginModel)
            {
                case CardModel cardModel:
                    return cardModel.TitleLocString;
                case PotionModel potionModel:
                    return potionModel.Title;
                case RelicModel relicModel:
                    return relicModel.Title;
                case PowerModel powerModel:
                    return powerModel.Title;
                case MonsterModel monsterModel:
                    return monsterModel.Title;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            List<IHoverTip> items = new List<IHoverTip>();
            IEnumerable<IHoverTip> collection;
            switch (OriginModel)
            {
                case CardModel card:
                    collection =[HoverTipFactory.FromCard(card)];
                    break;
                case PotionModel _:
                    collection = Array.Empty<IHoverTip>();
                    break;
                case RelicModel relic:
                    collection = HoverTipFactory.FromRelic(relic);
                    break;
                case PowerModel _:
                    collection = Array.Empty<IHoverTip>();
                    break;
                case MonsterModel _:
                    collection = Array.Empty<IHoverTip>();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            items.AddRange(collection);
            items.Add(HoverTipFactory.FromPower<StrengthPower>());
            return items;
        }
    }
}