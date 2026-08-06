using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act1;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class HelperTempStr : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Monster<AllAroundHelper>();
    
    public string CustomPackedIconPath => "flex.png".PowerImagePath();
    public string CustomBigIconPath => "flex.png".BigPowerImagePath();

    protected override bool IsPositive => true;
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!SkipNextDurationTick)
        {
            await base.AfterSideTurnEnd(choiceContext, side, participants);
        }
        else
        {
            SkipNextDurationTick = false;
        }
    }
    
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