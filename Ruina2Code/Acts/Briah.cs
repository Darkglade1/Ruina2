using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using Ruina2.Ruina2Code.Encounters.Act2;

public class Briah() : CustomActModel(1)
{
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return [
            ModelDb.Encounter<BowlbugsNormal>(),
            ModelDb.Encounter<BowlbugsWeak>(),
            ModelDb.Encounter<ChompersNormal>(),
            ModelDb.Encounter<DecimillipedeElite>(),
            ModelDb.Encounter<EntomancerElite>(),
            ModelDb.Encounter<ExoskeletonsNormal>(),
            ModelDb.Encounter<ExoskeletonsWeak>(),
            ModelDb.Encounter<HunterKillerNormal>(),
            ModelDb.Encounter<InfestedPrismsElite>(),
            ModelDb.Encounter<LouseProgenitorNormal>(),
            ModelDb.Encounter<MytesNormal>(),
            ModelDb.Encounter<OvicopterNormal>(),
            ModelDb.Encounter<SlumberingBeetleNormal>(),
            ModelDb.Encounter<SpinyToadNormal>(),
            ModelDb.Encounter<TheObscuraNormal>(),
            ModelDb.Encounter<ThievingHopperWeak>(),
            ModelDb.Encounter<TunnelerWeak>(),
            ModelDb.Encounter<RedWolfEncounterBoss>(),
        ];
    }

    public override IEnumerable<EventModel> AllEvents => ModelDb.Act<Hive>().AllEvents;

    protected override string CustomMapTopBgPath => ModelDb.Act<Hive>().MapTopBgPath;
    protected override string CustomMapMidBgPath => ModelDb.Act<Hive>().MapMidBgPath;
    protected override string CustomMapBotBgPath => ModelDb.Act<Hive>().MapBotBgPath;
    protected override string CustomRestSiteBackgroundPath => ModelDb.Act<Hive>().RestSiteBackgroundPath;
}