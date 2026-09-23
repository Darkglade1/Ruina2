using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Pluto;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Cards.Contracts;

[Pool(typeof(StatusCardPool))]
public class ContractLight() : Ruina2Card(-1, CardType.Status,
    CardRarity.Status, TargetType.None), Pluto.IChoosable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        EnergyHoverTip
    ];
    
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;
    
    public async Task OnChosen()
    {
        await PowerCmd.Apply<ContractLightPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
    }
}