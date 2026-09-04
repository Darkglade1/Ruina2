using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class Nihil() : EGOCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(2)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    
    protected override bool ShouldGlowGoldInternal => NoCardsInHand;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState != null)
        {
            int strLossMultiplier = 1;
            if (PileType.Hand.GetPile(Owner).Cards.Count <= 0)
            {
                strLossMultiplier = 2;
            }
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), CombatState.HittableEnemies,
                -DynamicVars["StrengthPower"].BaseValue * strLossMultiplier, Owner.Creature, this);
        }
    }

    public bool NoCardsInHand => PileType.Hand.GetPile(Owner).Cards.Count - 1 <= 0; //excludes itself

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1);
    }
}