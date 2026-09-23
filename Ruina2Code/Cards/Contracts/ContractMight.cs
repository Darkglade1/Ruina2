using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Pluto;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Cards.Contracts;

[Pool(typeof(StatusCardPool))]
public class ContractMight() : Ruina2Card(-1, CardType.Status,
    CardRarity.Status, TargetType.None), Pluto.IChoosable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(4), new CardsVar(6)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];
    
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;
    
    public async Task OnChosen()
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ContractMightPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
    }
}