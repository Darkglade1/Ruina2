using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class Nihil() : EGOCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(2)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    
    protected override bool ShouldGlowGoldInternal => OnlyCardInHand;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState != null)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), CombatState.HittableEnemies,
                -DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
        }
    }
    
    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        if (card == this)
        {
            if (OnlyCardInHand)
            {
                modifiedCost = 0M;
                return true;
            }   
        }
        modifiedCost = originalCost;
        return false;
    }

    public bool OnlyCardInHand => PileType.Hand.GetPile(Owner).Cards.Count <= 1 && PileType.Hand.GetPile(Owner).Cards.Contains(this);

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1);
    }
}