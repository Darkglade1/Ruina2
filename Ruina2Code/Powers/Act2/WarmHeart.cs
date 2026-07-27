using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class WarmHeart() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new ("StrengthCap", 3)];
    
    public override Decimal ModifyMaxEnergy(Player player, Decimal amount)
    {
        return amount + Amount;
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            int totalEnergyLeft = 0;
            foreach (var player in CombatState.Players)
            {
                if (player.PlayerCombatState != null)
                {
                    totalEnergyLeft += player.PlayerCombatState.Energy;
                }
            }

            if (totalEnergyLeft > DynamicVars["StrengthCap"].BaseValue)
            {
                totalEnergyLeft = DynamicVars["StrengthCap"].IntValue;
            }

            if (totalEnergyLeft > 0)
            {
                Flash();
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, totalEnergyLeft, Owner,  null);
            }
        }
    }
}