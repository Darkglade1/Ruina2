using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Cards.Performer;

[Pool(typeof(StatusCardPool))]
public class SecondChair() : PerformerCard(1,
    CardType.Status, CardRarity.Status,
    TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    { 
        bool alreadyHasWeak = Owner.Creature.HasPower<WeakPower>();
        PowerModel? powerModel = await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, DynamicVars.Weak.BaseValue, null, this);
        if (powerModel == null || alreadyHasWeak)
            return;
        powerModel.SkipNextDurationTick = true;
    }

    public override int MaxUpgradeLevel => 0;
}