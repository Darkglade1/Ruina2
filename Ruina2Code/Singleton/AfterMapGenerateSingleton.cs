using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Encounters.UninvitedGuests;
using Ruina2.Ruina2Code.Events.Act4;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Singleton;

public class AfterMapGenerateSingleton() : CustomSingletonModel(HookType.Run)
{
    public override Task AfterMapGenerated(ActMap map, int actIndex)
    {
        if (RunManager.Instance.State != null && RunManager.Instance.State.Act is UninvitedGuests)
        {
            if (RunManager.Instance.State.Players[0].GetRelic<BlueScarf>() != null)
            {
                List<EncounterModel> tier1Guests = [
                    ModelDb.Encounter<PhilipEncounter>(),
                    ModelDb.Encounter<EileenEncounter>(),
                    ModelDb.Encounter<GretaEncounter>(),
                ];
                List<EncounterModel> tier2Guests = [
                    ModelDb.Encounter<BremenEncounter>(),
                    ModelDb.Encounter<OswaldEncounter>(),
                    ModelDb.Encounter<TanyaEncounter>(),
                ];
                List<EncounterModel> tier3Guests = [
                    ModelDb.Encounter<PuppeteerEncounter>(),
                    ModelDb.Encounter<ElenaEncounter>(),
                    ModelDb.Encounter<PlutoEncounter>(),
                ];
                int count1 = 0;
                int count2 = 0;
                int count3 = 0;
                foreach (var mapPoint in map.GetAllMapPoints())
                {
                    if (mapPoint.PointType == MapPointType.Elite)
                    {
                        if (mapPoint.coord.row == 1 && count1 < tier1Guests.Count)
                        {
                            UninvitedGuestsActMap.MapPointSpecificEncounter.Set(mapPoint, tier1Guests[count1]);
                            count1++;
                        } else if (mapPoint.coord.row == 3 && count2 < tier2Guests.Count)
                        {
                            UninvitedGuestsActMap.MapPointSpecificEncounter.Set(mapPoint, tier2Guests[count2]);
                            count2++;
                        } else if (mapPoint.coord.row == 5 && count3 < tier3Guests.Count)
                        {
                            UninvitedGuestsActMap.MapPointSpecificEncounter.Set(mapPoint, tier3Guests[count3]);
                            count3++;
                        }
                    }
                }
            }
            else
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
    
    public override EventModel ModifyNextEvent(EventModel currentEvent)
    {
        if (RunManager.Instance.State != null && RunManager.Instance.State.Act is UninvitedGuests)
        {
            ModelDb.Event<Ensemble>();
        }
        return currentEvent;
    }
}