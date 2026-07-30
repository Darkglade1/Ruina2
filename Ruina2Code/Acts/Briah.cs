using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using Ruina2.Ruina2Code.Encounters.Act2;

namespace Ruina2.Ruina2Code.Acts;

public class Briah() : CustomActModel(1)
{
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return [
            ModelDb.Encounter<BatsWeak>(),
            ModelDb.Encounter<NosferatuWeak>(),
            ModelDb.Encounter<OzmaWeak>(),
            ModelDb.Encounter<BadWolfWeak>(),
            ModelDb.Encounter<NosferatuAndBatNormal>(),
            ModelDb.Encounter<WoodsmanNormal>(),
            ModelDb.Encounter<OzmaAndJackNormal>(),
            ModelDb.Encounter<WolfPackNormal>(),
            ModelDb.Encounter<KnightNormal>(),
            ModelDb.Encounter<GreedNormal>(),
            ModelDb.Encounter<QueenNormal>(),
            ModelDb.Encounter<TheObscuraNormal>(),
            ModelDb.Encounter<TunnelerWeak>(),
            ModelDb.Encounter<MountainElite>(),
            //ModelDb.Encounter<EntomancerElite>(),
            //ModelDb.Encounter<InfestedPrismsElite>(),
            ModelDb.Encounter<RedWolfBoss>(),
        ];
    }

    public override IEnumerable<EventModel> AllEvents => ModelDb.Act<Hive>().AllEvents;

    protected override string CustomMapTopBgPath => ModelDb.Act<Hive>().MapTopBgPath;
    protected override string CustomMapMidBgPath => ModelDb.Act<Hive>().MapMidBgPath;
    protected override string CustomMapBotBgPath => ModelDb.Act<Hive>().MapBotBgPath;
    protected override string CustomRestSiteBackgroundPath => ModelDb.Act<Hive>().RestSiteBackgroundPath;
}