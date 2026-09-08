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
public class Overexertion() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2), new PowerVar<WeakPower>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this), HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<FrailPower>()];
    
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
                await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            }

            if (Owner.PlayerCombatState.TurnNumber == 2)
            {
                Flash();
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Weak.IntValue, Owner.Creature, null);
                await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Weak.IntValue, Owner.Creature, null);
            }
        }
    }
}