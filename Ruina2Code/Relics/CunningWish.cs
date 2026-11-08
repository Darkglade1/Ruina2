using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class CunningWish() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3), new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    public override Decimal ModifyHandDraw(Player player, Decimal count)
    {
        return player != Owner || Owner.PlayerCombatState!.TurnNumber > 1 ? count : count + DynamicVars.Cards.BaseValue;
    }
    
    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState!.TurnNumber > 1)
            return;
        Flash();
        await PlayerCmd.LoseEnergy(DynamicVars.Energy.BaseValue, Owner);
    }
}