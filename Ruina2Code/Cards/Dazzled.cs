using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Ruina2.Ruina2Code.Cards;

[Pool(typeof(StatusCardPool))]
public class Dazzled() : Ruina2Card(0, CardType.Status,
    CardRarity.Status, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar("CostIncrease", 1)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (card == this)
        {
            EnergyCost.AddThisCombat(DynamicVars["CostIncrease"].IntValue);
        }
        return Task.CompletedTask;
    }
    
    public override int MaxUpgradeLevel => 0;
}