using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Ruina2.Ruina2Code.Afflictions;
using Ruina2.Ruina2Code.Monsters.Act3;

namespace Ruina2.Ruina2Code.Powers.Act3;
public class Advent() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars["CardCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CardCounter",0), new("Turns",6)];

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
        if (DynamicVars["CardCounter"].BaseValue < Amount)
        {
            DynamicVars["CardCounter"].BaseValue++;
            if (DynamicVars["CardCounter"].BaseValue % 12 == 0)
            {
                await CardCmd.Afflict<Judas>(cardPlay.Card, 3);
            }
            else
            {
                await CardCmd.Afflict<Apostle>(cardPlay.Card, 1);
            }
            InvokeDisplayAmountChanged();
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            if (DynamicVars["CardCounter"].BaseValue >= Amount || CombatState.RoundNumber >= DynamicVars["Turns"].IntValue)
            {
                if (Owner.Monster is WhiteNight monster)
                {
                    await monster.Awaken();
                }
            }
        }
    }
}