using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act3;

public class Remorse() : EGOCard(4,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move), new PowerVar<WeakPower>(2), new("Cap", 4),
        new CalculationBaseVar(0M),
        new CalculationExtraVar(1M),
        new CalculatedVar("CalculatedExhaust").WithMultiplier((card, _) => Math.Min(card.DynamicVars["Cap"].IntValue, PileType.Hand.GetPile(card.Owner).Cards.Count(e => e != card)))];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>(), 
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardAttack(this, play).Execute(choiceContext);
        if (play.Target != null)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, DynamicVars.Weak.IntValue, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, DynamicVars.Weak.IntValue, Owner.Creature, this);
        }

        var exhaustNum = (int)((CalculatedVar)DynamicVars["CalculatedExhaust"]).Calculate(null);
        if (exhaustNum > 0)
        {
            var cards = await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, exhaustNum), null, this);
            foreach (var card in cards)
            {
                await CardCmd.Exhaust(choiceContext, card);
            }
        }
    }
    
    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        if (card == this)
        {
            var costReduction = (int)((CalculatedVar)DynamicVars["CalculatedExhaust"]).Calculate(null);
            if (costReduction > 0)
            {
                modifiedCost = originalCost - costReduction;
                return true;
            }   
        }
        modifiedCost = originalCost;
        return false;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}