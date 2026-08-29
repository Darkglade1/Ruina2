using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

public class Wingbeat() : EGOCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];
    
    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        int num = ResolveEnergyXValue();
        if (IsUpgraded)
        {
            num++;
        }
        if (num > 0)
        {
            var cards = await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, num), null, this);
            foreach (var card in cards)
            {
                await CardCmd.Exhaust(choiceContext, card);
                await CreatureCmd.Heal(Owner.Creature, card.EnergyCost.GetResolved());
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature,
                    card.EnergyCost.GetResolved(), Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}