using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class FreshMeatPower : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private CardModel? _stolenCard;
    public CardModel? StolenCard
    {
        get => _stolenCard;
        set
        {
            AssertMutable();
            _stolenCard = value;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return StolenCard == null ? Array.Empty<IHoverTip>() : [HoverTipFactory.FromCard(StolenCard)];
        }
    }

    public override async Task BeforeDeath(Creature target)
    {
        if (Owner == target && StolenCard != null)
        {
            await CardPileCmd.Add([StolenCard], PileType.Hand);
        }
    }

    public async Task Steal(CardModel card)
    {
       Target = card.Owner.Creature; 
       StolenCard = card;
    }
    
}