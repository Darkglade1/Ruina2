using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers;

public class TransferBlockAlly() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.None;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("Ally"), new StringVar("AllyName")];
    
    public override async Task AfterBlockGained(
        Creature creature,
        Decimal amount,
        ValueProp props,
        CardModel? cardSource)
    {
        if (amount > 0 && creature == Owner && Owner.CombatState != null)
        {
            // Ensure only one of these trigger at a time
            if (Owner.GetPower<TransferBlockAlly>() != this)
            {
                return;
            }
            ModelId id = new ModelId("MONSTER", ((StringVar)DynamicVars["Ally"]).StringValue);
            var ally = Owner.CombatState.Enemies.FirstOrDefault(c => c.ModelId == id && c.IsAlive);
            if (ally?.Monster is AbstractAllyMonster)
            {
                var blockToGive = Math.Min(Amount, amount);
                if (blockToGive > 0 && Owner.Block >= blockToGive)
                {
                    Owner.Block -= (int)blockToGive;
                    await CreatureCmd.GainBlock(ally, blockToGive, ValueProp.Unpowered, null, true);
                    await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -blockToGive, null, null, true);
                }
            }
        }
    }
    
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}