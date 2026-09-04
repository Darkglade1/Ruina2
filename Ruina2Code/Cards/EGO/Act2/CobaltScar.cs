using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Powers.EGO;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class CobaltScar() : EGOCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Unpowered), new CardsVar(5)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        (await PowerCmd.Apply<CobaltScarPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this))?.SetDamage(DynamicVars.Damage.BaseValue);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}