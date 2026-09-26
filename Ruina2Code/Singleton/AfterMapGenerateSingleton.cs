using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Encounters.UninvitedGuests;

namespace Ruina2.Ruina2Code.Singleton;

public class AfterMapGenerateSingleton() : CustomSingletonModel(HookType.Run)
{
    public override Task AfterMapGenerated(ActMap map, int actIndex)
    {
        if (RunManager.Instance.State != null && RunManager.Instance.State.Act is UninvitedGuests)
        {
            List<EncounterModel> guests = [
                ModelDb.Encounter<PhilipEncounter>(),
                ModelDb.Encounter<EileenEncounter>(),
                ModelDb.Encounter<GretaEncounter>(),
                ModelDb.Encounter<BremenEncounter>(),
                ModelDb.Encounter<OswaldEncounter>(),
                ModelDb.Encounter<TanyaEncounter>(),
                ModelDb.Encounter<PuppeteerEncounter>(),
                ModelDb.Encounter<ElenaEncounter>(),
                ModelDb.Encounter<PlutoEncounter>(),
            ];
            int count = 0;
            foreach (var mapPoint in map.GetAllMapPoints())
            {
                if (mapPoint.PointType == MapPointType.Elite && count < guests.Count)
                {
                    UninvitedGuestsActMap.MapPointSpecificEncounter.Set(mapPoint, guests[count]);
                    count++;
                }
            }
        }
        return Task.CompletedTask;
    }
    
    public override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(
        IReadOnlySet<RoomType> roomTypes)
    {
        if (RunManager.Instance.State != null && RunManager.Instance.State.Act is UninvitedGuests)
        {
            return new HashSet<RoomType>
            {
                RoomType.Event
            };
        }
        return roomTypes;
    }
}