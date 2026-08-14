using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class PromiseOfWinter() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    
    public override int DisplayAmount => DynamicVars["CardCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CardCounter",0)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Frozen>();
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Target)
        {
            DynamicVars["CardCounter"].BaseValue += 1;
            if (DynamicVars["CardCounter"].BaseValue == Amount - 1)
            {
                StartPulsing();
            }
            if (DynamicVars["CardCounter"].BaseValue >= Amount)
            {
                Flash();
                await CardCmd.Afflict<Frozen>(cardPlay.Card, 1);
                DynamicVars["CardCounter"].BaseValue = 0;
                StopPulsing();
            }
            InvokeDisplayAmountChanged();
        }
    }
    
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation cardLocation)
    {
        if (card.Owner.Creature == Target && DynamicVars["CardCounter"].BaseValue == Amount - 1)
        {
            if (card.Type != CardType.Power && !card.Keywords.Contains(CardKeyword.Exhaust))
            {
                return new CardLocation(card.Owner, PileType.Draw, CardPilePosition.Random);
            }
        }
        return cardLocation;
    }
}