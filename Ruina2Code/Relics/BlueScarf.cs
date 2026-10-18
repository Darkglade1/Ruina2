using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class BlueScarf() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    public override bool HasUponPickupEffect => true;
    
    public override async Task AfterObtained()
    {
        await RunManager.Instance.GenerateMap();
    }

    public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
    {
        if (runState.Act is UninvitedGuests)
        {
            return new UninvitedGuestsActMapShort();
        }
        return map;
    }
}