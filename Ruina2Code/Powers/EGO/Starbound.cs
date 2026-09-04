using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class Starbound() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("Applier")];
    
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Applier != null && Applier.Player != null)
        {
            ((StringVar)DynamicVars["Applier"]).StringValue = PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, Applier.Player.NetId);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner))
            return;
        if (Applier != null && Applier.Player != null && Applier.Player.PlayerCombatState != null)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, Amount * Applier.Player.PlayerCombatState.Stars, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            await Cmd.CustomScaledWait(0.1f, 0.25f);
        }
    }
}