using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class GlitteringWish() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1), new GoldVar(300)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    
    public override bool HasUponPickupEffect => true;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) && Owner.PlayerCombatState != null)
        {
            if (Owner.PlayerCombatState.TurnNumber == 1)
            {
                Flash();
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, -DynamicVars.Strength.IntValue, Owner.Creature, null);
            }
        }
    }
    
    public override async Task AfterObtained()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
    }
}