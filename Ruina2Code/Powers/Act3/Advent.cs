using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Powers.Act3;
public class Advent() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override int DisplayAmount => DynamicVars["CardCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CardCounter",0), new("Turns",4)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var hoverTips = HoverTipFactory.FromAffliction<Apostle>();
            hoverTips = hoverTips.Concat(HoverTipFactory.FromAffliction<Judas>());
            return hoverTips;
        }
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature == Target && DynamicVars["CardCounter"].BaseValue < Amount)
        {
            DynamicVars["CardCounter"].BaseValue++;
            if (DynamicVars["CardCounter"].BaseValue >= Amount)
            {
                Flash();
                if (cardPlay.Card.Affliction != null)
                {
                    CardCmd.ClearAffliction(cardPlay.Card);
                }
                else
                {
                    await CardCmd.Afflict<Judas>(cardPlay.Card, 1);   
                }
                DynamicVars["CardCounter"].BaseValue = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
}