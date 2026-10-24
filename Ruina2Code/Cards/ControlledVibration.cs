using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Cards;

[Pool(typeof(StatusCardPool))]
public class ControlledVibration() : Ruina2Card(0, CardType.Status,
    CardRarity.Status, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<Vibration>(2)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<Vibration>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<Vibration>(choiceContext, Owner.Creature, DynamicVars["Vibration"].IntValue, Owner.Creature, this);
    }
    
    public override int MaxUpgradeLevel => 0;
}