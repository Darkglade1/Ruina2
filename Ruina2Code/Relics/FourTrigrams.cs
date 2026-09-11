using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class FourTrigrams() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new EnergyVar(1), new PowerVar<StrengthPower>(1), new PowerVar<DexterityPower>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this), HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>()];

    public override Decimal ModifyHandDraw(Player player, Decimal count)
    {
        if (player == Owner && Owner.PlayerCombatState != null && Owner.PlayerCombatState.TurnNumber == 1)
        {
            return count + DynamicVars.Cards.BaseValue;
        }
        return count;
    }
    
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
            }
            if (Owner.PlayerCombatState.TurnNumber == 2)
            {
                Flash();
                await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            }
            if (Owner.PlayerCombatState.TurnNumber == 3)
            {
                Flash();
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Strength.IntValue, Owner.Creature, null);
            }
            if (Owner.PlayerCombatState.TurnNumber == 4)
            {
                Flash();
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Dexterity.IntValue, Owner.Creature, null);
            }
        }
    }
}