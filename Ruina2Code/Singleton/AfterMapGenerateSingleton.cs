using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
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
            //MainFile.Logger.Info("Modifying act 4 map");
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
                    //MainFile.Logger.Info("Set encounter at: " + mapPoint.coord.col + mapPoint.coord.row + guests[count]);
                    UninvitedGuestsActMap.MapPointSpecificEncounter.Set(mapPoint, guests[count]);
                    count++;
                }
            }
        }
        return Task.CompletedTask;
    }
}