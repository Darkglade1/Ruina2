using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Cards.Performer;

[Pool(typeof(StatusCardPool))]
public class FourthChair() : PerformerCard(1,
    CardType.Status, CardRarity.Status,
    TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    { 
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, -DynamicVars.Strength.BaseValue, null, this);
    }

    public override int MaxUpgradeLevel => 0;
}