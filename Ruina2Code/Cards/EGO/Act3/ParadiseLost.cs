using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Powers.EGO;

namespace Ruina2.Ruina2Code.Cards.EGO.Act3;

public class ParadiseLost() : EGOCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(150, ValueProp.Unpowered), new CardsVar(1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        (await PowerCmd.Apply<ParadiseLostPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Damage.BaseValue, Owner.Creature, this))?.SetExhaustAmount(DynamicVars.Cards.BaseValue);
    }

    protected override void OnUpgrade()
    {
       DynamicVars.Damage.UpgradeValueBy(50);
    }
}