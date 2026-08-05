using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards;

[Pool(typeof(StatusCardPool))]
public class Enlightenment() : Ruina2Card(0, CardType.Status,
    CardRarity.Status, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(2), new BlockVar(5, ValueProp.Move), new CardsVar(1)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, play);
        await CommonActions.CardBlock(this, play);
        await CommonActions.Draw(this, choiceContext);
    }
    
    public override int MaxUpgradeLevel => 0;
}