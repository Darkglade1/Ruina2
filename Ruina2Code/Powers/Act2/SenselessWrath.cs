using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class SenselessWrath() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromPowerWithPowerHoverTips<ArtifactPower>();
    
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount != 0 && power.GetTypeForAmount(amount) == PowerType.Debuff && power.Owner == Owner &&
            !(power is ITemporaryPower))
        {
            Flash();
            await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner, Amount, Owner, null);
        }
    }
}