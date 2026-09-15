using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Powers.EGO;
public class DeadSilencePower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override string CustomPackedIconPath => "ticking_time.png".PowerImagePath();
    public override string CustomBigIconPath => "ticking_time.png".BigPowerImagePath();

    public override bool ShouldTakeExtraTurn(Player player) => player.Creature == Owner;
    
    public override Task AfterTakingExtraTurn(Player player)
    {
        return player.Creature == Owner ? PowerCmd.Decrement(this) : Task.CompletedTask;
    }
}